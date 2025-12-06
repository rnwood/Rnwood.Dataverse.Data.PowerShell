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
            this.toolbar = new System.Windows.Forms.ToolStrip();
            this.loginButton = new System.Windows.Forms.ToolStripButton();
            this.refreshButton = new System.Windows.Forms.ToolStripButton();
            this.toolStripSeparator = new System.Windows.Forms.ToolStripSeparator();
            this.statusLabel = new System.Windows.Forms.ToolStripLabel();
            this.filterPanel = new System.Windows.Forms.Panel();
            this.searchTextBox = new System.Windows.Forms.TextBox();
            this.searchLabel = new System.Windows.Forms.Label();
            this.tagFilterComboBox = new System.Windows.Forms.ComboBox();
            this.tagFilterLabel = new System.Windows.Forms.Label();
            this.applyFilterButton = new System.Windows.Forms.Button();
            this.clearFilterButton = new System.Windows.Forms.Button();
            this.splitContainer = new System.Windows.Forms.SplitContainer();
            this.listView = new System.Windows.Forms.ListView();
            this.titleColumn = new System.Windows.Forms.ColumnHeader();
            this.authorColumn = new System.Windows.Forms.ColumnHeader();
            this.tagsColumn = new System.Windows.Forms.ColumnHeader();
            this.votesColumn = new System.Windows.Forms.ColumnHeader();
            this.commentsColumn = new System.Windows.Forms.ColumnHeader();
            this.dateColumn = new System.Windows.Forms.ColumnHeader();
            this.detailPanel = new System.Windows.Forms.Panel();
            this.detailToolbar = new System.Windows.Forms.ToolStrip();
            this.loadToEditorButton = new System.Windows.Forms.ToolStripButton();
            this.upvoteButton = new System.Windows.Forms.ToolStripButton();
            this.detailWebView = new Microsoft.Web.WebView2.WinForms.WebView2();
            this.commentPanel = new System.Windows.Forms.Panel();
            this.addCommentButton = new System.Windows.Forms.Button();
            this.commentTextBox = new System.Windows.Forms.TextBox();
            this.commentLabel = new System.Windows.Forms.Label();
            
            this.toolbar.SuspendLayout();
            ((System.ComponentModel.ISupportInitialize)(this.splitContainer)).BeginInit();
            this.splitContainer.Panel1.SuspendLayout();
            this.splitContainer.Panel2.SuspendLayout();
            this.splitContainer.SuspendLayout();
            this.detailPanel.SuspendLayout();
            this.detailToolbar.SuspendLayout();
            this.commentPanel.SuspendLayout();
            this.SuspendLayout();
            
            // 
            // toolbar
            // 
            this.toolbar.GripStyle = System.Windows.Forms.ToolStripGripStyle.Hidden;
            this.toolbar.Items.AddRange(new System.Windows.Forms.ToolStripItem[] {
                this.loginButton,
                this.refreshButton,
                this.toolStripSeparator,
                this.statusLabel});
            this.toolbar.Location = new System.Drawing.Point(0, 0);
            this.toolbar.Name = "toolbar";
            this.toolbar.Size = new System.Drawing.Size(516, 31);
            this.toolbar.TabIndex = 0;
            
            // 
            // loginButton
            // 
            this.loginButton.Text = "Login to GitHub";
            this.loginButton.Name = "loginButton";
            this.loginButton.Click += new System.EventHandler(this.LoginButton_Click);
            
            // 
            // refreshButton
            // 
            this.refreshButton.Text = "Refresh";
            this.refreshButton.Name = "refreshButton";
            this.refreshButton.Click += new System.EventHandler(this.RefreshButton_Click);
            
            // 
            // toolStripSeparator
            // 
            this.toolStripSeparator.Name = "toolStripSeparator";
            
            // 
            // statusLabel
            // 
            this.statusLabel.Name = "statusLabel";
            this.statusLabel.Text = "Not logged in";
            
            // 
            // filterPanel
            // 
            this.filterPanel.Controls.Add(this.searchLabel);
            this.filterPanel.Controls.Add(this.searchTextBox);
            this.filterPanel.Controls.Add(this.tagFilterLabel);
            this.filterPanel.Controls.Add(this.tagFilterComboBox);
            this.filterPanel.Controls.Add(this.applyFilterButton);
            this.filterPanel.Controls.Add(this.clearFilterButton);
            this.filterPanel.Dock = System.Windows.Forms.DockStyle.Top;
            this.filterPanel.Location = new System.Drawing.Point(0, 31);
            this.filterPanel.Name = "filterPanel";
            this.filterPanel.Size = new System.Drawing.Size(516, 60);
            this.filterPanel.TabIndex = 1;
            
            // 
            // searchLabel
            // 
            this.searchLabel.AutoSize = true;
            this.searchLabel.Location = new System.Drawing.Point(10, 12);
            this.searchLabel.Name = "searchLabel";
            this.searchLabel.Size = new System.Drawing.Size(50, 13);
            this.searchLabel.TabIndex = 0;
            this.searchLabel.Text = "Search:";
            
            // 
            // searchTextBox
            // 
            this.searchTextBox.Location = new System.Drawing.Point(70, 9);
            this.searchTextBox.Name = "searchTextBox";
            this.searchTextBox.Size = new System.Drawing.Size(200, 20);
            this.searchTextBox.TabIndex = 1;
            
            // 
            // tagFilterLabel
            // 
            this.tagFilterLabel.AutoSize = true;
            this.tagFilterLabel.Location = new System.Drawing.Point(10, 37);
            this.tagFilterLabel.Name = "tagFilterLabel";
            this.tagFilterLabel.Size = new System.Drawing.Size(30, 13);
            this.tagFilterLabel.TabIndex = 2;
            this.tagFilterLabel.Text = "Tag:";
            
            // 
            // tagFilterComboBox
            // 
            this.tagFilterComboBox.DropDownStyle = System.Windows.Forms.ComboBoxStyle.DropDownList;
            this.tagFilterComboBox.FormattingEnabled = true;
            this.tagFilterComboBox.Location = new System.Drawing.Point(70, 34);
            this.tagFilterComboBox.Name = "tagFilterComboBox";
            this.tagFilterComboBox.Size = new System.Drawing.Size(200, 21);
            this.tagFilterComboBox.TabIndex = 3;
            
            // 
            // applyFilterButton
            // 
            this.applyFilterButton.Location = new System.Drawing.Point(280, 9);
            this.applyFilterButton.Name = "applyFilterButton";
            this.applyFilterButton.Size = new System.Drawing.Size(75, 46);
            this.applyFilterButton.TabIndex = 4;
            this.applyFilterButton.Text = "Apply";
            this.applyFilterButton.UseVisualStyleBackColor = true;
            this.applyFilterButton.Click += new System.EventHandler(this.ApplyFilterButton_Click);
            
            // 
            // clearFilterButton
            // 
            this.clearFilterButton.Location = new System.Drawing.Point(365, 9);
            this.clearFilterButton.Name = "clearFilterButton";
            this.clearFilterButton.Size = new System.Drawing.Size(75, 46);
            this.clearFilterButton.TabIndex = 5;
            this.clearFilterButton.Text = "Clear";
            this.clearFilterButton.UseVisualStyleBackColor = true;
            this.clearFilterButton.Click += new System.EventHandler(this.ClearFilterButton_Click);
            
            // 
            // splitContainer
            // 
            this.splitContainer.Dock = System.Windows.Forms.DockStyle.Fill;
            this.splitContainer.Location = new System.Drawing.Point(0, 91);
            this.splitContainer.Name = "splitContainer";
            this.splitContainer.Orientation = System.Windows.Forms.Orientation.Horizontal;
            // 
            // splitContainer.Panel1
            // 
            this.splitContainer.Panel1.Controls.Add(this.listView);
            // 
            // splitContainer.Panel2
            // 
            this.splitContainer.Panel2.Controls.Add(this.detailPanel);
            this.splitContainer.Size = new System.Drawing.Size(516, 1013);
            this.splitContainer.SplitterDistance = 400;
            this.splitContainer.TabIndex = 1;
            
            // 
            // listView
            // 
            this.listView.Columns.AddRange(new System.Windows.Forms.ColumnHeader[] {
                this.titleColumn,
                this.authorColumn,
                this.tagsColumn,
                this.votesColumn,
                this.commentsColumn,
                this.dateColumn});
            this.listView.Dock = System.Windows.Forms.DockStyle.Fill;
            this.listView.FullRowSelect = true;
            this.listView.Location = new System.Drawing.Point(0, 0);
            this.listView.Name = "listView";
            this.listView.Size = new System.Drawing.Size(516, 400);
            this.listView.TabIndex = 0;
            this.listView.View = System.Windows.Forms.View.Details;
            this.listView.SelectedIndexChanged += new System.EventHandler(this.ListView_SelectedIndexChanged);
            
            // 
            // titleColumn
            // 
            this.titleColumn.Text = "Title";
            this.titleColumn.Width = 200;
            
            // 
            // authorColumn
            // 
            this.authorColumn.Text = "Author";
            this.authorColumn.Width = 100;
            
            // 
            // tagsColumn
            // 
            this.tagsColumn.Text = "Tags";
            this.tagsColumn.Width = 120;
            
            // 
            // votesColumn
            // 
            this.votesColumn.Text = "Votes";
            this.votesColumn.Width = 60;
            
            // 
            // commentsColumn
            // 
            this.commentsColumn.Text = "Comments";
            this.commentsColumn.Width = 80;
            
            // 
            // dateColumn
            // 
            this.dateColumn.Text = "Date";
            this.dateColumn.Width = 100;
            
            // 
            // detailPanel
            // 
            this.detailPanel.Controls.Add(this.detailWebView);
            this.detailPanel.Controls.Add(this.commentPanel);
            this.detailPanel.Controls.Add(this.detailToolbar);
            this.detailPanel.Dock = System.Windows.Forms.DockStyle.Fill;
            this.detailPanel.Location = new System.Drawing.Point(0, 0);
            this.detailPanel.Name = "detailPanel";
            this.detailPanel.Size = new System.Drawing.Size(516, 609);
            this.detailPanel.TabIndex = 0;
            
            // 
            // detailToolbar
            // 
            this.detailToolbar.GripStyle = System.Windows.Forms.ToolStripGripStyle.Hidden;
            this.detailToolbar.Items.AddRange(new System.Windows.Forms.ToolStripItem[] {
                this.loadToEditorButton,
                this.upvoteButton});
            this.detailToolbar.Location = new System.Drawing.Point(0, 0);
            this.detailToolbar.Name = "detailToolbar";
            this.detailToolbar.Size = new System.Drawing.Size(516, 31);
            this.detailToolbar.TabIndex = 0;
            
            // 
            // loadToEditorButton
            // 
            this.loadToEditorButton.Text = "Load to Editor";
            this.loadToEditorButton.Name = "loadToEditorButton";
            this.loadToEditorButton.Click += new System.EventHandler(this.LoadToEditorButton_Click);
            
            // 
            // upvoteButton
            // 
            this.upvoteButton.Text = "👍 Upvote";
            this.upvoteButton.Name = "upvoteButton";
            this.upvoteButton.Click += new System.EventHandler(this.UpvoteButton_Click);
            
            // 
            // detailWebView
            // 
            this.detailWebView.Dock = System.Windows.Forms.DockStyle.Fill;
            this.detailWebView.Location = new System.Drawing.Point(0, 31);
            this.detailWebView.Name = "detailWebView";
            this.detailWebView.Size = new System.Drawing.Size(516, 478);
            this.detailWebView.TabIndex = 1;
            
            // 
            // commentPanel
            // 
            this.commentPanel.Controls.Add(this.addCommentButton);
            this.commentPanel.Controls.Add(this.commentTextBox);
            this.commentPanel.Controls.Add(this.commentLabel);
            this.commentPanel.Dock = System.Windows.Forms.DockStyle.Bottom;
            this.commentPanel.Location = new System.Drawing.Point(0, 509);
            this.commentPanel.Name = "commentPanel";
            this.commentPanel.Size = new System.Drawing.Size(516, 100);
            this.commentPanel.TabIndex = 2;
            
            // 
            // commentLabel
            // 
            this.commentLabel.AutoSize = true;
            this.commentLabel.Location = new System.Drawing.Point(3, 6);
            this.commentLabel.Name = "commentLabel";
            this.commentLabel.Size = new System.Drawing.Size(100, 13);
            this.commentLabel.TabIndex = 0;
            this.commentLabel.Text = "Add Comment:";
            
            // 
            // commentTextBox
            // 
            this.commentTextBox.Anchor = ((System.Windows.Forms.AnchorStyles)(((System.Windows.Forms.AnchorStyles.Top | System.Windows.Forms.AnchorStyles.Left) 
            | System.Windows.Forms.AnchorStyles.Right)));
            this.commentTextBox.Location = new System.Drawing.Point(6, 22);
            this.commentTextBox.Multiline = true;
            this.commentTextBox.Name = "commentTextBox";
            this.commentTextBox.Size = new System.Drawing.Size(420, 70);
            this.commentTextBox.TabIndex = 1;
            
            // 
            // addCommentButton
            // 
            this.addCommentButton.Anchor = ((System.Windows.Forms.AnchorStyles)((System.Windows.Forms.AnchorStyles.Top | System.Windows.Forms.AnchorStyles.Right)));
            this.addCommentButton.Location = new System.Drawing.Point(432, 22);
            this.addCommentButton.Name = "addCommentButton";
            this.addCommentButton.Size = new System.Drawing.Size(75, 23);
            this.addCommentButton.TabIndex = 2;
            this.addCommentButton.Text = "Comment";
            this.addCommentButton.UseVisualStyleBackColor = true;
            this.addCommentButton.Click += new System.EventHandler(this.AddCommentButton_Click);
            
            // 
            // ScriptGalleryControl
            // 
            this.Controls.Add(this.splitContainer);
            this.Controls.Add(this.filterPanel);
            this.Controls.Add(this.toolbar);
            this.Name = "ScriptGalleryControl";
            this.Size = new System.Drawing.Size(516, 1044);
            this.toolbar.ResumeLayout(false);
            this.toolbar.PerformLayout();
            this.filterPanel.ResumeLayout(false);
            this.filterPanel.PerformLayout();
            this.splitContainer.Panel1.ResumeLayout(false);
            this.splitContainer.Panel2.ResumeLayout(false);
            ((System.ComponentModel.ISupportInitialize)(this.splitContainer)).EndInit();
            this.splitContainer.ResumeLayout(false);
            this.detailPanel.ResumeLayout(false);
            this.detailPanel.PerformLayout();
            this.detailToolbar.ResumeLayout(false);
            this.detailToolbar.PerformLayout();
            this.commentPanel.ResumeLayout(false);
            this.commentPanel.PerformLayout();
            this.ResumeLayout(false);
            this.PerformLayout();
        }

        #endregion

        private ToolStrip toolbar;
        private ToolStripButton loginButton;
        private ToolStripButton refreshButton;
        private ToolStripSeparator toolStripSeparator;
        private ToolStripLabel statusLabel;
        private Panel filterPanel;
        private Label searchLabel;
        private TextBox searchTextBox;
        private Label tagFilterLabel;
        private ComboBox tagFilterComboBox;
        private Button applyFilterButton;
        private Button clearFilterButton;
        private SplitContainer splitContainer;
        private ListView listView;
        private ColumnHeader titleColumn;
        private ColumnHeader authorColumn;
        private ColumnHeader tagsColumn;
        private ColumnHeader votesColumn;
        private ColumnHeader commentsColumn;
        private ColumnHeader dateColumn;
        private Panel detailPanel;
        private ToolStrip detailToolbar;
        private ToolStripButton loadToEditorButton;
        private ToolStripButton upvoteButton;
        private Microsoft.Web.WebView2.WinForms.WebView2 detailWebView;
        private Panel commentPanel;
        private Label commentLabel;
        private TextBox commentTextBox;
        private Button addCommentButton;
    }
}