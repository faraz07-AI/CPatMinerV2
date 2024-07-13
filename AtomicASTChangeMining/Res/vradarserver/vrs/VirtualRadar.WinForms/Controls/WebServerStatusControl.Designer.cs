﻿namespace VirtualRadar.WinForms.Controls
{
    partial class WebServerStatusControl
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
            this.components = new System.ComponentModel.Container();
            this.groupBox1 = new System.Windows.Forms.GroupBox();
            this.checkBoxOfflineMode = new System.Windows.Forms.CheckBox();
            this.buttonToggleUPnpStatus = new System.Windows.Forms.Button();
            this.labelUPnpStatus = new System.Windows.Forms.Label();
            this.comboBoxShowAddressType = new System.Windows.Forms.ComboBox();
            this.comboBoxSite = new System.Windows.Forms.ComboBox();
            this.webServerUserList = new VirtualRadar.WinForms.Controls.WebServerUserListControl();
            this.buttonToggleServerStatus = new System.Windows.Forms.Button();
            this.labelServerStatus = new System.Windows.Forms.Label();
            this.linkLabelAddress = new System.Windows.Forms.LinkLabel();
            this.contextMenuStripWebSiteLink = new System.Windows.Forms.ContextMenuStrip(this.components);
            this.openInBrowserToolStripMenuItem = new System.Windows.Forms.ToolStripMenuItem();
            this.copyLinkToClipboardToolStripMenuItem = new System.Windows.Forms.ToolStripMenuItem();
            this.groupBox1.SuspendLayout();
            this.contextMenuStripWebSiteLink.SuspendLayout();
            this.SuspendLayout();
            // 
            // groupBox1
            // 
            this.groupBox1.Controls.Add(this.checkBoxOfflineMode);
            this.groupBox1.Controls.Add(this.buttonToggleUPnpStatus);
            this.groupBox1.Controls.Add(this.labelUPnpStatus);
            this.groupBox1.Controls.Add(this.comboBoxShowAddressType);
            this.groupBox1.Controls.Add(this.comboBoxSite);
            this.groupBox1.Controls.Add(this.webServerUserList);
            this.groupBox1.Controls.Add(this.buttonToggleServerStatus);
            this.groupBox1.Controls.Add(this.labelServerStatus);
            this.groupBox1.Controls.Add(this.linkLabelAddress);
            this.groupBox1.Dock = System.Windows.Forms.DockStyle.Fill;
            this.groupBox1.Location = new System.Drawing.Point(0, 0);
            this.groupBox1.Name = "groupBox1";
            this.groupBox1.Size = new System.Drawing.Size(658, 385);
            this.groupBox1.TabIndex = 0;
            this.groupBox1.TabStop = false;
            this.groupBox1.Text = "::WebServerStatus::";
            // 
            // checkBoxOfflineMode
            // 
            this.checkBoxOfflineMode.Anchor = ((System.Windows.Forms.AnchorStyles)((System.Windows.Forms.AnchorStyles.Bottom | System.Windows.Forms.AnchorStyles.Left)));
            this.checkBoxOfflineMode.AutoSize = true;
            this.checkBoxOfflineMode.Location = new System.Drawing.Point(352, 340);
            this.checkBoxOfflineMode.Name = "checkBoxOfflineMode";
            this.checkBoxOfflineMode.Size = new System.Drawing.Size(95, 17);
            this.checkBoxOfflineMode.TabIndex = 7;
            this.checkBoxOfflineMode.Text = "::OfflineMode::";
            this.checkBoxOfflineMode.UseVisualStyleBackColor = true;
            this.checkBoxOfflineMode.CheckedChanged += new System.EventHandler(this.checkBoxOfflineMode_CheckedChanged);
            // 
            // buttonToggleUPnpStatus
            // 
            this.buttonToggleUPnpStatus.Anchor = ((System.Windows.Forms.AnchorStyles)((System.Windows.Forms.AnchorStyles.Top | System.Windows.Forms.AnchorStyles.Right)));
            this.buttonToggleUPnpStatus.Location = new System.Drawing.Point(506, 44);
            this.buttonToggleUPnpStatus.Name = "buttonToggleUPnpStatus";
            this.buttonToggleUPnpStatus.Size = new System.Drawing.Size(145, 23);
            this.buttonToggleUPnpStatus.TabIndex = 3;
            this.buttonToggleUPnpStatus.Text = "Toggle Port Mapping";
            this.buttonToggleUPnpStatus.UseVisualStyleBackColor = true;
            this.buttonToggleUPnpStatus.Click += new System.EventHandler(this.buttonToggleUPnpStatus_Click);
            // 
            // labelUPnpStatus
            // 
            this.labelUPnpStatus.AutoSize = true;
            this.labelUPnpStatus.Location = new System.Drawing.Point(4, 49);
            this.labelUPnpStatus.Name = "labelUPnpStatus";
            this.labelUPnpStatus.Size = new System.Drawing.Size(220, 13);
            this.labelUPnpStatus.TabIndex = 2;
            this.labelUPnpStatus.Text = "UPnP port mapping status will be shown here";
            // 
            // comboBoxShowAddressType
            // 
            this.comboBoxShowAddressType.Anchor = ((System.Windows.Forms.AnchorStyles)((System.Windows.Forms.AnchorStyles.Bottom | System.Windows.Forms.AnchorStyles.Left)));
            this.comboBoxShowAddressType.DropDownStyle = System.Windows.Forms.ComboBoxStyle.DropDownList;
            this.comboBoxShowAddressType.FormattingEnabled = true;
            this.comboBoxShowAddressType.Location = new System.Drawing.Point(7, 338);
            this.comboBoxShowAddressType.Name = "comboBoxShowAddressType";
            this.comboBoxShowAddressType.Size = new System.Drawing.Size(193, 21);
            this.comboBoxShowAddressType.TabIndex = 5;
            this.comboBoxShowAddressType.SelectedIndexChanged += new System.EventHandler(this.comboBoxShowAddressType_SelectedIndexChanged);
            // 
            // comboBoxSite
            // 
            this.comboBoxSite.Anchor = ((System.Windows.Forms.AnchorStyles)((System.Windows.Forms.AnchorStyles.Bottom | System.Windows.Forms.AnchorStyles.Left)));
            this.comboBoxSite.DropDownStyle = System.Windows.Forms.ComboBoxStyle.DropDownList;
            this.comboBoxSite.FormattingEnabled = true;
            this.comboBoxSite.Location = new System.Drawing.Point(206, 338);
            this.comboBoxSite.Name = "comboBoxSite";
            this.comboBoxSite.Size = new System.Drawing.Size(140, 21);
            this.comboBoxSite.TabIndex = 6;
            this.comboBoxSite.SelectedIndexChanged += new System.EventHandler(this.comboBoxSite_SelectedIndexChanged);
            // 
            // webServerUserList
            // 
            this.webServerUserList.Anchor = ((System.Windows.Forms.AnchorStyles)((((System.Windows.Forms.AnchorStyles.Top | System.Windows.Forms.AnchorStyles.Bottom) 
            | System.Windows.Forms.AnchorStyles.Left) 
            | System.Windows.Forms.AnchorStyles.Right)));
            this.webServerUserList.Location = new System.Drawing.Point(7, 73);
            this.webServerUserList.Name = "webServerUserList";
            this.webServerUserList.Size = new System.Drawing.Size(644, 259);
            this.webServerUserList.TabIndex = 4;
            // 
            // buttonToggleServerStatus
            // 
            this.buttonToggleServerStatus.Anchor = ((System.Windows.Forms.AnchorStyles)((System.Windows.Forms.AnchorStyles.Top | System.Windows.Forms.AnchorStyles.Right)));
            this.buttonToggleServerStatus.Location = new System.Drawing.Point(506, 15);
            this.buttonToggleServerStatus.Name = "buttonToggleServerStatus";
            this.buttonToggleServerStatus.Size = new System.Drawing.Size(145, 23);
            this.buttonToggleServerStatus.TabIndex = 1;
            this.buttonToggleServerStatus.Text = "Toggle Server";
            this.buttonToggleServerStatus.UseVisualStyleBackColor = true;
            this.buttonToggleServerStatus.Click += new System.EventHandler(this.buttonToggleServerStatus_Click);
            // 
            // labelServerStatus
            // 
            this.labelServerStatus.AutoSize = true;
            this.labelServerStatus.Location = new System.Drawing.Point(4, 20);
            this.labelServerStatus.Name = "labelServerStatus";
            this.labelServerStatus.Size = new System.Drawing.Size(183, 13);
            this.labelServerStatus.TabIndex = 0;
            this.labelServerStatus.Text = "Web server status will be shown here";
            // 
            // linkLabelAddress
            // 
            this.linkLabelAddress.Anchor = ((System.Windows.Forms.AnchorStyles)(((System.Windows.Forms.AnchorStyles.Bottom | System.Windows.Forms.AnchorStyles.Left) 
            | System.Windows.Forms.AnchorStyles.Right)));
            this.linkLabelAddress.AutoEllipsis = true;
            this.linkLabelAddress.Location = new System.Drawing.Point(6, 362);
            this.linkLabelAddress.Name = "linkLabelAddress";
            this.linkLabelAddress.Size = new System.Drawing.Size(645, 20);
            this.linkLabelAddress.TabIndex = 8;
            this.linkLabelAddress.TabStop = true;
            this.linkLabelAddress.Text = "http://localhost/Whatever.html";
            this.linkLabelAddress.TextAlign = System.Drawing.ContentAlignment.MiddleLeft;
            this.linkLabelAddress.LinkClicked += new System.Windows.Forms.LinkLabelLinkClickedEventHandler(this.linkLabelAddress_LinkClicked);
            // 
            // contextMenuStripWebSiteLink
            // 
            this.contextMenuStripWebSiteLink.Items.AddRange(new System.Windows.Forms.ToolStripItem[] {
            this.openInBrowserToolStripMenuItem,
            this.copyLinkToClipboardToolStripMenuItem});
            this.contextMenuStripWebSiteLink.Name = "contextMenuStripWebSiteLink";
            this.contextMenuStripWebSiteLink.Size = new System.Drawing.Size(197, 48);
            // 
            // openInBrowserToolStripMenuItem
            // 
            this.openInBrowserToolStripMenuItem.Font = new System.Drawing.Font("Segoe UI", 9F, System.Drawing.FontStyle.Bold);
            this.openInBrowserToolStripMenuItem.Name = "openInBrowserToolStripMenuItem";
            this.openInBrowserToolStripMenuItem.Size = new System.Drawing.Size(196, 22);
            this.openInBrowserToolStripMenuItem.Text = "&Open in Browser";
            this.openInBrowserToolStripMenuItem.Click += new System.EventHandler(this.openInBrowserToolStripMenuItem_Click);
            // 
            // copyLinkToClipboardToolStripMenuItem
            // 
            this.copyLinkToClipboardToolStripMenuItem.Name = "copyLinkToClipboardToolStripMenuItem";
            this.copyLinkToClipboardToolStripMenuItem.Size = new System.Drawing.Size(196, 22);
            this.copyLinkToClipboardToolStripMenuItem.Text = "&Copy Link to Clipboard";
            this.copyLinkToClipboardToolStripMenuItem.Click += new System.EventHandler(this.copyLinkToClipboardToolStripMenuItem_Click);
            // 
            // WebServerStatusControl
            // 
            this.AutoScaleDimensions = new System.Drawing.SizeF(6F, 13F);
            this.AutoScaleMode = System.Windows.Forms.AutoScaleMode.Font;
            this.Controls.Add(this.groupBox1);
            this.Name = "WebServerStatusControl";
            this.Size = new System.Drawing.Size(658, 385);
            this.groupBox1.ResumeLayout(false);
            this.groupBox1.PerformLayout();
            this.contextMenuStripWebSiteLink.ResumeLayout(false);
            this.ResumeLayout(false);

        }

        #endregion

        private System.Windows.Forms.GroupBox groupBox1;
        private System.Windows.Forms.LinkLabel linkLabelAddress;
        private System.Windows.Forms.Label labelServerStatus;
        private System.Windows.Forms.Button buttonToggleServerStatus;
        private System.Windows.Forms.ContextMenuStrip contextMenuStripWebSiteLink;
        private System.Windows.Forms.ToolStripMenuItem openInBrowserToolStripMenuItem;
        private System.Windows.Forms.ToolStripMenuItem copyLinkToClipboardToolStripMenuItem;
        private WebServerUserListControl webServerUserList;
        private System.Windows.Forms.ComboBox comboBoxShowAddressType;
        private System.Windows.Forms.ComboBox comboBoxSite;
        private System.Windows.Forms.Button buttonToggleUPnpStatus;
        private System.Windows.Forms.Label labelUPnpStatus;
        private System.Windows.Forms.CheckBox checkBoxOfflineMode;
    }
}
