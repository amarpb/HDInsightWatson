using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using MDSLogAnalyzerCommon;

namespace ClusterLogAnalyzer.Tasks
{
    class FilteredHadoopServiceLogTableDownloadTask : PerClusterTableDownloadTask
    {
        public override string GetTableName()
        {
            return FormattedTables.FilteredHadoopServiceLogs;
        }
        public override string GetMDSTableName()
        {
            return "HdInsightClusterCentralFilteredHadoopServiceLogs_" + RegionHelper.GetRegionTableId(Region) + "_.*";
        }
        public override string LogFileName()
        {
            return "filteredHadoopServiceLogs.xlsx";
        }
    }
}
