using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;

namespace ClusterLogAnalyzer.Tasks
{
    //======================================================================
    //This is the abstract class that is implemented by the following classes
    //a) GetFailingClusterTask - This is a class
    //b) TableDownloadTask - This is an abstract class
    //c) InvokeAnalyzersTask - This is a class
    //d) LogResultsTask - This is a class.
    //<Methods>
    //a) RunWorkflow():void
    //</Methods>
    //<Properties>
    //a) Name: Which is readonly
    //b) ReadyToGo: read only
    //</Properties>
    //=======================================================================

    public abstract class WorkflowTask
    {
             
        public abstract bool ReadyToGo
        {
            get;
        }

        public abstract void RunWorkflow();
        /// <remarks></remarks>
        public abstract string Name
        { get; }
    }

}
