using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;

namespace ClusterLogAnalyzer
{
    public class Settings
    {
        public const string TempDirectory = @"C:\temp";
        public const int BaseSleepDurationInSeconds = 5;
        public static int SleepDurationInSeconds = 120;
        public static int ParallelTasksCount = 15;
        public static int TargetParallelTasksCount = ParallelTasksCount;
        public static int MaxParallelTasksCount = 60;
        public const string GlobalFileHeader = "DNSName,PreciseTimeStamp,Region,Version,Cluster Features,Failure Category, Failure Description";
        public const string GlobalFileLineFormat = "{0},{1},{2},{3},{4},{5},{6}";
        public static string MdsEndPoint = "https://production.diagnostics.monitoring.core.windows.net";
        public const string MdsTestEndPoint = @"https://test1.diagnostics.monitoring.core.windows.net/";
        public const string MdsMCEndPoint = @"https://monitoring.core.chinacloudapi.cn/";
        public static int TimerIntervalInSeconds = 2 * 60;
        public static int MaxRetryCount = 2;
        public static string BaseDirectoryForLogs = @"C:\mdslogsCustom";
        public static string NetworkPath = @"\\hdifs\mdslogs";
        public static string OutputFileName = "output.txt";
    }
}
