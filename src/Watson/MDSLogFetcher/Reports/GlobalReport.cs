using System;
using System.Collections.Generic;
using System.IO;
using System.Linq;
using System.Text;

namespace ClusterLogAnalyzer
{
    class GlobalReport
    {
        private static object syncObj = new object();
        public static void WriteToReport(string message, string clusterDateIdentifier)
        {
            string logDirectory = Path.Combine(Settings.BaseDirectoryForLogs, clusterDateIdentifier);
            if (Directory.Exists(logDirectory) == false)
            {
                Directory.CreateDirectory(logDirectory);
            }
            string reportFilePath = Path.Combine(logDirectory, "RollupReport.csv");
            lock(syncObj)
            {
                if (File.Exists(reportFilePath) == false)
                {
                    // New file. Write header
                    File.AppendAllText(reportFilePath, Settings.GlobalFileHeader + Environment.NewLine);
                }
                File.AppendAllText(reportFilePath, message + Environment.NewLine);
            }
        }
    }
}

