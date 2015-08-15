using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.IO;

namespace FileSaver.Models
{
	public class FileSpecification
	{
		public string FilePath { get; set; }
		public bool IsAddition { get; set; }
		public bool IsFolder { get { return Directory.Exists(FilePath); } }
		public bool IsValid { get { return !IsAddition || IsFolder || File.Exists(FilePath); } }
	}
}
