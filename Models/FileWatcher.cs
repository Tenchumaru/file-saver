using System;
using System.IO;

namespace FileSaver.Models
{
	public class FileWatcher : IDisposable
	{
		public event EventHandler<FileEventArgs> FileChanged = delegate { };
		public event EventHandler<FileEventArgs> FileDeleted = delegate { };
		private readonly string monitoredPath;
		private readonly FileSystemWatcher watcher;

		public FileWatcher(string monitoredPath)
		{
			this.monitoredPath = monitoredPath;
			if(Directory.Exists(monitoredPath))
			{
				// Watch a folder.
				watcher = new FileSystemWatcher(monitoredPath) { IncludeSubdirectories = true };
			}
			else
			{
				// Watch a file.
				string folderPath = Path.GetDirectoryName(monitoredPath);
				string fileName = Path.GetFileName(monitoredPath);
				watcher = new FileSystemWatcher(folderPath, fileName);
			}
			watcher.Changed += watcher_Changed;
			watcher.Created += watcher_Created;
			watcher.Deleted += watcher_Deleted;
			watcher.Error += watcher_Error;
			watcher.Renamed += watcher_Renamed;
			watcher.EnableRaisingEvents = true;
		}

		public void Dispose()
		{
			watcher.EnableRaisingEvents = false;
			watcher.Dispose();
		}

		private void FireFileChanged(string path)
		{
			FileChanged(this, new FileEventArgs(path));
		}

		private void FireFileDeleted(string path)
		{
			FileDeleted(this, new FileEventArgs(path));
		}

		private void watcher_Changed(object sender, FileSystemEventArgs e)
		{
			FireFileChanged(e.FullPath);
		}

		private void watcher_Created(object sender, FileSystemEventArgs e)
		{
			FireFileChanged(e.FullPath);
		}

		private void watcher_Deleted(object sender, FileSystemEventArgs e)
		{
			FireFileDeleted(e.FullPath);
		}

		private void watcher_Error(object sender, ErrorEventArgs e)
		{
			FireFileChanged(monitoredPath);
		}

		private void watcher_Renamed(object sender, RenamedEventArgs e)
		{
			FireFileChanged(e.FullPath);
			FireFileDeleted(e.OldFullPath);
		}
	}

	public class FileEventArgs : EventArgs
	{
		public string FilePath { get { return filePath; } }
		private readonly string filePath;

		public FileEventArgs(string filePath)
		{
			this.filePath = filePath;
		}
	}
}
