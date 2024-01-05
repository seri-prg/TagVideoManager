using System;
using System.Collections.Generic;
using System.Diagnostics;
using System.IO;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace tagVideoManager
{
	internal class VideoConberter
	{
		private const int BufferSize = 4096;

		// ffmpegを使ってwebmに変換したデータをメモリに書き込み
		public static MemoryStream WriteMemory(string path, int startSecond, int timeSpanSecond)
		{
			var sb = new StringBuilder();
			sb.Append($" -ss {startSecond} -t {timeSpanSecond}");
			sb.Append($" -i {path}");
			sb.Append($" -pix_fmt yuv420p -map_metadata -1");
			sb.Append($" -c:v libvpx -crf 30 -b:v 600k -qmin 3 -qmax 40 -colorspace 6 -color_range 0 -color_primaries 6 -color_trc 6");
			sb.Append($" -an");
			sb.Append($" -loglevel warning");
			sb.Append($" -f webm -");	// 標準出力へ

			var process = new Process
			{
				StartInfo = new ProcessStartInfo
				{
					FileName = ".\\ffmpeg\\ffmpeg",
					Arguments = sb.ToString(),
					UseShellExecute = false,
					CreateNoWindow = true,
					RedirectStandardOutput = true,
					RedirectStandardError = true
				},
				EnableRaisingEvents = true
			};

			process.Start();

			var ms = new MemoryStream();
			var ws = new BinaryWriter(ms);

			var rs = new BinaryReader(process.StandardOutput.BaseStream);

			var buffer = rs.ReadBytes(BufferSize);
			while (buffer.Length == BufferSize)
			{
				ws.Write(buffer);
				buffer = rs.ReadBytes(BufferSize);
			}
			ws.Write(buffer);
			var error = process.StandardError.ReadToEnd();
			process.WaitForExit();
			process.Dispose();

			if (error.Contains("Output file is empty"))
				return null;

			ms.Position = 0;
			return ms;
		}




	}
}
