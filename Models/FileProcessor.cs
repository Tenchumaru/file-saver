using System;
using System.Collections.Generic;
using System.IO;
using System.Linq;
using System.Text;
using System.Threading;

namespace FileSaver.Models
{
	class FileProcessor : IDisposable
	{
		public event EventHandler Changed = delegate { };
		public string FileSystemPath { get { return fileSystemPath; } }
		public ModelState State { get; private set; }
		public IEnumerable<string> ChangedFiles { get { return collector.ChangedFiles; } }
		private readonly string fileSystemPath;
		private readonly FileWatcher watcher;
		private readonly FileCollector collector;
		private readonly Compressor compressor;
		private readonly Thread thread;
		private readonly EventWaitHandle signal = new EventWaitHandle(false, EventResetMode.AutoReset);
		private readonly Properties.Settings settings;
		private bool isRunning = true;

		public FileProcessor(string fileSystemPath, IEnumerable<FileExclusion> exclusions, Properties.Settings settings)
		{
			this.fileSystemPath = fileSystemPath;
			watcher = new FileWatcher(fileSystemPath);
			collector = new FileCollector(exclusions);
			compressor = new Compressor(settings.CompressionApplication, settings.FileCompressionCommandLineFormat, settings.FolderCompressionCommandLineFormat, settings.ArchiveExtension);
			this.settings = settings;
			collector.Added += (s, e) => FireChanged((ModelState)Math.Max((int)ModelState.FilesChanged, (int)State));
			watcher.FileChanged += watcher_FileChanged;
			watcher.FileDeleted += watcher_FileDeleted;
			thread = new Thread(Run) { Name = "Compression loop" };
			thread.Start();
		}

		public void Reset()
		{
			compressor.Reset();
			collector.Reset();
		}

		public void Update()
		{
			signal.Set();
		}

		public void Dispose()
		{
			isRunning = false;
			watcher.Dispose();
			compressor.Stop();
			signal.Set();
			thread.Join();
			compressor.Dispose();
		}

		private void watcher_FileChanged(object sender, FileEventArgs e)
		{
			collector.AddFile(e.FilePath, fileSystemPath);
		}

		private void watcher_FileDeleted(object sender, FileEventArgs e)
		{
			collector.DeleteFile(e.FilePath);
		}

		private void FireChanged(ModelState modelState)
		{
			State = modelState;
			Changed(this, EventArgs.Empty);
		}

		private void Run()
		{
			string fileSystemName = Path.GetFileName(fileSystemPath);
			string targetFileName = Path.ChangeExtension(fileSystemName, settings.ArchiveExtension);
			for(string archiveFilePath = null; ; )
			{
				signal.WaitOne(settings.Interval);
				if(!isRunning)
					break;
				List<string> filePaths = collector.TakeAll();
				if(filePaths.Count > 0)
				{
					FireChanged(ModelState.Compressing);
					archiveFilePath = compressor.Compress(fileSystemPath, filePaths);
				}
				if(archiveFilePath != null)
				{
					if(Directory.Exists(settings.TargetFolderPath))
					{
						FireChanged(ModelState.Copying);
						string targetFilePath = Path.Combine(settings.TargetFolderPath, targetFileName);
						File.Copy(archiveFilePath, targetFilePath, true);
						archiveFilePath = null;
						FireChanged(ModelState.Enabled);
					}
					else
						FireChanged(ModelState.ReadyToCopy);
				}
				else
					FireChanged(ModelState.Enabled);
			}
		}
	}
}
