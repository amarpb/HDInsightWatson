using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using MDSLogAnalyzerCommon;

namespace ClusterLogAnalyzer.Tasks
{
    class HadoopInstallLogTableDownloadTask : PerClusterTableDownloadTask
    {
        public override string GetTableName()
        {
            return FormattedTables.HadoopInstallLogs;
        }
        public override string GetMDSTableName()
        {
            return "HdInsightClusterCentralHadoopInstallLogs_" + RegionHelper.GetRegionTableId(Region) + "_.*";
        }
        public override string LogFileName()
        {
            return "hadoopInstallLog.xlsx";
        }
        
    }
}       