using System.Drawing;
using System.Windows.Forms;

namespace tagVideoManager
{
	partial class Form1
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
			this.components = new System.ComponentModel.Container();
			System.ComponentModel.ComponentResourceManager resources = new System.ComponentModel.ComponentResourceManager(typeof(Form1));
			this.labelDragAndDropArea = new System.Windows.Forms.Label();
			this.checkedListBoxTag = new System.Windows.Forms.CheckedListBox();
			this.buttonOpenWeb = new System.Windows.Forms.Button();
			this.labelAddTag = new System.Windows.Forms.Label();
			this.labelMsg = new System.Windows.Forms.Label();
			this.checkAddTagParent = new System.Windows.Forms.CheckBox();
			this.checkAddTagDragAndDrop = new System.Windows.Forms.CheckBox();
			this.labelDirTag = new System.Windows.Forms.Label();
			this.notifyIcon1 = new System.Windows.Forms.NotifyIcon(this.components);
			this.tabControl1 = new System.Windows.Forms.TabControl();
			this.tabPageAddTag = new System.Windows.Forms.TabPage();
			this.tabPageSetting = new System.Windows.Forms.TabPage();
			this.labelEnableReboot = new System.Windows.Forms.Label();
			this.dataGridViewMonitor = new System.Windows.Forms.DataGridView();
			this.ColumnEnable = new System.Windows.Forms.DataGridViewCheckBoxColumn();
			this.ColumnPath = new System.Windows.Forms.DataGridViewTextBoxColumn();
			this.comboBoxLanguage = new System.Windows.Forms.ComboBox();
			this.labelLanguage = new System.Windows.Forms.Label();
			this.labelMonitorPath = new System.Windows.Forms.Label();
			this.tabControl1.SuspendLayout();
			this.tabPageAddTag.SuspendLayout();
			this.tabPageSetting.SuspendLayout();
			((System.ComponentModel.ISupportInitialize)(this.dataGridViewMonitor)).BeginInit();
			this.SuspendLayout();
			// 
			// labelDragAndDropArea
			// 
			this.labelDragAndDropArea.AllowDrop = true;
			this.labelDragAndDropArea.BorderStyle = System.Windows.Forms.BorderStyle.FixedSingle;
			this.labelDragAndDropArea.Location = new System.Drawing.Point(3, 18);
			this.labelDragAndDropArea.Name = "labelDragAndDropArea";
			this.labelDragAndDropArea.Size = new System.Drawing.Size(210, 270);
			this.labelDragAndDropArea.TabIndex = 0;
			this.labelDragAndDropArea.Text = "動画かフォルダをここにドラッグ＆ドロップしてください";
			this.labelDragAndDropArea.DragDrop += new System.Windows.Forms.DragEventHandler(this.label1_DragDrop);
			this.labelDragAndDropArea.DragEnter += new System.Windows.Forms.DragEventHandler(this.label1_DragEnter);
			// 
			// checkedListBoxTag
			// 
			this.checkedListBoxTag.FormattingEnabled = true;
			this.checkedListBoxTag.Location = new System.Drawing.Point(219, 18);
			this.checkedListBoxTag.Name = "checkedListBoxTag";
			this.checkedListBoxTag.Size = new System.Drawing.Size(206, 270);
			this.checkedListBoxTag.TabIndex = 1;
			// 
			// buttonOpenWeb
			// 
			this.buttonOpenWeb.AutoSize = true;
			this.buttonOpenWeb.Location = new System.Drawing.Point(8, 293);
			this.buttonOpenWeb.Name = "buttonOpenWeb";
			this.buttonOpenWeb.Size = new System.Drawing.Size(75, 23);
			this.buttonOpenWeb.TabIndex = 6;
			this.buttonOpenWeb.Text = "サイトを開く";
			this.buttonOpenWeb.UseVisualStyleBackColor = true;
			this.buttonOpenWeb.Click += new System.EventHandler(this.buttonOpenWeb_Click);
			// 
			// labelAddTag
			// 
			this.labelAddTag.AutoSize = true;
			this.labelAddTag.Location = new System.Drawing.Point(220, 3);
			this.labelAddTag.Name = "labelAddTag";
			this.labelAddTag.Size = new System.Drawing.Size(65, 12);
			this.labelAddTag.TabIndex = 7;
			this.labelAddTag.Text = "追加するタグ";
			// 
			// labelMsg
			// 
			this.labelMsg.Location = new System.Drawing.Point(3, 3);
			this.labelMsg.Name = "labelMsg";
			this.labelMsg.Size = new System.Drawing.Size(210, 17);
			this.labelMsg.TabIndex = 8;
			// 
			// checkAddTagParent
			// 
			this.checkAddTagParent.Appearance = System.Windows.Forms.Appearance.Button;
			this.checkAddTagParent.Location = new System.Drawing.Point(434, 18);
			this.checkAddTagParent.Name = "checkAddTagParent";
			this.checkAddTagParent.Size = new System.Drawing.Size(236, 131);
			this.checkAddTagParent.TabIndex = 9;
			this.checkAddTagParent.Text = "親フォルダ";
			this.checkAddTagParent.TextAlign = System.Drawing.ContentAlignment.BottomRight;
			this.checkAddTagParent.UseVisualStyleBackColor = true;
			// 
			// checkAddTagDragAndDrop
			// 
			this.checkAddTagDragAndDrop.Appearance = System.Windows.Forms.Appearance.Button;
			this.checkAddTagDragAndDrop.Location = new System.Drawing.Point(434, 155);
			this.checkAddTagDragAndDrop.Name = "checkAddTagDragAndDrop";
			this.checkAddTagDragAndDrop.Size = new System.Drawing.Size(236, 133);
			this.checkAddTagDragAndDrop.TabIndex = 10;
			this.checkAddTagDragAndDrop.Text = "ドラッグ＆ドロップしたフォルダ";
			this.checkAddTagDragAndDrop.TextAlign = System.Drawing.ContentAlignment.BottomRight;
			this.checkAddTagDragAndDrop.UseVisualStyleBackColor = true;
			this.checkAddTagDragAndDrop.CheckedChanged += new System.EventHandler(this.checkBox2_CheckedChanged);
			// 
			// labelDirTag
			// 
			this.labelDirTag.AutoSize = true;
			this.labelDirTag.Location = new System.Drawing.Point(432, 3);
			this.labelDirTag.Name = "labelDirTag";
			this.labelDirTag.Size = new System.Drawing.Size(128, 12);
			this.labelDirTag.TabIndex = 12;
			this.labelDirTag.Text = "フォルダ名をタグとして追加";
			// 
			// notifyIcon1
			// 
			this.notifyIcon1.BalloonTipIcon = System.Windows.Forms.ToolTipIcon.Info;
			this.notifyIcon1.BalloonTipText = "タスクが終了しました。";
			this.notifyIcon1.Icon = ((System.Drawing.Icon)(resources.GetObject("notifyIcon1.Icon")));
			this.notifyIcon1.Text = "タスクが終了しました。";
			this.notifyIcon1.Visible = true;
			// 
			// tabControl1
			// 
			this.tabControl1.Controls.Add(this.tabPageAddTag);
			this.tabControl1.Controls.Add(this.tabPageSetting);
			this.tabControl1.Dock = System.Windows.Forms.DockStyle.Fill;
			this.tabControl1.Location = new System.Drawing.Point(0, 0);
			this.tabControl1.Name = "tabControl1";
			this.tabControl1.SelectedIndex = 0;
			this.tabControl1.Size = new System.Drawing.Size(698, 349);
			this.tabControl1.TabIndex = 13;
			// 
			// tabPageAddTag
			// 
			this.tabPageAddTag.Controls.Add(this.labelDragAndDropArea);
			this.tabPageAddTag.Controls.Add(this.buttonOpenWeb);
			this.tabPageAddTag.Controls.Add(this.labelDirTag);
			this.tabPageAddTag.Controls.Add(this.checkedListBoxTag);
			this.tabPageAddTag.Controls.Add(this.checkAddTagDragAndDrop);
			this.tabPageAddTag.Controls.Add(this.labelAddTag);
			this.tabPageAddTag.Controls.Add(this.checkAddTagParent);
			this.tabPageAddTag.Controls.Add(this.labelMsg);
			this.tabPageAddTag.Location = new System.Drawing.Point(4, 22);
			this.tabPageAddTag.Name = "tabPageAddTag";
			this.tabPageAddTag.Padding = new System.Windows.Forms.Padding(3);
			this.tabPageAddTag.Size = new System.Drawing.Size(690, 323);
			this.tabPageAddTag.TabIndex = 0;
			this.tabPageAddTag.Text = "タグ追加";
			this.tabPageAddTag.UseVisualStyleBackColor = true;
			// 
			// tabPageSetting
			// 
			this.tabPageSetting.Controls.Add(this.labelMonitorPath);
			this.tabPageSetting.Controls.Add(this.labelLanguage);
			this.tabPageSetting.Controls.Add(this.comboBoxLanguage);
			this.tabPageSetting.Controls.Add(this.labelEnableReboot);
			this.tabPageSetting.Controls.Add(this.dataGridViewMonitor);
			this.tabPageSetting.Location = new System.Drawing.Point(4, 22);
			this.tabPageSetting.Name = "tabPageSetting";
			this.tabPageSetting.Padding = new System.Windows.Forms.Padding(3);
			this.tabPageSetting.Size = new System.Drawing.Size(690, 323);
			this.tabPageSetting.TabIndex = 1;
			this.tabPageSetting.Text = "ファイル監視";
			this.tabPageSetting.UseVisualStyleBackColor = true;
			// 
			// labelEnableReboot
			// 
			this.labelEnableReboot.AutoSize = true;
			this.labelEnableReboot.Location = new System.Drawing.Point(6, 306);
			this.labelEnableReboot.Name = "labelEnableReboot";
			this.labelEnableReboot.Size = new System.Drawing.Size(183, 12);
			this.labelEnableReboot.TabIndex = 1;
			this.labelEnableReboot.Text = "設定は再起動後から有効になります。";
			// 
			// dataGridViewMonitor
			// 
			this.dataGridViewMonitor.Anchor = ((System.Windows.Forms.AnchorStyles)((((System.Windows.Forms.AnchorStyles.Top | System.Windows.Forms.AnchorStyles.Bottom) 
            | System.Windows.Forms.AnchorStyles.Left) 
            | System.Windows.Forms.AnchorStyles.Right)));
			this.dataGridViewMonitor.ColumnHeadersHeightSizeMode = System.Windows.Forms.DataGridViewColumnHeadersHeightSizeMode.AutoSize;
			this.dataGridViewMonitor.Columns.AddRange(new System.Windows.Forms.DataGridViewColumn[] {
            this.ColumnEnable,
            this.ColumnPath});
			this.dataGridViewMonitor.Location = new System.Drawing.Point(3, 74);
			this.dataGridViewMonitor.Name = "dataGridViewMonitor";
			this.dataGridViewMonitor.RowTemplate.Height = 21;
			this.dataGridViewMonitor.Size = new System.Drawing.Size(679, 217);
			this.dataGridViewMonitor.TabIndex = 0;
			// 
			// ColumnEnable
			// 
			this.ColumnEnable.AutoSizeMode = System.Windows.Forms.DataGridViewAutoSizeColumnMode.ColumnHeader;
			this.ColumnEnable.HeaderText = "有効";
			this.ColumnEnable.Name = "ColumnEnable";
			this.ColumnEnable.Width = 35;
			// 
			// ColumnPath
			// 
			this.ColumnPath.AutoSizeMode = System.Windows.Forms.DataGridViewAutoSizeColumnMode.Fill;
			this.ColumnPath.HeaderText = "パス";
			this.ColumnPath.Name = "ColumnPath";
			// 
			// comboBoxLanguage
			// 
			this.comboBoxLanguage.FormattingEnabled = true;
			this.comboBoxLanguage.Location = new System.Drawing.Point(6, 24);
			this.comboBoxLanguage.Name = "comboBoxLanguage";
			this.comboBoxLanguage.Size = new System.Drawing.Size(187, 20);
			this.comboBoxLanguage.TabIndex = 2;
			this.comboBoxLanguage.SelectionChangeCommitted += new System.EventHandler(this.comboBoxLanguage_SelectionChangeCommitted);
			// 
			// labelLanguage
			// 
			this.labelLanguage.AutoSize = true;
			this.labelLanguage.Location = new System.Drawing.Point(8, 9);
			this.labelLanguage.Name = "labelLanguage";
			this.labelLanguage.Size = new System.Drawing.Size(116, 12);
			this.labelLanguage.TabIndex = 3;
			this.labelLanguage.Text = "言語/language/語言/";
			// 
			// labelMonitorPath
			// 
			this.labelMonitorPath.AutoSize = true;
			this.labelMonitorPath.Location = new System.Drawing.Point(8, 59);
			this.labelMonitorPath.Name = "labelMonitorPath";
			this.labelMonitorPath.Size = new System.Drawing.Size(64, 12);
			this.labelMonitorPath.TabIndex = 4;
			this.labelMonitorPath.Text = "監視フォルダ";
			// 
			// Form1
			// 
			this.AutoScaleDimensions = new System.Drawing.SizeF(6F, 12F);
			this.AutoScaleMode = System.Windows.Forms.AutoScaleMode.Font;
			this.ClientSize = new System.Drawing.Size(698, 349);
			this.Controls.Add(this.tabControl1);
			this.FormBorderStyle = System.Windows.Forms.FormBorderStyle.FixedSingle;
			this.Icon = ((System.Drawing.Icon)(resources.GetObject("$this.Icon")));
			this.Margin = new System.Windows.Forms.Padding(3, 2, 3, 2);
			this.Name = "Form1";
			this.Text = "Tag Video Manager";
			this.Activated += new System.EventHandler(this.Form1_Activated);
			this.FormClosing += new System.Windows.Forms.FormClosingEventHandler(this.Form1_FormClosing);
			this.tabControl1.ResumeLayout(false);
			this.tabPageAddTag.ResumeLayout(false);
			this.tabPageAddTag.PerformLayout();
			this.tabPageSetting.ResumeLayout(false);
			this.tabPageSetting.PerformLayout();
			((System.ComponentModel.ISupportInitialize)(this.dataGridViewMonitor)).EndInit();
			this.ResumeLayout(false);

		}

		#endregion

		private Label labelDragAndDropArea;
		private CheckedListBox checkedListBoxTag;
		private Button buttonOpenWeb;
		private Label labelAddTag;
		private Label labelMsg;
		private CheckBox checkAddTagParent;
		private CheckBox checkAddTagDragAndDrop;
		private Label labelDirTag;
		private NotifyIcon notifyIcon1;
		private TabControl tabControl1;
		private TabPage tabPageAddTag;
		private TabPage tabPageSetting;
		private DataGridView dataGridViewMonitor;
		private DataGridViewCheckBoxColumn ColumnEnable;
		private DataGridViewTextBoxColumn ColumnPath;
		private Label labelEnableReboot;
		private Label labelLanguage;
		private ComboBox comboBoxLanguage;
		private Label labelMonitorPath;
	}
}