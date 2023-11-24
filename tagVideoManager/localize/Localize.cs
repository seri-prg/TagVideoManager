using System;
using System.Collections.Generic;
using System.IO;
using System.Windows.Forms;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using System.Globalization;
using DotLiquid;
using Csv;
using System.Diagnostics;
using System.Windows.Shapes;
using System.Windows.Navigation;
using static tagVideoManager.Localize;

namespace tagVideoManager
{
	public class Localize
	{
		private const string _localizePath = "localize.csv"; // 翻訳データ
		private const string _tmpPrefix = "tw_"; // テンプレートエンジン用キーワード
		private const string _key = "key"; // 言語情報が始まる１つ前の列のヘッダー
		private const string _configKey = "culture"; // DBのコンフィグキー


		// 選択言語情報
		public class LangItem
		{
			public string Name { get; set; }
			public string Culture { get; set; }

			public override string ToString()
			{
				return this.Name;
			}

            public LangItem(string inName, string inCulture)
            {
				this.Name = inName;
				this.Culture = inCulture;
			}
        }


		private Dictionary<string,string> _wordData = new Dictionary<string, string>();

		// 選択可能言語リスト
		public List<LangItem> LangItems { get; private set; } = new List<LangItem>();

		public string CurrentCulture { get; private set; } = string.Empty;


		public string Find(string key)
		{
			_wordData.TryGetValue(key, out var word);
			return word;
		}


		public string TaskMsg { get { return this.Find("taskMsg"); } }	// タスク実行中メッセージ

		public string TaskEnd { get { return this.Find("taskEnd"); } }	// タスク終了メッセージ


		// サポートする中からデフォルトのカルチャを決める
		public string GetDefaultCulture(string[] header)
		{
			if (header.Contains(CultureInfo.CurrentUICulture.Name))
			{
				return CultureInfo.CurrentCulture.Name;
			}
			
			if (header.Contains(CultureInfo.CurrentUICulture.Parent.Name))
			{
				return CultureInfo.CurrentCulture.Parent.Name;
			}

			return "en";
		}


		// 選択できる言語のリストセットアップ
		private void SetupSelectLanguage(ICsvLine line)
		{
			LangItems.Clear();

			var skipIndex = Array.IndexOf(line.Headers, _key);
            if (skipIndex < 0)
				return;

            for (int i = skipIndex+1; i < line.ColumnCount; i++)
			{
				LangItems.Add(new LangItem(line.Values[i], line.Headers[i]));
			}
		}

		// DBから初期値を取得してセットアップ
		public void Setup(DB db)
		{
			var culture = dbSystem.LoadConfig(db, _configKey);
			this.Setup(culture);
		}

		// 設定をセーブ
		public void Save(DB db)
		{
			dbSystem.SaveConfig(db, _configKey, this.CurrentCulture);
		}


		public void Setup(string cName = "")
		{
			try
			{
				_wordData.Clear();
				var csvText = File.ReadAllText(_localizePath);
				foreach (var line in CsvReader.ReadFromText(csvText))
				{
					// 使用言語の決定
					if (string.IsNullOrEmpty(cName))
					{
						cName = GetDefaultCulture(line.Headers);
					}

					// データがない場合は無処理
					if (!line.HasColumn(_key) || !line.HasColumn(cName))
						continue;

					var key = line[_key];
					var value = line[cName];
					if (string.IsNullOrEmpty(key) || string.IsNullOrEmpty(value))
						continue;

					_wordData.Add(key, value);

					// 言語選択なら全てとっておく
					if (key == "select")
					{
						SetupSelectLanguage(line);
					}
				}

				this.CurrentCulture = cName;

			}
			catch (Exception e)
			{
				throw new Exception($"read locaize file error. [{_localizePath}] {e.Message}");
			}
		}


		// 翻訳データがあればコントロールを上書き
		public void OverwriteControls(Control control)
		{
			// Trace.WriteLine($"control:{control.Name}");
			if (control is DataGridView)
			{
				var dataGridView = (DataGridView)control;
				foreach (DataGridViewColumn column in dataGridView.Columns) 
				{
					// Trace.WriteLine($"control column :{column.Name}");
					var headerText = this.Find(column.Name);
					if (!string.IsNullOrEmpty(headerText))
					{
						column.HeaderText = headerText;
					}
				}
			}

			var text = this.Find(control.Name);
			if (!string.IsNullOrEmpty(text))
			{
				control.Text = text;
			}

			foreach (var item in control.Controls)
			{
				OverwriteControls((Control)item);
			}
		}


		// htmlのテンプレートエンジンのハッシュにローカライズテキストを足す
		public void AddText(Hash hash)
		{
			foreach (var item in _wordData)
			{
				if (item.Key.StartsWith(_tmpPrefix))
				{
					hash[item.Key] = item.Value;
				}
			}
		}


		// 言語選択を設定
		public void SetLanguageSelect(ComboBox comboBox)
		{
			comboBox.Items.Clear();
			comboBox.Items.AddRange(this.LangItems.ToArray());
			comboBox.SelectedIndex = this.LangItems.FindIndex(l => l.Culture == this.CurrentCulture);
		}

	}
}
