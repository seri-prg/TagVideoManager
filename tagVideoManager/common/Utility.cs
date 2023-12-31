﻿using QRCoder;
using System;
using System.Collections.Generic;
using System.Drawing.Imaging;
using System.IO;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace tagVideoManager
{
	// 後で振り分けるかもしれない細かいもの
	internal class Utility
	{
		private const string ASCII_NUMBER = "0123456789";                       //数字
		private const string ASCII_UPPER_ALPHA = "ABCDEFGHIJKLMNOPQRSTUVWXYZ";  //英字大文字
		private const string ASCII_LOWER_ALPHA = "abcdefghijklmnopqrstuvwxyz";  //英字小文字
		private const string USE_RAND_CHARA = ASCII_NUMBER + ASCII_UPPER_ALPHA + ASCII_LOWER_ALPHA;

		// ランダムな文字列作成
		public static string CreateRandomUrl(int count)
		{
			var rand = new Random();
			var pass = new char[count];
			for (int i = 0; i < count; i++)
			{
				pass[i] = USE_RAND_CHARA[rand.Next(USE_RAND_CHARA.Length)];
			}

			return new string(pass);
		}

	}
}
