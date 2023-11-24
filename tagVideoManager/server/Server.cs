using QRCoder;
using System;
using System.Collections.Generic;
using System.Diagnostics;
using System.Drawing;
using System.Drawing.Imaging;
using System.IO;
using System.Net;
using System.Net.Http;
using System.Net.WebSockets;
using System.Text;
using System.Threading;
using System.Windows.Data;
using System.Windows.Documents;
using System.Xml.Linq;
// using static System.Net.WebRequestMethods;

namespace tagVideoManager
{
	public class Server
	{
		private const string URL_KEY = "/tagVideoManager/";


		private HttpListener _listener = new HttpListener();
		private List<UIBase> _uiList = new List<UIBase>();


		public DB Db { get; private set; }

		public Localize Localize { get; private set; } = new Localize();



		public string UrlPrefix { get; private set; }

		// ドキュメントのルートパス
		public string RootPath { get { return @".\template\"; } }



		public string RootUrl { get { return $"http://{NetUtil.GetServerIP()}:10263{this.UrlPrefix}"; } }

		// 待ち受けるURL
		public string ListenerUrl { get { return $"http://+:10263{URL_KEY}"; } }

		// スタートURL
		public string StartUrl { get { return $"{RootUrl}{UIList.ListHtmlName}"; } }


		public Server(DB db)
		{
			UrlPrefix = $"{URL_KEY}{Utility.CreateRandomUrl(48)}/";
			Db = db;
			Localize.Setup(db);

			// ページ毎の処理を登録
			_uiList.AddRange(new UIBase[] 
			{
				new UIList(),
				new UIVideoPlayer()
			});
		}


        public void Start()
		{
			// netshにURLを登録
			NetUtil.RegistWindowsSetting(ListenerUrl, "tag Video Manager", 10263);

			_listener.Prefixes.Add(ListenerUrl);
			_listener.Start();
			_listener.BeginGetContext(OnRequested, null);


			this.OpenStartPage();
			// スタートページのQRコード画像を作成
			this.CreateQRImage();
		}

		// スタートページのQRコード画像を作成
		public void CreateQRImage()
		{
			var qrGenerator = new QRCodeGenerator();
			var qrCodeData = qrGenerator.CreateQrCode(StartUrl, QRCodeGenerator.ECCLevel.Q);
			var qrCode = new QRCode(qrCodeData);
			var qrCodeImage = qrCode.GetGraphic(10);
			var outPath = Path.Combine(RootPath, "start.png");
			qrCodeImage.Save(outPath, ImageFormat.Png);
		}


		// 既定ブラウザで最初のページを開く
		public void OpenStartPage()
		{
			var pi = new ProcessStartInfo()
			{
				FileName = StartUrl,
				UseShellExecute = true,
			};
			Process.Start(pi);
		}



		void OnRequested(IAsyncResult ar)
		{
			if (!_listener.IsListening)
				return;

			HttpListenerContext context = null;

			try
			{
				context = _listener.EndGetContext(ar);
				_listener.BeginGetContext(OnRequested, _listener);

				context.Response.AddHeader("Accept-Ranges", "bytes");   // 分割ダウンロード対応
				context.Response.AddHeader("Connection", "keep-alive");
				context.Response.AddHeader("Permissions-Policy", "autoplay=(*)");	// 動画の自動再生を許可


				if (ProcessGetRequest(this, context))
					return;
				if (ProcessPostRequest(this, context))
					return;
				if (ProcessWebSocketRequest(this, context))
					return;
			}
			catch (Exception e)
			{
				Trace.WriteLine($"[エラー]{e.Message}\n{e.StackTrace}");
				if (context != null)
				{
					ReturnInternalError(context.Response, e);
				}
			}
		}

		static bool CanAccept(HttpMethod expected, string requested)
		{
			return string.Equals(expected.Method, requested, StringComparison.CurrentCultureIgnoreCase);
		}


		// ダウンロードの位置とサイズを取得
		private static Tuple<long, long> GetDownloadRange(HttpListenerRequest request)
		{
			long startByte = 0;
			long endByte = -1;
			
			var rangeValue = request.Headers["Range"];
			if (rangeValue != null)
			{
				// Trace.WriteLine($"range[{rangeValue}]");

				var rangeHeader = rangeValue.Replace("bytes=", "");
				var range = rangeHeader.Split('-');
				startByte = long.Parse(range[0]);
				if (range[1].Trim().Length > 0)
				{
					long.TryParse(range[1], out endByte);
				}
			}

			return Tuple.Create(startByte, endByte);
		}


		// 拡張子から.があれば削除
		public static string GetExtOnly(string ext)
		{
			if (string.IsNullOrEmpty(ext))
				return "";

			if (ext[0] == '.')
			{
				return ext.Substring(1);
			}

			return ext;
		}

		// ページの表示
		private string Show(string path, string query)
		{
			// データ更新処理
			UIUpdate.UpdateInfo(Db, query);

			foreach (var item in _uiList)
			{
				var html = item.Show(this, path, query);
				if (string.IsNullOrEmpty(html))
					continue;

				return html;
			}

			return string.Empty;
		}



		static bool ProcessGetRequest(Server owner, HttpListenerContext context)
		{
			var request = context.Request;
			var response = context.Response;
			if (!CanAccept(HttpMethod.Get, request.HttpMethod) || request.IsWebSocketRequest)
			{
				Trace.WriteLine("no get");
				return false;
			}

			response.StatusCode = (int)HttpStatusCode.OK;
			var htmlFileName = request.Url?.LocalPath;

			// URLのルートが一致しないばあいは無処理
			if (!htmlFileName.StartsWith(owner.UrlPrefix))
				return false;

			htmlFileName = htmlFileName?.Substring(owner.UrlPrefix.Length) ?? "";	// リクエストファイル名を取得

			var ext = GetExtOnly(Path.GetExtension(htmlFileName));

			// Trace.WriteLine($"write[{htmlFileName}]====");


			var filePath = $@"{owner.RootPath}\{htmlFileName}";
			switch (ext)
			{
				case "html":
					{
						response.ContentType = $"text/{ext}";
						var data = owner.Show(filePath, request.Url.Query);
						if (string.IsNullOrEmpty(data))
						{
							data = File.ReadAllText(filePath, Encoding.UTF8);
						}
						WriteText(context, data);
					}
					break;

				// 該当テキストをそのまま返す
				case "css":
				case "js":
					{
						response.ContentType = $"text/{ext}";
						WriteText(context, File.ReadAllText(filePath, Encoding.UTF8));
					}
					break;

				// 特殊な拡張子:ファイル名をファイルIDとしてDBのblobを画像データとして渡す
				case "mini_image":
					{
						response.ContentType = $"image/png";

						var name = Path.GetFileNameWithoutExtension(htmlFileName);
						var ids = UIUtil.GetFileIds(name);
						if (ids != null)
						{
							WriteBinary(context, dbFile.GetMiniImage(owner.Db, ids.volume_serial, ids.file_id));
						}
					}
					break;
				case "link_img":
					{
						response.ContentType = $"image/png";
						var name = Path.GetFileNameWithoutExtension(htmlFileName);
						if (int.TryParse(name, out var linkId))
						{
							WriteBinary(context, dbTag.GetLinkImage(owner.Db, linkId));
						}
					}

					break;

				// 特殊な拡張子:ファイル名をFileIDとしてロードしたファイルを動画として返す
				case "custom_video":
					{
						var name = Path.GetFileNameWithoutExtension(htmlFileName);
						var ids = UIUtil.GetFileIds(name);
						if (ids != null)
						{
							var videoPath = FileIdHelper.GetFilePath(ids.volume_serial, (long)ids.file_id);

							var outExt = GetExtOnly(Path.GetExtension(videoPath));  // ファイルの拡張子使う
							// 動画の中身見るのが正解っぽい。
							// https://www.iana.org/assignments/media-types/media-types.xhtml#video
							response.ContentType = $"video/{outExt}";
							WriteBinary(context, videoPath);
						}
					}
					break;

				// 特殊な拡張子:ファイル名をFileIDとしてロードしたファイルを動画として返す
				case "direct_video":
					{
						var name = Path.GetFileNameWithoutExtension(htmlFileName);
						var ids = UIUtil.GetFileIds(name);
						if (ids != null)
						{
							var videoPath = FileIdHelper.GetFilePath(ids.volume_serial, (long)ids.file_id);
							var pi = new ProcessStartInfo()
							{
								FileName = videoPath,
								UseShellExecute = true,
							};
							Process.Start(pi);
						}

						// クローズ
						response.Close();
						// htmlにはエラーレスポンスを返す
						// ReturnInternalError(response, new Exception("サポート外の動画です。"));
					}
					break;
				case "direct_open":
					{
						// ファイルがあるフォルダを開く
						var name = Path.GetFileNameWithoutExtension(htmlFileName);
						var ids = UIUtil.GetFileIds(name);
						if (ids != null)
						{
							var videoPath = FileIdHelper.GetFilePath(ids.volume_serial, (long)ids.file_id);
							var fi = new FileInfo(videoPath);
							var pi = new ProcessStartInfo()
							{
								FileName = fi.Directory.FullName,
								UseShellExecute = true,
							};
							Process.Start(pi);
						}
						response.Close();
					}
					break;

				// 特殊な拡張子：文字列をプログラムで作成して返す(主にjavascriptからくる部分更新リクエスト用)
				case "custom_text":
					{
						response.ContentType = $"text/plain";
						var text = owner.Show(filePath, request.Url.Query);
						if (string.IsNullOrEmpty(text))
						{
							text = $"[{htmlFileName}] error request.";
						}

						WriteText(context, text);
					}

					break;
				case "webp":
					{
						response.ContentType = $"image/png";
						WriteBinary(context, filePath);
					}
					break;

				case "jpeg":
				case "jpg":
				case "png":
				case "gif":
				case "bmp":
				case "ico":
					{
						response.ContentType = $"image/{ext}";
						WriteBinary(context, filePath);
					}
					break;

				case "mp4":
				case "mpeg":
					{
						response.ContentType = $"video/{ext}";
						WriteBinary(context, filePath);
					}
					break;
				case "avi":
				case "wav":
					{
						response.ContentType = $"video/mpeg";
						WriteBinary(context, filePath);
					}
					break;

				default:
					break;
			}
			return true;
		}

		// テキストをレスポンスに出力
		private static void WriteText(HttpListenerContext context, string data)
		{
			var response = context.Response;
			using (var writer = new StreamWriter(response.OutputStream, Encoding.UTF8))
			{
				writer.Write(data);
			}
			response.Close();
		}

		// ストリームをレスポンスに出力
		private static void WriteBinary(HttpListenerContext context, Stream fs)
		{
			try
			{
				var response = context.Response;
				var range = GetDownloadRange(context.Request);

				var maxByte = 10480760; // ContentLengthの上限10M未満にする

				var endPos = (range.Item2 > 0) ? range.Item2 : fs.Length;
				var total = Math.Min(Math.Max(endPos - range.Item1, 0), maxByte);

				fs.Position = range.Item1;
				response.StatusCode = (int)HttpStatusCode.PartialContent;
				response.AddHeader("Content-Range", $"bytes {range.Item1}-{range.Item1 + total - 1}/{fs.Length}");
				//response.ContentLength64 = total;

				// Trace.WriteLine($"Content-Range bytes {range.Item1}-{range.Item1 + total - 1}/{fs.Length}");
				// Trace.WriteLine($"ContentLength64 [{total}]");

				var buffer = new byte[total];
				fs.Read(buffer, 0, buffer.Length);
				response.OutputStream.Write(buffer, 0, buffer.Length);
				response.Close();
			}
			catch (Exception)
			{
				// throw;
			}
		}


		// バイト配列をレスポンスに出力
		private static void WriteBinary(HttpListenerContext context, byte[] buffer)
		{
			// 出力できるものがない
			if (buffer == null)
			{
				context.Response.Close();
				return;
			}

			using (var ms = new MemoryStream(buffer))
			{
				WriteBinary(context, ms);
			}
		}

		// ファイルをレスポンスに出力
		private static void WriteBinary(HttpListenerContext context, string filePath)
		{
			using (var fs = new FileStream(filePath, FileMode.Open, FileAccess.Read))
			{
				WriteBinary(context, fs);
			}
		}


		static bool ProcessPostRequest(Server owner, HttpListenerContext context)
		{
			var request = context.Request;
			var response = context.Response;
			if (!CanAccept(HttpMethod.Post, request.HttpMethod))
				return false;

			response.StatusCode = (int)HttpStatusCode.OK;
			using (var writer = new StreamWriter(response.OutputStream, Encoding.UTF8))
				writer.WriteLine($"you have sent headers:\n{request.Headers}");
			response.Close();
			return true;
		}

		static bool ProcessWebSocketRequest(Server owner, HttpListenerContext context)
		{
			if (!context.Request.IsWebSocketRequest)
				return false;
#if false
			WebSocket webSocket = context.AcceptWebSocketAsync(null).Result.WebSocket;
			ProcessReceivedMessage(webSocket, message =>
			{
				webSocket.SendAsync(
					Encoding.UTF8.GetBytes($"you have sent message:\n{message}"),
					WebSocketMessageType.Text,
					true,
					CancellationToken.None);
			});
#endif
			return true;
		}
#if false
		static async void ProcessReceivedMessage(WebSocket webSocket, Action<string> onMessage)
		{
			var buffer = new ArraySegment<byte>(new byte[1024]);
			while (webSocket.State == WebSocketState.Open)
			{
				WebSocketReceiveResult receiveResult = await webSocket.ReceiveAsync(
					buffer,
					CancellationToken.None);
				if (receiveResult.MessageType == WebSocketMessageType.Text)
				{
					var message = Encoding.UTF8.GetString(
						buffer
							.Slice(0, receiveResult.Count)
							.ToArray());
					onMessage.Invoke(message);
				}
			}
		}
#endif

		static void ReturnInternalError(HttpListenerResponse response, Exception cause)
		{
			Console.Error.WriteLine(cause);
			response.StatusCode = (int)HttpStatusCode.InternalServerError;
			response.ContentType = "text/plain";
			try
			{
				using (var writer = new StreamWriter(response.OutputStream, Encoding.UTF8))
					writer.Write(cause.ToString());
				response.Close();
			}
			catch (Exception e)
			{
				Console.Error.WriteLine(e);
				response.Abort();
			}
		}

		public void Stop()
		{
			_listener.Stop();
			_listener.Close();
		}


	}
}
