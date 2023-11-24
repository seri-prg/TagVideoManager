using DotLiquid.Util;
using DotLiquid;
using System;
using System.Collections.Generic;
using System.Diagnostics;
using System.Linq;
using System.Security.Cryptography.X509Certificates;
using System.Text;
using System.Threading.Tasks;
using static System.Windows.Forms.VisualStyles.VisualStyleElement;
using static tagVideoManager.dbFile;
using static tagVideoManager.dbTableMaker;
using System.Windows.Controls.Primitives;
using System.Windows.Forms;

namespace tagVideoManager
{
	// タグ関係操作
	internal class dbTag
	{

		private const string _tagDigestName = "------";	// ダイジェストタグの名前

		public enum tag_type : int
		{
			normal = 100, // 通常のタグ
			digest = 150, // 内部で使う用
			secret = 50, // 非表示(後の拡張用)
		}


		// ファイル管理テーブル作成
		public static void CreateTable(DB db)
		{
			// タグリスト
			dbTableMaker.CreateTable(db, "tag_list", new List<ColumInfo>()
			{
				new ColumInfo("tag_id", ColumInfo.Type.Int, "primary key AUTOINCREMENT"),
				new ColumInfo("name", ColumInfo.Type.Text, "UNIQUE"),
				new ColumInfo("tag_type", ColumInfo.Type.Int, "default 100"),
				new ColumInfo("color", ColumInfo.Type.Text)
			});


			// タグとメディアの紐づけ
			// media_id、tag_idが一致するレコードは複数存在しうる。
			var query = @"CREATE TABLE IF NOT EXISTS tag_link(
				tag_link_id integer primary key AUTOINCREMENT,
				media_id integer,
				tag_id integer,
				start_time real,
				mini_image blob
			);";

			db.Execute(query);


			AddTag(db, _tagDigestName, tag_type.digest);	// ダイジェストタグが無ければ追加

			// ダミーデータを入れる
			// DebugCreateTable(db);
		}

		// ダイジェストタグ取得
		public static int GetDigestTagId(DB db)
		{
			return db.Query<int>(
				@"select tag_id from tag_list
					where name = @name and tag_type = @tag_type",
				new { name = _tagDigestName, tag_type = (int)tag_type.digest }).FirstOrDefault();
		}

		// タグリンク情報
		public class TAG_LINK_INFO
		{
			public long tag_link_id;
			public long media_id;
			public long tag_id;
			public float start_time;
			public byte[] mini_image;   // サムネイル
		}

		// タグ名付きタグリンク情報
		public class TAG_LINK_NAME_INFO : TAG_LINK_INFO
		{
			public string tag_name;
			public string color; // タグカラー
			public int tag_type; // タグのタイプ
		}

		// タグ情報
		public class TAG_INFO
		{
			public long tag_id;
			public string name;
			public string color;
		}


		// 全てのタグ情報を取得
		public static List<TAG_INFO> GetTagList(DB db)
		{
			return db.Query<TAG_INFO>($"select * from tag_list where tag_type = $tag_type",
											new { tag_type = (int)tag_type.normal}).ToList();
		}

		public class TAG_TEABLE_INFO
		{
			public long max_row_id;
			public long tag_list_count;

			// テーブルの状態が以前の状態と同じか否か
			public bool IsSame(TAG_TEABLE_INFO tf)
			{
				return ((this.max_row_id == tf.max_row_id) && (this.tag_list_count == tf.tag_list_count));
			}
		}

