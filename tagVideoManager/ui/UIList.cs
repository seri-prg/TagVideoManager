using DotLiquid;
using System.Collections.Generic;
using System.IO;
using System.Linq;
using System.Text;
using System.Text.Json;
using System.Web.UI;
using static tagVideoManager.dbFile;
using static tagVideoManager.dbTag;

namespace tagVideoManager
{
	// データ一覧を取得
	internal class UIList : UIBase
	{
		private const int _maxReadData = 10; // １回のリクエストで読み込むメディア数

		public const string ListHtmlName = "list.html";


		public static Template GetTemplate(Server owner)
		{
			var tmpPath = Path.Combine(owner.RootPath, UIList.ListHtmlName);
			return Template.Parse(File.ReadAllText(tmpPath));
		}


		public override string Show(Server server, string filePath, Query query)
		{
			var name = Path.GetFileName(filePath);

			// html表示
			if (name == UIList.ListHtmlName)
			{
				return Show(server);
			}

			// メディアリスト更新要求
			if (name == "update_videolist.custom_text")
			{
				return GetMediaListJson(server.Db, query);
			}

			// タグリスト更新要求
			if (name == "update_taglist.custom_text")
			{
				return GetTagListJson(server.Db);
			}

			// メディア１つ更新要求
			if (name == "update_videoicon.custom_text")
			{
				return GetMediaOneJson(server.Db, query);
			}

			return string.Empty;
		}


		// 
		private static string Show(Server server)
		{
			var db = server.Db;
			var Template = GetTemplate(server);

			var hash = Hash.FromAnonymousObject(new
			{
				initTagString = GetTagListJson(db),
				initMediaString = GetMediaListJson(server.Db),
				list_url = UIList.ListHtmlName,
				root_url = server.RootUrl,
			});
			server.Localize.AddText(hash); // ローカライズ用データ追加

			return Template.Render(hash);
		}

		// タグリストをJson形式で取得
		public static string GetTagListJson(DB db)
		{
			// タグ情報の整形
			var list = dbTag.GetTagList(db);
			return JsonSerializer.Serialize(list.Select(t => new
			{
				tag_id = t.tag_id,
				name = t.name,
				color = t.color
			}));
		}

		/*
			mt= タグの追加対象メディアのfile_id
		*/
		// メディタ１つの情報を更新
		private static string GetMediaOneJson(DB db, Query query)
		{
			query.TryGetString("mt", out var mediaLinkName);
			var mediaInfo = query.GetFileIds();
			var mediaId = dbFile.GetMediaId(db, mediaInfo);

			// タグ情報を取得
			var tagList = dbFile.GetTagLink(db, mediaId);

			// 更新
			return JsonSerializer.Serialize(new
			{
				file_id = mediaLinkName,
				media_tag_list = tagList.Select(t => new { name = t.tag_name, color = t.color })
			});
		}


		/*
			s= 表示チェックの入ったTagId(複数指定可)
			o= データの表示開始オフセット
		*/
		// リスト更新
		private static string GetMediaListJson(DB db, Query query = null)
		{
			// クエリパラメータから有効なタグを取得
			var enableTags = new List<int>();
			var offset = 0;
			if (query != null)
			{
				query.TryGetArrayInt("s", out enableTags);
				query.TryGetInt("o", out offset);
			}

			var mediaJson = $"{{}}";	// データが取得できなかった場合
			dbFile.GetFileList(db, enableTags, offset, _maxReadData,
				(List<MEDIA_FILE_INFO> mediaList, Dictionary<long, List<TAG_LINK_NAME>> tagInfo, int totalCount) =>
			{
				var ml = mediaList.Select(m =>
				{
					tagInfo.TryGetValue(m.id, out var tagList);
					var list = (tagList == null) ?
								null : tagList.Select(t => new { name = t.tag_name, color= t.color });
					return new
					{
						file_id = UIUtil.GetMediaLinkName(m),
						tag_list = list
					};
				});

				// 最大表示数取得できたなら次のオフセットを設定。なければ0以下をいれて終了
				offset = (mediaList.Count >= _maxReadData) ? offset + _maxReadData : -1;

				mediaJson = JsonSerializer.Serialize(new
				{
					server_msg = "",
					media_parts_list = ml,
					total = totalCount,
					item_offset = offset
				});
			});

			return mediaJson;
		}
	}
}
