using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using MDSLogAnalyzerCommon;
using ClusterLogAnalyzer.Tasks;

namespace ClusterLogAnalyzer
{
    public static class Mapper
    {
        private static Dictionary<TableDownloadTask, ClusterMetadata> tableToCluster = new Dictionary<TableDownloadTask, ClusterMetadata>();
        private static HashSet<ILogAnalyzer> analyzers = new HashSet<ILogAnalyzer>();
        private static object syncObj = new object();

        public static void AddAnalyzer(ILogAnalyzer analyzer)
        {
            analyzers.Add(analyzer);
        }

        public static ClusterMetadata GetCluster(TableDownloadTask table)
        {
            if (tableToCluster.ContainsKey(table))
            {
                return tableToCluster[table];
            }
            return null;
        }
        public static void AddMapping(TableDownloadTask table, ClusterMetadata cluster)
        {
            lock (syncObj)
            {
                tableToCluster[table] = cluster;
            }
        }
        public static void RemoveMapping(TableDownloadTask table)
        {
            lock (syncObj)
            {
                if (tableToCluster.ContainsKey(table))
                {
                    tableToCluster.Remove(table);
                }
            }
        }
        public static List<ILogAnalyzer> GetAnalyzers()
        {
            return analyzers.ToList();
        }
    }
}
