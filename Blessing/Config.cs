using System;
using System.Collections.Generic;
using System.Diagnostics;
using System.IO;
using System.Linq;
using System.Text;
using System.Text.RegularExpressions;

namespace Blessing
{
	class Config
	{
		public static Config Current { get; private set; }

		public uint Width { get; private set; }
		public uint Height { get; private set; }

		private string content;
		private readonly string Path;
		
		public Config(string Path)
		{
			Current = this;
			this.Path = Path;
		}

		public void Load()
		{
			content = File.ReadAllText(Path);
		}

		public void Parse()
		{
			// 删除注释
			Regex re = new Regex(@"#[^\n]*", RegexOptions.Multiline);
			string content2 = re.Replace(content, "");

			// 直接找出 width 和 height
			Width = Convert.ToUInt32(Find(content2, "WidthFullscreen"));
			Height = Convert.ToUInt32(Find(content2, "HeightFullscreen"));
		}

		public static string Find(string content, string key)
		{
			Regex reWidth = new Regex(@"\s*" + key + @"\s*=\s*(\d*)", RegexOptions.IgnoreCase);
			Match res = reWidth.Match(content);
			return res.Groups[1].ToString();
		}
	}
}
