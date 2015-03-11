using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using MDSLogAnalyzerCommon;

namespace ClusterLogAnalyzer.Tasks
{
    class ClusterHealthServiceLogTableDownloadTask : PerClusterTableDownloadTask
    {
        public override string GetTableName()
        {
            return FormattedTables.ClusterHealthServiceLogs;
        }
        public override string GetMDSTableName()
        {
            return "HdInsightClusterCentralClusterHealthServiceLogEvent_" + RegionHelper.GetRegionTableId(Region) + "_.*";
        }
        public override string LogFileName()
        {
            return "clusterHealthServiceLogEvent.xlsx";
        }
    }
}
