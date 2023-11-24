using System;
using System.Collections.Generic;
using System.Data;
using System.Data.SQLite;
using System.IO;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using Dapper;

namespace tagVideoManager
{
	public class DB
	{
		private SQLiteConnection _connection = null;
		private object _lockConnection = new object();  // 排他ロック用

		public void Open(string dbname, Action onDbOpen = null)
		{
			if (!File.Exists(dbname))
			{
				SQLiteConnection.CreateFile(dbname);
			}

			var conn_str = "Data Source=" + dbname + ";";
			_connection = new SQLiteConnection(conn_str);
			_connection.Open();

			onDbOpen?.Invoke();    // DB接続後処理
		}

		public void Close()
		{
			lock (_lockConnection) 
			{
				_connection?.Close();
				_connection?.Dispose();
				_connection = null;
			}
		}

		public SQLiteDataAdapter ExecuteQueryAdapter(string sql)
		{
			lock (_lockConnection)
			{
				var Adapter = new SQLiteDataAdapter(sql, _connection);
				return Adapter;
			}
		}

		public IEnumerable<T> Query<T>(string sql, object param = null)
		{
			if (_connection == null)
			{
				return Enumerable.Empty<T>();
			}

			lock (_lockConnection)
			{
				return _connection.Query<T>(sql, param);
			}
		}

		public void Execute(string sql, object data = null)
		{
			if (_connection == null)
				return;

			lock (_lockConnection)
			{
				_connection.Execute(sql, data);
			}
		}

	}
}
