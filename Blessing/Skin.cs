using System;
using System.Collections.Generic;
using System.IO;
using System.Linq;
using System.Text;
using System.Text.RegularExpressions;

namespace Blessing
{
	internal class Skin
	{
		public Section[] Sections { get; private set; }

		internal StringBuilder ContentBuilder;

		readonly string Path;

		public Skin(string Path)
		{
			this.Path = Path;
		}

		public void Load()
		{
			ContentBuilder = new StringBuilder(File.ReadAllText(Path));
		}

		public void Save()
		{
			File.WriteAllText(Path, ContentBuilder.ToString());
		}

		public void Parse()
		{
			var contentStr = ContentBuilder.ToString();

			Regex re = new Regex(@"\[([^]]+)\]", RegexOptions.IgnoreCase | RegexOptions.Multiline);
			MatchCollection res = re.Matches(contentStr);

			int count = res.Count;
			Sections = new Section[count];
			for (int i = 0; i < count; i++)
			{
				Match match = res[i];
				string name = match.Groups[1].ToString();

				if (i == count - 1)
				{
					// 最后一个小节内容直接取到最后
					Sections[i] = new Section
					(
						Skin: this,
						Name: name,
						Content: contentStr.Substring(match.Index),
						Index: match.Index // 这个 Index 应该是正确的，会多出行数的数值，应该是 \r\n 会有2个字符
					);
				}
				else
				{
					// 从这一小节取到下一小节
					Match nextMatch = res[i + 1];
					Sections[i] = new Section
					(
						Skin: this,
						Name: name,
						Content: contentStr.Substring(match.Index, nextMatch.Index - match.Index),
						Index: match.Index
					);
				}
			}
		}

		public void CenterAll()
		{
			for (int i = Sections.Length - 1; i >= 0; i--)
			{
				Sections[i].Center();
			}
		}
	}

	internal class Section
	{
		private Skin Skin;

		readonly string Name;
		internal Dictionary<string, Line> Lines = new Dictionary<string, Line>();

		private string Content;
		internal int Index;

		public Section(Skin Skin, string Name, string Content, int Index)
		{
			this.Skin = Skin;
			this.Name = Name;
			this.Content = Content;
			this.Index = Index;

			// Initialize `Lines`
			var re = new Regex(@"(?<!\s*//)\s*(?<key>\w+)\s*:\s*(?<value>.+(?<!\r))\r?$", RegexOptions.Multiline | RegexOptions.ExplicitCapture);
			var matches = re.Matches(Content);
			var length = matches.Count;

			for (int i = 0; i < length; i++)
			{
				var groups = matches[i].Groups;

				Lines[groups["key"].Value] = new Line
				(
					ContentBuilder: Skin.ContentBuilder,
					Value: groups["value"].Value,
					ValueIndex: groups["value"].Index + Index
				);
			}
		}

		/// <summary>
		/// JS : 
		///		function calc(screenHeight, screenWidth, columns)
		///		{
		///			return ((480 / (screenHeight / screenWidth)) / 2) - (columns.reduce(function(a, b) { return a + b}) / 2);
		///		}
		/// </summary>
		internal void Center()
		{
			if (Name.ToLower() != "mania") return;

			var config = Config.Current;

			// TODO: 没有写 ColumnWidth 怎么办？算了跳过吧
			object columnWidthValue;
			try
			{
				columnWidthValue = Lines["ColumnWidth"].Value;
			}
			catch (Exception)
			{
				return;
			}

			// 1K 并不会解析成数组
			var columnStart = 0;
			if (columnWidthValue is IEnumerable<double>)
			{
				var columnWidth = columnWidthValue as IEnumerable<double>;
				columnStart = (int)(((480d / ((double)config.Height / config.Width)) / 2d) - (columnWidth.Aggregate((a, b) => a + b) / 2d));
			}
			else
			{
				columnStart = (int)(Math.Round((480d / ((double)config.Height / config.Width)) / 2d) - (double)columnWidthValue / 2d);
			}

			Lines["ColumnStart"].WriteValue(columnStart);
		}
	}

	internal class Line
	{
		private StringBuilder ContentBuilder;

		internal object Value;
		readonly string OriginalValue;
		readonly int ValueIndex;

		/// <summary>
		/// 
		/// </summary>
		/// <param name="ContentBuilder"></param>
		/// <param name="Value"></param>
		/// <param name="ValueIndex">相对于 Section 的索引</param>
		public Line(StringBuilder ContentBuilder, string Value, int ValueIndex)
		{
			this.ContentBuilder = ContentBuilder;
			this.OriginalValue = Value;
			this.ValueIndex = ValueIndex;

			// Initialize `Value`
			string[] splitValue = Value.Split(',');
			if (splitValue.Count() > 1)
			{
				// 如果是用逗号分隔的数据，那么就是数组类型
				this.Value = splitValue.Select(str => Convert.ToDouble(str.Trim()));
			}
			else
			{
				// 能转成数值那就用数值，不然就用字符串
				try
				{
					this.Value = Convert.ToDouble(Value);
				}
				catch (FormatException)
				{
					this.Value = Value;
				}
			}
		}

		/// <summary>
		/// 必须从最后一个 Line 开始 SetValue()，不然 Value 改变后 ValueIndex 是无效的
		/// </summary>
		/// <param name="Value"></param>
		internal void WriteValue(object Value)
		{
			ContentBuilder.Remove(ValueIndex, OriginalValue.Length);
			ContentBuilder.Insert(ValueIndex, Value);
		}
	}
}
