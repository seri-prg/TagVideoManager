using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using static tagVideoManager.dbFile;

namespace tagVideoManager
{
	internal class UIUtil
	{
		// [数値],[数値],[数値],... という文字列をList<int>に変換
		public static List<int> ParseArrayNum(string queryParam)
		{
			var numList= new List<int>();
			foreach (var item in queryParam.Split(','))
			{
				if (int.TryParse(item, out var num))
				{
					numList.Add(num);
				}
			}

			return numList;
		}

		// シリアルボリューム＋ファイルIDからリンク用の文字列を取得
		public static string GetMediaLinkName(MEDIA_FILE_INFO mediaInfo)
		{
			return $"{mediaInfo.volume_serial}_{mediaInfo.file_id}";
		}

		// リンク用の文字列からシリアルボリュームとファイルIDを取得
		public static MEDIA_FILE_INFO GetFileIds(string linkName)
		{
			var parts = linkName.Split('_');
			if (parts.Length != 2)
				return null;

			if (!ulong.TryParse(parts[0], out var volumeSerial))
				return null;

			if (!ulong.TryParse(parts[1], out var fileId))
				return null;

			return new MEDIA_FILE_INFO() { volume_serial = volumeSerial, file_id = fileId };
		}

		public static string GetTimeString(float time)
		{
			int sec = (int)time;
			int min = sec / 60;
			int hour = min / 60;
			int ms = (int)(time * 100.0f) % 100;
			//return $"{sec/60:D3}:{sec%60:D2}.{ms:D2}";
			return $"{hour:D2}:{min%60:D2}:{sec%60:D2}";	// 表示は秒まで
		}


	}
}
