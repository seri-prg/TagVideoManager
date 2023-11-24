using System.Collections.Generic;
using System.IO;
using System.Threading;

namespace tagVideoManager
{
	/*
		ファイル監視

	*/
	internal class FileMonitor
	{
		private List<FileSystemWatcher> _watcher = new List<FileSystemWatcher>();
		private FileRegister _register = null;

		public void Setup(DB db, FileRegister register)
		{
			// データが１つもない場合は初期値を入れる
			var monitorInfo = dbSystem.LoadMonitorPathInfo(db);
			if (monitorInfo.Count <= 0)
			{
				monitorInfo = InitMonitorInfo(db);
			}

			_register = register;

			foreach (var item in monitorInfo) 
			{
				if (item.enable == 0)
					continue;

				var w = new FileSystemWatcher();
				w.Path = item.path;
				w.NotifyFilter = NotifyFilters.FileName;
				w.Filter = "*.*";
				w.IncludeSubdirectories = true;
				w.Created += _watcher_Created;
				w.Renamed += _watcher_Created;
				w.EnableRaisingEvents = true;

				_watcher.Add(w);
			}
		}


		// モニター情報が１つもないなら初期値を入れる
		private static List<dbSystem.MonitorPath> InitMonitorInfo(DB db)
		{
			var dinfo = GetDriveInfo();
			var result = new List<dbSystem.MonitorPath>();

			foreach (var item in dinfo)
			{
				result.Add(new dbSystem.MonitorPath(1, item));
			}

			dbSystem.SaveMonitorPathInfo(db, result);
			return result;
		}


		private static List<string> GetDriveInfo()
		{
			var result = new List<string>();
			// 論理ドライブの一覧をforeachでループ
			foreach (DriveInfo driveInfo in DriveInfo.GetDrives())
			{
				// 監視対象は固定ドライブのみ
				if (driveInfo.DriveType != DriveType.Fixed)
					continue;

				if (!driveInfo.IsReady)
					continue;

				// fat32は対象外
				if (driveInfo.DriveFormat == "FAT32")
					continue;

				result.Add(driveInfo.Name);
			}

			return result;
		}


		// ファイルがロックされているか確認
		private static bool IsFileLocked(FileInfo file)
		{
			try
			{
				using (FileStream stream = file.Open(FileMode.Open, FileAccess.Read, FileShare.None))
				{
					stream.Close();
				}
			}
			catch (IOException)
			{
				//the file is unavailable because it is:
				//still being written to
				//or being processed by another thread
				//or does not exist (has already been processed)
				return true;
			}

			//file is not locked
			return false;
		}


		// 登録スレッド
		private void RegisterAsync(object param)
		{
			var fi = param as FileInfo;
			if (fi == null)
				return;

			while(fi.Exists)
			{
				if (_register == null)
					return;

				// ロック解除されたなら
				if (!IsFileLocked(fi))
				{
					_register.RegisterDragAndDrop(new[] { fi.FullName }, null, false, false);
					return;
				}

				Thread.Sleep(1000);	// １秒に１回程度				
			}
		}


		private void _watcher_Created(object sender, FileSystemEventArgs e)
		{
			// 動画生成時に処理を実行
			if ((e.ChangeType != WatcherChangeTypes.Created) && 
				(e.ChangeType != WatcherChangeTypes.Renamed))
				return;

			// ゴミ箱に移動した場合は無処理
			if (e.FullPath.ToUpper().Contains("$RECYCLE.BIN"))
				return;

			// ファイル以外は無処理
			if (!File.Exists(e.FullPath))
				return;

			// 登録対象外なら無処理
			var fi = new FileInfo(e.FullPath);
			if (!FileRegister._tragetExt.Contains(fi.Extension))
				return;

			// 大量のコピーが入ってもスレッドが生成されないようにスレッドプールで呼び出す。
			ThreadPool.QueueUserWorkItem(RegisterAsync, fi);
			// Trace.WriteLine($"{e.FullPath}: 作成");
		}
	}


}
