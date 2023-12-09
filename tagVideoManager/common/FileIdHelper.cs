using Microsoft.Win32.SafeHandles;
using System;
using System.Diagnostics;
using System.Runtime.InteropServices;
using System.Text;
using System.Runtime.InteropServices.ComTypes;
using System.Management;
using System.IO;

namespace tagVideoManager
{
	public static class FileIdHelper
	{
		[StructLayout(LayoutKind.Sequential)]
		public struct BY_HANDLE_FILE_INFORMATION
		{
			public uint FileAttributes;
			public System.Runtime.InteropServices.ComTypes.FILETIME CreationTime;
			public System.Runtime.InteropServices.ComTypes.FILETIME LastAccessTime;
			public System.Runtime.InteropServices.ComTypes.FILETIME LastWriteTime;
			public uint VolumeSerialNumber;
			public uint FileSizeHigh;
			public uint FileSizeLow;
			public uint NumberOfLinks;
			public uint FileIndexHigh;
			public uint FileIndexLow;
		}

		[StructLayout(LayoutKind.Explicit)]
		public struct FILE_ID_DESCRIPTOR
		{
			[FieldOffset(0)]
			public int dwSize;
			[FieldOffset(4)]
			public FILE_ID_TYPE Type;
			[FieldOffset(8)]
			public long FileId;
			[FieldOffset(8)]
			public Guid ObjectId;
			[FieldOffset(8)]
			public Guid ExtendedFileId; //Use for ReFS; need to use v3 structures or later instead of v2 as done in this sample
		}

		public enum FILE_ID_TYPE
		{
			FileIdType = 0,
			ObjectIdType = 1,
			ExtendedFileIdType = 2,
			MaximumFileIdType
		};



		// ボリュームシリアルナンバーからドライブを取得
		public static string GetRootDirve(ulong volumeSerialNumber)
		{
			// Trace.WriteLine($"volumeSerialNumber[{volumeSerialNumber}]");
			var volumes = new ManagementClass("Win32_LogicalDisk").GetInstances();
			foreach (var volume in volumes)
			{
				var serialString = volume.Properties["VolumeSerialNumber"].Value;
				if (serialString == null)
					continue;

				if (!ulong.TryParse(serialString.ToString(), System.Globalization.NumberStyles.HexNumber, null, out var serialNum ))
					continue;

				if (volumeSerialNumber != serialNum)
					continue;

				return volume.Properties["Name"].Value?.ToString() ?? string.Empty;
			}

			return string.Empty;
		}



		public static string GetFilePath(ulong volumeSerialNumber, long fileSystemId)
		{
			// FileIDと同じドライブのハンドルが必要。

			var drive = GetRootDirve(volumeSerialNumber);
			using var handle = _CreateSafeFileHandle($"{drive}\\");

			if (handle == null || handle.IsInvalid)
			{
				Marshal.ThrowExceptionForHR(Marshal.GetHRForLastWin32Error());
			}

			var size = Marshal.SizeOf(typeof(FILE_ID_DESCRIPTOR));
			var descriptor = new FILE_ID_DESCRIPTOR { Type = FILE_ID_TYPE.FileIdType, FileId = fileSystemId, dwSize = size };

			try
			{
				using var handle2 = _OpenFileById(handle, ref descriptor);

				if (handle2 == null || handle2.IsInvalid)
				{
					Marshal.ThrowExceptionForHR(Marshal.GetHRForLastWin32Error());
				}

				const int length = 1024;
				var builder = new StringBuilder(length);
				var needSize = GetFinalPathNameByHandleW(handle2, builder, length, 0);
				var resultPath = builder.ToString();

				if (needSize >= length)
				{
					builder = new StringBuilder(needSize + 1);
					needSize = GetFinalPathNameByHandleW(handle2, builder, length, 0);
				}

				if (resultPath.StartsWith("\\\\?\\UNC\\"))
				{
					resultPath = resultPath.Substring("\\\\?\\UNC\\".Length);
				}
				else if (resultPath.StartsWith("\\\\?\\"))
				{
					resultPath = resultPath.Substring("\\\\?\\".Length);
				}

				return resultPath;
			}
			catch(Exception ex)
			{
				Trace.WriteLine($"[FileID error]{ex.Message}\n{ex.StackTrace}");
				// Marshal.ThrowExceptionForHR(Marshal.GetHRForLastWin32Error());
			}

			return string.Empty;
		}

		public struct FileIdInfo
		{
			public ulong volumeSerial;
			public ulong fileId;
			public ulong createTime;
		}


		// Windows ApiのFILETIMEをunix timeに変換
		const ulong WINDOWS_TICK = 10000000;
		const ulong SEC_TO_UNIX_EPOCH = 11644473600;
		private static ulong WindowsTickToUnixSeconds(System.Runtime.InteropServices.ComTypes.FILETIME ftime)
		{
			ulong windowsTicks = (((ulong)ftime.dwHighDateTime) << 32) | (uint)ftime.dwLowDateTime;
			return windowsTicks / WINDOWS_TICK - SEC_TO_UNIX_EPOCH;
		}

