﻿namespace VirtualRadar.WinForms.SettingPage
{
    partial class PageRebroadcastServer
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
            if(disposing && (components != null)) {
                components.Dispose();
            }
            base.Dispose(disposing);
        }

        #region Component Designer generated code

        /// <summary> 
        /// Required method for Designer support - do not modify 
        /// the contents of this method with the code editor.
        /// </summary>
        private void InitializeComponent()
        {
            this.label3 = new System.Windows.Forms.Label();
            this.numericStaleSeconds = new System.Windows.Forms.NumericUpDown();
            this.label6 = new System.Windows.Forms.Label();
            this.numericPort = new System.Windows.Forms.NumericUpDown();
            this.label1 = new System.Windows.Forms.Label();
            this.comboBoxFormat = new System.Windows.Forms.ComboBox();
            this.label4 = new System.Windows.Forms.Label();
            this.comboBoxReceiver = new System.Windows.Forms.ComboBox();
            this.checkBoxEnabled = new System.Windows.Forms.CheckBox();
            this.label2 = new System.Windows.Forms.Label();
            this.textBoxName = new System.Windows.Forms.TextBox();
            this.groupBoxAccessControl = new System.Windows.Forms.GroupBox();
            this.accessControl = new VirtualRadar.WinForms.Controls.AccessControl();
            this.checkBoxIsTransmitter = new System.Windows.Forms.CheckBox();
            this.label7 = new System.Windows.Forms.Label();
            this.textBoxTransmitAddress = new System.Windows.Forms.TextBox();
            this.label16 = new System.Windows.Forms.Label();
            this.label15 = new System.Windows.Forms.Label();
            this.numericIdleTimeout = new System.Windows.Forms.NumericUpDown();
            this.checkBoxUseKeepAlive = new System.Windows.Forms.CheckBox();
            this.label8 = new System.Windows.Forms.Label();
            this.label9 = new System.Windows.Forms.Label();
            this.textBoxPassphrase = new System.Windows.Forms.TextBox();
            this.label5 = new System.Windows.Forms.Label();
            this.numericSendInterval = new System.Windows.Forms.NumericUpDown();
            this.label10 = new System.Windows.Forms.Label();
            ((System.ComponentModel.ISupportInitialize)(this.numericStaleSeconds)).BeginInit();
            ((System.ComponentModel.ISupportInitialize)(this.numericPort)).BeginInit();
            this.groupBoxAccessControl.SuspendLayout();
            ((System.ComponentModel.ISupportInitialize)(this.numericIdleTimeout)).BeginInit();
            ((System.ComponentModel.ISupportInitialize)(this.numericSendInterval)).BeginInit();
            this.SuspendLayout();
            // 
            // label3
            // 
            this.label3.AutoSize = true;
            this.label3.Location = new System.Drawing.Point(0, 232);
            this.label3.Name = "label3";
            this.label3.Size = new System.Drawing.Size(88, 13);
            this.label3.TabIndex = 17;
            this.label3.Text = "::StaleSeconds:::";
            // 
            // numericStaleSeconds
            // 
            this.numericStaleSeconds.Location = new System.Drawing.Point(200, 230);
            this.numericStaleSeconds.Maximum = new decimal(new int[] {
            3600,
            0,
            0,
            0});
            this.numericStaleSeconds.Minimum = new decimal(new int[] {
            1,
            0,
            0,
            0});
            this.numericStaleSeconds.Name = "numericStaleSeconds";
            this.numericStaleSeconds.Size = new System.Drawing.Size(77, 20);
            this.numericStaleSeconds.TabIndex = 18;
            this.numericStaleSeconds.TextAlign = System.Windows.Forms.HorizontalAlignment.Right;
            this.numericStaleSeconds.Value = new decimal(new int[] {
            1,
            0,
            0,
            0});
            // 
            // label6
            // 
            this.label6.AutoSize = true;
            this.label6.Location = new System.Drawing.Point(0, 180);
            this.label6.Name = "label6";
            this.label6.Size = new System.Drawing.Size(41, 13);
            this.label6.TabIndex = 13;
            this.label6.Text = "::Port:::";
            // 
            // numericPort
            // 
            this.numericPort.Location = new System.Drawing.Point(200, 178);
            this.numericPort.Maximum = new decimal(new int[] {
            65535,
            0,
            0,
            0});
            this.numericPort.Name = "numericPort";
            this.numericPort.Size = new System.Drawing.Size(77, 20);
            this.numericPort.TabIndex = 14;
            this.numericPort.TextAlign = System.Windows.Forms.HorizontalAlignment.Right;
            // 
            // label1
            // 
            this.label1.AutoSize = true;
            this.label1.Location = new System.Drawing.Point(0, 79);
            this.label1.Name = "label1";
            this.label1.Size = new System.Drawing.Size(54, 13);
            this.label1.TabIndex = 5;
            this.label1.Text = "::Format:::";
            // 
            // comboBoxFormat
            // 
            this.comboBoxFormat.DropDownStyle = System.Windows.Forms.ComboBoxStyle.DropDownList;
            this.comboBoxFormat.FormattingEnabled = true;
            this.comboBoxFormat.Location = new System.Drawing.Point(200, 76);
            this.comboBoxFormat.Name = "comboBoxFormat";
            this.comboBoxFormat.Size = new System.Drawing.Size(150, 21);
            this.comboBoxFormat.TabIndex = 6;
            // 
            // label4
            // 
            this.label4.AutoSize = true;
            this.label4.Location = new System.Drawing.Point(0, 52);
            this.label4.Name = "label4";
            this.label4.Size = new System.Drawing.Size(65, 13);
            this.label4.TabIndex = 3;
            this.label4.Text = "::Receiver:::";
            // 
            // comboBoxReceiver
            // 
            this.comboBoxReceiver.DropDownStyle = System.Windows.Forms.ComboBoxStyle.DropDownList;
            this.comboBoxReceiver.FormattingEnabled = true;
            this.comboBoxReceiver.Location = new System.Drawing.Point(200, 49);
            this.comboBoxReceiver.Name = "comboBoxReceiver";
            this.comboBoxReceiver.Size = new System.Drawing.Size(150, 21);
            this.comboBoxReceiver.TabIndex = 4;
            // 
            // checkBoxEnabled
            // 
            this.checkBoxEnabled.AutoSize = true;
            this.checkBoxEnabled.Location = new System.Drawing.Point(200, 0);
            this.checkBoxEnabled.Name = "checkBoxEnabled";
            this.checkBoxEnabled.Size = new System.Drawing.Size(77, 17);
            this.checkBoxEnabled.TabIndex = 0;
            this.checkBoxEnabled.Text = "::Enabled::";
            this.checkBoxEnabled.UseVisualStyleBackColor = true;
            // 
            // label2
            // 
            this.label2.AutoSize = true;
            this.label2.Location = new System.Drawing.Point(0, 26);
            this.label2.Name = "label2";
            this.label2.Size = new System.Drawing.Size(50, 13);
            this.label2.TabIndex = 1;
            this.label2.Text = "::Name:::";
            // 
            // textBoxName
            // 
            this.textBoxName.Anchor = ((System.Windows.Forms.AnchorStyles)(((System.Windows.Forms.AnchorStyles.Top | System.Windows.Forms.AnchorStyles.Left) 
            | System.Windows.Forms.AnchorStyles.Right)));
            this.textBoxName.Location = new System.Drawing.Point(200, 23);
            this.textBoxName.Name = "textBoxName";
            this.textBoxName.Size = new System.Drawing.Size(436, 20);
            this.textBoxName.TabIndex = 2;
            // 
            // groupBoxAccessControl
            // 
            this.groupBoxAccessControl.Anchor = ((System.Windows.Forms.AnchorStyles)((((System.Windows.Forms.AnchorStyles.Top | System.Windows.Forms.AnchorStyles.Bottom) 
            | System.Windows.Forms.AnchorStyles.Left) 
            | System.Windows.Forms.AnchorStyles.Right)));
            this.groupBoxAccessControl.Controls.Add(this.accessControl);
            this.groupBoxAccessControl.Location = new System.Drawing.Point(0, 305);
            this.groupBoxAccessControl.Name = "groupBoxAccessControl";
            this.groupBoxAccessControl.Size = new System.Drawing.Size(636, 192);
            this.groupBoxAccessControl.TabIndex = 24;
            this.groupBoxAccessControl.TabStop = false;
            this.groupBoxAccessControl.Text = "::AccessControl::";
            // 
            // accessControl
            // 
            this.accessControl.Anchor = ((System.Windows.Forms.AnchorStyles)((((System.Windows.Forms.AnchorStyles.Top | System.Windows.Forms.AnchorStyles.Bottom) 
            | System.Windows.Forms.AnchorStyles.Left) 
            | System.Windows.Forms.AnchorStyles.Right)));
            this.accessControl.Location = new System.Drawing.Point(6, 19);
            this.accessControl.Name = "accessControl";
            this.accessControl.Size = new System.Drawing.Size(624, 167);
            this.accessControl.TabIndex = 0;
            // 
            // checkBoxIsTransmitter
            // 
            this.checkBoxIsTransmitter.AutoSize = true;
            this.checkBoxIsTransmitter.Location = new System.Drawing.Point(200, 129);
            this.checkBoxIsTransmitter.Name = "checkBoxIsTransmitter";
            this.checkBoxIsTransmitter.Size = new System.Drawing.Size(102, 17);
            this.checkBoxIsTransmitter.TabIndex = 10;
            this.checkBoxIsTransmitter.Text = "::TransmitFeed::";
            this.checkBoxIsTransmitter.UseVisualStyleBackColor = true;
            // 
            // label7
            // 
            this.label7.AutoSize = true;
            this.label7.Location = new System.Drawing.Point(0, 155);
            this.label7.Name = "label7";
            this.label7.Size = new System.Drawing.Size(45, 13);
            this.label7.TabIndex = 11;
            this.label7.Text = "::UNC:::";
            // 
            // textBoxTransmitAddress
            // 
            this.textBoxTransmitAddress.Location = new System.Drawing.Point(200, 152);
            this.textBoxTransmitAddress.Name = "textBoxTransmitAddress";
            this.textBoxTransmitAddress.Size = new System.Drawing.Size(150, 20);
            this.textBoxTransmitAddress.TabIndex = 12;
            // 
            // label16
            // 
            this.label16.AutoSize = true;
            this.label16.Location = new System.Drawing.Point(283, 281);
            this.label16.Name = "label16";
            this.label16.Size = new System.Drawing.Size(68, 13);
            this.label16.TabIndex = 23;
            this.label16.Text = "::PSeconds::";
            // 
            // label15
            // 
            this.label15.AutoSize = true;
            this.label15.Location = new System.Drawing.Point(0, 281);
            this.label15.Name = "label15";
            this.label15.Size = new System.Drawing.Size(77, 13);
            this.label15.TabIndex = 21;
            this.label15.Text = "::IdleTimeout:::";
            // 
            // numericIdleTimeout
            // 
            this.numericIdleTimeout.Location = new System.Drawing.Point(200, 279);
            this.numericIdleTimeout.Maximum = new decimal(new int[] {
            86400,
            0,
            0,
            0});
            this.numericIdleTimeout.Minimum = new decimal(new int[] {
            5,
            0,
            0,
            0});
            this.numericIdleTimeout.Name = "numericIdleTimeout";
            this.numericIdleTimeout.Size = new System.Drawing.Size(77, 20);
            this.numericIdleTimeout.TabIndex = 22;
            this.numericIdleTimeout.TextAlign = System.Windows.Forms.HorizontalAlignment.Right;
            this.numericIdleTimeout.Value = new decimal(new int[] {
            5,
            0,
            0,
            0});
            // 
            // checkBoxUseKeepAlive
            // 
            this.checkBoxUseKeepAlive.AutoSize = true;
            this.checkBoxUseKeepAlive.Location = new System.Drawing.Point(200, 256);
            this.checkBoxUseKeepAlive.Name = "checkBoxUseKeepAlive";
            this.checkBoxUseKeepAlive.Size = new System.Drawing.Size(105, 17);
            this.checkBoxUseKeepAlive.TabIndex = 20;
            this.checkBoxUseKeepAlive.Text = "::UseKeepAlive::";
            this.checkBoxUseKeepAlive.UseVisualStyleBackColor = true;
            // 
            // label8
            // 
            this.label8.AutoSize = true;
            this.label8.Location = new System.Drawing.Point(283, 232);
            this.label8.Name = "label8";
            this.label8.Size = new System.Drawing.Size(68, 13);
            this.label8.TabIndex = 19;
            this.label8.Text = "::PSeconds::";
            // 
            // label9
            // 
            this.label9.AutoSize = true;
            this.label9.Location = new System.Drawing.Point(0, 207);
            this.label9.Name = "label9";
            this.label9.Size = new System.Drawing.Size(77, 13);
            this.label9.TabIndex = 15;
            this.label9.Text = "::Passphrase:::";
            // 
            // textBoxPassphrase
            // 
            this.textBoxPassphrase.Anchor = ((System.Windows.Forms.AnchorStyles)(((System.Windows.Forms.AnchorStyles.Top | System.Windows.Forms.AnchorStyles.Left) 
            | System.Windows.Forms.AnchorStyles.Right)));
            this.textBoxPassphrase.Location = new System.Drawing.Point(200, 204);
            this.textBoxPassphrase.MaxLength = 512;
            this.textBoxPassphrase.Name = "textBoxPassphrase";
            this.textBoxPassphrase.Size = new System.Drawing.Size(436, 20);
            this.textBoxPassphrase.TabIndex = 16;
            // 
            // label5
            // 
            this.label5.AutoSize = true;
            this.label5.Location = new System.Drawing.Point(0, 105);
            this.label5.Name = "label5";
            this.label5.Size = new System.Drawing.Size(71, 13);
            this.label5.TabIndex = 7;
            this.label5.Text = "::SendEvery::";
            // 
            // numericSendInterval
            // 
            this.numericSendInterval.DecimalPlaces = 1;
            this.numericSendInterval.Location = new System.Drawing.Point(200, 103);
            this.numericSendInterval.Maximum = new decimal(new int[] {
            30,
            0,
            0,
            0});
            this.numericSendInterval.Minimum = new decimal(new int[] {
            1,
            0,
            0,
            0});
            this.numericSendInterval.Name = "numericSendInterval";
            this.numericSendInterval.Size = new System.Drawing.Size(77, 20);
            this.numericSendInterval.TabIndex = 8;
            this.numericSendInterval.TextAlign = System.Windows.Forms.HorizontalAlignment.Right;
            this.numericSendInterval.Value = new decimal(new int[] {
            1,
            0,
            0,
            0});
            // 
            // label10
            // 
            this.label10.AutoSize = true;
            this.label10.Location = new System.Drawing.Point(283, 105);
            this.label10.Name = "label10";
            this.label10.Size = new System.Drawing.Size(68, 13);
            this.label10.TabIndex = 9;
            this.label10.Text = "::PSeconds::";
            // 
            // PageRebroadcastServer
            // 
            this.AutoScaleDimensions = new System.Drawing.SizeF(6F, 13F);
            this.Controls.Add(this.label10);
            this.Controls.Add(this.label5);
            this.Controls.Add(this.numericSendInterval);
            this.Controls.Add(this.label9);
            this.Controls.Add(this.textBoxPassphrase);
            this.Controls.Add(this.label8);
            this.Controls.Add(this.label16);
            this.Controls.Add(this.label15);
            this.Controls.Add(this.numericIdleTimeout);
            this.Controls.Add(this.checkBoxUseKeepAlive);
            this.Controls.Add(this.label7);
            this.Controls.Add(this.textBoxTransmitAddress);
            this.Controls.Add(this.checkBoxIsTransmitter);
            this.Controls.Add(this.groupBoxAccessControl);
            this.Controls.Add(this.label3);
            this.Controls.Add(this.numericStaleSeconds);
            this.Controls.Add(this.label6);
            this.Controls.Add(this.numericPort);
            this.Controls.Add(this.label1);
            this.Controls.Add(this.comboBoxFormat);
            this.Controls.Add(this.label4);
            this.Controls.Add(this.comboBoxReceiver);
            this.Controls.Add(this.checkBoxEnabled);
            this.Controls.Add(this.label2);
            this.Controls.Add(this.textBoxName);
            this.Name = "PageRebroadcastServer";
            this.Size = new System.Drawing.Size(636, 499);
            ((System.ComponentModel.ISupportInitialize)(this.numericStaleSeconds)).EndInit();
            ((System.ComponentModel.ISupportInitialize)(this.numericPort)).EndInit();
            this.groupBoxAccessControl.ResumeLayout(false);
            ((System.ComponentModel.ISupportInitialize)(this.numericIdleTimeout)).EndInit();
            ((System.ComponentModel.ISupportInitialize)(this.numericSendInterval)).EndInit();
            this.ResumeLayout(false);
            this.PerformLayout();

        }

        #endregion

        private System.Windows.Forms.Label label3;
        private System.Windows.Forms.NumericUpDown numericStaleSeconds;
        private System.Windows.Forms.Label label6;
        private System.Windows.Forms.NumericUpDown numericPort;
        private System.Windows.Forms.Label label1;
        private System.Windows.Forms.ComboBox comboBoxFormat;
        private System.Windows.Forms.Label label4;
        private System.Windows.Forms.ComboBox comboBoxReceiver;
        private System.Windows.Forms.CheckBox checkBoxEnabled;
        private System.Windows.Forms.Label label2;
        private System.Windows.Forms.TextBox textBoxName;
        private System.Windows.Forms.GroupBox groupBoxAccessControl;
        private System.Windows.Forms.CheckBox checkBoxIsTransmitter;
        private System.Windows.Forms.Label label7;
        private System.Windows.Forms.TextBox textBoxTransmitAddress;
        private System.Windows.Forms.Label label16;
        private System.Windows.Forms.Label label15;
        private System.Windows.Forms.NumericUpDown numericIdleTimeout;
        private System.Windows.Forms.CheckBox checkBoxUseKeepAlive;
        private System.Windows.Forms.Label label8;
        private System.Windows.Forms.Label label9;
        private System.Windows.Forms.TextBox textBoxPassphrase;
        private Controls.AccessControl accessControl;
        private System.Windows.Forms.Label label5;
        private System.Windows.Forms.NumericUpDown numericSendInterval;
        private System.Windows.Forms.Label label10;
    }
}
