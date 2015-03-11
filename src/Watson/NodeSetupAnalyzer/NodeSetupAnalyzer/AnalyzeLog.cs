using System;
using System.Data;
using System.Diagnostics;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using MDSLogAnalyzerCommon;

namespace NodeSetupAnalyzer
{
    public class NodeSetupAnalyzer : ILogAnalyzer
    {
        public void AnalyzeLogs(ClusterMetadata cluster)
        {
            if (cluster.FormattedTables.ContainsKey(FormattedTables.LogEntry))
            {
                CheckLogEntryLogs(cluster, cluster.FormattedTables[FormattedTables.LogEntry]);
            }
            if(cluster.FormattedTables.ContainsKey(FormattedTables.SetupLog))
            {
                CheckSetupLogs(cluster, cluster.FormattedTables[FormattedTables.SetupLog]);
            }
            if (cluster.FormattedTables.ContainsKey(FormattedTables.HadoopInstallLogs))
            {
                CheckHadoopInstallLogs(cluster, cluster.FormattedTables[FormattedTables.HadoopInstallLogs]);
            }
            // Check if all services are running
            if (cluster.FormattedTables.ContainsKey(FormattedTables.ClusterHealthServiceLogs) &&
                cluster.FormattedTables.ContainsKey(FormattedTables.AvailabilityEventLogs))
            {
                CheckClusterHealthServiceLogs(cluster, cluster.FormattedTables[FormattedTables.ClusterHealthServiceLogs],
                    cluster.FormattedTables[FormattedTables.AvailabilityEventLogs]);
            }
        }
        public void CheckClusterHealthServiceLogs(ClusterMetadata cluster, DataTable clusterHealthTable, DataTable availabilityEventTable)
        {
            if ((clusterHealthTable == null) || (clusterHealthTable.Rows.Count == 0) || (availabilityEventTable == null) || (availabilityEventTable.Rows.Count == 0))
            {
                // no data
                return;
            }
            // Find active head node
            string activeHeadNode = "";
            foreach(DataRow row in availabilityEventTable.Rows)
            {
                if((row["Role"].ToString() == "IsotopeHeadNode") && (row["EventName"].ToString() == "FCMasterTransitionToActive"))
                {
                    activeHeadNode = row["RoleInstance"].ToString();
                }
            }
            HashSet<string> servicesNotRunning = new HashSet<string>();

            foreach (DataRow row in clusterHealthTable.Rows)
            {
                // Get RoleInstance and Details
                string roleInstance = row["RoleInstance"].ToString();
                // if not active headnode, skip it
                if(roleInstance != activeHeadNode)
                {
                    continue;
                }
                string status = row["Status"].ToString();
                string component = row["Component"].ToString();
                string dtStr = row["PreciseTimeStamp"].ToString();
                DateTime preciseTimeStamp = DateTime.Parse(dtStr);
                if(status != "ok")
                {
                    servicesNotRunning.Add(component);

                }
                else if(servicesNotRunning.Contains(component))
                {
                    servicesNotRunning.Remove(component);
                }
            }
            if (servicesNotRunning.Count > 0)
            {
                string failureDescription = "ClusterHealthServiceLog: Service " + string.Join(", ", servicesNotRunning.ToList()) + " not running";
                cluster.AddVerboseFailureDetails(failureDescription);
                cluster.RecordFailure("HDP Service failure", failureDescription);
            }               
        }

