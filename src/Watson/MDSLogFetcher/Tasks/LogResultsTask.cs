using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using MDSLogAnalyzerCommon;

namespace ClusterLogAnalyzer.Tasks
{
    public class LogResultsTask : WorkflowTask
    {
        ClusterMetadata cluster = null;
        public LogResultsTask(ClusterMetadata cluster)
        {
            this.cluster = cluster;
        }
        public override void RunWorkflow()
        {
            // Log results
            // format : dnsname startTime   Region  Version FailureCategory Description

            string line = string.Format(Settings.GlobalFileLineFormat,
                cluster.DnsName,
                cluster.StartTime,
                cluster.Region,
                cluster.Version,
                cluster.ClusterFeatures,
                cluster.FailureCategory,
                cluster.FailureDescription.Replace(',', '-').Replace('\n', '-').Replace('\r', '-'));
            GlobalReport.WriteToReport(line, cluster.ClusterDateIdentifier);
            CustomReport.WriteToReport(cluster.GetCustomFailureDetails());

            OutputReport.DeleteReportIfExist(cluster.ClusterIdentifier);
            OutputReport.WriteToReport("Cluster DNS         : " + cluster.DnsName, cluster.ClusterIdentifier);
            OutputReport.WriteToReport("Cluster Create time : " + cluster.StartTime, cluster.ClusterIdentifier);
            OutputReport.WriteToReport("Region              : " + cluster.Region, cluster.ClusterIdentifier);
            OutputReport.WriteToReport("Cluster Version     : " + cluster.Version, cluster.ClusterIdentifier);
            OutputReport.WriteToReport("Cluster Features    : " + cluster.ClusterFeatures, cluster.ClusterIdentifier);
            OutputReport.WriteToReport("Failure Category    : " + cluster.FailureCategory, cluster.ClusterIdentifier);
            OutputReport.WriteToReport("Failure Description : " + cluster.FailureDescription, cluster.ClusterIdentifier);
            OutputReport.WriteToReport("----------------------", cluster.ClusterIdentifier);
            OutputReport.WriteToReport(Environment.NewLine + cluster.GetVerboseFailureDetails(), cluster.ClusterIdentifier);

            string reportPath = OutputReport.GetReportFileNetworkPath(cluster.ClusterIdentifier);
            string mailMessageBody = "Here are the auto analysis details for the cluster " + Environment.NewLine + line;
            mailMessageBody += Environment.NewLine + "MDS Logs available at " + System.IO.Path.GetDirectoryName(reportPath);
            //EmailUtils.MailReport("Cluster failure auto analysis",
            //    mailMessageBody,
            //    "vipula@microsoft.com",
            //    "HDInsight Watson Cluster Failure Analysis Tool",
            //    "",
            //    reportPath);

        }
        public override bool ReadyToGo
        {
            get { return cluster.ClusterAnalysisDone; }
        }
        public override string Name
        {
            get { return "Log Result"; }
        }
    }
}
