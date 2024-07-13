﻿namespace VirtualRadar.WinForms.SettingPage
{
    partial class PageMergedFeeds
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
            this.columnHeaderIcaoTimeout = ((System.Windows.Forms.ColumnHeader)(new System.Windows.Forms.ColumnHeader()));
            this.listMergedFeeds = new VirtualRadar.WinForms.Controls.MasterListView();
            this.columnHeaderName = ((System.Windows.Forms.ColumnHeader)(new System.Windows.Forms.ColumnHeader()));
            this.columnHeaderReceivers = ((System.Windows.Forms.ColumnHeader)(new System.Windows.Forms.ColumnHeader()));
            this.columnHeaderIgnoreNoPosition = ((System.Windows.Forms.ColumnHeader)(new System.Windows.Forms.ColumnHeader()));
            this.SuspendLayout();
            // 
            // columnHeaderIcaoTimeout
            // 
            this.columnHeaderIcaoTimeout.Text = "::IcaoTimeoutTitle::";
            this.columnHeaderIcaoTimeout.TextAlign = System.Windows.Forms.HorizontalAlignment.Right;
            this.columnHeaderIcaoTimeout.Width = 100;
            // 
            // listMergedFeeds
            // 
            this.listMergedFeeds.Columns.AddRange(new System.Windows.Forms.ColumnHeader[] {
            this.columnHeaderName,
            this.columnHeaderReceivers,
            this.columnHeaderIcaoTimeout,
            this.columnHeaderIgnoreNoPosition});
            this.listMergedFeeds.Dock = System.Windows.Forms.DockStyle.Fill;
            this.listMergedFeeds.Location = new System.Drawing.Point(0, 0);
            this.listMergedFeeds.Name = "listMergedFeeds";
            this.listMergedFeeds.Size = new System.Drawing.Size(636, 375);
            this.listMergedFeeds.TabIndex = 1;
            // 
            // columnHeaderName
            // 
            this.columnHeaderName.Text = "::Name::";
            this.columnHeaderName.Width = 150;
            // 
            // columnHeaderReceivers
            // 
            this.columnHeaderReceivers.Text = "::Receivers::";
            this.columnHeaderReceivers.TextAlign = System.Windows.Forms.HorizontalAlignment.Right;
            this.columnHeaderReceivers.Width = 100;
            // 
            // columnHeaderIgnoreNoPosition
            // 
            this.columnHeaderIgnoreNoPosition.Text = "::IgnoreModeS::";
            this.columnHeaderIgnoreNoPosition.Width = 100;
            // 
            // PageMergedFeeds
            // 
            this.AutoScaleDimensions = new System.Drawing.SizeF(6F, 13F);
            this.Controls.Add(this.listMergedFeeds);
            this.Name = "PageMergedFeeds";
            this.Size = new System.Drawing.Size(636, 375);
            this.ResumeLayout(false);

        }

        #endregion

        private System.Windows.Forms.ColumnHeader columnHeaderIcaoTimeout;
        private Controls.MasterListView listMergedFeeds;
        private System.Windows.Forms.ColumnHeader columnHeaderName;
        private System.Windows.Forms.ColumnHeader columnHeaderReceivers;
        private System.Windows.Forms.ColumnHeader columnHeaderIgnoreNoPosition;
    }
}
