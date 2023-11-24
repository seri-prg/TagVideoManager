using DotLiquid;
using System;
using System.Collections.Generic;
using System.IO;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace tagVideoManager
{
	/*
		ファイル登録タスク１つ分


	*/
	internal class FileRegisterTask
	{
		private bool _parentDir = false;         // 親フォルダをタグとして追加
		private bool _dragAndDropDir = false;    // D&Dしたフォルダをタグとして追加
		private bool _allDir = false;            // 全てのフォルダをタグとして追加
		private string[] _drops;
		private List<int> _checkTag;


        public FileRegisterTask(string[] drops, List<int> checkTag, bool parentDir, bool dragAndDropDir)
        {
			_parentDir = parentDir;
			_dragAndDropDir = dragAndDropDir;
			_drops = drops;
			_checkTag = (checkTag == null) ? new List<int>() : checkTag ;
		}


		public void Regist(FileRegister register)
		{
			foreach (var p in _drops)
			{
				// ファイルなら
				if (File.Exists(p))
				{
					register.RegisterFileAsync(p, _checkTag);
				}
				// ディレクトリなら
				else if (Directory.Exists(p))
				{
					var dirInfo = new DirectoryInfo(p);
					var dragDir = ""; // ドラッグ＆ドロップしたフォルダ
					if (_dragAndDropDir && (dirInfo.Parent != null))
					{
						dragDir = dirInfo.Name;
					}

					FileRegister.DoAllFiles(dirInfo, (FileInfo fi) =>
					{
						var tags = new List<string>();
						AddUniqeString(tags, dragDir);
						if (_parentDir && (fi.Directory.Parent != null))
						{
							AddUniqeString(tags, fi.Directory.Name);
						}

						register.RegisterFileAsync(fi.FullName, _checkTag, tags);
					});
				}
			}
		}

		// 有効な文字列で重複がなければ登録
		private static void AddUniqeString(List<string> src, string text)
		{
			if (string.IsNullOrEmpty(text))
				return;

			if (src.Contains(text))
				return;

			src.Add(text);
		}
	}
}
