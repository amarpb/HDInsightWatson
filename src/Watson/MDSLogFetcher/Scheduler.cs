using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading;
using System.Threading.Tasks;
using ClusterLogAnalyzer.Tasks;

namespace ClusterLogAnalyzer
{
    public class Scheduler
    {
        private static Scheduler scheduler = null;
        private static object syncObj = new object();
        private Semaphore resourceAvailableSp;
        private List<WorkflowTask> workflowTasks;
        public delegate void WorkflowProgress(int completed, int total);
        public event WorkflowProgress WorkflowProgressUpdated;
        private static int totalTasks = 0;
        private static int completedTasks = 0;

        private Scheduler()
        {
            resourceAvailableSp = new Semaphore(Settings.ParallelTasksCount, Settings.MaxParallelTasksCount);
            workflowTasks = new List<WorkflowTask>();
        }
        public static Scheduler GetInstance()
        {
            if (scheduler == null)
            {
                lock (syncObj)
                {
                    if (scheduler == null)
                    {
                        scheduler = new Scheduler();
                        Task schedulerTask = new Task(() => scheduler.Start());
                        schedulerTask.Start();
                    }
                }
            }
            return scheduler;
        }
        public void ScheduleTask(WorkflowTask task)
        {
            lock (syncObj)
            {
                if(totalTasks == completedTasks)
                {
                    // Previous set of tasks have completed. Start new batch.
                    // Reset progress bar
                    totalTasks = completedTasks = 0;
                }
                workflowTasks.Add(task);
                totalTasks++;
                if (WorkflowProgressUpdated != null)
                {
                    WorkflowProgressUpdated(completedTasks, totalTasks);
                }
            }
        }
        private void Start()
        {
            while (true)
            {
                CheckThreadCount();
                if (resourceAvailableSp.WaitOne())
                {
                    try
                    {
                        WorkflowTask workflow = null;
                        lock (syncObj)
                        {
                            workflow = workflowTasks.FirstOrDefault(t => t.ReadyToGo);
                            if(workflow != null)
                            {
                                workflowTasks.Remove(workflow);
                            }
                        }
                        if (workflow != null)
                        {
                            Task t = new Task(() => StartWorkflow(workflow));
                            t.Start();
                        }
                        else
                        {
                            // nothing to download
                            Thread.Sleep(5 * 1000);
                            resourceAvailableSp.Release();
                        }
                    }
                    catch (Exception ex)
                    {
                        Console.Write(ex.Message);
                        resourceAvailableSp.Release();
                    }
                }
            }
        }
        private void CheckThreadCount()
        {
            while (Settings.ParallelTasksCount != Settings.TargetParallelTasksCount)
            {
                if (Settings.TargetParallelTasksCount > Settings.ParallelTasksCount)
                {
                    // Add more threads
                    Settings.ParallelTasksCount++;
                    resourceAvailableSp.Release();
                }
                else
                {
                    // Remove threads
                    Settings.ParallelTasksCount--;
                    resourceAvailableSp.WaitOne();
                }
            }

        }
        private void SafeExecute(Action action, out Exception exception)
        {
            exception = null;

            try
            {
                action();
            }
            catch (Exception ex)
            {
                exception = ex;
            }
        }
        private void StartWorkflow(WorkflowTask workflow)
        {
            try
            {
                Thread workflowThread = new Thread(workflow.RunWorkflow);
                workflowThread.Name = workflow.Name;
                workflowThread.Start();
                workflowThread.Join();
            }
            catch (Exception ex)
            {
                Console.Write(ex.Message);
            }
            lock (syncObj)
            {
                completedTasks++;
            }
            if(WorkflowProgressUpdated != null)
            {
                WorkflowProgressUpdated(completedTasks, totalTasks);
            }

            resourceAvailableSp.Release();
        }

    }
}
