using System;
using System.Collections;
using System.Collections.Generic;
using System.Diagnostics;
using System.Linq;
using System.Text;
using MDSLogAnalyzerCommon;

namespace ClusterLogAnalyzer.Tasks
{
    public class LogEntryTable : TableDownloadTask
    {
        public override string GetTableName()
        {
            return FormattedTables.LogEntry;
        }
        public override string GetMDSTableName()
        {
            return "HdInsightLogEntryVer5v0";
        }
        public override string LogFileName()
        {
            return "LogEntry.xlsx";
        }

        public override bool ReadyToGo
        {
            // Version information is required by LogEntry
            get
            {
                var clusterMetadata = Mapper.GetCluster(this);
                if (string.IsNullOrEmpty(clusterMetadata.Version))
                {
                    return false;
                }
                return true;
            }
        }
    }
}
