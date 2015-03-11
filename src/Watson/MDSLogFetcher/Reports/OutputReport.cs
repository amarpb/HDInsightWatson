using System;
using System.Collections.Generic;
using System.IO;
using System.Linq;
using System.Text;

namespace ClusterLogAnalyzer
{
    public class OutputReport
    {
        private static object syncObj = new object();
        public static void WriteToReport(string message, string clusterIdentifier)
        {
            lock (syncObj)
            {
                string reportFilePath = GetReportFilePath(clusterIdentifier);
                File.AppendAllText(reportFilePath, message + Environment.NewLine);
            }
        }
        public static string GetReportFilePath(string clusterIdentifier)
        {
            string dirPath = Path.Combine(Settings.BaseDirectoryForLogs, clusterIdentifier);
            if (Directory.Exists(dirPath) == false)
            {
                Directory.CreateDirectory(dirPath);
            }
            return Path.Combine(dirPath, Settings.OutputFileName);
        }
        public static void DeleteReportIfExist(string clusterIdentifier)
        {
            var path = GetReportFilePath(clusterIdentifier);
            if(File.Exists(path))
            {
                File.Delete(path);
            }
        }
        public static string GetReportFileNetworkPath(string clusterIdentifier)
        {
            string dirPath = Path.Combine(Settings.NetworkPath, clusterIdentifier);

            return Path.Combine(dirPath, Settings.OutputFileName);
        }
    }
}
