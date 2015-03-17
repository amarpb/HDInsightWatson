namespace Watson
{
    partial class RDPFailures
    {
        /// <summary>
        /// Required designer variable.
        /// </summary>
        private System.ComponentModel.IContainer components = null;

        /// <summary>
        /// Clean up any resources being used.
        /// </summary>
        /// <param name="disposing">true if managed resources should be disposed; otherwise, false.</param>
        protected override void Dispose(bool disposing)
        {
            if (disposing && (components != null))
            {
                components.Dispose();
            }
            base.Dispose(disposing);
        }

        #region Windows Form Designer generated code

        /// <summary>
        /// Required method for Designer support - do not modify
        /// the contents of this method with the code editor.
        /// </summary>
        private void InitializeComponent()
        {
            this.lblDNSName = new System.Windows.Forms.Label();
            this.txtClusterDNSName = new System.Windows.Forms.TextBox();
            this.lblStartTime = new System.Windows.Forms.Label();
            this.startTimePicker = new System.Windows.Forms.DateTimePicker();
            this.lblEndTime = new System.Windows.Forms.Label();
            this.endTimePicker = new System.Windows.Forms.DateTimePicker();
            this.endpointSelectionComboBox = new System.Windows.Forms.ComboBox();
            this.lblEnvironment = new System.Windows.Forms.Label();
            this.btnGetRDPFailureLogs = new System.Windows.Forms.Button();
            this.btnExit = new System.Windows.Forms.Button();
            this.progressBarRDPFailure = new System.Windows.Forms.ProgressBar();
            this.SuspendLayout();
            // 
            // lblDNSName
            // 
            this.lblDNSName.AutoSize = true;
            this.lblDNSName.Location = new System.Drawing.Point(13, 27);
            this.lblDNSName.Name = "lblDNSName";
            this.lblDNSName.Size = new System.Drawing.Size(99, 13);
            this.lblDNSName.TabIndex = 0;
            this.lblDNSName.Text = "Cluster DNS Name:";
            // 
            // txtClusterDNSName
            // 
            this.txtClusterDNSName.Location = new System.Drawing.Point(118, 24);
            this.txtClusterDNSName.Name = "txtClusterDNSName";
            this.txtClusterDNSName.Size = new System.Drawing.Size(200, 20);
            this.txtClusterDNSName.TabIndex = 1;
            // 
            // lblStartTime
            // 
            this.lblStartTime.AutoSize = true;
            this.lblStartTime.Location = new System.Drawing.Point(16, 92);
            this.lblStartTime.Name = "lblStartTime";
            this.lblStartTime.Size = new System.Drawing.Size(86, 13);
            this.lblStartTime.TabIndex = 6;
            this.lblStartTime.Text = "Start Time (UTC)";
            // 
            // startTimePicker
            // 
            this.startTimePicker.Format = System.Windows.Forms.DateTimePickerFormat.Custom;
            this.startTimePicker.Location = new System.Drawing.Point(118, 85);
            this.startTimePicker.Name = "startTimePicker";
            this.startTimePicker.ShowUpDown = true;
            this.startTimePicker.Size = new System.Drawing.Size(200, 20);
            this.startTimePicker.TabIndex = 7;
            this.startTimePicker.Value = new System.DateTime(2014, 9, 28, 20, 47, 0, 0);
            // 
            // lblEndTime
            // 
            this.lblEndTime.AutoSize = true;
            this.lblEndTime.Location = new System.Drawing.Point(19, 133);
            this.lblEndTime.Name = "lblEndTime";
            this.lblEndTime.Size = new System.Drawing.Size(83, 13);
            this.lblEndTime.TabIndex = 8;
            this.lblEndTime.Text = "End Time (UTC)";
            // 
            // endTimePicker
            // 
            this.endTimePicker.Format = System.Windows.Forms.DateTimePickerFormat.Custom;
            this.endTimePicker.Location = new System.Drawing.Point(118, 127);
            this.endTimePicker.Name = "endTimePicker";
            this.endTimePicker.ShowUpDown = true;
            this.endTimePicker.Size = new System.Drawing.Size(200, 20);
            this.endTimePicker.TabIndex = 9;
            this.endTimePicker.Value = new System.DateTime(2014, 9, 28, 23, 47, 0, 0);
            // 
            // endpointSelectionComboBox
            // 
            this.endpointSelectionComboBox.AutoCompleteCustomSource.AddRange(new string[] {
            "AzureGlobal",
            "Mooncake",
            "Test"});
            this.endpointSelectionComboBox.FormattingEnabled = true;
            this.endpointSelectionComboBox.Location = new System.Drawing.Point(118, 160);
            this.endpointSelectionComboBox.Name = "endpointSelectionComboBox";
            this.endpointSelectionComboBox.Size = new System.Drawing.Size(121, 21);
            this.endpointSelectionComboBox.TabIndex = 13;
            // 
            // lblEnvironment
            // 
            this.lblEnvironment.AutoSize = true;
            this.lblEnvironment.Location = new System.Drawing.Point(19, 163);
            this.lblEnvironment.Name = "lblEnvironment";
            this.lblEnvironment.Size = new System.Drawing.Size(66, 13);
            this.lblEnvironment.TabIndex = 14;
            this.lblEnvironment.Text = "Environment";
            // 
            // btnGetRDPFailureLogs
            // 
            this.btnGetRDPFailureLogs.Location = new System.Drawing.Point(16, 200);
            this.btnGetRDPFailureLogs.Name = "btnGetRDPFailureLogs";
            this.btnGetRDPFailureLogs.Size = new System.Drawing.Size(129, 23);
            this.btnGetRDPFailureLogs.TabIndex = 15;
            this.btnGetRDPFailureLogs.Text = "Get RDP Failure Logs";
            this.btnGetRDPFailureLogs.UseVisualStyleBackColor = true;
            // 
            // btnExit
            // 
            this.btnExit.Location = new System.Drawing.Point(189, 200);
            this.btnExit.Name = "btnExit";
            this.btnExit.Size = new System.Drawing.Size(129, 23);
            this.btnExit.TabIndex = 16;
            this.btnExit.Text = "Cancel";
            this.btnExit.UseVisualStyleBackColor = true;
            // 
            // progressBarRDPFailure
            // 
            this.progressBarRDPFailure.Location = new System.Drawing.Point(118, 51);
            this.progressBarRDPFailure.Name = "progressBarRDPFailure";
            this.progressBarRDPFailure.Size = new System.Drawing.Size(200, 18);
            this.progressBarRDPFailure.TabIndex = 17;
            // 
            // RDPFailures
            // 
            this.AutoScaleDimensions = new System.Drawing.SizeF(6F, 13F);
            this.AutoScaleMode = System.Windows.Forms.AutoScaleMode.Font;
            this.ClientSize = new System.Drawing.Size(348, 248);
            this.Controls.Add(this.progressBarRDPFailure);
            this.Controls.Add(this.btnExit);
            this.Controls.Add(this.btnGetRDPFailureLogs);
            this.Controls.Add(this.lblEnvironment);
            this.Controls.Add(this.endpointSelectionComboBox);
            this.Controls.Add(this.endTimePicker);
            this.Controls.Add(this.lblEndTime);
            this.Controls.Add(this.startTimePicker);
            this.Controls.Add(this.lblStartTime);
            this.Controls.Add(this.txtClusterDNSName);
            this.Controls.Add(this.lblDNSName);
            this.Name = "RDPFailures";
            this.Text = "RDPFailures";
            this.Load += new System.EventHandler(this.RDPFailures_Load);
            this.ResumeLayout(false);
            this.PerformLayout();

        }

        #endregion

        private System.Windows.Forms.Label lblDNSName;
        private System.Windows.Forms.TextBox txtClusterDNSName;
        private System.Windows.Forms.Label lblStartTime;
        private System.Windows.Forms.DateTimePicker startTimePicker;
        private System.Windows.Forms.Label lblEndTime;
        private System.Windows.Forms.DateTimePicker endTimePicker;
        private System.Windows.Forms.ComboBox endpointSelectionComboBox;
        private System.Windows.Forms.Label lblEnvironment;
        private System.Windows.Forms.Button btnGetRDPFailureLogs;
        private System.Windows.Forms.Button btnExit;
        private System.Windows.Forms.ProgressBar progressBarRDPFailure;
    }
}