using DotLiquid;
using DotLiquid.Util;
using System.Collections.Generic;
using System.IO;
using System.Linq;
using System.Text.Json;
using System.Windows.Shell;
using videoEditor;
using static tagVideoManager.dbFile;

namespace tagVideoManager
{
	internal class UIVideoPlayer : UIBase
	{
		public const string TmpFileName = "video_player.html";
		public const string TestFileName = "test.html";


		public override string Show(Server server, string filePath, Query query)
		{
			var name = Path.GetFileName(filePath);

			if ((name == TmpFileName) ||
				(name == TestFileName))	// 表示テスト
			{
				var basePath = Path.Combine(server.RootPath, name);
				return Show(server, query, basePath);
			}

			// タグリスト更新要求
			if (name == "video_update_taglist.custom_text")
			{
				return UIList.GetTagListJson(server.Db);
			}

			// 途中再生更新要求
			if (name == "video_update_play_point.custom_text")
			{
				return GetPlayMenuJson(server.Db, query);
			}

			return string.Empty;
		}


		private static readonly Dictionary<string, string> _typeList = new Dictionary<string, string>
		{
			{".webm", "webm"},
			{".ogm", "ogg"},
			{".ogv", "ogg"},
			{".ogg", "ogg"},
			{".mpeg", "mpeg"},
		};

		// 拡張子からビデオタイプ取得
		public static string GetVideoType(string ext)
		{
			if (_typeList.TryGetValue(ext, out var result))
				return result;

			return "mp4";	// ない場合はmp4にしておく
		}



		// 
		public static string Show(Server server, Query query, string basePath)
		{
			var db = server.Db;
			query.TryGetString("mt", out var mediaLinkName);

			var ids = UIUtil.GetFileIds(mediaLinkName);
			var mediaInfo = dbFile.GetMediaInfo(db, ids);
			var videoPath = ids.GetFilePath();

			var ext = GetVideoType(Path.GetExtension(videoPath));

			var hash = Hash.FromAnonymousObject(new
			{
				file_path = videoPath,
				video_ext = ext,
				media_id = mediaInfo.id,
				media_type = mediaInfo.media_type,
				vr_dome = mediaInfo.vr_dome,
				vr_source_type = mediaInfo.vr_source_type,
				initTagString = UIList.GetTagListJson(db),
				list_url = UIList.ListHtmlName,
				file_id = mediaLinkName
			});
			server.Localize.AddText(hash); // 翻訳データ追加

			var tmp = Template.Parse(File.ReadAllText(basePath));
			return tmp.Render(hash);
		}




		// タグ別時間指定再生
		/*
			mt= 対象メディアのfile_id
		 */
		public static string GetPlayMenuJson(DB db, Query query)
		{
			// クエリから複数のパラメータ取得
			query.TryGetString("mt", out var mediaLinkName);
			var mediaInfo = UIUtil.GetFileIds(mediaLinkName);


			var tagInfo = dbTag.GetTagLinkInfo(db, mediaInfo);

			// タグ用の情報がない場合は付与
			if (!tagInfo.Any(d => d.Value.First().tag_type == (int)dbTag.tag_type.digest))
			{
				CreateDigest(db, mediaInfo);	// ダイジェスト作成
				tagInfo = dbTag.GetTagLinkInfo(db, mediaInfo);　// 情報を再取得
			}

			tagInfo.First().Value.First();

			return JsonSerializer.Serialize(new
			{
				file_id = mediaLinkName,
				tag_list = tagInfo.Select(t =>
				{
					var firstTimeInfo = t.Value?.FirstOrDefault();
					var color = (firstTimeInfo.tag_type == (int)dbTag.tag_type.normal)
								? (firstTimeInfo.color.Substring(0, firstTimeInfo.color.Length -2) + "65")
								: "#00000000";

					return new
					{
						id = t.Value.First().tag_id,
						name = t.Key,
						color = color,
						time_list = t.Value.Select(v => new
						{
							id = v.tag_link_id,
							second = v.start_time,
							type = v.tag_type,
							link_img = $"{v.tag_link_id}.link_img", //
							disp = UIUtil.GetTimeString(v.start_time)
						})
					};
				}),
			});
		}

		// ダイジェスト作成
		private static void CreateDigest(DB db, MEDIA_FILE_IDS mediaInfo)
		{
			var tagId = dbTag.GetDigestTagId(db);
			var mediaId = dbFile.GetMediaId(db, mediaInfo);
			var filePath = mediaInfo.GetFilePath();

			using (var video = new VideoController(filePath))
			{
				var totalTime = video.TotalTime;

				// 動画を開くのに失敗した場合
				if (totalTime <= 0)
					return;

				var addTime = totalTime / 9;
				var picTime = addTime;
				for (int i = 0; i < 8; i++)
				{
					using (var bmp = video.GetImage((float)picTime))
					{
						var image = CnvBmp2SmallBmpByte(bmp);
						if (image == null)
							break;

						dbTag.AddTagLink(db, mediaId, tagId, image, (float)picTime);
					}
					picTime += addTime;
				}
			}
		}

	}
}
