using System;
using System.Collections.Generic;
using System.Linq;
using System.Runtime.CompilerServices;
using System.Text;
using System.Threading.Tasks;

namespace tagVideoManager
{
	/*
		クエリコントロール

	*/
	public class Query
	{
		private Dictionary<string, string> _params = new Dictionary<string, string>();


		// 文字列からパラメータセットアップ
		public void Setup(string query)
		{
			this.Clear();

			var keys = query.Split('?', '&');
			foreach (var key in keys)
			{
				var keyValue = key.Split('=');
				if (keyValue.Length != 2)
					continue;

				_params[keyValue[0]] = keyValue[1];
			}
		}


		public void Clear()
		{
			_params.Clear();
		}


		// 全てのパラメータを取得
		public void DoAll(Action<string, string> func)
		{
			foreach (var item in _params)
			{
				func?.Invoke(item.Key, item.Value);
			}
		}


		public bool TryGetString(string key, out string value) 
		{
			value = null;
			return _params.TryGetValue(key, out value);
		}

		public bool TryGetInt(string key, out int result)
		{
			result = 0;
			if (!_params.TryGetValue(key, out var value)) return false;
			return int.TryParse(value, out result);
		}

		public bool TryGetUlong(string key, out ulong result)
		{
			result = 0;
			if (!_params.TryGetValue(key, out var value)) return false;
			return ulong.TryParse(value, out result);
		}

		public bool TryGetLong(string key, out long result)
		{
			result = 0;
			if (!_params.TryGetValue(key, out var value)) return false;
			return long.TryParse(value, out result);
		}


		public bool TryGetFloat(string key, out float result)
		{
			result = 0;
			if (!_params.TryGetValue(key, out var value)) return false;
			return float.TryParse(value, out result);
		}

		public bool TryGetArrayInt(string key, out List<int> result)
		{
			result = new List<int>();

			if (!this.TryGetString(key, out var queryParam))
				return false;

			foreach (var item in queryParam.Split(','))
			{
				if (int.TryParse(item, out var num))
				{
					result.Add(num);
				}
			}

			return true;
		}


	}
}
