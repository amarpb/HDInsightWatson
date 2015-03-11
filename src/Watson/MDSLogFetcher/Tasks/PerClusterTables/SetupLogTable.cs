using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using MDSLogAnalyzerCommon;

namespace ClusterLogAnalyzer.Tasks
{
    class SetupLogTableDownloadTask : PerClusterTableDownloadTask
    {
        public override string GetTableName()
        {
            return FormattedTables.SetupLog;
        }
        public override string GetMDSTableName()
        {
            return "HdInsightClusterCentralSetupLog_" + RegionHelper.GetRegionTableId(Region) + "_.*";
        }
        public override string LogFileName()
        {
            return "setuplog.xlsx";
        }
    }
}
