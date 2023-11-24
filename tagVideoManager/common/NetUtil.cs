using System;
using System.Collections.Generic;
using System.Diagnostics;
using System.Linq;
using System.Net;
using System.Text;
using System.Threading.Tasks;

namespace tagVideoManager
{
	public class NetUtil
	{
		// 任意のURLがHttp.sysに登録されているか判定
		public static bool IsRegisterHttpSys(string url)
		{
			var proc = new Process();
			proc.StartInfo.FileName = "netsh";
			proc.StartInfo.Arguments = $"http show urlacl url={url}";
			proc.StartInfo.UseShellExecute = false;
			proc.StartInfo.CreateNoWindow = true;
			proc.StartInfo.RedirectStandardOutput = true;

			proc.Start();
			proc.WaitForExit();
			var result = proc.StandardOutput.ReadToEnd();

			// 任意の文字列があるなら登録済
			return result.Contains(url);
		}

		// 任意の名前のファイアーウォールの受信ルールが登録されているか
		public static bool IsRegisterFwRule(string name)
		{
			var proc = new Process();
			proc.StartInfo.FileName = "netsh";
			proc.StartInfo.Arguments = $"advfirewall firewall show rule name=\"{name}\" dir=in";
			proc.StartInfo.UseShellExecute = false;
			proc.StartInfo.CreateNoWindow = true;
			proc.StartInfo.RedirectStandardOutput = true;

			proc.Start();
			proc.WaitForExit();
			var result = proc.StandardOutput.ReadToEnd();

			// 任意の文字列があるなら登録済
			return result.Contains(name);
		}

		// 任意のURLをHttp.sysに登録するコマンドを取得
		public static string GetAddHttpSysCommand(string url)
		{
			return $"http add urlacl url={url} user=Everyone";
		}

		// FWの受信ルールに任意のポートを許可するコマンドを取得
		public static string GetArrowPortFwRule(string name, int port)
		{
			return $"advfirewall firewall add rule name= \"{name}\" dir=in action=allow protocol=TCP localport={port}";
		}

		// Http.sysに許可URLを登録し、FWにポートの許可をする。
		public static void RegistWindowsSetting(string url, string fwRuleName, int port)
		{
			// Http.sysに登録済か確認
			if (!IsRegisterHttpSys(url))
			{
				var proc = new Process();
				proc.StartInfo.FileName = "netsh";
				proc.StartInfo.Arguments = GetAddHttpSysCommand(url);
				proc.StartInfo.Verb = "runas";
				proc.Start();
				proc.WaitForExit();
			}

			// FWにポートを開けているか確認
			if (!IsRegisterFwRule(fwRuleName))
			{
				var proc = new Process();
				proc.StartInfo.FileName = "netsh";
				proc.StartInfo.Arguments = GetArrowPortFwRule(fwRuleName, port);
				proc.StartInfo.Verb = "runas";
				proc.Start();
				proc.WaitForExit();
			}
		}


		// IPアドレス取得
		public static string GetServerIP()
		{
			var hostname = Dns.GetHostName();
			// ホスト名からIPアドレスを取得する
			var adrList = Dns.GetHostAddresses(hostname);

			foreach (var address in adrList)
			{
				if (address.AddressFamily == System.Net.Sockets.AddressFamily.InterNetwork)
				{
					return address.ToString();
				}
			}

			return "";
		}


	}
}
