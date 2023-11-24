using System;
using System.Diagnostics;
using System.Drawing;
using System.IO;
using System.Security.Policy;
using System.Web;
using System.Windows;
using Microsoft.Win32.SafeHandles;
using OpenCvSharp;
using OpenCvSharp.Extensions;       // Bitmap変換に必要
using Shell32;
using tagVideoManager;

namespace videoEditor
{
	internal class VideoController : IDisposable
	{
		private VideoCapture _vcap = null;
		// 最後に表示したイメージ
		private Mat _lastMat = new Mat();

		private string _lastSymbolicLink = string.Empty;


		// 現在のフレーム位置取得
		public int PosFrames { get { return _vcap?.PosFrames ?? 0; } }

		// トータルフレーム取得
		public int FrameCount { get { return _vcap?.FrameCount ?? 0; } }


		// トータル再生時間(1.0f = 1秒)
		public double TotalTime
		{
			get
			{
				if (_vcap == null)
					return 0.0;

				// Fpsが正しく取れない事がある模様。
				// 
				return (double)_vcap.FrameCount / _vcap.Fps;
			}
		}


        // 動画再生開始
        public VideoController(string path)
        {
			_lastSymbolicLink = Path.GetRandomFileName();
#if true
			FileIdHelper.CreateFileLink(_lastSymbolicLink, path);
#else
			File.CreateSymbolicLink(_lastSymbolicLink, path);
#endif
			_vcap = VideoCapture.FromFile(_lastSymbolicLink);


			// var mdt = GetTime(_lastSymbolicLink);
		}

		// 動画の時間を取得
		// メインスレッドでないと動かない模様
		public static TimeSpan GetTime(string path)
		{
			if (!File.Exists(path))
				return TimeSpan.Zero;

			var fi = new FileInfo(path);
			var shell = new Shell();
			var folder = shell.NameSpace(fi.DirectoryName);
			var item = folder.ParseName(fi.Name);
			var data = folder.GetDetailsOf(item, 27);
			return TimeSpan.Parse(data);
		}


		// 任意のフレームの画像を取得
		// 指定しない場合は最後設定したフレームの次のフレーム
        public Bitmap GetImage(int flame = -1)
        {
            if (_vcap == null)
                return null;

            if (!_vcap.IsOpened())
                return null;

            if (flame >= 0)
			{
                _vcap.PosFrames = flame;
            }

            if (!_vcap.Read(_lastMat))
                return null;

            if (!_lastMat.IsContinuous())
                return null;

            return BitmapConverter.ToBitmap(_lastMat);
        }


		// 1.0 = 1秒とする時間指定で任意の時間の画像を取得
		public Bitmap GetImage(float timeSpan)
		{
			if (_vcap == null)
				return null;

			if (!_vcap.IsOpened())
				return null;

			// 1秒入れてPosMsecが想定通りか確認
			_vcap.PosMsec = 1000;
			if (!_vcap.Read(_lastMat))
				return null;


			if (timeSpan >= 0)
			{
				_vcap.PosMsec = (int)(timeSpan * 1000.0f);
			}

			if (!_vcap.Read(_lastMat))
				return null;

			if (!_lastMat.IsContinuous())
				return null;

			return BitmapConverter.ToBitmap(_lastMat);
		}

		public void Dispose()
		{
			_vcap?.Dispose();//Memory release
			_vcap = null;

			if (!string.IsNullOrEmpty(_lastSymbolicLink))
			{
				if (File.Exists(_lastSymbolicLink))
				{
					File.Delete(_lastSymbolicLink);
				}

				_lastSymbolicLink = string.Empty;
			}
		}
	}
}
