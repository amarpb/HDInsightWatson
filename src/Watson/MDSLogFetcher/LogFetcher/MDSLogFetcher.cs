using System;
using System.Collections;
using System.Collections.Concurrent;
using System.Collections.Generic;
using System.Data;
using System.Diagnostics;
using System.Linq;
using System.IO;
using System.Text;
using System.Threading;
using Microsoft.Cis.Monitoring.DataAccess;
using Microsoft.Cis.Monitoring.Mds.mdscommon;
using Microsoft.Cis.Monitoring.Mds.MdsStorageClient;
using Microsoft.Cis.Monitoring.Mds.Subscription;
using Microsoft.Cis.Monitoring.Mds.MdsActiveClient;
using System.Data.Services.Client;
using MDSLogAnalyzerCommon;
using System.Windows.Forms;

namespace ClusterLogAnalyzer
{

    public class MDSLogFetcher : ILogFetcher
    {
        private string logTableName;
        private DateTime startTime;
        private DateTime endTime;
        string query;
        bool useIndex = false;
        EndPoint endPoint;

        public MDSLogFetcher(string tableName, DateTime startTime, DateTime endTime, string query, bool useIndex, EndPoint endPoint)
        {
            this.logTableName = tableName;
            this.startTime = startTime;
            this.endTime = endTime;
            this.query = query;
            this.useIndex = useIndex;
            this.endPoint = endPoint;
        }
        public DataTable Fetch()
        {
            MdsDataAccessClient mdsDataAccessClient = MDSHelper.MDSClient.GetDataAccessClient(endPoint);
            IEnumerable<GenericLogicEntity> log = null;

            int retryNum = 0;

            while (retryNum < Settings.MaxRetryCount)
            {
                try
                {
                    // Fetching data using the MdaDataAccessClient
                    // Looks like date time is in local time
                    startTime = startTime.ToLocalTime();
                    endTime = endTime.ToLocalTime();
                    // start and end time got from MDS are generally off by few minutes. Adjust them
                    startTime = startTime.AddMinutes(-3);
                    endTime = endTime.AddMinutes(3);
                    log = mdsDataAccessClient.GetTabularData(logTableName, startTime, endTime, query, useIndex);

           //         log = mdsDataAccessClient.QueryMdsTableAsync(logTableName, 0, startTime, endTime, query, 3 * 60, null, false, "", 0, useIndex);

                    if ((log == null) || (log.Count() == 0))
                    {
                        // no data
                        return null;
                    }
                    log = log.OrderBy(x => x.ToDictionary()["PreciseTimeStamp"], new DateComparer());
                    break;
                }
                catch (Exception e)
                {
                    Console.Write(e.ToString());
                    System.Threading.Thread.Sleep(5000);
                    retryNum++;
                }
            }
            if(retryNum == Settings.MaxRetryCount)
            {
                return null;
            }
            var table = new DataTable();
            var row = table.NewRow();

            try
            {
                // Write header
                foreach (var property in log.First().ToDictionary())
                {
                    table.Columns.Add(property.Key);
                }

                // Write content rows
                foreach (var logEntry in log)
                {
                    row = table.NewRow();
                    foreach (var property in logEntry.ToDictionary())
                    {
                        if (!table.Columns.Contains(property.Key))
                        {
                            table.Columns.Add(property.Key);
                        }
                        row[property.Key] = property.Value;
                    }
                    table.Rows.Add(row);
                }
            }
            catch (DataServiceQueryException queryException)
            {
                var dataServiceClientException = queryException.InnerException as DataServiceClientException;
                if (dataServiceClientException != null && dataServiceClientException.StatusCode != 404)
                {
                    throw;
                }
            }
            catch(Exception e)
            {
                return null;
            }
            return table;
        }
    }
    public class DateComparer : IComparer<object>
    {
        public int Compare(object x, object y)
        {
            DateTime d1 = (DateTime)x;
            DateTime d2 = (DateTime)y;

            return DateTime.Compare(d1, d2);
        }
    }
}
