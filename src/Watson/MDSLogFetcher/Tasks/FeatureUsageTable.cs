using System;
using System.Collections;
using System.Collections.Generic;
using System.Data;
using System.Diagnostics;
using System.Linq;
using System.Text;
using MDSLogAnalyzerCommon;

namespace ClusterLogAnalyzer.Tasks
{
    public class FeatureUsageTable : TableDownloadTask
    {
        public override string GetTableName()
        {
            return FormattedTables.FeatureUsage;
        }
        public override string GetMDSTableName()
        {
            return "HdInsightFeatureUsageLogEventVer5v0";
        }
        public override string LogFileName()
        {
            return "FeatureUsage.xlsx";
        }
        public override void PruneLogs()
        {
            if (logOutput == null || logOutput.Rows.Count == 0)
            {
                return;
            }
            base.PruneLogs();
            var clusterMetadata = Mapper.GetCluster(this);

            foreach (DataRow row in logOutput.Rows)
            {
                // Get features
                string featureName = row["FeatureName"].ToString();
                string subFeatureName = row["SubFeatureName"].ToString();
                string featureProperty1 = row["FeatureProperty1"].ToString();
                string featureProperty2 = row["FeatureProperty2"].ToString();
                string featureProperty3 = row["FeatureProperty3"].ToString();
                if (featureName.Equals("CustomActionFeatureCategory", StringComparison.OrdinalIgnoreCase))
                {
                    clusterMetadata.AddClusterFeatures("Customized Cluster");
                }
                if (featureName.Equals("metastores", StringComparison.OrdinalIgnoreCase))
                {
                    clusterMetadata.AddClusterFeatures("Customized Metastore");
                }
                if (featureName.Equals("Bootstrap Actions", StringComparison.OrdinalIgnoreCase))
                {
                    clusterMetadata.AddClusterFeatures("Bootstrap Actions");
                }
                if (featureName.Equals("CustomHadoopConfiguration", StringComparison.OrdinalIgnoreCase))
                {
                    clusterMetadata.AddClusterFeatures("CustomHadoopConfiguration");
                }
            }
        }

        public override bool ReadyToGo
        {
            // dnsName and StartTime are only dependencies for this table
            get { return true; }
        }
    }
}
