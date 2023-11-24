using System;
using System.Collections.Generic;
using System.IO;
using System.Linq;
using System.Threading;
using System.Timers;

namespace tagVideoManager
{
	/*
		ファイルの登録管理
	*/
	internal class FileRegister
	{
		public DB Db { get; private set; }

		public int InputCount { get; private set; } = 0;

		private int _taskCount = 0; // 現在処理予定のタスクの数
		public int TaskCount => _taskCount;

		public event Action OnStartTask = null;	// タスク開始時
		public event Action<int> OnEndTask = null;	// タスク終了時:(経過時間)
		public event Action OnPlayTask = null;  // タスク実行中(定期的に呼びだされる

		// 対象ファイル拡張子
		public static readonly List<string> _tragetExt = new List<string>()
		{
			".mp4", ".flv", ".avi", ".wmv", ".mpeg",".mpg", ".mov"
		};

		private System.Timers.Timer _timer = new System.Timers.Timer();
		private DateTime _startTime;

        public static void DoAllFiles(DirectoryInfo parent, Action<FileInfo> func = null)
		{
			try
			{
				if (parent.Name.ToUpper() == "$RECYCLE.BIN")
					return;

				var files = parent.EnumerateFiles().Where(fi => _tragetExt.Contains(fi.Extension));
				foreach (var file in files)
				{
					func?.Invoke(file);
				}

				var child = parent.EnumerateDirectories("*", SearchOption.TopDirectoryOnly);
				foreach (var item in child)
				{
					DoAllFiles(item, func);
				}
			}
			catch (Exception)
			{
				// Trace.WriteLine($"アクセス例外[{parent.FullName}]");
			}
		}

		public void Setup(DB db)
		{
			Db = db;
			_timer.Interval = 1500;
			_timer.Elapsed += OnTimerElapsed;
			_timer.Enabled = false;
		}

		private void OnTimerElapsed(object sender, ElapsedEventArgs e)
		{
			OnPlayTask?.Invoke();
		}



		public void RegisterDragAndDrop(string[] drops, List<int> checkTag, bool parentDir, bool dragAndDropDir)
		{
			var p = new FileRegisterTask(drops, checkTag, parentDir, dragAndDropDir);
			ThreadPool.QueueUserWorkItem(DragAndDropPathAsync, p);
		}


		// ファイル１つを登録
		public void RegisterFileAsync(string path, List<int> checkTag, List<string> makeAndAddTag = null)
		{
			lock (this)
			{
				var mediaId = dbFile.Import(Db, path);

				dbTag.AddTag(Db, makeAndAddTag);    // 名前からタグ登録
				var tagId = dbTag.GetTagIdList(Db, makeAndAddTag); // 名前からタグIDを取得
				dbTag.AddTagLink(Db, mediaId, checkTag.Union(tagId));
				InputCount++;
			}
		}

		// D&D受け取り
		private void DragAndDropPathAsync(object param)
		{
			lock(_timer)
			{
				_taskCount++;
				if (_taskCount == 1)
				{
					OnStartTask?.Invoke();
					_startTime = DateTime.Now;
					_timer.Enabled = true;
				}
			}

			lock (this)
			{
				InputCount = 0;
				var regParam = param as FileRegisterTask;
				regParam.Regist(this);
			}

			lock (_timer)
			{
				_taskCount--;
				// タスクが終わったら通知
				if (_taskCount == 0)
				{
					_timer.Enabled = false;
					var span = DateTime.Now - _startTime;

					OnEndTask?.Invoke(span.Seconds);
				}
			}
		}



	}
}
