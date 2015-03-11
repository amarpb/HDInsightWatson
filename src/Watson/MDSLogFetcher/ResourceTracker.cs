using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;

namespace ClusterLogAnalyzer
{
    public class ResourceTracker
    {
        private static ResourceTracker resourceTracker = null;
        private static object syncObj = new object();
        private int logProcessingCount;
        private ResourceTracker()
        {
            logProcessingCount = 0;
        }
        public static ResourceTracker GetInstance()
        {
            if (resourceTracker == null)
            {
                lock (syncObj)
                {
                    resourceTracker = new ResourceTracker();
                }
            }
            return resourceTracker;
        }
        public void LogProcessingStart()
        {
            lock (syncObj)
            {
                logProcessingCount++;
            }
        }
        public void LogProcessingStop()
        {
            lock (syncObj)
            {
                logProcessingCount--;
            }
        }
        public int GetCountOfLogsInProcessing()
        {
            return logProcessingCount;
        }

    }
}
