using System;
using System.Collections.Generic;
using System.IO;
using System.Linq;
using System.Text;
using ClusterLogAnalyzer.Tasks;
using System.Data;
using MDSLogAnalyzerCommon;

namespace ClusterLogAnalyzer
{
    //======================================================================
    //This is the abstract class that is implemented by the following classes
    //a) FeatureUsageTable
    //b) LogEntryTable
    //c) PerClusterTableDownloadTask
    //d) CrudTable
    //<Methods>
    //  RunWorkflow() - this method calls the following 4 methods, namely:
    //                  FetchLogs();
    //                  FormatLogs();
    //                  SaveLogs();
    //                  PruneLogs();
    //                  Cleanup();
    //  GetClusterMetadata() - 
    //  FetchLogs() - this calls GetClusterMetadata() 
    //</Methods>
    //
    //
    //
    //
    //
    //
    //
    //
    //
    //
    //
    //
    //
    //
    //
    //
    //
    //=======================================================================

    public abstract class TableDownloadTask : WorkflowTask
    {
        DateTime startTime;
        DateTime endTime;
        string dnsName;
        string clusterIdentifier;
        EndPoint endPoint;
        protected DataTable logOutput = null;

        public TableDownloadTask()
        {
        }
        public override void RunWorkflow()
        {
            FetchLogs();
            FormatLogs();
            SaveLogs();
            PruneLogs();
            Cleanup();
        }

        public override string Name
        {
            get { return GetTableName(); }
        }
        private void FetchLogs()
        {
            GetClusterMetadata();
            ILogFetcher log;

            // Check if log file already exist from previous session. Download log from MDS if log file do not already exist locally
            if (File.Exists(LogFilePath))
            {
                // File already exist, read from local log fetcher
                log = new ExcelLogFetcher(LogFilePath);
            }
            else
            {
                log = new MDSLogFetcher(GetMDSTableName(), startTime, endTime, GetQuery(), GetUseIndex(), endPoint);
            }
            try
            {
                logOutput = log.Fetch();
            }
            catch(Exception)
            {
                Console.Write("Exception");
            }
            if (logOutput == null || logOutput.Rows.Count == 0)
            {
                Mapper.GetCluster(this).AddVerboseFailureDetails(
                    string.Format("Unable to download {0} logs from table {1} or table has 0 log entries, startTime {2}, endTime {3}, query {4}",
                    LogFileName(), GetMDSTableName(), startTime, endTime, GetQuery()));
            }
        }

        private void GetClusterMetadata()
        {
            // cached data
            var metadata = Mapper.GetCluster(this);
            dnsName = metadata.DnsName;
            startTime = metadata.StartTime;
            endTime = metadata.EndTime;
            clusterIdentifier = metadata.ClusterIdentifier;
            endPoint = metadata.ClusterEndPoint;
        }
        public virtual string LogFilePath
        {
            get {
                string logFileName = LogFileName();
                int dotIndex = logFileName.LastIndexOf('.');
                logFileName = logFileName.Insert(dotIndex, "-" + dnsName);
                return Path.Combine(Settings.BaseDirectoryForLogs, clusterIdentifier, logFileName); 
            }
        }

        public abstract string LogFileName();

        public virtual string GetQuery()
        {
            return "clusterdnsname = \"" + dnsName + "\"";
        }
        public virtual bool GetUseIndex()
        {
            return false;
        }
        public abstract string GetMDSTableName();
        public abstract string GetTableName();



        public virtual void Cleanup()
        {
            Mapper.GetCluster(this).RecordLogDownloaded(GetTableName());
            logOutput = null;
            Mapper.RemoveMapping(this);
        }
 
        public virtual void PruneLogs()
        {
        }

        public void SaveLogs()
        {
            int retryNum = 0;

            while (retryNum < Settings.MaxRetryCount)
            {
                try
                {
                    if (!File.Exists(LogFilePath))
                    {
                        ExcelHelper.ExportToExcel(logOutput, LogFilePath);
                    }
                    break;
                }
                catch (Exception)
                {
                    System.Threading.Thread.Sleep(5000);
                    retryNum++;
                }
            }
            // Save log silently continues if not able to save log
        }
        public void FormatLogs()
        {
            if (logOutput != null)
            {
                Mapper.GetCluster(this).FormattedTables.Add(this.GetTableName(), logOutput);
            }
        }

    }
}
