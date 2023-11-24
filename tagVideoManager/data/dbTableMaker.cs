using System;
using System.Collections.Generic;
using System.Data.Entity.Migrations.Model;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using Dapper;
using OpenCvSharp.Internal.Vectors;

namespace tagVideoManager
{
	/*
		・テーブルが無ければ新規作成
		・既にあれば差分の列を追加

		過去のデータを引継ぎできるように

	*/

	public class dbTableMaker
	{
		// カラム１つ分の情報
		public class ColumInfo
		{
			// 名前
            public string Name { get; private set; }

			public enum Type
			{
				Int,
				Real,
				Text,
				Blob
			}

			// 型
			public Type DataType { get; private set; }

			// 付加情報
			public string ExParam { get; private set; }

            public ColumInfo(string name, Type dataType, string exParam = "")
            {
                this.Name = name;
				this.DataType = dataType;
				this.ExParam = exParam;
            }

			// クエリ取得
			public string GetQuery() 
			{
				var format = "";
				switch (DataType)
				{
					case Type.Int: 
						format = "integer";
						break;
					case Type.Real: 
						format = "real";
						break;
					case Type.Text:
						format = "text";
						break;
					case Type.Blob:
						format = "blob";
						break;
					default:
						throw new Exception("フォーマットが未定です");
				}

				return $"{this.Name} {format} {ExParam}";
			}
        }


		class PragmaTableInfo
		{
			public string name { get; set; }
			public string type { get; set; }
		}


		// テーブル作成
		public static void CreateTable(DB db, string tableName, List<ColumInfo> columns)
		{
			var srcColum = db.Query<PragmaTableInfo>($"pragma table_info('{tableName}')").ToList();

			// データがない場合は新規作成
			if (srcColum.Count <= 0)
			{
				CreateNewTable(db, tableName, columns);
				return;
			}

			// 足りない分を足す
			UpdateTable(db, tableName, columns, srcColum);
		}


		private static void UpdateTable(DB db, string tableName, List<ColumInfo> columns, List<PragmaTableInfo> srcColumns)
		{
			foreach (var columInfo in columns)
			{
				// 既にテーブルに存在する場合は無処理
				if (srcColumns.FindIndex(s => s.name == columInfo.Name) >= 0)
					continue;

				var query = $"alter table {tableName} add column {columInfo.GetQuery()}";
				db.Execute(query);
			}

			// 不要なカラムを削除する？
			// 必要なものを消してしまわないよう、不要なデータが邪魔になるまでは実装しない。
			// 

		}

		// 新規作成
		private static void CreateNewTable(DB db, string tableName, List<ColumInfo> columns)
		{
			var sb = new StringBuilder();
			sb.AppendLine($"CREATE TABLE IF NOT EXISTS {tableName} (");

			var splitter = "";
			foreach (var columInfo in columns) 
			{
				sb.AppendLine(splitter + columInfo.GetQuery());
				splitter = ",";
			}
			sb.AppendLine($");");
			db.Execute(sb.ToString());
		}



	}
}
