using System;
using System.Collections.Generic;
using System.Data;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace ClusterLogAnalyzer
{
    public interface ILogFetcher
    {
        DataTable Fetch();
    }
}
