using System;
using System.Collections.Generic;
using System.Diagnostics;
using System.Drawing;
using System.Drawing.Imaging;
using System.IO;
using System.Linq;
using System.Security.Cryptography.X509Certificates;
using System.Windows.Shell;
using videoEditor;
using static tagVideoManager.dbTableMaker;
using static tagVideoManager.FileIdHelper;

namespace tagVideoManager
{
	// DBのファイル１つに対する操作
	public class dbFile
	{
		public const int _miniImageFrame = 200;    // アイコンに使う画像のフレーム
		// サムネイルの画像サイズ
		private const int _miniImageW = 160;
		private const int _miniImageH = 100;


		public class MEDIA_FILE_IDS
		{
			public ulong volume_serial = 0; // ボリュームシリアルナンバー
			public ulong file_id = 0;   // ファイル固有ID


			public readonly static MEDIA_FILE_IDS Empty = new MEDIA_FILE_IDS();

			public static MEDIA_FILE_IDS Create(FileIdInfo ids)
			{
				return new MEDIA_FILE_IDS()
				{
					volume_serial = ids.volumeSerial,
					file_id = ids.fileId
				};
			}

			// 値が入っているか
			public bool IsValid { get { return (volume_serial != 0) && (file_id != 0); } }

			// ファイルパス取得
			public string GetFilePath()
			{
				try
				{
					return FileIdHelper.GetFilePath(volume_serial, (long)file_id);
				}
				catch (Exception)
				{
					return "no path";
				}
			}
		}

		// 
		public class MEDIA_FILE_INFO : MEDIA_FILE_IDS
		{
			public long id = 0;
			public byte[] mini_image = new byte[] { };   // サムネイル
			public int media_type = 0;
			public int vr_dome = 0;
			public int vr_source_type = 0;
		}


		// ファイル１つをDBに追加
		public static ulong Import(DB db, string filePath)
		{
			if (!System.IO.File.Exists(filePath))
			{
				throw new Exception($"ファイルインポートエラー");
			}

			// File IDを取得
			var idInfo = FileIdHelper.GetFileId(filePath);
			var mediaId = GetMediaId(db, MEDIA_FILE_IDS.Create(idInfo));

			// 既に登録済ならそれを返す
			if (mediaId > 0)
				return mediaId;

			// サムネイル取得
			var miniImage = CreateMiniImage(filePath);

			// 既にある場合は無処理
			// ファイル情報登録
			db.Execute(
				$@"insert into 
					media_file (volume_serial, file_id, mini_image, register_time, create_time) 
					values(@volume_serial, @file_id, @mini_image, strftime('%s','now'), @create_time) 
					on conflict(file_id) do nothing",
				new[] { new 
				{
					volume_serial = idInfo.volumeSerial, 
					file_id = idInfo.fileId, 
					mini_image = miniImage,
					create_time = idInfo.createTime
				}});

			return GetMediaId(db, MEDIA_FILE_IDS.Create(idInfo));
		}

		// サムネイル更新
		public static void UpdateMiniImage(DB db, ulong mediaId, byte[] image)
		{
			db.Execute(@"update media_file set mini_image = @image where id = @mediaId",
						new { image, mediaId });
		}

		public static void UpdateMedia(DB db, ulong mediaId, string key, int value)
		{
			db.Execute(@$"update media_file set '{key}' = @value where id = @mediaId",
														new { mediaId, value});
		}


		// タグ名前付きリンク情報
		public struct TAG_LINK_NAME
		{
			public long media_id;
			public long tag_id;
			public string tag_name;
			public string color;
		}

		// メディアIDからタグリンクを取得
		public static List<TAG_LINK_NAME> GetTagLink(DB db, ulong mediaId)
		{
			return db.Query<TAG_LINK_NAME>(
				@"select DISTINCT 
							tag_link.media_id, 
							tag_link.tag_id, 
							tag_list.name as tag_name,
							tag_list.color as color
						from
							tag_link 
						left outer join tag_list on
							tag_link.tag_id = tag_list.tag_id 
						where
							tag_link.media_id = @media_id and 
							tag_list.tag_type = @tag_type",
						new { media_id = mediaId, tag_type = (int)dbTag.tag_type.normal }
				).ToList();
		}


		// 任意の条件(タグを持つ）ファイルリストを取得。サムネイル画像は取得しない
		public static void GetFileList(DB db, List<int> enableTags, int offset, int maxData,
						Action<List<MEDIA_FILE_INFO>, Dictionary<long,List<TAG_LINK_NAME>>, int> func )
		{
			// フィルタリングがない場合は全て出す
			if (enableTags.Count <= 0)
			{
				var all = db.Query<MEDIA_FILE_INFO>(
									@"select 
										id, 
										volume_serial, 
										file_id 
									from 
										media_file
									order by 
										register_time DESC, create_time DESC
									limit
										@offset, @max_data",
									new { max_data = maxData, offset = offset }).ToList();

				var linkInfoAll = db.Query<TAG_LINK_NAME>(
						@"select DISTINCT 
							tag_link.media_id, 
							tag_link.tag_id, 
							tag_list.name as tag_name,
							tag_list.tag_type as tag_type,
							tag_list.color as color
						from
							tag_link 
						left outer join tag_list on
							tag_link.tag_id = tag_list.tag_id 
						where
							tag_list.tag_type = @tag_type
						order by
							tag_link.media_id",
							new { tag_type = (int)dbTag.tag_type.normal })
								.GroupBy(g => g.media_id)
								.ToDictionary(g => g.Key, g => g.ToList());

				// 全メディア数取得
				var totalCount = db.Query<int>(@"select count(*) from media_file").First();

				func.Invoke(all, linkInfoAll, totalCount);
				return;
			}

			// enableTagsのいずれかを持つメディアidとそれが持つタグをDictionaryとして取得
			var linkInfo = db.Query<TAG_LINK_NAME>(
						@"select DISTINCT 
							tag_link.media_id, 
							tag_link.tag_id, 
							tag_list.name as tag_name,
							tag_list.color as color
						from
							tag_link 
						left outer join tag_list on
							tag_link.tag_id = tag_list.tag_id 
						where
							tag_link.tag_id in @tag_id 
						order by
							tag_link.media_id
						",
						new { tag_id = enableTags })
							.GroupBy(g => g.media_id)
							.ToDictionary(g => g.Key, g => g.ToList());

			// 上で取得したメディアの追加情報を取得
			var media_ids = linkInfo.Select(g => g.Key).ToArray();
			var mediaList = db.Query<MEDIA_FILE_INFO>(
						@"select
							id,
							volume_serial,
							file_id 
						from 
							media_file 
						where
							id in @media_id
						order by 
							register_time DESC, create_time DESC
						limit
							@offset, @max_data",
						new { media_id = media_ids, max_data = maxData, offset = offset }).ToList();

			func.Invoke(mediaList, linkInfo, media_ids.Length);
		}

