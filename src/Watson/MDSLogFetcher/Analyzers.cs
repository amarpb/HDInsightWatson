using System;
using System.Collections.Generic;
using System.IO;
using System.Linq;
using System.Reflection;
using System.Text;
using MDSLogAnalyzerCommon;

namespace ClusterLogAnalyzer
{
    public class Analyzers
    {
        private Analyzers()
        {

        }
        private static Analyzers instance = null;
        private static object syncObj = new object();
        public static void InitAnalyzers()
        {
            lock (syncObj)
            {
                if(instance == null)
                {
                    instance = new Analyzers();
                    instance.Load();
                }
            }
        }
        private void Load()
        {
            // Load all analyzers
            string baseDir = Path.GetDirectoryName(Assembly.GetExecutingAssembly().Location) + @"\Analyzers";
            var analyzers = Directory.GetFiles(baseDir, "*.dll");
            foreach (string analyzer in analyzers)
            {
                Assembly analyzerDll = Assembly.LoadFile(analyzer);
                var types = analyzerDll.GetTypes();
                foreach(var type in types)
                {
                    if (type.GetInterfaces().Contains(typeof(ILogAnalyzer)))
                    { 
                        var obj = Activator.CreateInstance(type);
                        Mapper.AddAnalyzer((ILogAnalyzer)obj);
                    }
                }
            }
        }
    }
}
