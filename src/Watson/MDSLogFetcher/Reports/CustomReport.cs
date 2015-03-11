using System;
using System.Collections.Generic;
using System.IO;
using System.Linq;
using System.Text;

namespace ClusterLogAnalyzer
{
    class CustomReport
    {
        private static object syncObj = new object();
        private const string ReportHeader = "Create Time,Region,Cluster DNS Name,Azure Deployement ID,Nodes stuck,Details";
        
        public static void WriteToReport(string message)
        {
            if(string.IsNullOrWhiteSpace(message))
            {
                return;
            }
            string logDirectory = Settings.BaseDirectoryForLogs;
            if (Directory.Exists(logDirectory) == false)
            {
                Directory.CreateDirectory(logDirectory);
            }
            string reportFilePath = Path.Combine(logDirectory, "CustomReport.csv");
            lock (syncObj)
            {
                if (File.Exists(reportFilePath) == false)
                {
                    // New file. Write header
                    File.AppendAllText(reportFilePath, ReportHeader + Environment.NewLine);
                }
                File.AppendAllText(reportFilePath, message + Environment.NewLine);
            }
        }
    }
}

