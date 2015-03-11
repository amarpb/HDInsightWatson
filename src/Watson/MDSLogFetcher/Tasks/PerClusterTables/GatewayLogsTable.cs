using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using MDSLogAnalyzerCommon;

namespace ClusterLogAnalyzer.Tasks
{
    class GatewayLogsTableDownloadTask : PerClusterTableDownloadTask
    {
        public override string GetTableName()
        {
            return FormattedTables.GatewayLogs;
        }
        public override string GetMDSTableName()
        {
            return "HdInsightClusterCentralGatewayLogs_" + RegionHelper.GetRegionTableId(Region) + "_.*";
        }
        public override string LogFileName()
        {
            return "gatewayLogs.xlsx";
        }
    }
}
