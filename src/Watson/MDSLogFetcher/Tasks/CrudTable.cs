using System;
using System.Collections.Generic;
using System.Data;
using System.Linq;
using System.Text;
using MDSLogAnalyzerCommon;

namespace ClusterLogAnalyzer.Tasks
{
    public class CrudTable : TableDownloadTask
    {
        public override string GetTableName()
        {
            return FormattedTables.CRUD;
        }
        public override string GetMDSTableName()
        {
            return "HdInsightClusterCRUDEventVer5v0";
        }
        public override string LogFileName()
        {
            return "crud.xlsx";
        }
        public override void PruneLogs()
        {
            var clusterMetadata = Mapper.GetCluster(this);
            if (logOutput == null || logOutput.Rows.Count == 0)
            {
                return;
            }
            base.PruneLogs();
            
            int curRow = 0;
            DateTime createTime = clusterMetadata.StartTime;
            string deploymentState = "";
            for (curRow = 0; curRow < logOutput.Rows.Count; curRow++)
            {
                // Skip rows untill CreateTime from the log file is not approx. within few minutes of create datetime supplied on input
                var createDateStr = logOutput.Rows[curRow]["CreateTime"].ToString();
                createTime = DateTime.Parse(createDateStr);
                if ((clusterMetadata.StartTime - createTime).TotalMinutes > 5)
                {
                    continue;
                }

                // Update end time with every row in case we never see deleted, or aborted state in log
                clusterMetadata.EndTime = DateTime.Parse(logOutput.Rows[curRow]["PreciseTimeStamp"].ToString());

                if (!string.IsNullOrWhiteSpace(logOutput.Rows[curRow]["AzureDeploymentId"].ToString()))
                {
                    clusterMetadata.AzureDeploymentId = logOutput.Rows[curRow]["AzureDeploymentId"].ToString();
                }

                if (string.IsNullOrWhiteSpace(clusterMetadata.Version))
                {
                    clusterMetadata.Version = logOutput.Rows[curRow]["PortalDeploymentVersion"].ToString().Substring(0, 3);
                }
                if (string.IsNullOrWhiteSpace(clusterMetadata.Region))
                {
                    clusterMetadata.Region = logOutput.Rows[curRow]["DataCenterLocation"].ToString();
                }

                deploymentState = logOutput.Rows[curRow]["DeploymentState"].ToString();
                if (deploymentState == "Aborted" || deploymentState == "Deleted" || string.IsNullOrWhiteSpace(deploymentState))
                {
                    break;
                }
            }
            // if cluster hasn't been deleted, add 30 minutes to end time to give all nodes sufficient time to complete node setup
            if(deploymentState.Equals("Running", StringComparison.OrdinalIgnoreCase))
            {
                clusterMetadata.EndTime += TimeSpan.FromMinutes(30);
            }
            // Verify that we have everything that is required at this stage
            double clusterDurationInMinutes = (clusterMetadata.EndTime - clusterMetadata.StartTime).TotalMinutes;
            if(clusterDurationInMinutes < 0
                || string.IsNullOrWhiteSpace(clusterMetadata.Region)
                || string.IsNullOrWhiteSpace(clusterMetadata.Version))
            {
                string line = string.Format(Settings.GlobalFileLineFormat,
                    clusterMetadata.DnsName, clusterMetadata.StartTime, clusterMetadata.Region, clusterMetadata.Version,
                    clusterMetadata.ClusterFeatures, clusterMetadata.FailureCategory,
                    "Not able to find all required details in crud table. Check input : AzureDeploymentId = " + clusterMetadata.AzureDeploymentId
                    + " Cluster lifetime in minutes = " + clusterDurationInMinutes);
                // Not able to find all details in crud table
                GlobalReport.WriteToReport(line, clusterMetadata.ClusterDateIdentifier);
                return;
            }
            var tablesToDownload = new List<TableDownloadTask>();
            tablesToDownload.Add(new LogEntryTable());
            tablesToDownload.Add(new FeatureUsageTable());
            
            if(!string.IsNullOrWhiteSpace(clusterMetadata.AzureDeploymentId) && !clusterMetadata.Version.Equals("1.6"))
            {
                // Now that we have deploymentId, schdule downloading per cluster table tasks
                tablesToDownload.Add(new SetupLogTableDownloadTask());
                tablesToDownload.Add(new ApplicationEventsTableDownloadTask());
                tablesToDownload.Add(new ClusterHealthServiceLogTableDownloadTask());
                tablesToDownload.Add(new FilteredHadoopServiceLogTableDownloadTask());
                tablesToDownload.Add(new HadoopInstallLogTableDownloadTask());
                tablesToDownload.Add(new GatewayLogsTableDownloadTask());
                tablesToDownload.Add(new LogEntryPerClusterTableDownloadTask());
                tablesToDownload.Add(new AvailabilityEventTableDownloadTask());
            }

            // Create a cluster object and add it to Dictionary
            var scheduler = Scheduler.GetInstance();

            foreach (var table in tablesToDownload)
            {
                clusterMetadata.AddTableToCluster(table.GetTableName());
                Mapper.AddMapping(table, clusterMetadata);
                scheduler.ScheduleTask(table);
            }

            // Run Analyzers
            scheduler.ScheduleTask(new InvokeAnalyzersTask(clusterMetadata));

            // Output report (global)
            scheduler.ScheduleTask(new LogResultsTask(clusterMetadata));
        }

        public override bool ReadyToGo
        {
            // dnsName and StartTime are only dependencies for this table
            get { return true; }
        }
    }
}
