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
			meId=[media_id]: メディアのユニークID(汎用)
			 t=[tag_id] :タグリンクの追加(対象:mtにタグ:tを追加。開始時間:tiはあれば設定なければ0)
			ti=[数値]	:追加タグリンクの再生時間(あれば)
			rv=[file_id]:管理から外すメディア
			rt=[tagId(複数)] : タグの削除
			rl=[tag_link_id] タグリンク削除
			rla=[tagId] 任意のメディアのタグIDのリンクを全て削除(対象:mtからタグID:rlaのリンクを全て削除)
			tht=[数値]	:サムネイルにする動画の時間
			vr_toggle=[0 or 1] : 0:通常動画 1:VR動画
			vr_half=[180 or 360] : 180:ハーフドーム 360:フルドーム
			vr_mode=[0-2]: 0:MODE_MONOSCOPIC 1:MODE_SIDEBYSIDE 2:MODE_TOPBOTTOM

		*/
		public static void UpdateInfo(DB db, Query query)
		{
			// タグの追加
			TryAddTag(db, query);

			// タグリンクの追加
			TryAddTagLink(db, query);

			// 動画を管理から外す
			TryRemoveVideo(db, query);

			// タグの削除
			if (query.TryGetString("rt", out var removeTagList))
			{
				var removeTags = UIUtil.ParseArrayNum(removeTagList);
				dbTag.RemoveTag(db, removeTags);
			}

			// パラメータがあればタグリンク削除
			// 同じカテゴリのタグがない場合はstartTimeを０にして残しておく
			TryRemoveTagLink(db, query);

			// 任意のメディアから任意のタグのリンクを全て削除
			TryRemoveTagLinkAll(db, query);

			// サムネイル更新
			TryUpdateThumbnail(db, query);

			// 
			TryUpdateVrMode(db, query);
		}

		// VR設定の更新
		private static void TryUpdateVrMode(DB db, Query query)
		{
			if (!query.TryGetUlong("meId", out var mediaId))
				return;

			if (query.TryGetInt("vr_toggle", out var vrType))
			{
				dbFile.UpdateMedia(db, mediaId, "media_type", vrType);
			}

			if (query.TryGetInt("vr_half", out var vrDome))
			{
				dbFile.UpdateMedia(db, mediaId, "vr_dome", vrDome);
			}

			if (query.TryGetInt("vr_mode", out var vrSource))
			{
				dbFile.UpdateMedia(db, mediaId, "vr_source_type", vrSource);
			}
		}


		// サムネイル更新
		private static void TryUpdateThumbnail(DB db, Query query)
		{
			if (!query.TryGetString("mt", out var mediaLinkName) ||
				!query.TryGetFloat("tht", out var startTime))
				return;

			var mediaInfo = UIUtil.GetFileIds(mediaLinkName);
			var mediaId = dbFile.GetMediaId(db, mediaInfo);
			var filePath = FileIdHelper.GetFilePath(mediaInfo.volume_serial, (long)mediaInfo.file_id);
			var miniImage = dbFile.CreateMiniImage(filePath, startTime);
			dbFile.UpdateMiniImage(db, mediaId, miniImage);
		}


		// 動画を管理から外す
		private static void TryRemoveVideo(DB db, Query query)
		{
			if (!query.TryGetString("rv", out var removeMediaName))
				return;

			var mediaInfo = UIUtil.GetFileIds(removeMediaName);
			var mediaId = dbFile.GetMediaId(db, mediaInfo);

			// メディアと関係するタグリンクの削除
			dbTag.RemoveTagLinkMedia(db, mediaId);

			// メディアの削除
			dbFile.RemoveMedia(db, mediaId);
		}


		// クエリ文字列からタグを追加
		private static void TryAddTag(DB db, Query query)
		{
			if (!query.TryGetString("at", out var addTagId))
				return;

			var tagName = System.Web.HttpUtility.UrlDecode(addTagId);
			tagName = tagName.Trim();
			if (string.IsNullOrEmpty(tagName))
				return;

			dbTag.AddTag(db, tagName);
		}


		// クエリ文字列からタグリンク追加
		private static void TryAddTagLink(DB db, Query query)
		{
			if (!query.TryGetString("mt", out var mediaLinkName) ||
				!query.TryGetInt("t", out var tagId))
				return;


			// あれば開始時間取得
			if (!query.TryGetFloat("ti", out var startTime))
			{
				startTime = 0.0f;
			}

			var mediaInfo = UIUtil.GetFileIds(mediaLinkName);
			// メディアにタグを追加
			dbTag.AddTagLink(db, mediaInfo, tagId, startTime);
		}


		// パラメータがあればタグリンク削除
		// 同じカテゴリのタグがない場合はstartTimeを０にして残しておく
		private static void TryRemoveTagLink(DB db, Query query)
		{
			if (!query.TryGetInt("rl", out var tagLinkId))
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
		private static void TryRemoveTagLinkAll(DB db, Query query)
		{
			// 必要なパラメータが存在するか
			if (!query.TryGetString("mt", out var mediaLinkName) || 
				!query.TryGetInt("rla", out var tagId))
				return;

			var mediaInfo = UIUtil.GetFileIds(mediaLinkName);
			var mediaId = dbFile.GetMediaId(db, mediaInfo);
			dbTag.RemoveTagLink(db, mediaId, tagId);
		}


	}
}