        public void CheckLogEntryLogs(ClusterMetadata cluster, DataTable logEntryTable)
        {
            if ((logEntryTable == null) || (logEntryTable.Rows.Count == 0))
            {
                // no data
                return;
            }
            // Write content rows
            DateTime lastTimeStamp = DateTime.MinValue;
            DateTime firstRPConnectErrorTimeStamp = DateTime.MaxValue;
            DateTime firstAmbari503ErrorTimeStamp = DateTime.MaxValue;
            DateTime lastRPConnectErrorTimeStamp = DateTime.MinValue;
            DateTime lastAmbari503ErrorTimeStamp = DateTime.MinValue;
            int gatewayInReadyState = 0;
            bool allHeadNodesInReady = false;
            int zookeeperInReadyState = 0;
            int headnodesInReadyState = 0;
            int nodesInReadyState = 0;
            string lastDetails = "";
            bool vmsProvisioned = false;
            foreach (DataRow row in logEntryTable.Rows)
            {
                // Get PreciseTimeStamp, Class and Details
                string dtStr = row["PreciseTimeStamp"].ToString();
                DateTime preciseTimeStamp = DateTime.Parse(dtStr);
                string logClass = row["Class"].ToString();
                string details = row["Details"].ToString();
                string traceLevel = row["TraceLevel"].ToString();

                // Record errors in LogEntry
                if(traceLevel.Equals("Error") || traceLevel.Equals("Failure") || traceLevel.Equals("Fatal"))
                {
                    cluster.AddVerboseFailureDetails("Errors from LogEntry : " + details);
                }

                // Rows have already been sorted, validate same here
                Debug.Assert(preciseTimeStamp >= lastTimeStamp, "Rows in LogEntry Table not sorted");
                lastTimeStamp = preciseTimeStamp;
                if (logClass != "PollCreatingAzureHostedServiceStateActivity")
                {
                    continue;
                }
                if (details.StartsWith("Status. Overall Deployment:"))
                {
                    vmsProvisioned = true;
                    // Count number of VMs in running state
                    int curNodesInReady = details.Split(new string[] { "ReadyRole" }, StringSplitOptions.RemoveEmptyEntries).Count() - 1;
                    nodesInReadyState = Math.Max(nodesInReadyState, curNodesInReady);
                    lastDetails = details;

                    headnodesInReadyState = Math.Max(headnodesInReadyState,
                        (details.Contains("HeadNode_IN_0, ReadyRole") ? 1 : 0)
                        + (details.Contains("HeadNode_IN_1, ReadyRole") ? 1 : 0));

                    gatewayInReadyState = Math.Max(gatewayInReadyState,
                        (details.Contains("Gateway_IN_0, ReadyRole") ? 1 : 0)
                        + (details.Contains("Gateway_IN_1, ReadyRole") ? 1 : 0));

                    zookeeperInReadyState = Math.Max(zookeeperInReadyState,
                        (details.Contains("IsotopeZookeeperNode_IN_0, ReadyRole") ? 1 : 0)
                        + (details.Contains("IsotopeZookeeperNode_IN_1, ReadyRole") ? 1 : 0)
                        + (details.Contains("IsotopeZookeeperNode_IN_2, ReadyRole") ? 1 : 0));
                }
                else if (details.StartsWith("Poller for create workflow checking to see if all head nodes are in ReadyRole state"))
                {
                    allHeadNodesInReady |= details.EndsWith("Result=True.");
                }
                else if (details.StartsWith("Error trying to communicate with Cluster over Ambari. Error: Unable to connect to the remote server"))
                {
                    if (preciseTimeStamp < firstRPConnectErrorTimeStamp)
                    {
                        firstRPConnectErrorTimeStamp = preciseTimeStamp;
                    }
                    lastRPConnectErrorTimeStamp = preciseTimeStamp;
                }
                else if (details.StartsWith("Error trying to communicate with Cluster over Ambari. Error: The remote server returned an error: (503)"))
                {
                    if (preciseTimeStamp < firstAmbari503ErrorTimeStamp)
                    {
                        firstAmbari503ErrorTimeStamp = preciseTimeStamp;
                    }
                    lastAmbari503ErrorTimeStamp = preciseTimeStamp;
                }
            }
            if (vmsProvisioned == false)
            {
                cluster.AddVerboseFailureDetails("Failure happened before VMs were provisioned. Check LogEntry log table for more details");
                cluster.RecordFailure("RP Failure", "Failure happened before VMs were provisioned. Check LogEntry log table for more details");
                return;
            }
            int totalNodes = lastDetails.Split(new string[] { "_IN_" }, StringSplitOptions.RemoveEmptyEntries).Count() - 1;
            // Clusters except 1.6 goes out of HDInsightConfiguration state, into Operational state when
            // Atleast one headnode is in running state
            // Atleast one gateway node is in running state
            // At least 1 zookeeper nodes is in running state
            // 90% of worker nodes are in ready state
            // For 1.6 cluster, we have only 1 head node and no zookeeper nodes

            if (cluster.Version.StartsWith("1.6"))
            {
                int totalWorkerNodes = totalNodes - 1 /* headnode */ - 2 /* gateway */;
                int workerNodesInReadyState = nodesInReadyState - headnodesInReadyState - gatewayInReadyState;
                if (gatewayInReadyState == 0 || headnodesInReadyState == 0 || (((double)workerNodesInReadyState / totalWorkerNodes) < 0.9))
                {
                    string failureDescription = "LogEntryTable: CRUD failed because nodes didn't reach ready state. ";
                    failureDescription += string.Format("Ready state counts : Gateway : {0}, Headnode : {1}, Worker : {2} / {3}", gatewayInReadyState, headnodesInReadyState, workerNodesInReadyState, totalWorkerNodes);
                    cluster.AddVerboseFailureDetails(failureDescription);
                    cluster.RecordFailure("Nodes stuck", failureDescription);
                }

            }
            else
            {
                int totalWorkerNodes = totalNodes - 2 /* headnode */ - 2 /* gateway */ - 3 /* zookeeper */;
                int workerNodesInReadyState = nodesInReadyState - headnodesInReadyState - gatewayInReadyState - zookeeperInReadyState;
                //if (gatewayInReadyState == 0 || zookeeperInReadyState == 0 || headnodesInReadyState == 0 || (((double)workerNodesInReadyState / totalWorkerNodes) < 0.9))
                if (gatewayInReadyState == 0 || zookeeperInReadyState == 0 || headnodesInReadyState == 0 || workerNodesInReadyState == 0 || (double)workerNodesInReadyState < Math.Floor(0.9 * totalWorkerNodes))
                {
                    string failureDescription = "LogEntryTable: CRUD failed because nodes didn't reach ready state. ";
                    failureDescription += string.Format("Ready state counts : Gateway : {0}, Zookeeper : {1}, Headnode : {2}, Worker : {3} / {4}", gatewayInReadyState, zookeeperInReadyState, headnodesInReadyState, workerNodesInReadyState, totalWorkerNodes);
                    cluster.AddVerboseFailureDetails(failureDescription);
                    cluster.RecordFailure("Nodes stuck", failureDescription);
                    string customFailureDescription = "";
                    if(gatewayInReadyState == 0)
                    {
                        customFailureDescription += "Gateway = 0 ";
                    }
                    if(zookeeperInReadyState == 0)
                    {
                        customFailureDescription += "Zookeeper = 0 ";
                    }
                    if(headnodesInReadyState == 0)
                    {
                        customFailureDescription += "Headnode = 0 ";
                    }
                    if(headnodesInReadyState != 0 && zookeeperInReadyState != 0 && gatewayInReadyState != 0)
                    {
                        customFailureDescription += "Workernode = " + workerNodesInReadyState + " / " + totalWorkerNodes;
                    }
                    if (totalWorkerNodes > 1)
                    {
                        //private const string ReportHeader = "Create Time,Region,Cluster DNS Name,Azure Deployement ID,Nodes stuck,Details";
        
                        cluster.AddCustomFailureDetails(string.Format("{0},{1},{2},{3},{4},{5}",
                            cluster.StartTime, cluster.Region, cluster.DnsName, cluster.AzureDeploymentId, customFailureDescription, lastDetails.Replace(',', '-').Replace('\n', '-').Replace('\r', '-')));
                    }
                }
            }
            if (firstRPConnectErrorTimeStamp != DateTime.MaxValue)
            {
                if (lastRPConnectErrorTimeStamp.Subtract(firstRPConnectErrorTimeStamp).TotalMinutes > 10)
                {
                    // RP not able to connect to gateway
                    string failureDescription = "LogEntryTable: CRUD failed because RP not able to connect to gateway from " + firstRPConnectErrorTimeStamp + " to " + lastRPConnectErrorTimeStamp;
                    cluster.AddVerboseFailureDetails(failureDescription);
                    cluster.RecordFailure("RP not able to connect to gateway", failureDescription);
                }
            }
            if(firstAmbari503ErrorTimeStamp != DateTime.MaxValue)
            {
                if(lastAmbari503ErrorTimeStamp.Subtract(firstAmbari503ErrorTimeStamp).TotalMinutes > 10)
                {
                    // RP not able to connect to Ambari and getting 503 error
                    string failureDescription = "LogEntryTable: CRUD failed because RP not able to connect to cluster over Ambari from " + firstAmbari503ErrorTimeStamp + " to " + lastAmbari503ErrorTimeStamp;
                    cluster.AddVerboseFailureDetails(failureDescription);
                    cluster.RecordFailure("RP not able to connect to Cluster(503)", failureDescription);

                }
            }
            if (allHeadNodesInReady == false)
            {
                cluster.AddVerboseFailureDetails("Not all head nodes reached ready state");
            }
        }
        public void CheckHadoopInstallLogs(ClusterMetadata cluster, DataTable hadoopInstallLogs)
        {
            if ((hadoopInstallLogs == null) || (hadoopInstallLogs.Rows.Count == 0))
            {
                // no data
                return;
            }
            bool fHiveMetastoreCreateStarted = false, fHiveMetastoreCreated = false;
            bool fOozieMetastoreCreateStarted = false, fOozieMetastoreCreated = false;
            bool fSharedLibsCopyStarted = false;
            bool fSharedLibsCopyed = false;
            foreach (DataRow row in hadoopInstallLogs.Rows)
            {
                // Get RoleInstance and Details
                string role = row["Role"].ToString();
                string details = row["Message"].ToString();

                if (role == "IsotopeHeadNode")
                {
                    if (details.Contains("BEGIN: CreateHiveMetastoreSchema"))
                    {
                        fHiveMetastoreCreateStarted = true;
                    }
                    else if (details.Contains("END: CreateHiveMetastoreSchema"))
                    {
                        fHiveMetastoreCreated = true;
                    }
                    else if (details.Contains("BEGIN: CreateOozieMetastoreSchema"))
                    {
                        fOozieMetastoreCreateStarted = true;
                    }
                    else if (details.Contains("END: CreateOozieMetastoreSchema"))
                    {
                        fOozieMetastoreCreated = true;
                    }
                    else if(details.Contains("Begin: Checking existence of Blob CopyShareLibs"))
                    {
                        fSharedLibsCopyStarted = true;
                    }
                    else if (details.Contains("End: Set Status of operation: CopyShareLibs as Completed"))
                    {
                        fSharedLibsCopyed = true;
                    }
                }
            }
            if (fHiveMetastoreCreateStarted && !fHiveMetastoreCreated)
            {
                cluster.AddVerboseFailureDetails("HadoopInstallLog: Error Hive metastore creation failed");
                cluster.RecordFailure("Metastore creation failed", "HadoopInstallLog: Error Hive metastore creation failed");
            }
            if (fOozieMetastoreCreateStarted && !fOozieMetastoreCreated)
            {
                cluster.AddVerboseFailureDetails("HadoopInstallLog: Error Oozie metastore creation failed");
                cluster.RecordFailure("Metastore creation failed", "HadoopInstallLog: Error Oozie metastore creation failed");
            }
            if (!fHiveMetastoreCreateStarted)
            {
                cluster.AddVerboseFailureDetails("HadoopInstallLog: Hive metastore creation not started");
            }
            if (!fOozieMetastoreCreateStarted)
            {
                cluster.AddVerboseFailureDetails("HadoopInstallLog: Oozie metastore creation not started");
            }
            if (fSharedLibsCopyStarted && !fSharedLibsCopyed)
            {
                cluster.AddVerboseFailureDetails("HadoopInstallLog: Failure while copying shared libs to ASV");
                cluster.RecordFailure("Failure to copy jars to ASV", "HadoopInstallLog: Error while copying shared libs to ASV");
            }
        }
        private void CheckSetupLogs(ClusterMetadata cluster, DataTable setupLogs)
        {
            if ((setupLogs == null) || (setupLogs.Rows.Count == 0))
            {
                // no data
                return;
            }
            var allNodes = new Dictionary<string, DateTime>();
            var successfullNodes = new Dictionary<string, DateTime>();
            DateTime minTimeStamp = DateTime.MaxValue;
            DateTime maxTimeStamp = DateTime.MinValue;
            string lastNodeInstance = string.Empty;
            foreach (DataRow row in setupLogs.Rows)
            {
                // Get RoleInstance and Details
                string roleInstance = row["RoleInstance"].ToString();
                string traceLevel = row["TraceLevel"].ToString();
                string details = row["Details"].ToString();
                string exceptionType = row["ExceptionType"].ToString();
                string innerExceptionType = row["InnerExceptionType"].ToString();
                string innerExceptionMessage = row["InnerExceptionMessage"].ToString();
                string exception = row["Exception"].ToString();

                string dtStr = row["PreciseTimeStamp"].ToString();
                DateTime preciseTimeStamp = DateTime.Parse(dtStr);
                if (traceLevel == "Error")
                {
                    cluster.AddVerboseFailureDetails("Errors from SetupLog: RoleInstance " + roleInstance + "-" + details);
                    if (exceptionType == "Microsoft.WindowsAzure.Storage.StorageException" && innerExceptionMessage.Contains("The remote name could not be resolved"))
                    {
                        cluster.AddVerboseFailureDetails("Remote server could not be resolved. Details : " + details + " - exceptionType : " + exceptionType + " innerExceptionType : " + innerExceptionType + " InnerExceptionMessage : " + innerExceptionMessage + " Exception : " + exception);
                        cluster.RecordFailure("Node setup failure", "Remote server could not be resolved. For more details see output file generated along with log files");
                    }
                    else if (exceptionType == "System.AggregateException" && innerExceptionType == "System.IO.IOException" && innerExceptionMessage == "Unable to read data from the transport connection: The connection was closed.")
                    {
                        cluster.AddVerboseFailureDetails("I/O exception while reading from azure. Details : " + details + " - exceptionType : " + exceptionType + " innerExceptionType : " + innerExceptionType + " InnerExceptionMessage : " + innerExceptionMessage + " Exception : " + exception);
                        cluster.RecordFailure("Node setup failure", "I/O exception while reading from azure");
                    }
                    else if(exceptionType == "System.AggregateException" && exception.Contains("The given key was not present in the dictionary"))
                    {
                        cluster.AddVerboseFailureDetails("OneTime operation failed to get blob metadata after multiple retries");
                        cluster.RecordFailure("Node setup failure", "One time operation blob metadata not found");
                    }
                    else if (exception.Contains("GetDistroManifest"))
                    {
                        cluster.AddVerboseFailureDetails("Downloading Distro manifest failed");
                        cluster.RecordFailure("Node setup failure", "Downloading distro manifest failede");
                    }
                }
                else
                {
                    if (allNodes.ContainsKey(roleInstance) == false)
                    {
                        allNodes[roleInstance] = preciseTimeStamp;
                        if (minTimeStamp > preciseTimeStamp)
                        {
                            minTimeStamp = preciseTimeStamp;
                        }
                        if (maxTimeStamp < preciseTimeStamp)
                        {
                            maxTimeStamp = preciseTimeStamp;
                            lastNodeInstance = roleInstance;
                        }
                    }
                    if (details.Contains("Successfully applied manifest. Version") && successfullNodes.ContainsKey(roleInstance) == false)
                    {
                        successfullNodes[roleInstance] = preciseTimeStamp;
                    }
                }
            }
            if (successfullNodes.Count() != allNodes.Count())
            {
                var failedNodes = allNodes.Keys.ToList().Except(successfullNodes.Keys.ToList());
                TimeSpan deviationBetweenNodeSetupStart = maxTimeStamp - minTimeStamp;
                string errorMsg = "Node setup didn't successfully finish on following nodes : " + string.Join(", ", failedNodes.ToArray()) + ". ";
                if(deviationBetweenNodeSetupStart.TotalMinutes > 20)
                {
                    errorMsg += "Node setup on " + lastNodeInstance + " node started " + deviationBetweenNodeSetupStart.TotalMinutes + " minutes late compared to first node";
                }

                cluster.RecordFailure("Node setup failure", errorMsg);
            }
        }
    }
}
