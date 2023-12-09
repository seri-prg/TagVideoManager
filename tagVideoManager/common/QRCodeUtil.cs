using OpenCvSharp;
using QRCoder;
using System;
using System.Collections.Generic;
using System.Drawing.Imaging;
using System.IO;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using System.Windows.Forms;

namespace tagVideoManager
{
	public class QRCodeUtil
	{
		public static void CreateImage(string outPath, string text)
		{
			var qrGenerator = new QRCodeGenerator();
			var qrCodeData = qrGenerator.CreateQrCode(text, QRCodeGenerator.ECCLevel.Q);
			var qrCode = new QRCode(qrCodeData);
			var qrCodeImage = qrCode.GetGraphic(10);
			qrCodeImage.Save(outPath, ImageFormat.Png);
		}


		// ストリームにQRコードを出力
		public static void CreateImage(Stream outStream, string text)
		{
			var qrGenerator = new QRCodeGenerator();
			var qrCodeData = qrGenerator.CreateQrCode(text, QRCodeGenerator.ECCLevel.Q);
			var qrCode = new QRCode(qrCodeData);
			var qrCodeImage = qrCode.GetGraphic(10);

			qrCodeImage.Save(outStream, ImageFormat.Png);
		}

		public static MemoryStream CreateImage(string text)
		{
			var ms = new MemoryStream();
			CreateImage(ms, text);
			return ms;
		}

		// パラメータからコードを出力
		public static MemoryStream CreateImage(string root, Query query)
		{
			if (!query.TryGetString("html", out var pageName))
				return new MemoryStream();

			var sb = new StringBuilder();
			sb.Append($"{root}{pageName}.html");
			var splitter = "?";
			query.DoAll((string key, string value) =>
			{
				sb.Append($"{splitter}{key}={value}");
				splitter = "&";
			});

			return CreateImage(sb.ToString());
		}


	}
}
