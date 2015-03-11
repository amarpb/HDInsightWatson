using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using ClusterLogAnalyzer.Tasks;

namespace ClusterLogAnalyzer.Tasks.TableDownload
{
    interface ITableDownloadFactory
    {
        WorkflowTask GenerateTask(string tableName);

    }
    class TableDownloadFactory : ITableDownloadFactory
    {
        public WorkflowTask GenerateTask(string tableName)
        {
           // switch(tableName)
           // {
           ////     default:
           //  //       return new WorkflowTask();
           // }
            return null;
        }
    }
}
