using DotLiquid;
using DotLiquid.Util;
using System.IO;
using System.Linq;
using System.Text.Json;
using videoEditor;
using static tagVideoManager.dbFile;

namespace tagVideoManager
{
	internal class UIVideoVr : UIBase
	{
		public const string TmpFileName = "video_vr.html";

		public static Template GetTemplate(Server owner)
		{
			var tmpPath = Path.Combine(owner.RootPath, TmpFileName);
			return Template.Parse(File.ReadAllText(tmpPath));
		}


		public override string Show(Server server, string filePath, Query query)
		{
			var name = Path.GetFileName(filePath);

			if (name == TmpFileName)
			{
				return Show(server, query);
			}

			return string.Empty;
		}



		// 
		public static string Show(Server server, Query query)
		{
			var db = server.Db;
			if (!query.TryGetLong("meId", out var mediaId))
				return string.Empty;

			var mediaInfo = dbFile.GetMediaInfo(db, mediaId);
			if (mediaInfo == null)
				return string.Empty;

			var tmp = GetTemplate(server);
			var hash = Hash.FromAnonymousObject(new
			{
				file_id = UIUtil.GetMediaLinkName(mediaInfo),
				media_id = mediaInfo.id,
				media_type = mediaInfo.media_type,
				vr_dome = mediaInfo.vr_dome,
				vr_source_type = mediaInfo.vr_source_type,
			});
			server.Localize.AddText(hash); // 翻訳データ追加

			return tmp.Render(hash);
		}

	}
}
