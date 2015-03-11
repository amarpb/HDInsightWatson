using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;

namespace ClusterLogAnalyzer.Tasks
{
    public abstract class WorkflowTask
    {

        public abstract bool ReadyToGo
        {
            get;
        }

        public abstract void RunWorkflow();
        public abstract string Name
        { get; }
    }

}
