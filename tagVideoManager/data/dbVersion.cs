using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace tagVideoManager
{
	/*
		バージョン別処理
		DB内とバージョンが違う場合に整合性を取る処理を走らせる
			→新規追加したカラムでデータ毎の設定がある場合等。
	*/
	internal class dbVersion
	{
		public const string _dbVersion = "0.21";
		public const string _configKey = "dataVersion"; // コンフィグ確認用

		public static void CheckDb(DB db)
		{
			var oldVersion = dbSystem.LoadConfig(db, _configKey);
			// バージョンが同じなら無処理
			if (oldVersion == _dbVersion)
				return;

			Update(db);

			dbSystem.SaveConfig(db, _configKey, _dbVersion);
		}


		private static void Update(DB db)
		{
			var cont = 0;
			var tagList = dbTag.GetTagList(db);
			foreach (var tag in tagList)
			{
				db.Execute($"update tag_list set color = @color where tag_id = @tag_id",
					new 
					{
						tag_id = tag.tag_id, 
						color = dbTag.defaultTagColor[cont % dbTag.defaultTagColor.Length]
					});
				cont++;
			}

		}



	}
}
