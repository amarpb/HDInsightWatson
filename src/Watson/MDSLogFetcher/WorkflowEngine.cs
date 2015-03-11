using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using ClusterLogAnalyzer.Tasks;
using MDSLogAnalyzerCommon;
using System.Threading;

namespace ClusterLogAnalyzer
{
    public class WorkflowEngine
    {
        private static WorkflowEngine instance = null;
        private static object lockObj = new object();
        private Dictionary<TableDownloadTask, ClusterMetadata> tables;
        private Timer timer;

        private WorkflowEngine()
        {
            tables = new Dictionary<TableDownloadTask, ClusterMetadata>();
        }

        public static WorkflowEngine GetInstance()
        {
            if (instance == null)
            {
                lock (lockObj)
                {
                    if (instance == null)
                    {
                        instance = new WorkflowEngine();
                    }

                }
            }
            return instance;
        }
        public void AnalyzeLogsForAllClusters(string[] clusterDetails, EndPoint endPoint)
        {
            // Before downloading, initialize connection to MDS
            MDSHelper.MDSClient.GetDataAccessClient(endPoint);
            foreach (var line in clusterDetails)
            {
                var fields = line.Split('\t');
                int dnsIndex = 0;
                int dateIndex = 1;
                DateTime startTime, endTime;

                if (DateTime.TryParse(fields[0], out startTime))
                {
                    dateIndex = 0;
                    dnsIndex = 1;
                }
                string dnsName = fields[dnsIndex];
                if (DateTime.TryParse(fields[dateIndex], out startTime) == false)
                {
                    throw new FormatException("Unable to parse line " + line + " in input file");
                }
                endTime = startTime.AddHours(24);
                if (fields.Count() > 2)
                {
                    // endTime specified
                    if (DateTime.TryParse(fields[2], out endTime) == false)
                    {
                        throw new FormatException("Unable to parse line " + line + " in input file");
                    }
                }

                // Enqueue task to Download and analyze logs for this cluster
                Task analyzeLogsTask = new Task(() => AnalyzeLogsForACluster(dnsName, startTime, endTime, endPoint));
                analyzeLogsTask.Start();
            }
        }
        public void Init()
        {
            // Init Analyzers
            Analyzers.InitAnalyzers();
        }

        public void AnalyzeLogsForACluster(string dnsName, DateTime startTime, DateTime endTime, EndPoint endPoint)
        {
            var tablesToDownload = new List<TableDownloadTask>();
            tablesToDownload.Add(new CrudTable());

            // Create a cluster object and add it to Dictionary
            var metadata = new ClusterMetadata(dnsName, startTime, endPoint);
            var scheduler = Scheduler.GetInstance();

            foreach(var table in tablesToDownload)
            {
                metadata.AddTableToCluster(table.GetTableName());
                Mapper.AddMapping(table, metadata);
                scheduler.ScheduleTask(table);
            }
        }

        public void FindFailingClusters(DateTime startTime, DateTime endTime, EndPoint endPoint)
        {
            var failureClusterTask = new GetFailingClustersTask(startTime, endTime, endPoint);
            Scheduler.GetInstance().ScheduleTask(failureClusterTask);
        }
        public void Setuptimer()
        {
            timer = new Timer(TimerTick, null, 0, Settings.TimerIntervalInSeconds * 1000);
        }
        public void TimerTick(object obj)
        {
            Console.WriteLine("Start Fired :" + DateTime.Now.ToString());
            // MDS has a lag of 2 minutes
            DateTime now = DateTime.Now.AddMinutes(-3);
            FindFailingClusters(now.AddSeconds((Settings.TimerIntervalInSeconds * -1) - 2), now, EndPoint.AzureGlobal);
            FindFailingClusters(now.AddSeconds((Settings.TimerIntervalInSeconds * -1) - 2), now, EndPoint.Mooncake);
        }
    }
}
