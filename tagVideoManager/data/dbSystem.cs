using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using System.Windows.Forms;
using static tagVideoManager.dbTableMaker;

namespace tagVideoManager
{
	/*
		監視ファイル

	*/
	internal class dbSystem
	{

		public class MonitorPath
		{
			public int enable;
			public string path;

            public MonitorPath(){}

            public MonitorPath(int inEnable, string inPath)
            {
				enable = inEnable;
				path = inPath;
            }


            public void Read(DataGridViewRow row)
			{
				var checkCell = (DataGridViewCheckBoxCell)(row.Cells["ColumnEnable"]);
				enable = ((checkCell.Value != null) && (bool)checkCell.Value) ? 1 : 0;

				var pathCell = (DataGridViewTextBoxCell)(row.Cells["ColumnPath"]);
				path = (string)pathCell.Value;
			}

			public void AddRow(DataGridView parent)
			{
				var rowIndex = parent.Rows.Add();

				var row = parent.Rows[rowIndex];
				// row.CreateCells(parent);
				row.Cells["ColumnEnable"].Value = (bool)(enable != 0);
				row.Cells["ColumnPath"].Value = path;
			}
		}


		public static void CreateTable(DB db)
		{
			// タグリスト
			dbTableMaker.CreateTable(db, "monitor_path", new List<ColumInfo>()
			{
				new ColumInfo("enable", ColumInfo.Type.Int),
				new ColumInfo("path", ColumInfo.Type.Text)
			});

			dbTableMaker.CreateTable(db, "system_config", new List<ColumInfo>()
			{
				new ColumInfo("key", ColumInfo.Type.Text, "UNIQUE"),
				new ColumInfo("value", ColumInfo.Type.Text)
			});

		}

		public static string LoadConfig(DB db, string key)
		{
			return db.Query<string>("select value from system_config where key = @key",
					new { key }).FirstOrDefault();
		}

		public static void SaveConfig(DB db, string key, string value)
		{
			db.Execute(@"insert into system_config (key, value)
							values (@key, @value) on conflict(key) do update set value = @value",
					new { key, value });
		}


		// データ取得
		public static List<MonitorPath> LoadMonitorPathInfo(DB db)
		{
			return db.Query<MonitorPath>(@"select * from monitor_path").ToList();
		}

		// データ保存
		public static void SaveMonitorPathInfo(DB db, List<MonitorPath> monitorInfo)
		{
			db.Execute(@"DELETE FROM monitor_path;");
			if (monitorInfo.Count > 0)
			{
				db.Execute(@"insert into monitor_path(enable, path) values (@enable, @path)",
					monitorInfo.Select(m => new { enable = m.enable, path = m.path }).ToList()
				);
			}
		}


		public static void Load(DB db, DataGridView dgvMonitor)
		{
			var rows = LoadMonitorPathInfo(db);

			dgvMonitor.Rows.Clear();
			foreach (var row in rows)
			{
				row.AddRow(dgvMonitor);
			}
		}


		public static void Save(DB db, DataGridView dgvMonitor)
		{
			var queryParam = new List<MonitorPath>();
			foreach (DataGridViewRow row in dgvMonitor.Rows)
			{
				if (row.Index == dgvMonitor.Rows.GetLastRow(DataGridViewElementStates.None))
					continue;

				var mc = new MonitorPath();
				mc.Read(row);
				queryParam.Add(mc);
			}

			SaveMonitorPathInfo(db, queryParam);
		}
	}
}
