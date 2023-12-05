using DotLiquid;
using DotLiquid.Util;
using System.IO;
using System.Linq;
using System.Text.Json;
using videoEditor;
using static tagVideoManager.dbFile;

namespace tagVideoManager
{
	internal class UIVideoPlayer : UIBase
	{
		public const string TmpFileName = "video_player.html";

		public static Template GetTemplate(Server owner)
		{
			var tmpPath = Path.Combine(owner.RootPath, TmpFileName);
			return Template.Parse(File.ReadAllText(tmpPath));
		}


		public override string Show(Server server, string filePath, string query)
		{
			var name = Path.GetFileName(filePath);

			if (name == TmpFileName)
			{
				return Show(server, query);
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



		// 
		public static string Show(Server server, string query)
		{
			var tmp = GetTemplate(server);
			var db = server.Db;
			var mediaLinkName = Utility.GetQueryValue(query, "mt");
			var ids = UIUtil.GetFileIds(mediaLinkName);
			var mediaInfo = dbFile.GetMediaInfo(db, ids.volume_serial, ids.file_id);
			var videoPath = (ids != null)
						? FileIdHelper.GetFilePath(ids.volume_serial, (long)ids.file_id)
						: "no path";



			var hash = Hash.FromAnonymousObject(new
			{
				file_path = videoPath,
				media_id = mediaInfo.id,
				media_type = mediaInfo.media_type,
				vr_dome = mediaInfo.vr_dome,
				vr_source_type = mediaInfo.vr_source_type,
				initTagString = UIList.GetTagListJson(db),
				list_url = UIList.ListHtmlName,
				file_id = mediaLinkName
			});
			server.Localize.AddText(hash); // 翻訳データ追加

			return tmp.Render(hash);
		}




		// タグ別時間指定再生
		/*
			mt= 対象メディアのfile_id
		 */
		public static string GetPlayMenuJson(DB db, string query)
		{
			// クエリから複数のパラメータ取得
			var mediaLinkName = Utility.GetQueryValue(query, "mt");
			var mediaInfo = UIUtil.GetFileIds(mediaLinkName);


			var tagInfo = dbTag.GetTagLinkInfo(db, mediaInfo.volume_serial, mediaInfo.file_id);

			// タグ用の情報がない場合は付与
			if (!tagInfo.Any(d => d.Value.First().tag_type == (int)dbTag.tag_type.digest))
			{
				CreateDigest(db, mediaInfo);	// ダイジェスト作成
				tagInfo = dbTag.GetTagLinkInfo(db, mediaInfo.volume_serial, mediaInfo.file_id);　// 情報を再取得
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
		private static void CreateDigest(DB db, MEDIA_FILE_INFO mediaInfo)
		{
			var tagId = dbTag.GetDigestTagId(db);
			var mediaId = dbFile.GetMediaId(db, mediaInfo);
			var filePath = FileIdHelper.GetFilePath(mediaInfo.volume_serial, (long)mediaInfo.file_id);

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
