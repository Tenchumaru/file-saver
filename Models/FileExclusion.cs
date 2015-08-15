using System.Text.RegularExpressions;

namespace FileSaver.Models
{
	class FileExclusion
	{
		public string Text { get; private set; }
		private Regex rx;

		public FileExclusion(string text)
		{
			Text = text;
			string pattern = text.Replace(@"\", @"\\").Replace(".", @"\.").Replace("*", ".*").Replace('?', '.');
			rx = new Regex(pattern, RegexOptions.Compiled | RegexOptions.IgnoreCase | RegexOptions.Singleline);
		}

		public bool Matches(string text)
		{
			return rx.IsMatch(text);
		}
	}
}
