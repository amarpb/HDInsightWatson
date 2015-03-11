using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace MDSLogAnalyzerCommon
{
    public interface ILogAnalyzer
    {
        void AnalyzeLogs(ClusterMetadata cluster);
    }
}
