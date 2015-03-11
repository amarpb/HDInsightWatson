using System;
using System.Collections.Generic;
using System.Data;
using System.IO;
using System.Linq;
using System.Text;

namespace MDSLogAnalyzerCommon
{

    [Flags]
    public enum EndPoint
    {
        AzureGlobal,
        Mooncake,
        Test,
    };
    public class FormattedTables
    {
        public const string CRUD = "CRUD";
        public const string LogEntry = "LogEntry";
        public const string SetupLog = "SetupLog";
        public const string HadoopInstallLogs = "HadoopInstallLogs";
        public const string ApplicationEventLogs = "ApplicationEvents";
        public const string ClusterHealthServiceLogs = "ClusterHealthServiceLogs";
        public const string FilteredHadoopServiceLogs = "FilteredHadoopServiceLogs";
        public const string GatewayLogs = "GatewayLogs";
        public const string LogEntryPerCluster = "LogEntryPerCluster";
        public const string AvailabilityEventLogs = "AvailabilityEventLogs";
        public const string FeatureUsage = "FeatureUsage";
    }
    public class ClusterMetadata
    {
        private HashSet<string> failureCategory;
        private string failureText = "";
        private string verboseFailureDetails = "";
        private string customFailureDetails = "";
        private static object syncObj = new object();
        public Dictionary<string, DataTable> FormattedTables;
        private Dictionary<string, string> tableDownloadProgress;
        private HashSet<string> featureUsages;


        public void AddCustomFailureDetails(string details)
        {
            customFailureDetails += details;
        }
        public string GetCustomFailureDetails()
        {
            return customFailureDetails;
        }
        public void AddVerboseFailureDetails(string details)
        {
            verboseFailureDetails += details + Environment.NewLine + Environment.NewLine;
        }
        public string GetVerboseFailureDetails()
        {
            return verboseFailureDetails;
        }
        public string Version
        {
            get;
            set;
        }
        public string ClusterFeatures
        {
            get {
                var usages = featureUsages.ToArray();
                return string.Join(";", usages); }
        }
        public void AddClusterFeatures(string feature)
        {
            featureUsages.Add(feature);
        }

        private void AddFailureCategory(string category)
        {
            failureCategory.Add(category);
        }

        private void AddFailureDescription(string description)
        {
            failureText += description + ";";
        }
        public string FailureCategory
        {
            private set { }
            get
            {
                return string.Join(";", failureCategory.ToArray());

            }
        }
        public string FailureDescription
        {
            private set { }
            get
            {
                return failureText;
            }

        }
        public ClusterMetadata(string dnsName, DateTime startTime, EndPoint endPoint)
        {
            DnsName = dnsName;
            StartTime = startTime;
            EndTime = StartTime.AddHours(24);
            ClusterEndPoint = endPoint;
            AzureDeploymentId = "";
            FailureRecorded = false;
            FormattedTables = new Dictionary<string, DataTable>();
            tableDownloadProgress = new Dictionary<string, string>();
            failureCategory = new HashSet<string>();
            featureUsages = new HashSet<string>();
        }
        public void AddTableToCluster(string tableName)
        {
            tableDownloadProgress.Add(tableName, "Starting");
        }
        public string DnsName
        {
            get;
            set;
        }
        public EndPoint ClusterEndPoint
        {
            get;
            set;
        }
        public DateTime StartTime
        {
            get;
            set;
        }
        public DateTime EndTime
        {
            get;
            set;
        }
        public string Region
        {
            get;
            set;
        }
        public string AzureDeploymentId
        {
            get;
            set;
        }
        public bool FailureRecorded
        {
            get;
            set;
        }
        public void RecordFailure(string failureCategory, string failureDescription)
        {
            AddFailureCategory(failureCategory);
            AddFailureDescription(failureDescription);
            FailureRecorded = true;
        }
        public void RecordLogDownloaded(string tableName)
        {
            tableDownloadProgress[tableName] = "Finished";
        }
        public bool LogDownloadInProgress(string tableName)
        {
            if (tableDownloadProgress.ContainsKey(tableName) && !tableDownloadProgress[tableName].Equals("Finished"))
            {
                return true;
            }
            return false;
        }
        public bool ClusterAnalysisDone
        {
            get;
            set;
        }
        public string ClusterIdentifier
        {
            private set { }
            get
            {
                return string.Format("{0}\\H{1}-M{2}-{3}", ClusterDateIdentifier, StartTime.Hour, StartTime.Minute, DnsName);
            }
        }

        public string ClusterDateIdentifier
        {
            private set{ }
            get
            {
                return string.Format("{0}-{1}-{2}", StartTime.Year, StartTime.Month, StartTime.Day);
            }
        }
    }
}