		// File IDを取得
		public static FileIdInfo GetFileId(string filePath)
		{
			using (var fs = File.OpenRead(filePath))
			{
				FileIdHelper.BY_HANDLE_FILE_INFORMATION info;
				FileIdHelper.GetFileInformationByHandle(fs.SafeFileHandle, out info);

				var result = new FileIdInfo();
				result.volumeSerial = (ulong)info.VolumeSerialNumber;
				result.fileId = ((ulong)info.FileIndexHigh) << 32 | (ulong)info.FileIndexLow;
				result.createTime = WindowsTickToUnixSeconds(info.CreationTime);
				return result;
			}
		}



		// 読み取り専用
		static SafeFileHandle _CreateSafeFileHandle(string path) =>
			CreateFileW(
				path,
				FileAccess.Read,
				FileShare.Read,
				IntPtr.Zero,
				FileMode.Open,
				(FileAttributes)0x02000000, //FILE_FLAG_BACKUP_SEMANTICS
				IntPtr.Zero);

		// 読み取り専用
		static SafeFileHandle _OpenFileById(SafeFileHandle hint, ref FILE_ID_DESCRIPTOR fileId) =>
		OpenFileById(
			hint,
			ref fileId,
			FileAccess.Read,
			FileShare.Read,
			IntPtr.Zero,
			(FileAttributes)0x02000000); //FILE_FLAG_BACKUP_SEMANTICS


		// シンボリックリンクの作成
		private const int targetIsAFile = 0;
		private const int SYMBOLIC_LINK_FLAG_ALLOW_UNPRIVILEGED_CREATE = 2; // プロセスが昇格されていない場合にシンボリック リンクを作成できるようにする


		public static void CreateFileLink(string linkPath, string targetPath)
		{
			if (!CreateSymbolicLink(linkPath, targetPath, targetIsAFile+ SYMBOLIC_LINK_FLAG_ALLOW_UNPRIVILEGED_CREATE))
			{
				Marshal.ThrowExceptionForHR(Marshal.GetHRForLastWin32Error());
			}
		}



		[DllImport("kernel32.dll", SetLastError = true)]
		public static extern bool GetFileInformationByHandle(
			SafeFileHandle hFile,
			out BY_HANDLE_FILE_INFORMATION lpFileInformation
		);



		[DllImport("kernel32.dll", SetLastError = true, CharSet = CharSet.Unicode, EntryPoint = "CreateFileW")]
		static extern SafeFileHandle CreateFileW(
			[In, MarshalAs(UnmanagedType.LPWStr)] string lpFileName,
			[In, MarshalAs(UnmanagedType.U4)] FileAccess dwDesiredAccess,
			[In, MarshalAs(UnmanagedType.U4)] FileShare dwShareMode,
			[In] IntPtr lpSecurityAttributes,
			[In, MarshalAs(UnmanagedType.U4)] FileMode dwCreationDisposition,
			[In, MarshalAs(UnmanagedType.U4)] FileAttributes dwFlagsAndAttributes,
			[In] IntPtr hTemplateFile);

		[DllImport("kernel32.dll", SetLastError = true, CharSet = CharSet.Unicode, EntryPoint = "OpenFileById")]
		static extern SafeFileHandle OpenFileById(
			[In] SafeFileHandle hVolumeHint,
			[In, Out] ref FILE_ID_DESCRIPTOR lpFileId,
			[In, MarshalAs(UnmanagedType.U4)] FileAccess dwDesiredAccess,
			[In, MarshalAs(UnmanagedType.U4)] FileShare dwShareMode,
			[In] IntPtr lpSecurityAttributes,
			[In, MarshalAs(UnmanagedType.U4)] FileAttributes dwFlagsAndAttributes
		);

		[DllImport("kernel32.dll", SetLastError = true, CharSet = CharSet.Unicode, EntryPoint = "CloseHandle")]
		static extern bool CloseHandle([In] SafeFileHandle handle);


		[DllImport("kernel32.dll", SetLastError = true, CharSet = CharSet.Unicode, EntryPoint = "GetFinalPathNameByHandleW")]
		static extern int GetFinalPathNameByHandleW(
			SafeFileHandle hFile,
			StringBuilder lpszFilePath,
			int cchFilePath,
			int dwFlags);


		[DllImport("kernel32.dll", SetLastError = true, CharSet = CharSet.Unicode, EntryPoint = "CreateSymbolicLinkW")]
		static extern bool CreateSymbolicLink(string lpSymlinkFileName, string lpTargetFileName, int dwFlags);

	}
}
