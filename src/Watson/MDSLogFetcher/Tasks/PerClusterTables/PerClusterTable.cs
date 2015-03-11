using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;

namespace ClusterLogAnalyzer.Tasks
{
    public abstract class PerClusterTableDownloadTask : TableDownloadTask
    {
        string azureDeploymentId;
        public override bool GetUseIndex()
        {
            return false;
        }
        public override string GetQuery()
        {
            return "tenant = \"" + azureDeploymentId + "\"";
        }

        public string Region
        {
            get;
            set;
        }
        public override bool ReadyToGo
        {
            // dnsName and StartTime are only dependencies for this table
            get
            {
                var clusterMetadata = Mapper.GetCluster(this);
                azureDeploymentId = clusterMetadata.AzureDeploymentId;
                Region = clusterMetadata.Region;
                if (string.IsNullOrEmpty(azureDeploymentId))
                {
                    return false;
                }
                return true;
            }
        }
    }
}
