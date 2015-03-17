using System;
using System.Collections.Generic;
using System.ComponentModel;
using System.Configuration;
using System.Data;
using System.Drawing;
using System.IO;
using System.Linq;
using System.Text;
using System.Threading;
using System.Threading.Tasks;
using System.Windows.Forms;
using Microsoft.Cis.Monitoring.DataAccess;
using Microsoft.Cis.Monitoring.Mds.mdscommon;
using ClusterLogAnalyzer;
using ClusterLogAnalyzer.Tasks;
using MDSLogAnalyzerCommon;

namespace Watson
{
    public partial class Form1 : Form
    {
        public Form1()
        {
            InitializeComponent();
        }

        private void label1_Click(object sender, EventArgs e)
        {

        }


        private void Form1_Load(object sender, EventArgs e)
        {
            startTime.CustomFormat = "yyyy.MM.dd HH:mm";
            startTime.Format = DateTimePickerFormat.Custom;

            endTime.CustomFormat = "yyyy.MM.dd HH:mm";
            endTime.Format = DateTimePickerFormat.Custom;

            startTime.Value = DateTime.Now.AddHours(-5).ToUniversalTime();
            endTime.Value = DateTime.Now.ToUniversalTime();

            progressBar1.Step = 1;
            progressBar1.Minimum = 0;
            progressBar1.Maximum = 100;
            progressBar1.Value = 0;
            numericUpDown1.Value = Settings.ParallelTasksCount;
            numericUpDown1.Minimum = 1;
            numericUpDown1.Maximum = Settings.MaxParallelTasksCount;
            Scheduler.GetInstance().WorkflowProgressUpdated += Form1_WorkflowProgressUpdated;

            var mdsEndPoint = ConfigurationManager.AppSettings["MdsEndPoint"];
            if(mdsEndPoint != null)
            {
                Settings.MdsEndPoint = mdsEndPoint;
            }
            var baseDirectoryForLogs = ConfigurationManager.AppSettings["BaseDirectoryForLogs"];
            if(string.IsNullOrWhiteSpace(baseDirectoryForLogs) == false)
            {
                Settings.BaseDirectoryForLogs = baseDirectoryForLogs;
            }
        }

        void Form1_WorkflowProgressUpdated(int completed, int total)
        {
            int value = 0;
            if (total != 0)
            {
                value = (int)((completed / (double)total) * 100);
            }
            progressBar1.BeginInvoke(new Action(() => {progressBar1.Value = value; }));
        }

        private void startTime_ValueChanged(object sender, EventArgs e)
        {
            // Cluster life is during CRUD failure is generally few hours
            // For convinience, set end time as start time + 5 hours
            endTime.Value = startTime.Value.AddHours(5);
        }


        private void numericUpDown1_ValueChanged(object sender, EventArgs e)
        {
            Settings.TargetParallelTasksCount = (int)numericUpDown1.Value;
        }

        private void AnalyzeLogs_Click(object sender, EventArgs e)
        {
            var engine = WorkflowEngine.GetInstance();
            engine.Init();
            EndPoint endPoint = EndPoint.AzureGlobal;
            var endPointStr = endpointSelectionComboBox.Text.ToString();
            if (endPointStr.Contains("AzureGlobal"))
            {
                    endPoint = EndPoint.AzureGlobal;
            }
            else if (endPointStr.Contains("Mooncake"))
            {
                endPoint = EndPoint.Mooncake;
            }
            else if (endPointStr.Contains("Test"))
            {
                endPoint = EndPoint.Test;
            }
    //        EndPoint endPoint = EndPoint.Mooncake; // mcCheckBox.CheckState == CheckState.Checked ? EndPoint.Mooncake : EndPoint.AzureGlobal;
            if(string.IsNullOrEmpty(textBox1.Text))
            {
                Task failureClusterTask = new Task(() => engine.FindFailingClusters(startTime.Value, endTime.Value, endPoint));
                failureClusterTask.Start();
            }
            else if(textBox1.Text == "Timer")
            {
                Task setupTimerTask = new Task(() => engine.Setuptimer());
                
                setupTimerTask.Start();   
            }
            else if (File.Exists(textBox1.Text))
            {
                // Input method is text file, containg following tab seperated fields in order of : dnsName, startTime and endTime(optional).
                // or startTime, dnsName, endtime(optional)
                var lines = File.ReadAllLines(textBox1.Text);
                Task downloadLogsTask = new Task(() => engine.AnalyzeLogsForAllClusters(lines, endPoint));
                downloadLogsTask.Start();
            }
            else
            {
                // Input method is dnsname, startTime and endTime specified on the form
                Task downloadLogsTask = new Task(() => engine.AnalyzeLogsForACluster(textBox1.Text, startTime.Value, endTime.Value, endPoint));
                downloadLogsTask.Start();
            }
        }

        private void environmentSelectionComboBox_SelectedIndexChanged(object sender, EventArgs e)
        {

        }
    }
}
