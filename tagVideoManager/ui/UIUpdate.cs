using DotLiquid.Util;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using static tagVideoManager.dbFile;

namespace tagVideoManager
{
	// クエリからのデータ更新処理
	internal class UIUpdate
	{

		// クエリの情報からデータを更新
		/*
			at=[タグ名] :タグの追加
			mt=[file_id]:対象のメディア(汎用)
			 t=[tag_id] :タグリンクの追加(対象:mtにタグ:tを追加。開始時間:tiはあれば設定なければ0)
			ti=[数値]	:追加タグリンクの再生時間(あれば)
			rv=[file_id]:管理から外すメディア
			rt=[tagId(複数)] : タグの削除
			rl=[tag_link_id] タグリンク削除
			rla=[tagId] 任意のメディアのタグIDのリンクを全て削除(対象:mtからタグID:rlaのリンクを全て削除)
			tht=[数値]	:サムネイルにする動画の時間

		*/
		public static void UpdateInfo(DB db, string query)
		{
			var queryParam = Utility.GetQueryValues(query);

			// タグの追加
			TryAddTag(db, queryParam);

			// タグリンクの追加
			TryAddTagLink(db, queryParam);

			// 動画を管理から外す
			TryRemoveVideo(db, queryParam);

			// タグの削除
			if (queryParam.TryGetValue("rt", out var removeTagList))
			{
				var removeTags = UIUtil.ParseArrayNum(removeTagList);
				dbTag.RemoveTag(db, removeTags);
			}

			// パラメータがあればタグリンク削除
			// 同じカテゴリのタグがない場合はstartTimeを０にして残しておく
			TryRemoveTagLink(db, queryParam);

			// 任意のメディアから任意のタグのリンクを全て削除
			TryRemoveTagLinkAll(db, queryParam);

			// サムネイル更新
			TryUpdateThumbnail(db, queryParam);

		}

		// サムネイル更新
		private static void TryUpdateThumbnail(DB db, Dictionary<string, string> queryParam)
		{
			if (!queryParam.TryGetValue("mt", out var mediaLinkName) ||
				!queryParam.TryGetValue("tht", out var timeString))
				return;

			if (!float.TryParse(timeString, out var startTime))
				return;

			var mediaInfo = UIUtil.GetFileIds(mediaLinkName);
			var mediaId = dbFile.GetMediaId(db, mediaInfo);
			var filePath = FileIdHelper.GetFilePath(mediaInfo.volume_serial, (long)mediaInfo.file_id);
			var miniImage = dbFile.CreateMiniImage(filePath, startTime);
			dbFile.UpdateMiniImage(db, mediaId, miniImage);
		}


		// 動画を管理から外す
		private static void TryRemoveVideo(DB db, Dictionary<string, string> queryParam)
		{
			if (!queryParam.TryGetValue("rv", out var removeMediaName))
				return;

			var mediaInfo = UIUtil.GetFileIds(removeMediaName);
			var mediaId = dbFile.GetMediaId(db, mediaInfo);

			// メディアと関係するタグリンクの削除
			dbTag.RemoveTagLinkMedia(db, mediaId);

			// メディアの削除
			dbFile.RemoveMedia(db, mediaId);
		}


		// クエリ文字列からタグを追加
		private static void TryAddTag(DB db, Dictionary<string, string> queryParam)
		{
			if (!queryParam.TryGetValue("at", out var addTagId))
				return;

			var tagName = System.Web.HttpUtility.UrlDecode(addTagId);
			tagName = tagName.Trim();
			if (string.IsNullOrEmpty(tagName))
				return;

			dbTag.AddTag(db, tagName);
		}


		// クエリ文字列からタグリンク追加
		private static void TryAddTagLink(DB db, Dictionary<string, string> queryParam)
		{
			if (!queryParam.TryGetValue("mt", out var mediaLinkName) ||
				!queryParam.TryGetValue("t", out var tagIdStr))
				return;

			// 追加タグ情報が取れるならタグを追加
			if (!int.TryParse(tagIdStr, out var tagId))
				return;

			// あれば開始時間取得
			var startTime = 0.0f;
			if (queryParam.TryGetValue("ti", out var startTimeString))
			{
				float.TryParse(startTimeString, out startTime);
			}

			var mediaInfo = UIUtil.GetFileIds(mediaLinkName);
			// メディアにタグを追加
			dbTag.AddTagLink(db, mediaInfo, tagId, startTime);
		}


		// パラメータがあればタグリンク削除
		// 同じカテゴリのタグがない場合はstartTimeを０にして残しておく
		private static void TryRemoveTagLink(DB db, Dictionary<string, string> queryParam)
		{
			if (!queryParam.TryGetValue("rl", out var tagLinkIdString))
				return;

			if (!int.TryParse(tagLinkIdString, out var tagLinkId))
				return;

			// ２つ以上あるなら削除する
			if (dbTag.SameTagCount(db, tagLinkId) >= 2)
			{
				dbTag.RemoveTagLink(db, tagLinkId);
			}
			// 最後の１つの場合はタグの存在確認用にスタート時間を０にして残しておく
			else
			{
				dbTag.UpdateTagLinkStartTime(db, tagLinkId, 0.0f);
			}
		}


		// パラメータがあればタグを全て削除
		private static void TryRemoveTagLinkAll(DB db, Dictionary<string, string> queryParam)
		{
			// 必要なパラメータが存在するか
			if (!queryParam.TryGetValue("mt", out var mediaLinkName) || 
				!queryParam.TryGetValue("rla", out var tagIdString))
				return;

			if (!int.TryParse(tagIdString, out var tagId))
				return;

			var mediaInfo = UIUtil.GetFileIds(mediaLinkName);
			var mediaId = dbFile.GetMediaId(db, mediaInfo);
			dbTag.RemoveTagLink(db, mediaId, tagId);
		}


	}
}
