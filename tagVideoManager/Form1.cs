using System;
using System.Collections.Generic;
using System.ComponentModel;
using System.Data;
using System.Diagnostics;
using System.Drawing;
using System.Linq;
using System.Reactive;
using System.Text;
using System.Threading;
using System.Threading.Tasks;
using System.Web.UI.WebControls;
using System.Windows.Forms;
using Shell32;
using static tagVideoManager.dbTag;

namespace tagVideoManager
{
	public partial class Form1 : Form
	{
		private Server _server = null;
		private DB _db = null;
		private FileRegister _filesRegister = new FileRegister();
		private FileMonitor _monitor = new FileMonitor();

		private dbTag.TAG_TEABLE_INFO _lastTagList = null; // 最後に取得したタグリストの状態


		public Form1(DB db, Server server)
		{
			_db = db;
			_server = server;
			_filesRegister.Setup(_db);
			_monitor.Setup(_db, _filesRegister);

			InitializeComponent();

			// コントロールの文字を差し替え
			_server.Localize.OverwriteControls(this);
			// 言語設定のラベルには英語の注釈をつけておく
			this.labelLanguage.Text = this.labelLanguage.Text + "(Language)";

			// タグ更新
			this.UpdateTagList();

			// パスの表示
			dbSystem.Load(_db, dataGridViewMonitor);
			// 選択言語リスト設定
			_server.Localize.SetLanguageSelect(comboBoxLanguage);

			// タスク実行中の表示
			_filesRegister.OnPlayTask += () =>
			{
				this.Invoke(() =>
				{
					this.labelMsg.Text = string.Format(_server.Localize.TaskMsg,
										_filesRegister.TaskCount, _filesRegister.InputCount);
				});
			};

			// タスク終了時
			_filesRegister.OnEndTask += (int second) =>
			{
				this.Invoke(() =>
				{
					this.labelMsg.Text = _server.Localize.TaskEnd;

					// 処理が一定秒以上かかった場合は通知を出す
					if (second > 5)
					{
						notifyIcon1.BalloonTipIcon = ToolTipIcon.Info;
						notifyIcon1.ShowBalloonTip(3000);
					}
				});
			};
		}

		#region 動画のD&D関係
		private void label1_DragEnter(object sender, DragEventArgs e)
		{
			if (e.Data.GetDataPresent(DataFormats.FileDrop))
			{
				e.Effect = DragDropEffects.Copy;
			}
			else
			{
				e.Effect = DragDropEffects.None;
			}
		}

		private void label1_DragDrop(object sender, DragEventArgs e)
		{
			var files = (string[])e.Data.GetData(DataFormats.FileDrop, false);

			var checksTag = checkedListBoxTag.CheckedItems
						.Cast<CheckTagItem>()
						.Select(c => (int)c.TagInfo.tag_id)
						.ToList();

			_filesRegister.RegisterDragAndDrop(files, checksTag,
				checkAddTagParent.Checked,
				checkAddTagDragAndDrop.Checked);
		}

		private void label1_DragLeave(object sender, EventArgs e)
		{

		}


		#endregion

		// webを開く
		private void buttonOpenWeb_Click(object sender, EventArgs e)
		{
			_server.OpenStartPage();
		}


		internal class CheckTagItem
		{
			public dbTag.TAG_INFO TagInfo { get; private set; }

			public CheckTagItem(dbTag.TAG_INFO tagInfo)
            {
				TagInfo = tagInfo;
			}

			public override string ToString()
			{
				return TagInfo.name;
			}
		}

		// タグリストを更新
		private void UpdateTagList()
		{
			var nowTagInfo = dbTag.GetTagListTeableInfo(_db);
			// 前回取得から状態が変わっていない場合は無処理
			if ((_lastTagList != null) && _lastTagList.IsSame(nowTagInfo))
				return;

			_lastTagList = nowTagInfo; // タグ情報の更新

			var tagList = dbTag.GetTagList(_db);
			checkedListBoxTag.Items.Clear();
			var checks = checkedListBoxTag.CheckedItems
									.Cast<CheckTagItem>()
									.Select(c => c.ToString())
									.ToList();

			foreach (var tag in tagList) 
			{
				var isChecked = checks.Contains(tag.name);
				checkedListBoxTag.Items.Add(new CheckTagItem(tag), isChecked);
			}
		}

		private void checkBox2_CheckedChanged(object sender, EventArgs e)
		{

		}

		// フォーム閉じる時
		private void Form1_FormClosing(object sender, FormClosingEventArgs e)
		{
			dbSystem.Save(_db, dataGridViewMonitor);
		}

		private void comboBoxLanguage_SelectionChangeCommitted(object sender, EventArgs e)
		{
			var item = comboBoxLanguage.SelectedItem as Localize.LangItem;
			if (item == null)
				return;

			// 再セットアップ
			_server.Localize.Setup(item.Culture);
			// コントロールの文字を差し替え
			_server.Localize.OverwriteControls(this);
			// 言語設定のラベルには英語の注釈をつけておく
			this.labelLanguage.Text = this.labelLanguage.Text + "(Language)";
			// 設定を保存
			_server.Localize.Save(_db);
		}


		// フォームがアクティブになった時
		private void Form1_Activated(object sender, EventArgs e)
		{
			// タグリストを更新
			this.UpdateTagList();
		}
	}
}
