using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using MDSLogAnalyzerCommon;

namespace ClusterLogAnalyzer.Tasks
{
    class LogEntryPerClusterTableDownloadTask : PerClusterTableDownloadTask
    {
        public override string GetTableName()
        {
            return FormattedTables.LogEntryPerCluster;
        }
        public override string GetMDSTableName()
        {
            return "HdInsightClusterCentralLogEntry_" + RegionHelper.GetRegionTableId(Region) + "_.*";
        }
        public override string LogFileName()
        {
            return "LogEntryPerCluster.xlsx";
        }
    }
}
