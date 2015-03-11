using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using MDSLogAnalyzerCommon;

namespace ClusterLogAnalyzer.Tasks
{
    class ApplicationEventsTableDownloadTask : PerClusterTableDownloadTask
    {
        public override string GetTableName()
        {
            return FormattedTables.ApplicationEventLogs;
        }
        public override string GetMDSTableName()
        {
            return "HdInsightClusterCentralApplicationEvents_" + RegionHelper.GetRegionTableId(Region) + "_.*";
        }
        public override string LogFileName()
        {
            return "applicationEvents.xlsx";
        }
    }
}
