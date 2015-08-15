using System;
using System.Collections.Generic;
using System.Diagnostics;
using System.IO;
using System.Linq;
using System.Threading;

namespace FileSaver.Models
{
	public sealed class Compressor : IDisposable
	{
		private readonly string compressionApplication;
		private readonly string fileCompressionCommandLineFormat;
		private readonly string folderCompressionCommandLineFormat;
		private readonly string inclusionFilePath = Path.Combine(Path.GetTempPath(), Path.ChangeExtension(Guid.NewGuid().ToString(), "txt"));
		private readonly string archiveFilePath;
		private Process process;
		private bool isDisposed;
#if DEBUG
		private static bool showingOutput = false;
#endif

		public Compressor(string compressionApplication, string fileCompressionCommandLineFormat, string folderCompressionCommandLineFormat, string archiveExtension)
		{
			this.compressionApplication = compressionApplication;
			this.fileCompressionCommandLineFormat = fileCompressionCommandLineFormat;
			this.folderCompressionCommandLineFormat = folderCompressionCommandLineFormat;
			archiveFilePath = Path.Combine(Path.GetTempPath(), Path.ChangeExtension(Guid.NewGuid().ToString(), archiveExtension));
		}

		public void Dispose()
		{
			if(!isDisposed)
			{
				isDisposed = true;
				Stop();
				File.Delete(archiveFilePath);
				File.Delete(inclusionFilePath);
			}
		}

		public string Compress(string fileSystemPath, List<string> filePaths)
		{
			string commandLine;
			if(File.Exists(fileSystemPath))
				commandLine = string.Format(fileCompressionCommandLineFormat, archiveFilePath, fileSystemPath);
			else
			{
				using(var writer = new StreamWriter(inclusionFilePath))
				{
					foreach(string filePath in filePaths)
						writer.WriteLine(filePath.Substring(fileSystemPath.Length + 1));
				}
				commandLine = string.Format(folderCompressionCommandLineFormat, archiveFilePath, inclusionFilePath);
			}
			var startInfo = new ProcessStartInfo(compressionApplication, commandLine)
			{
				CreateNoWindow = true,
				UseShellExecute = false,
				WorkingDirectory = Directory.Exists(fileSystemPath) ? fileSystemPath : null,
			};
#if DEBUG
			if(showingOutput)
			{
				startInfo.RedirectStandardError = true;
				startInfo.RedirectStandardOutput = true;
			}
#endif
			process = new Process { StartInfo = startInfo, EnableRaisingEvents = true };
#if DEBUG
			if(showingOutput)
			{
				process.ErrorDataReceived += process_ErrorDataReceived;
				process.OutputDataReceived += process_OutputDataReceived;
			}
#endif
			try
			{
				if(!process.Start())
				{
					process.Dispose();
					process = null;
					throw new Exception();
				}
				process.WaitForExit();
				bool succeeded = process.ExitCode == 0 || process.ExitCode == 1;
				Process p = Interlocked.Exchange(ref process, null);
				if(p != null)
					p.Dispose();
				return succeeded ? archiveFilePath : null;
			}
			catch(NullReferenceException)
			{
				return null;
			}
		}

		public void Reset()
		{
			Stop();
			File.Delete(archiveFilePath);
		}

		public void Stop()
		{
			Process process = Interlocked.Exchange(ref this.process, null);
			if(process != null)
			{
				if(!process.HasExited)
				{
					process.Kill();
					process.WaitForExit();
				}
				process.Dispose();
			}
		}

#if DEBUG
		private void process_ErrorDataReceived(object sender, DataReceivedEventArgs e)
		{
			Debug.WriteLine("err: " + e.Data);
		}

		private void process_OutputDataReceived(object sender, DataReceivedEventArgs e)
		{
			Debug.WriteLine("out: " + e.Data);
		}
#endif
	}
}
