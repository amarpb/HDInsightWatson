using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using MDSLogAnalyzerCommon;

namespace ClusterLogAnalyzer.Tasks
{
    public class InvokeAnalyzersTask : WorkflowTask
    {
        ClusterMetadata cluster = null;
        public InvokeAnalyzersTask(ClusterMetadata cluster)
        {
            this.cluster = cluster;
        }

        public override void RunWorkflow()
        {
            // Call each registered analyzer and invoke AnalyzeLogs method
            var analyzers = Mapper.GetAnalyzers();
            foreach(var analyzer in analyzers)
            {
                analyzer.AnalyzeLogs(cluster);
            }

            cluster.ClusterAnalysisDone = true;
        }

        public override bool ReadyToGo
        {
            get
            {
                return !cluster.LogDownloadInProgress(FormattedTables.LogEntry) &&
                    !cluster.LogDownloadInProgress(FormattedTables.SetupLog) &&
                    !cluster.LogDownloadInProgress(FormattedTables.HadoopInstallLogs) &&
                    !cluster.LogDownloadInProgress(FormattedTables.ClusterHealthServiceLogs) &&
                    !cluster.LogDownloadInProgress(FormattedTables.AvailabilityEventLogs);
            }
        }
        public override string Name
        {
            get { return "Invoke Analyzers"; }
        }
    }
}


