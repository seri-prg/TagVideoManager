using Shell32;
using System;
using System.Collections.Generic;
using System.Diagnostics;
using System.IO;
using System.Linq;
using System.Threading;
using System.Threading.Tasks;
using System.Windows.Forms;
using videoEditor;

namespace tagVideoManager
{
	internal static class Program
	{
		/// <summary>
		///  The main entry point for the application.
		/// </summary>
		[STAThread]
		static void Main()
		{
			if (Debugger.IsAttached)
			{
				Directory.SetCurrentDirectory(Path.Combine(AppDomain.CurrentDomain.BaseDirectory, @"..\..\..\"));
			}

			Trace.WriteLine($"current path[{Directory.GetCurrentDirectory()}]");

			var db = new DB();  // データベース初期化
			// DBを開く
			db.Open("video_manager.db", () =>
			{
				dbFile.CreateTable(db);
				dbTag.CreateTable(db);
				dbSystem.CreateTable(db);

				// DBのバージョン差毎の整合性処理。テーブル作成後に実行する。
				dbVersion.CheckDb(db);
			});

			// dbFile.Debug(db);

			var server = new Server(db);
			server.Start();

			Application.EnableVisualStyles();
			Application.SetCompatibleTextRenderingDefault(false);
			Application.Run(new Form1(db, server));

			server.Stop();
			db.Close();
		}
	}
}