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

