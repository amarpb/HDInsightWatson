using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using Microsoft.Cis.Monitoring.DataAccess;
using System.Security.Cryptography.X509Certificates;
using MDSLogAnalyzerCommon;

namespace ClusterLogAnalyzer
{
    public class MDSHelper
    {

        public class MDSClient
        {
            public static MdsDataAccessClient GetDataAccessClient(EndPoint endPoint)
            {
                switch(endPoint)
                {
                    case EndPoint.AzureGlobal:
                        return MDSGlobalAzureClient.GetDataAccessClient();
                    case EndPoint.Mooncake:
                        return MDSMooncakeClient.GetDataAccessClient();
                    case EndPoint.Test:
                        return MDSTestClient.GetDataAccessClient();
                    default:
                        throw new InvalidOperationException("EndPoint " + endPoint + " not supported");

                }
            }
            private MDSClient()
            {
            }
        }
        public class MDSGlobalAzureClient
        {
            private static MdsDataAccessClient client = null;
            private static object syncObj = new object();
            private static DateTime lastRefreshTime = DateTime.MinValue;
            public static MdsDataAccessClient GetDataAccessClient()
            {
                lock (syncObj)
                {
                    if (client == null || DateTime.Now.Subtract(lastRefreshTime).TotalMinutes > 5)
                    {
                       // client = new MdsDataAccessClient(Settings.MdsEndPoint, "CN=MaHDInsightProd");
                        client = new MdsDataAccessClient(Settings.MdsEndPoint);
                        lastRefreshTime = DateTime.Now;
                    }
                }
                return client;
            }
            private MDSGlobalAzureClient()
            { }
        }
        public class MDSMooncakeClient
        {
            private static MdsDataAccessClient client = null;
            private static object syncObj = new object();
            private static DateTime lastRefreshTime = DateTime.MinValue;
            public static MdsDataAccessClient GetDataAccessClient()
            {
                lock (syncObj)
                {
                    if (client == null || DateTime.Now.Subtract(lastRefreshTime).TotalMinutes > 5)
                    {
                        client = new MdsDataAccessClient(Settings.MdsMCEndPoint, "CN=mdsprodclientcert.hdinsightservices.cn, OU=Azure, O=Shanghai Blue Cloud Technology Co. Ltd, L=Shanghai, S=Shanghai, C=CN");
                        lastRefreshTime = DateTime.Now;
                    }
                }
                return client;
            }
            private MDSMooncakeClient()
            { }
        }
        public class MDSTestClient
        {
            private static MdsDataAccessClient client = null;
            private static object syncObj = new object();
            private static DateTime lastRefreshTime = DateTime.MinValue;
            public static MdsDataAccessClient GetDataAccessClient()
            {
                lock (syncObj)
                {
                    if (client == null || DateTime.Now.Subtract(lastRefreshTime).TotalMinutes > 5)
                    {
                        client = new MdsDataAccessClient(Settings.MdsTestEndPoint, "CN=MaHDInsightTest");
                        // client = new MdsDataAccessClient(Settings.MdsEndPoint);
                        lastRefreshTime = DateTime.Now;
                    }
                }
                return client;
            }
            private MDSTestClient()
            { }
        }
    }
}