		// 任意のファイルIDのサムネイル画像を取得
		public static byte[] GetMiniImage(DB db, MEDIA_FILE_IDS mediaId)
		{
			var result = db.Query<MEDIA_FILE_INFO>(
						$@"select 
							mini_image 
						from 
							media_file 
						where
							volume_serial='{mediaId.volume_serial}' and 
							file_id ='{mediaId.file_id}'");
			var data = result.FirstOrDefault();
			if (data == null)
				return null;

			return data.mini_image;
		}

		// メディアID取得
		public static ulong GetMediaId(DB db, MEDIA_FILE_IDS mediaId)
		{
			var info = GetMediaInfo(db, mediaId);
			return (info == null) ? 0 : (ulong)info.id;
		}


		public static MEDIA_FILE_INFO GetMediaInfo(DB db, MEDIA_FILE_IDS mediaId)
		{
			var result = db.Query<MEDIA_FILE_INFO>(
						$@"select 
							id, 
							media_type,
							vr_dome,
							vr_source_type
						from 
							media_file 
						where
							volume_serial=@volumeSerial and 
							file_id =@fileId", new { volumeSerial = mediaId.volume_serial, fileId = mediaId.file_id});
			var retVal = result.FirstOrDefault();

			return (retVal == null) ? new MEDIA_FILE_INFO() : retVal;
		}


		// メディアIDから情報取得。最終的に全てこれにする
		public static MEDIA_FILE_INFO GetMediaInfo(DB db, long mediaId)
		{
			var result = db.Query<MEDIA_FILE_INFO>(
						@"select 
							id, 
							media_type,
							volume_serial, 
							file_id,
							vr_dome,
							vr_source_type
						from 
							media_file 
						where
							id=@mediaId", new { mediaId }); 
			return result.FirstOrDefault();
		}



		public static void RemoveMedia(DB db, ulong mediaId)
		{
			db.Execute(@$"delete from media_file where id = @media_id",
										new[] { new { media_id = mediaId } });
		}

		// ファイル管理テーブル作成
		public static void CreateTable(DB db) 
		{
			dbTableMaker.CreateTable(db, "media_file", new List<ColumInfo>()
			{
				new ColumInfo("id", ColumInfo.Type.Int, "primary key AUTOINCREMENT"),
				new ColumInfo("volume_serial", ColumInfo.Type.Int),
				new ColumInfo("file_id", ColumInfo.Type.Int, "UNIQUE"),
				new ColumInfo("mini_image", ColumInfo.Type.Blob),
				new ColumInfo("register_time", ColumInfo.Type.Int, "default 0"),
				new ColumInfo("create_time", ColumInfo.Type.Int, "default 0"),
				// 0:動画 1:VR動画
				new ColumInfo("media_type", ColumInfo.Type.Int, "default 0"),
				// 180 or 360
				new ColumInfo("vr_dome", ColumInfo.Type.Int, "default 180"),
				// 0:１枚絵 1:左右分割 2:上下分割
				new ColumInfo("vr_source_type", ColumInfo.Type.Int, "default 0"),
			});
		}




		// 1.0を1秒とした時間指定からサムネイル作成
		public static byte[] CreateMiniImage(string filePath, float time)
		{
			using (var video = new VideoController(filePath))
			{
				using (var bmp = video.GetImage(time))
				{
					return CnvBmp2SmallBmpByte(bmp);
				}
			}
		}


		// サムネイル作成
		public static byte[] CreateMiniImage(string filePath, int frame = _miniImageFrame)
		{
			using (var video = new VideoController(filePath))
			{
				// 表示フレーム
				var showFrame = Math.Min(Math.Max(frame, 0), video.FrameCount - 1);
				using (var bmp = video.GetImage(showFrame))
				{
					return CnvBmp2SmallBmpByte(bmp);
				}
			}
		}


		// Bmpからサイズの小さいBmpのバイト配列を取得
		public static byte[] CnvBmp2SmallBmpByte(Bitmap bmp)
		{
			if (bmp == null)
				return null;

			// 倍率の小さい方を採用する
			var scale = Math.Min((float)_miniImageW / bmp.Width, (float)_miniImageH / bmp.Height);
			using (var ms = new MemoryStream())
			{
				using (var miniBmp = new Bitmap(bmp, (int)(scale * bmp.Width), (int)(scale * bmp.Height)))
				{
					miniBmp.Save(ms, ImageFormat.Jpeg);
				}
				return ms.ToArray();
			}
		}
	}
}
