using System;
using System.Collections.Generic;
using System.IO;
using System.Linq;
using System.Text;

namespace FileSaver.Models
{
	class FileCollector
	{
		public event EventHandler Added = delegate { };
		public IEnumerable<string> ChangedFiles { get { return changedFiles; } }
		private readonly HashSet<string> changedFiles = new HashSet<string>();
		private readonly List<FileExclusion> exclusions;
		private readonly HashSet<string> filePaths = new HashSet<string>();
		private bool isFirstRun = true;

		public FileCollector(IEnumerable<FileExclusion> exclusions)
		{
			this.exclusions = exclusions.ToList();
		}

		public void AddFile(string filePath, string monitoredPath)
		{
			if(exclusions.All(e => !e.Matches(filePath)) && !Directory.Exists(filePath))
			{
				changedFiles.Add(filePath);
				if(isFirstRun)
				{
					AddFile(monitoredPath);
					isFirstRun = false;
				}
				else
					AddFile(filePath);
			}
		}

		private void AddFile(string filePath)
		{
			for(; ; )
			{
				try
				{
					lock(filePaths)
					{
						if(Directory.Exists(filePath))
						{
							var q = from p in Directory.GetFiles(filePath, "*", SearchOption.AllDirectories)
									where isFirstRun || IsDirty(p)
									select p;
							q.ToList().ForEach(TryAdd);
						}
						else
							TryAdd(filePath);
					}
					if(filePaths.Any())
						Added(this, EventArgs.Empty);
					break;
				}
				catch(IOException)
				{
					// Perhaps the file was deleted or moved, the folder
					// containing the file was deleted or moved, or the file or
					// folder is in use.  This might occur if filePath is a
					// folder and some descendant items get deleted while
					// trying to collect them.
				}
				catch(UnauthorizedAccessException)
				{
					// See above.
				}
			}
		}

		private void TryAdd(string filePath)
		{
			try
			{
				File.SetAttributes(filePath, File.GetAttributes(filePath) & ~FileAttributes.Archive);
				filePaths.Add(filePath);
			}
			catch(IOException)
			{
				// Perhaps the file was deleted or moved, the folder containing
				// the file was deleted or moved, or the file or folder is in use.
			}
			catch(UnauthorizedAccessException)
			{
				// See above.
			}
		}

		private static bool IsDirty(string filePath)
		{
			try
			{
				return (File.GetAttributes(filePath) & FileAttributes.Archive) != 0;
			}
			catch(IOException)
			{
				// Perhaps the file was deleted or moved, the folder
				// containing the file was deleted or moved, or the file or
				// folder is in use.
				return false;
			}
			catch(UnauthorizedAccessException)
			{
				// See above.
				return false;
			}
		}

		public void DeleteFile(string filePath)
		{
			lock(filePaths)
				filePaths.Remove(filePath);
		}

		public void Reset()
		{
			isFirstRun = true;
			changedFiles.Clear();
		}

		public List<string> TakeAll()
		{
			lock(filePaths)
			{
				var list = new List<string>(filePaths);
				filePaths.Clear();
				return list;
			}
		}
	}
}
