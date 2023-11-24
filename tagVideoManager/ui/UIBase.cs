using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace tagVideoManager
{
	// UI管理ベースクラス
	internal abstract class UIBase
	{
		// 表示処理
		// 処理をした場合はnull以外の文字列を返す
		public virtual string Show(Server owner, string filePath, string query) { return string.Empty; } 

	}
}
