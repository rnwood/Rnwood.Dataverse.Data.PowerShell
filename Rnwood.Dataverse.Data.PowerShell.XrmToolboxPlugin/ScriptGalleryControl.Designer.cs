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
            this.filterPanel = new System.Windows.Forms.FlowLayoutPanel();
            this.searchLabel = new System.Windows.Forms.Label();
            this.searchTextBox = new System.Windows.Forms.TextBox();
            this.tagFilterLabel = new System.Windows.Forms.Label();
            this.tagFilterComboBox = new System.Windows.Forms.ComboBox();
            this.mySubmissionsCheckBox = new System.Windows.Forms.CheckBox();
            this.applyFilterButton = new System.Windows.Forms.Button();
            this.clearFilterButton = new System.Windows.Forms.Button();
            this.splitContainer = new System.Windows.Forms.SplitContainer();
            this.listView = new System.Windows.Forms.ListView();
            this.titleColumn = ((System.Windows.Forms.ColumnHeader)(new System.Windows.Forms.ColumnHeader()));
            this.authorColumn = ((System.Windows.Forms.ColumnHeader)(new System.Windows.Forms.ColumnHeader()));
            this.tagsColumn = ((System.Windows.Forms.ColumnHeader)(new System.Windows.Forms.ColumnHeader()));
            this.votesColumn = ((System.Windows.Forms.ColumnHeader)(new System.Windows.Forms.ColumnHeader()));
            this.commentsColumn = ((System.Windows.Forms.ColumnHeader)(new System.Windows.Forms.ColumnHeader()));
            this.dateColumn = ((System.Windows.Forms.ColumnHeader)(new System.Windows.Forms.ColumnHeader()));
            this.detailPanel = new System.Windows.Forms.Panel();
            this.detailWebView = new Microsoft.Web.WebView2.WinForms.WebView2();
            this.commentPanel = new System.Windows.Forms.FlowLayoutPanel();
            this.addCommentButton = new System.Windows.Forms.Button();
            this.commentTextBox = new System.Windows.Forms.TextBox();
            this.commentLabel = new System.Windows.Forms.Label();
            this.detailToolbar = new System.Windows.Forms.ToolStrip();
            this.loadToEditorButton = new System.Windows.Forms.ToolStripButton();
            this.upvoteButton = new System.Windows.Forms.ToolStripButton();
            this.thumbsDownButton = new System.Windows.Forms.ToolStripButton();
            this.editButton = new System.Windows.Forms.ToolStripButton();
            this.closeButton = new System.Windows.Forms.ToolStripButton();
            this.toolbar.SuspendLayout();
            this.filterPanel.SuspendLayout();
            ((System.ComponentModel.ISupportInitialize)(this.splitContainer)).BeginInit();
            this.splitContainer.Panel1.SuspendLayout();
            this.splitContainer.Panel2.SuspendLayout();
            this.splitContainer.SuspendLayout();
            this.detailPanel.SuspendLayout();
            ((System.ComponentModel.ISupportInitialize)(this.detailWebView)).BeginInit();
            this.commentPanel.SuspendLayout();
            this.detailToolbar.SuspendLayout();
            this.SuspendLayout();
            // 
            // toolbar
            // 
            this.toolbar.GripStyle = System.Windows.Forms.ToolStripGripStyle.Hidden;
            this.toolbar.ImageScalingSize = new System.Drawing.Size(28, 28);
            this.toolbar.Items.AddRange(new System.Windows.Forms.ToolStripItem[] {
            this.loginButton,
            this.refreshButton,
            this.toolStripSeparator,
            this.statusLabel});
            this.toolbar.Location = new System.Drawing.Point(0, 0);
            this.toolbar.Name = "toolbar";
            this.toolbar.Size = new System.Drawing.Size(1043, 25);
            this.toolbar.TabIndex = 0;
            // 
            // loginButton
            // 
            this.loginButton.Name = "loginButton";
            this.loginButton.Size = new System.Drawing.Size(96, 22);
            this.loginButton.Text = "Login to GitHub";
            this.loginButton.Click += new System.EventHandler(this.LoginButton_Click);
            // 
            // refreshButton
            // 
            this.refreshButton.Name = "refreshButton";
            this.refreshButton.Size = new System.Drawing.Size(50, 22);
            this.refreshButton.Text = "Refresh";
            this.refreshButton.Click += new System.EventHandler(this.RefreshButton_Click);
            // 
            // toolStripSeparator
            // 
            this.toolStripSeparator.Name = "toolStripSeparator";
            this.toolStripSeparator.Size = new System.Drawing.Size(6, 25);
            // 
            // statusLabel
            // 
            this.statusLabel.Name = "statusLabel";
            this.statusLabel.Size = new System.Drawing.Size(80, 22);
            this.statusLabel.Text = "Not logged in";
            // 
            // filterPanel
            // 
            this.filterPanel.AutoSize = true;
            this.filterPanel.Controls.Add(this.searchLabel);
            this.filterPanel.Controls.Add(this.searchTextBox);
            this.filterPanel.Controls.Add(this.tagFilterLabel);
            this.filterPanel.Controls.Add(this.tagFilterComboBox);
            this.filterPanel.Controls.Add(this.mySubmissionsCheckBox);
            this.filterPanel.Controls.Add(this.applyFilterButton);
            this.filterPanel.Controls.Add(this.clearFilterButton);
            this.filterPanel.Dock = System.Windows.Forms.DockStyle.Top;
            this.filterPanel.Location = new System.Drawing.Point(0, 25);
            this.filterPanel.Name = "filterPanel";
            this.filterPanel.Size = new System.Drawing.Size(1043, 29);
            this.filterPanel.TabIndex = 1;
            // 
            // searchLabel
            // 
            this.searchLabel.Anchor = ((System.Windows.Forms.AnchorStyles)((((System.Windows.Forms.AnchorStyles.Top | System.Windows.Forms.AnchorStyles.Bottom) 
            | System.Windows.Forms.AnchorStyles.Left) 
            | System.Windows.Forms.AnchorStyles.Right)));
            this.searchLabel.AutoSize = true;
            this.searchLabel.Location = new System.Drawing.Point(3, 0);
            this.searchLabel.Name = "searchLabel";
            this.searchLabel.Size = new System.Drawing.Size(44, 29);
            this.searchLabel.TabIndex = 0;
            this.searchLabel.Text = "Search:";
            this.searchLabel.TextAlign = System.Drawing.ContentAlignment.MiddleLeft;
            // 
            // searchTextBox
            // 
            this.searchTextBox.Location = new System.Drawing.Point(53, 3);
            this.searchTextBox.Name = "searchTextBox";
            this.searchTextBox.Size = new System.Drawing.Size(150, 20);
            this.searchTextBox.TabIndex = 1;
            // 
            // tagFilterLabel
            // 
            this.tagFilterLabel.Anchor = ((System.Windows.Forms.AnchorStyles)((((System.Windows.Forms.AnchorStyles.Top | System.Windows.Forms.AnchorStyles.Bottom) 
            | System.Windows.Forms.AnchorStyles.Left) 
            | System.Windows.Forms.AnchorStyles.Right)));
            this.tagFilterLabel.AutoSize = true;
            this.tagFilterLabel.Location = new System.Drawing.Point(209, 0);
            this.tagFilterLabel.Name = "tagFilterLabel";
            this.tagFilterLabel.Size = new System.Drawing.Size(29, 29);
            this.tagFilterLabel.TabIndex = 2;
            this.tagFilterLabel.Text = "Tag:";
            this.tagFilterLabel.TextAlign = System.Drawing.ContentAlignment.MiddleLeft;
            // 
            // tagFilterComboBox
            // 
            this.tagFilterComboBox.DropDownStyle = System.Windows.Forms.ComboBoxStyle.DropDownList;
            this.tagFilterComboBox.FormattingEnabled = true;
            this.tagFilterComboBox.Location = new System.Drawing.Point(244, 3);
            this.tagFilterComboBox.Name = "tagFilterComboBox";
            this.tagFilterComboBox.Size = new System.Drawing.Size(150, 21);
            this.tagFilterComboBox.TabIndex = 3;
            // 
            // mySubmissionsCheckBox
            // 
            this.mySubmissionsCheckBox.Anchor = ((System.Windows.Forms.AnchorStyles)((((System.Windows.Forms.AnchorStyles.Top | System.Windows.Forms.AnchorStyles.Bottom) 
            | System.Windows.Forms.AnchorStyles.Left) 
            | System.Windows.Forms.AnchorStyles.Right)));
            this.mySubmissionsCheckBox.AutoSize = true;
            this.mySubmissionsCheckBox.Location = new System.Drawing.Point(400, 3);
            this.mySubmissionsCheckBox.Name = "mySubmissionsCheckBox";
            this.mySubmissionsCheckBox.Size = new System.Drawing.Size(101, 23);
            this.mySubmissionsCheckBox.TabIndex = 4;
            this.mySubmissionsCheckBox.Text = "My Submissions";
            this.mySubmissionsCheckBox.UseVisualStyleBackColor = true;
            // 
            // applyFilterButton
            // 
            this.applyFilterButton.AutoSize = true;
            this.applyFilterButton.AutoSizeMode = System.Windows.Forms.AutoSizeMode.GrowAndShrink;
            this.applyFilterButton.Location = new System.Drawing.Point(507, 3);
            this.applyFilterButton.Name = "applyFilterButton";
            this.applyFilterButton.Size = new System.Drawing.Size(43, 23);
            this.applyFilterButton.TabIndex = 4;
            this.applyFilterButton.Text = "Apply";
            this.applyFilterButton.UseVisualStyleBackColor = true;
            this.applyFilterButton.Click += new System.EventHandler(this.ApplyFilterButton_Click);
            // 
            // clearFilterButton
            // 
            this.clearFilterButton.AutoSize = true;
            this.clearFilterButton.AutoSizeMode = System.Windows.Forms.AutoSizeMode.GrowAndShrink;
            this.clearFilterButton.Location = new System.Drawing.Point(556, 3);
            this.clearFilterButton.Name = "clearFilterButton";
            this.clearFilterButton.Size = new System.Drawing.Size(41, 23);
            this.clearFilterButton.TabIndex = 5;
            this.clearFilterButton.Text = "Clear";
            this.clearFilterButton.UseVisualStyleBackColor = true;
            this.clearFilterButton.Click += new System.EventHandler(this.ClearFilterButton_Click);
            // 
            // splitContainer
            // 
            this.splitContainer.Dock = System.Windows.Forms.DockStyle.Fill;
            this.splitContainer.Location = new System.Drawing.Point(0, 54);
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
            this.splitContainer.Size = new System.Drawing.Size(1043, 1137);
            this.splitContainer.SplitterDistance = 448;
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
            this.listView.HideSelection = false;
            this.listView.Location = new System.Drawing.Point(0, 0);
            this.listView.Name = "listView";
            this.listView.Size = new System.Drawing.Size(1043, 448);
            this.listView.TabIndex = 0;
            this.listView.UseCompatibleStateImageBehavior = false;
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
            this.detailPanel.Size = new System.Drawing.Size(1043, 685);
            this.detailPanel.TabIndex = 0;
            // 
            // detailWebView
            // 
            this.detailWebView.AllowExternalDrop = true;
            this.detailWebView.CreationProperties = null;
            this.detailWebView.DefaultBackgroundColor = System.Drawing.Color.White;
            this.detailWebView.Dock = System.Windows.Forms.DockStyle.Fill;
            this.detailWebView.Location = new System.Drawing.Point(0, 25);
            this.detailWebView.Name = "detailWebView";
            this.detailWebView.Size = new System.Drawing.Size(1043, 525);
            this.detailWebView.TabIndex = 1;
            this.detailWebView.ZoomFactor = 1D;
            // 
            // commentPanel
            // 
            this.commentPanel.AutoSize = true;
            this.commentPanel.AutoSizeMode = System.Windows.Forms.AutoSizeMode.GrowAndShrink;
            this.commentPanel.Controls.Add(this.commentLabel);
            this.commentPanel.Controls.Add(this.commentTextBox);
            this.commentPanel.Controls.Add(this.addCommentButton);
            this.commentPanel.Dock = System.Windows.Forms.DockStyle.Bottom;
            this.commentPanel.FlowDirection = System.Windows.Forms.FlowDirection.TopDown;
            this.commentPanel.Location = new System.Drawing.Point(0, 550);
            this.commentPanel.Name = "commentPanel";
            this.commentPanel.Size = new System.Drawing.Size(1043, 135);
            this.commentPanel.TabIndex = 2;
            this.commentPanel.WrapContents = false;
            // 
            // addCommentButton
            // 
            this.addCommentButton.AutoSize = true;
            this.addCommentButton.AutoSizeMode = System.Windows.Forms.AutoSizeMode.GrowAndShrink;
            this.addCommentButton.Location = new System.Drawing.Point(3, 109);
            this.addCommentButton.Name = "addCommentButton";
            this.addCommentButton.Size = new System.Drawing.Size(61, 23);
            this.addCommentButton.TabIndex = 2;
            this.addCommentButton.Text = "Comment";
            this.addCommentButton.UseVisualStyleBackColor = true;
            this.addCommentButton.Click += new System.EventHandler(this.AddCommentButton_Click);
            // 
            // commentTextBox
            // 
            this.commentTextBox.Dock = System.Windows.Forms.DockStyle.Left;
            this.commentTextBox.Location = new System.Drawing.Point(3, 16);
            this.commentTextBox.Multiline = true;
            this.commentTextBox.Name = "commentTextBox";
            this.commentTextBox.Size = new System.Drawing.Size(1037, 87);
            this.commentTextBox.TabIndex = 1;
            // 
            // commentLabel
            // 
            this.commentLabel.AutoSize = true;
            this.commentLabel.Location = new System.Drawing.Point(3, 0);
            this.commentLabel.Name = "commentLabel";
            this.commentLabel.Size = new System.Drawing.Size(76, 13);
            this.commentLabel.TabIndex = 0;
            this.commentLabel.Text = "Add Comment:";
            // 
            // detailToolbar
            // 
            this.detailToolbar.GripStyle = System.Windows.Forms.ToolStripGripStyle.Hidden;
            this.detailToolbar.ImageScalingSize = new System.Drawing.Size(28, 28);
            this.detailToolbar.Items.AddRange(new System.Windows.Forms.ToolStripItem[] {
            this.loadToEditorButton,
            this.upvoteButton,
            this.thumbsDownButton,
            this.editButton,
            this.closeButton});
            this.detailToolbar.Location = new System.Drawing.Point(0, 0);
            this.detailToolbar.Name = "detailToolbar";
            this.detailToolbar.Size = new System.Drawing.Size(1043, 25);
            this.detailToolbar.TabIndex = 0;
            // 
            // loadToEditorButton
            // 
            this.loadToEditorButton.Name = "loadToEditorButton";
            this.loadToEditorButton.Size = new System.Drawing.Size(85, 22);
            this.loadToEditorButton.Text = "Load to Editor";
            this.loadToEditorButton.Click += new System.EventHandler(this.LoadToEditorButton_Click);
            // 
            // upvoteButton
            // 
            this.upvoteButton.Name = "upvoteButton";
            this.upvoteButton.Size = new System.Drawing.Size(88, 22);
            this.upvoteButton.Text = "👍 Thumbs Up";
            this.upvoteButton.Click += new System.EventHandler(this.UpvoteButton_Click);
            // 
            // thumbsDownButton
            // 
            this.thumbsDownButton.Name = "thumbsDownButton";
            this.thumbsDownButton.Size = new System.Drawing.Size(104, 22);
            this.thumbsDownButton.Text = "👎 Thumbs Down";
            this.thumbsDownButton.Click += new System.EventHandler(this.ThumbsDownButton_Click);
            // 
            // editButton
            // 
            this.editButton.Name = "editButton";
            this.editButton.Size = new System.Drawing.Size(31, 22);
            this.editButton.Text = "Edit";
            this.editButton.Visible = false;
            this.editButton.Click += new System.EventHandler(this.EditButton_Click);
            // 
            // closeButton
            // 
            this.closeButton.Name = "closeButton";
            this.closeButton.Size = new System.Drawing.Size(40, 22);
            this.closeButton.Text = "Close";
            this.closeButton.Visible = false;
            this.closeButton.Click += new System.EventHandler(this.CloseButton_Click);
            // 
            // ScriptGalleryControl
            // 
            this.Controls.Add(this.splitContainer);
            this.Controls.Add(this.filterPanel);
            this.Controls.Add(this.toolbar);
            this.Name = "ScriptGalleryControl";
            this.Size = new System.Drawing.Size(1043, 1191);
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
            ((System.ComponentModel.ISupportInitialize)(this.detailWebView)).EndInit();
            this.commentPanel.ResumeLayout(false);
            this.commentPanel.PerformLayout();
            this.detailToolbar.ResumeLayout(false);
            this.detailToolbar.PerformLayout();
            this.ResumeLayout(false);
            this.PerformLayout();

        }

        #endregion

        private ToolStrip toolbar;
        private ToolStripButton loginButton;
        private ToolStripButton refreshButton;
        private ToolStripSeparator toolStripSeparator;
        private ToolStripLabel statusLabel;
        private FlowLayoutPanel filterPanel;
        private Label searchLabel;
        private TextBox searchTextBox;
        private Label tagFilterLabel;
        private ComboBox tagFilterComboBox;
        private CheckBox mySubmissionsCheckBox;
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
        private ToolStripButton thumbsDownButton;
        private ToolStripButton editButton;
        private ToolStripButton closeButton;
        private Microsoft.Web.WebView2.WinForms.WebView2 detailWebView;
        private FlowLayoutPanel commentPanel;
        private Label commentLabel;
        private TextBox commentTextBox;
        private Button addCommentButton;
    }
}