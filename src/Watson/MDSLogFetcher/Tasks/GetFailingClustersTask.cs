using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using ClusterLogAnalyzer.Tasks;
using System.Data;
using System.IO;
using MDSLogAnalyzerCommon;

namespace ClusterLogAnalyzer.Tasks
{
    //======================================================================
    //This class implements the workflow class
    //<Fields>
    //The following are the Fields of the Class:
    //a) failQuery : We define this query to pick up the Failed cluster creation for which the Deployment State is Error.
    //b) timedOutQuery: We define this query to pick up failed cluster creation for which the cluster creation timed out.
    //c) abortQuery: We define the query to pick up failed cluster creation for which the cluster creation Aborted.
    //d) startTime : The startTime for the request to create the cluster.
    //e) endTime: The End Time for the request .
    //f) EndPoint : Defined in the ClusterMetadata.cs file in the MDSLogAnalyzerCommon dll. 
    //   Has three values: a) AzureGlobal
    //                     b) Mooncake
    //                     c) Test
    // </Fields>     
    //<Properties>
    //ReadyToGo - Read Only Property - derived from WorkflowTask abstract class.
    //Name - Read Only Property - returns "Get Failures"
    //</Properties>
    //<Methods>
    //RunWorkflow
    //RunWorkflow()- executes the following three methods - GetFailingClustersFromMDS(),GetAbortedClustersFromMDS(),GetTimedoutClustersFromMDS();
    // the three methods get the list of each of the threee categories of cluster failures and then pass them along to ScheduleFailedClusterAnalysis()
    // The ScheduleFailedClusterAnalysis Looks at the the Hashset ClustersAnalyzed and if the cluster name is not there in the Hashset initalizes 
    // an instance of WorkflowEngine and adds an entry in the hashset.
    //</Methods>
    //=======================================================================
    public class GetFailingClustersTask : WorkflowTask
    {
        DateTime startTime, endTime;
        string failQuery = "DeploymentState = \"Error\"";
        string timedOutQuery = "DeploymentState = \"Timedout\"";
        string abortQuery = "DeploymentState = \"Aborted\" and (PreciseTimeStamp - CreateTime) > Timespan(0, 44, 0)";
        static HashSet<string> clustersAnalyzed = new HashSet<string>();
        EndPoint endPoint;
        public GetFailingClustersTask(DateTime startTime, DateTime endTime, EndPoint endPoint)
        {
            this.startTime = startTime;
            this.endTime = endTime;
            this.endPoint = endPoint;
        }
        public override void RunWorkflow()
        {
            // Get list of all clusters failing within timerange from MDS
            GetFailingClustersFromMDS();
            GetAbortedClustersFromMDS();
            GetTimedoutClustersFromMDS();
        }
        private void GetTimedoutClustersFromMDS()
        {
            ILogFetcher log = new MDSLogFetcher("HdInsightClusterCRUDEventVer5v0", startTime, endTime, timedOutQuery, false, endPoint);
            DataTable table = log.Fetch();

            ScheduleFailedClusterAnalysis(table);
            ExcelHelper.ExportToExcel(table, Path.Combine(Settings.TempDirectory, "timedout.xlsx"));

        }
        private void GetAbortedClustersFromMDS()
        {
            ILogFetcher log = new MDSLogFetcher("HdInsightClusterCRUDEventVer5v0", startTime, endTime, abortQuery, false, endPoint);
            DataTable table = log.Fetch();

            ScheduleFailedClusterAnalysis(table);
            ExcelHelper.ExportToExcel(table, Path.Combine(Settings.TempDirectory, "aborts.xlsx"));

        }
        private void GetFailingClustersFromMDS()
        {
            ILogFetcher log = new MDSLogFetcher("HdInsightClusterCRUDEventVer5v0", startTime, endTime, failQuery, false, endPoint);
            DataTable table = log.Fetch();

            ScheduleFailedClusterAnalysis(table);
            ExcelHelper.ExportToExcel(table, Path.Combine(Settings.TempDirectory, "failures.xlsx"));

        }
        void ScheduleFailedClusterAnalysis(DataTable table)
        {
            if(table == null || table.Rows.Count == 0)
            {
                return;
            }
            for (int curRow = 0; curRow < table.Rows.Count; curRow++)
            {
                var createDateStr = table.Rows[curRow]["CreateTime"].ToString();
                DateTime createTime = DateTime.Parse(createDateStr);

                var endDateStr = table.Rows[curRow]["PreciseTimeStamp"].ToString();
                DateTime endTime = DateTime.Parse(endDateStr);

                var clusterDnsName = table.Rows[curRow]["ClusterDnsName"].ToString();
                if (!clustersAnalyzed.Contains(clusterDnsName + createTime.ToShortTimeString()))
                {
                    clustersAnalyzed.Add(clusterDnsName + createTime.ToShortTimeString());
                    WorkflowEngine.GetInstance().AnalyzeLogsForACluster(clusterDnsName, createTime, endTime, endPoint);
                }
            }
        }

        public override bool ReadyToGo
        {
            get { return true; }
        }
        public override string Name
        {
            get { return "Get Failures"; }
        }
    }
}

