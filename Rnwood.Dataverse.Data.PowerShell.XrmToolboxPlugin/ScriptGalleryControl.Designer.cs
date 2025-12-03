using System;
using System.Collections.Generic;
using System.ComponentModel;
using System.Drawing;
using System.Linq;
using System.Threading.Tasks;
using System.Windows.Forms;

namespace Rnwood.Dataverse.Data.PowerShell.XrmToolboxPlugin
{
    partial class ScriptGalleryControl
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

        #region Component Designer generated code

        /// <summary>
        /// Required method for Designer support - do not modify
        /// the contents of this method with the code editor.
        /// </summary>
        private void InitializeComponent()
        {
            this.topPanel = new System.Windows.Forms.Panel();
            this.signInButton = new System.Windows.Forms.Button();
            this.userLabel = new System.Windows.Forms.Label();
            this.manageGistsLink = new System.Windows.Forms.LinkLabel();
            this.refreshButton = new System.Windows.Forms.Button();
            this.openButton = new System.Windows.Forms.Button();
            this.myScriptsCheckBox = new System.Windows.Forms.CheckBox();
            this.searchTextBox = new System.Windows.Forms.TextBox();
            this.searchLabel = new System.Windows.Forms.Label();
            this.statusLabel = new System.Windows.Forms.Label();
            this.splitContainer = new System.Windows.Forms.SplitContainer();
            this.scriptListView = new System.Windows.Forms.ListView();
            this.columnTitle = new System.Windows.Forms.ColumnHeader();
            this.columnAuthor = new System.Windows.Forms.ColumnHeader();
            this.columnUpdated = new System.Windows.Forms.ColumnHeader();
            this.descriptionPanel = new System.Windows.Forms.Panel();
            this.descriptionTextBox = new System.Windows.Forms.TextBox();
            this.descriptionLabel = new System.Windows.Forms.Label();
            
            this.topPanel.SuspendLayout();
            ((System.ComponentModel.ISupportInitialize)(this.splitContainer)).BeginInit();
            this.splitContainer.Panel1.SuspendLayout();
            this.splitContainer.Panel2.SuspendLayout();
            this.splitContainer.SuspendLayout();
            this.descriptionPanel.SuspendLayout();
            this.SuspendLayout();
            
            // 
            // topPanel
            // 
            this.topPanel.Controls.Add(this.signInButton);
            this.topPanel.Controls.Add(this.userLabel);
            this.topPanel.Controls.Add(this.manageGistsLink);
            this.topPanel.Controls.Add(this.refreshButton);
            this.topPanel.Controls.Add(this.openButton);
            this.topPanel.Controls.Add(this.myScriptsCheckBox);
            this.topPanel.Controls.Add(this.searchLabel);
            this.topPanel.Controls.Add(this.searchTextBox);
            this.topPanel.Controls.Add(this.statusLabel);
            this.topPanel.Dock = System.Windows.Forms.DockStyle.Top;
            this.topPanel.Location = new System.Drawing.Point(0, 0);
            this.topPanel.Name = "topPanel";
            this.topPanel.Padding = new System.Windows.Forms.Padding(5);
            this.topPanel.Size = new System.Drawing.Size(516, 100);
            this.topPanel.TabIndex = 0;
            
            // 
            // signInButton
            // 
            this.signInButton.Location = new System.Drawing.Point(8, 12);
            this.signInButton.Name = "signInButton";
            this.signInButton.Size = new System.Drawing.Size(80, 30);
            this.signInButton.TabIndex = 0;
            this.signInButton.Text = "Sign In";
            this.signInButton.UseVisualStyleBackColor = true;
            this.signInButton.Click += new System.EventHandler(this.SignInButton_Click);
            
            // 
            // userLabel
            // 
            this.userLabel.AutoSize = true;
            this.userLabel.Location = new System.Drawing.Point(94, 20);
            this.userLabel.Name = "userLabel";
            this.userLabel.Size = new System.Drawing.Size(120, 13);
            this.userLabel.TabIndex = 1;
            this.userLabel.Text = "Signed in as: username";
            this.userLabel.Visible = false;
            
            // 
            // manageGistsLink
            // 
            this.manageGistsLink.AutoSize = true;
            this.manageGistsLink.Location = new System.Drawing.Point(220, 20);
            this.manageGistsLink.Name = "manageGistsLink";
            this.manageGistsLink.Size = new System.Drawing.Size(100, 13);
            this.manageGistsLink.TabIndex = 2;
            this.manageGistsLink.TabStop = true;
            this.manageGistsLink.Text = "Manage on GitHub";
            this.manageGistsLink.Visible = false;
            this.manageGistsLink.LinkClicked += new System.Windows.Forms.LinkLabelLinkClickedEventHandler(this.ManageGistsLink_LinkClicked);
            
            // 
            // refreshButton
            // 
            this.refreshButton.Location = new System.Drawing.Point(8, 48);
            this.refreshButton.Name = "refreshButton";
            this.refreshButton.Size = new System.Drawing.Size(80, 30);
            this.refreshButton.TabIndex = 3;
            this.refreshButton.Text = "Refresh";
            this.refreshButton.UseVisualStyleBackColor = true;
            this.refreshButton.Click += new System.EventHandler(this.RefreshButton_Click);
            
            // 
            // openButton
            // 
            this.openButton.Location = new System.Drawing.Point(94, 48);
            this.openButton.Name = "openButton";
            this.openButton.Size = new System.Drawing.Size(80, 30);
            this.openButton.TabIndex = 4;
            this.openButton.Text = "Open";
            this.openButton.UseVisualStyleBackColor = true;
            this.openButton.Enabled = false;
            this.openButton.Click += new System.EventHandler(this.OpenButton_Click);
            
            // 
            // myScriptsCheckBox
            // 
            this.myScriptsCheckBox.AutoSize = true;
            this.myScriptsCheckBox.Location = new System.Drawing.Point(180, 55);
            this.myScriptsCheckBox.Name = "myScriptsCheckBox";
            this.myScriptsCheckBox.Size = new System.Drawing.Size(90, 17);
            this.myScriptsCheckBox.TabIndex = 5;
            this.myScriptsCheckBox.Text = "My Scripts";
            this.myScriptsCheckBox.UseVisualStyleBackColor = true;
            this.myScriptsCheckBox.Enabled = false;
            this.myScriptsCheckBox.CheckedChanged += new System.EventHandler(this.MyScriptsCheckBox_CheckedChanged);
            
            // 
            // searchLabel
            // 
            this.searchLabel.AutoSize = true;
            this.searchLabel.Location = new System.Drawing.Point(280, 56);
            this.searchLabel.Name = "searchLabel";
            this.searchLabel.Size = new System.Drawing.Size(44, 13);
            this.searchLabel.TabIndex = 6;
            this.searchLabel.Text = "Search:";
            
            // 
            // searchTextBox
            // 
            this.searchTextBox.Location = new System.Drawing.Point(328, 53);
            this.searchTextBox.Name = "searchTextBox";
            this.searchTextBox.Size = new System.Drawing.Size(180, 20);
            this.searchTextBox.TabIndex = 7;
            this.searchTextBox.TextChanged += new System.EventHandler(this.SearchTextBox_TextChanged);
            
            // 
            // statusLabel
            // 
            this.statusLabel.AutoSize = true;
            this.statusLabel.Location = new System.Drawing.Point(8, 82);
            this.statusLabel.Name = "statusLabel";
            this.statusLabel.Size = new System.Drawing.Size(70, 13);
            this.statusLabel.TabIndex = 8;
            this.statusLabel.Text = "Loading...";
            
            // 
            // splitContainer
            // 
            this.splitContainer.Dock = System.Windows.Forms.DockStyle.Fill;
            this.splitContainer.Location = new System.Drawing.Point(0, 100);
            this.splitContainer.Name = "splitContainer";
            this.splitContainer.Orientation = System.Windows.Forms.Orientation.Horizontal;
            // 
            // splitContainer.Panel1
            // 
            this.splitContainer.Panel1.Controls.Add(this.scriptListView);
            // 
            // splitContainer.Panel2
            // 
            this.splitContainer.Panel2.Controls.Add(this.descriptionPanel);
            this.splitContainer.Size = new System.Drawing.Size(516, 944);
            this.splitContainer.SplitterDistance = 600;
            this.splitContainer.TabIndex = 1;
            
            // 
            // scriptListView
            // 
            this.scriptListView.Columns.AddRange(new System.Windows.Forms.ColumnHeader[] {
                this.columnTitle,
                this.columnAuthor,
                this.columnUpdated});
            this.scriptListView.Dock = System.Windows.Forms.DockStyle.Fill;
            this.scriptListView.FullRowSelect = true;
            this.scriptListView.GridLines = true;
            this.scriptListView.HideSelection = false;
            this.scriptListView.Location = new System.Drawing.Point(0, 0);
            this.scriptListView.MultiSelect = false;
            this.scriptListView.Name = "scriptListView";
            this.scriptListView.Size = new System.Drawing.Size(516, 650);
            this.scriptListView.TabIndex = 0;
            this.scriptListView.UseCompatibleStateImageBehavior = false;
            this.scriptListView.View = System.Windows.Forms.View.Details;
            this.scriptListView.SelectedIndexChanged += new System.EventHandler(this.ScriptListView_SelectedIndexChanged);
            this.scriptListView.DoubleClick += new System.EventHandler(this.ScriptListView_DoubleClick);
            
            // 
            // columnTitle
            // 
            this.columnTitle.Text = "Script Title";
            this.columnTitle.Width = 250;
            
            // 
            // columnAuthor
            // 
            this.columnAuthor.Text = "Author";
            this.columnAuthor.Width = 120;
            
            // 
            // columnUpdated
            // 
            this.columnUpdated.Text = "Updated";
            this.columnUpdated.Width = 130;
            
            // 
            // descriptionPanel
            // 
            this.descriptionPanel.Controls.Add(this.descriptionTextBox);
            this.descriptionPanel.Controls.Add(this.descriptionLabel);
            this.descriptionPanel.Dock = System.Windows.Forms.DockStyle.Fill;
            this.descriptionPanel.Location = new System.Drawing.Point(0, 0);
            this.descriptionPanel.Name = "descriptionPanel";
            this.descriptionPanel.Size = new System.Drawing.Size(516, 340);
            this.descriptionPanel.TabIndex = 0;
            
            // 
            // descriptionLabel
            // 
            this.descriptionLabel.AutoSize = true;
            this.descriptionLabel.Dock = System.Windows.Forms.DockStyle.Top;
            this.descriptionLabel.Font = new System.Drawing.Font("Microsoft Sans Serif", 9F, System.Drawing.FontStyle.Bold);
            this.descriptionLabel.Location = new System.Drawing.Point(0, 0);
            this.descriptionLabel.Name = "descriptionLabel";
            this.descriptionLabel.Padding = new System.Windows.Forms.Padding(5);
            this.descriptionLabel.Size = new System.Drawing.Size(75, 25);
            this.descriptionLabel.TabIndex = 0;
            this.descriptionLabel.Text = "Details:";
            
            // 
            // descriptionTextBox
            // 
            this.descriptionTextBox.Dock = System.Windows.Forms.DockStyle.Fill;
            this.descriptionTextBox.Location = new System.Drawing.Point(0, 25);
            this.descriptionTextBox.Multiline = true;
            this.descriptionTextBox.Name = "descriptionTextBox";
            this.descriptionTextBox.ReadOnly = true;
            this.descriptionTextBox.ScrollBars = System.Windows.Forms.ScrollBars.Vertical;
            this.descriptionTextBox.Size = new System.Drawing.Size(516, 315);
            this.descriptionTextBox.TabIndex = 1;
            
            // 
            // ScriptGalleryControl
            // 
            this.Controls.Add(this.splitContainer);
            this.Controls.Add(this.topPanel);
            this.Name = "ScriptGalleryControl";
            this.Size = new System.Drawing.Size(516, 1044);
            this.BackColor = System.Drawing.SystemColors.Control;
            
            this.topPanel.ResumeLayout(false);
            this.topPanel.PerformLayout();
            this.splitContainer.Panel1.ResumeLayout(false);
            this.splitContainer.Panel2.ResumeLayout(false);
            ((System.ComponentModel.ISupportInitialize)(this.splitContainer)).EndInit();
            this.splitContainer.ResumeLayout(false);
            this.descriptionPanel.ResumeLayout(false);
            this.descriptionPanel.PerformLayout();
            this.ResumeLayout(false);
        }

        #endregion

        private Panel topPanel;
        private Button signInButton;
        private Label userLabel;
        private LinkLabel manageGistsLink;
        private Button refreshButton;
        private Button openButton;
        private CheckBox myScriptsCheckBox;
        private Label searchLabel;
        private TextBox searchTextBox;
        private Label statusLabel;
        private SplitContainer splitContainer;
        private ListView scriptListView;
        private ColumnHeader columnTitle;
        private ColumnHeader columnAuthor;
        private ColumnHeader columnUpdated;
        private Panel descriptionPanel;
        private Label descriptionLabel;
        private TextBox descriptionTextBox;
    }
}