using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using MDSLogAnalyzerCommon;

namespace ClusterLogAnalyzer.Tasks
{
    class AvailabilityEventTableDownloadTask : PerClusterTableDownloadTask
    {
        public override string GetTableName()
        {
            return FormattedTables.AvailabilityEventLogs;
        }
        public override string GetMDSTableName()
        {
            return "HdInsightClusterCentralAvailabilityEvent_" + RegionHelper.GetRegionTableId(Region) + "_.*";
        }
        public override string LogFileName()
        {
            return "availabilityEvent.xlsx";
        }
    }
}