		// タグの情報取得
		public static TAG_TEABLE_INFO GetTagListTeableInfo(DB db)
		{
			return db.Query<TAG_TEABLE_INFO>(
					@"select max(ROWID) as max_row_id, count(*) as tag_list_count 
									from tag_list").First();
		}


		// タグ名からIDリスト取得
		public static List<int> GetTagIdList(DB db, IEnumerable<string> names)
		{
			if (names == null)
				return new List<int>();

			return db.Query<int>($"select tag_id from tag_list where tag_type = $tag_type and name in @tag_name",
											new 
											{
												tag_name = names.ToArray(), 
												tag_type = (int)tag_type.normal 
											}).ToList();
		}


		// １つ以上のmediaIdから参照しているタグを取得
		public static List<TAG_LINK_INFO> GetTagLinkInfo(DB db, ulong[] mediaId)
		{
			return db.Query<TAG_LINK_INFO>(
					@$"select
						* 
					from
						tag_link 
					where
						media_id in @media_id
					order by
						media_id, tag_id, start_time;",
					new { media_id  = mediaId}).ToList();
		}


		// 特定のメディアファイルのタグを名前付きで取得
		public static Dictionary<string, List<TAG_LINK_NAME_INFO>> GetTagLinkInfo(DB db, ulong serialVlume, ulong file_id)
		{
			return db.Query<TAG_LINK_NAME_INFO>(
			   @$"select 
					tag_link.tag_link_id,
					tag_link.media_id, 
					tag_link.tag_id, 
					tag_link.start_time,
					tag_list.name as tag_name,
					tag_list.color as color,
					tag_list.tag_type as tag_type
				from
					tag_link
				left outer join tag_list on
					tag_link.tag_id = tag_list.tag_id
				where 
					tag_link.media_id = 
							(select id from media_file where file_id = @file_id and volume_serial = @volume_serial)
				order by
					tag_list.tag_type, tag_link.tag_id, tag_link.start_time",
			   new { file_id  = file_id, volume_serial = serialVlume })
					.GroupBy(g => g.tag_name)
					.ToDictionary(g => g.Key, g => g.ToList());	// タグ名でグループ化
		}

		// タグの画像出力
		public static byte[] GetLinkImage(DB db, int tagLinkId)
		{
			var result = db.Query<TAG_LINK_INFO>(
					$@"select
						mini_image
					from 
						tag_link
					where
						tag_link_id = @tag_link_id",
					new { tag_link_id = tagLinkId });


			var tagInfo = result.FirstOrDefault();

			return tagInfo?.mini_image ?? null;
		}

		// 同じデータが無ければメディアにタグを追加
		public static void AddTagLink(DB db, MEDIA_FILE_INFO mediaInfo, int tagId, float startTime)
		{
			var mediaId = dbFile.GetMediaId(db, mediaInfo);
			var filePath = FileIdHelper.GetFilePath(mediaInfo.volume_serial, (long)mediaInfo.file_id);
			byte[] miniImage = null;

			// ほぼ０の場合はリンク画像は必要ない
			if (startTime > 0.0001f)
			{
				miniImage = dbFile.CreateMiniImage(filePath, startTime);
			}

			AddTagLink(db, mediaId, tagId, miniImage, startTime);
		}


		// タグリンクを追加するクエリ
		private const string _queryAddTagLink =
				@$"insert into 
					tag_link (media_id, tag_id, start_time, mini_image) 
				select
					@media_id, 
					@tag_id, 
					@start_time,
					@mini_image
				where not exists 
					(
						select
							1 
						from 
							tag_link 
						where 
							media_id = @media_id and
							tag_id = @tag_id and 
							abs(start_time - @start_time) < 0.001
					)";


		// 同じデータが無ければメディアにタグを追加
		public static void AddTagLink(DB db, ulong mediaId, int tagId, byte[] miniImage, float startTime)
		{
			db.Execute(_queryAddTagLink, new {
				media_id = mediaId,
				tag_id = tagId,
				start_time = startTime,
				mini_image = miniImage
			});
		}

		// 
		public static void AddTagLink(DB db, ulong mediaId, IEnumerable<int> tagIds)
		{
			db.Execute(_queryAddTagLink, tagIds.Select(t => new
			{
				media_id = mediaId,
				tag_id = t,
				start_time = 0.0f,
				mini_image = (byte[])null
			}));
		}


		// タグリンクのスタート時間変更
		public static void UpdateTagLinkStartTime(DB db, int tagLinkId, float startTime)
		{
			db.Execute(
				@$"update
					tag_link
				set 
					start_time = {startTime} 
				where
					tag_link_id = {tagLinkId}
				");
		}


		public static void RemoveTagLink(DB db, int tagLinkId)
		{
			db.Execute(
				@$"delete 
				from tag_link
				where 
					tag_link_id = {tagLinkId}");
		}

		// メディア内の任意のタグを全て削除
		public static void RemoveTagLink(DB db, ulong mediaId, int tagId)
		{
			db.Execute(
				@$"delete 
				from tag_link
				where 
					media_id = {mediaId} and
					tag_id = {tagId}");
		}

		// mediaIdの一致するタグリンクを全て削除
		public static void RemoveTagLinkMedia(DB db, ulong mediaId)
		{
			db.Execute(
				@$"delete 
				from tag_link
				where 
					media_id = {mediaId}");
		}


		// メディア内にtagLinkIdと同じ種類のタグがいくつあるか取得
		public static int SameTagCount(DB db, int tagLinkId)
		{
			var tagLink = db.Query<TAG_LINK_INFO>(
				$@"select 
						media_id,
						tag_id				
					from
						tag_link
					where
						tag_link_id = @tag_link_id
				", new {tag_link_id = tagLinkId}).FirstOrDefault();

			// 見つからない場合
			if (tagLink == null)
				return 0;

			var tagLinkCount = db.Query<int>(
				$@"select 
						count(*)
					from
						tag_link
					where
						media_id = @media_id and
						tag_id = @tag_id
				", new { media_id = tagLink.media_id, tag_id = tagLink.tag_id });

			return tagLinkCount.First();
		}


		// タグ追加クエリ
		private const string _queryAddTag =
					@$"insert into 
						tag_list (name, tag_type, color) 
						values (@name, @tag_type, @color)
					on conflict(name) do nothing";

		// 初期カラー
		public static readonly string[] defaultTagColor = new string[]
		{
			"#C00000C0", "#7F4000C0", "#407F00C0", "#00C000C0", "#007F40C0",
			"#00407FC0", "#0000C0C0", "#40007FC0", "#7F0040C0",
		};

		// タグの総数を取得
		private static int GetTagCount(DB db)
		{
			return db.Query<int>("select count(*) from tag_list").First();
		}

		// 同名が無ければタグを追加
		public static void AddTag(DB db, string tagName, tag_type tagType = tag_type.normal)
		{
			var tagCount = GetTagCount(db);
			db.Execute(_queryAddTag, new 
			{ 
				name = tagName, 
				tag_type = (int)tagType,
				color = defaultTagColor[tagCount % defaultTagColor.Length]
			});
		}

		// 複数タグ追加
		public static void AddTag(DB db, IEnumerable<string> tagNames, tag_type tagType = tag_type.normal)
		{
			if (tagNames == null)
				return;

			var tagCount = GetTagCount(db);
			db.Execute(_queryAddTag,
					tagNames.Select(t => new
					{
						name = t,
						tag_type = (int)tagType,
						color = defaultTagColor[(tagCount++) % defaultTagColor.Length]
					})); ;
		}



		// 任意のタグIDとそれを使うタグリンクを全て削除
		public static void RemoveTag(DB db, List<int> tagIds)
		{
			var ids = tagIds.Select(t => new { tag_id = t }).ToArray();
			db.Execute(@$"delete from tag_link where tag_id = @tag_id", ids);
			db.Execute(@$"delete from tag_list where tag_id = @tag_id", ids);
		}


		// ダミーデータを入れる
		private static void DebugCreateTable(DB db)
		{
			AddTag(db, "テスト");
			AddTag(db, "テスト1");
			AddTag(db, "テスト2");
			AddTag(db, "テスト3");
			AddTag(db, "テスト4");


			var list = GetTagList(db);
			foreach (var tag in list) 
			{
				Trace.WriteLine($"[tag]{tag}");
			}

			var linkList = GetTagLinkInfo(db, new ulong[] { 100, 111 });
			foreach (var item in linkList)
			{
				Trace.WriteLine($"[tag link]{item}");
			}
		}
	}
}
