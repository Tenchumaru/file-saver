using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using FileSaver.Properties;

namespace FileSaver.Models
{
	public class MainModel : IDisposable
	{
		public event EventHandler Changed = delegate { };
		public IEnumerable<FileSpecification> FileSpecifications { get { return ExtractFileSpecifications(); } }
		public string TargetFolderPath
		{
			get { return settings.TargetFolderPath; }
			set { settings.TargetFolderPath = value; }
		}
		public bool CompressOnStart
		{
			get { return settings.CompressOnStart; }
			set { settings.CompressOnStart = value; }
		}
		public ModelState State { get; private set; }
		public IEnumerable<string> ChangedFiles { get { return processors.SelectMany(p => p.ChangedFiles); } }
		private readonly Settings settings;
		private readonly List<FileExclusion> exclusions;
		private readonly List<FileProcessor> processors;

		public MainModel(Settings settings)
		{
			this.settings = settings;
			var q1 = from f in FileSpecifications
					 where !f.IsAddition
					 select new FileExclusion(f.FilePath);
			exclusions = new List<FileExclusion>(q1);
			var q2 = from f in FileSpecifications
					 where f.IsAddition && f.IsValid
					 select new FileProcessor(f.FilePath, exclusions, settings);
			processors = new List<FileProcessor>(q2);
			processors.ForEach(p => p.Changed += processor_Changed);
		}

		public void AddFile(string filePath, bool isAddition)
		{
			var fileSpecification = string.Format("{0};{1}", isAddition, filePath);
			settings.AddFileSpecification(fileSpecification);
			if(isAddition)
			{
				var processor = new FileProcessor(filePath, exclusions, settings);
				processors.Add(processor);
				processor.Changed += processor_Changed;
			}
			else
				exclusions.Add(new FileExclusion(filePath));
		}

		public void RemoveFile(string filePath)
		{
			string current = settings.FileSpecifications.Cast<string>().FirstOrDefault(s => s.Split(new[] { ';' }, 2)[1] == filePath);
			settings.RemoveFileSpecification(current);
			if(exclusions.RemoveAll(f => f.Text == filePath) == 0)
			{
				var q = processors.Where(p => p.FileSystemPath == filePath).ToList();
				q.ForEach(p => processors.Remove(p));
				q.ForEach(p => p.Dispose());
			}
		}

		public void Dispose()
		{
			processors.ForEach(p => p.Dispose());
			processors.Clear();
		}

		public void Reset()
		{
			processors.ForEach(p => p.Reset());
		}

		public void Update()
		{
			processors.ForEach(p => p.Update());
		}

		private IEnumerable<FileSpecification> ExtractFileSpecifications()
		{
			return from s in settings.FileSpecifications.Cast<string>()
				   let a = s.Split(new[] { ';' }, 2)
				   select new FileSpecification
				   {
					   FilePath = a[1],
					   IsAddition = bool.Parse(a[0]),
				   };
		}

		private void processor_Changed(object sender, EventArgs e)
		{
			ModelState maximumState = processors.Select(p => p.State).Max();
			if(maximumState != State)
				FireChanged(maximumState);
		}

		private void FireChanged(ModelState modelState)
		{
			State = modelState;
			Changed(this, EventArgs.Empty);
		}
	}

	public enum ModelState { Enabled, ReadyToCopy, FilesChanged, Compressing, Copying, Error, Disabled }
}
