using System.Collections.ObjectModel;
using System.Linq;
using System.Windows;
using Adrezdi.Windows;
using FileSaver.Models;
using Microsoft.Win32;

namespace FileSaver
{
    class MainViewModel : DependencyObject
    {
        public DelegatedCommand BrowseCommand { get; private set; }
        public DelegatedCommand ResetCommand { get; private set; }
        public DelegatedCommand IncludeFileCommand { get; private set; }
        public DelegatedCommand IncludeFolderCommand { get; private set; }
        public DelegatedCommand ExcludeFileCommand { get; private set; }
        public DelegatedCommand ExcludeFolderCommand { get; private set; }
        public DelegatedCommand ExcludePatternCommand { get; private set; }
        public DelegatedCommand SetExclusionPatternCommand { get; private set; }
        public DelegatedCommand CancelExclusionPatternCommand { get; private set; }
        public DelegatedCommand RemoveCommand { get; private set; }
        public ObservableCollection<FileViewModel> Files
        {
            get { return files; }
        }

        public FileViewModel SelectedFile
        {
            get { return (FileViewModel)GetValue(SelectedFileProperty); }
            set { SetValue(SelectedFileProperty, value); }
        }

        public string TargetFolderPath
        {
            get { return (string)GetValue(TargetFolderPathProperty); }
            set { SetValue(TargetFolderPathProperty, value); }
        }

        public bool CompressOnStart
        {
            get { return (bool)GetValue(CompressOnStartProperty); }
            set { SetValue(CompressOnStartProperty, value); }
        }

        public Visibility ExcludeCommandVisibility
        {
            get { return (Visibility)GetValue(ExcludeCommandVisibilityProperty); }
            set { SetValue(ExcludeCommandVisibilityProperty, value); }
        }

        public string ExclusionPattern
        {
            get { return (string)GetValue(ExclusionPatternProperty); }
            set { SetValue(ExclusionPatternProperty, value); }
        }

        public Visibility ExclusionPatternVisibility
        {
            get { return (Visibility)GetValue(ExclusionPatternVisibilityProperty); }
            set { SetValue(ExclusionPatternVisibilityProperty, value); }
        }

        public static readonly DependencyProperty SelectedFileProperty =
            DependencyProperty.Register("SelectedFile", typeof(FileViewModel), typeof(MainViewModel));
        public static readonly DependencyProperty TargetFolderPathProperty =
            DependencyProperty.Register("TargetFolderPath", typeof(string), typeof(MainViewModel),
            new PropertyMetadata((d, e) => ((MainViewModel)d).OnTargetFolderPathChanged((string)e.NewValue)));
        public static readonly DependencyProperty CompressOnStartProperty =
            DependencyProperty.Register("CompressOnStart", typeof(bool), typeof(MainViewModel),
            new PropertyMetadata((d, e) => ((MainViewModel)d).OnCompressOnStartChanged((bool)e.NewValue)));
        public static readonly DependencyProperty ExcludeCommandVisibilityProperty =
            DependencyProperty.Register("ExcludeCommandVisibility", typeof(Visibility), typeof(MainViewModel),
            new UIPropertyMetadata(Visibility.Visible));
        public static readonly DependencyProperty ExclusionPatternProperty =
            DependencyProperty.Register("ExclusionPattern", typeof(string), typeof(MainViewModel));
        public static readonly DependencyProperty ExclusionPatternVisibilityProperty =
            DependencyProperty.Register("ExclusionPatternVisibility", typeof(Visibility), typeof(MainViewModel),
            new UIPropertyMetadata(Visibility.Collapsed));
        private MainModel model;
        private ObservableCollection<FileViewModel> files = new ObservableCollection<FileViewModel>();

        public MainViewModel()
        {
#if DEBUG
            files.Add(new FileViewModel { IsAddition = true, IsFolder = true, Path = @"C:\Users\Chris\Documents\Important" });
            files.Add(new FileViewModel { Path = @"C:\Users\Chris\Documents\Important\Not.txt" });
#endif
        }

        public MainViewModel(MainModel model)
        {
            BrowseCommand = new DelegatedCommand(Browse);
            ResetCommand = new DelegatedCommand(Reset);
            IncludeFileCommand = new DelegatedCommand(IncludeFile, CanInclude);
            IncludeFolderCommand = new DelegatedCommand(IncludeFolder, CanInclude);
            ExcludeFileCommand = new DelegatedCommand(ExcludeFile);
            ExcludeFolderCommand = new DelegatedCommand(ExcludeFolder);
            ExcludePatternCommand = new DelegatedCommand(ExcludePattern);
            SetExclusionPatternCommand = new DelegatedCommand(SetExclusionPattern);
            CancelExclusionPatternCommand = new DelegatedCommand(CancelExcludePattern);
            RemoveCommand = new DelegatedCommand(Remove, CanRemove);
            this.model = model;
            if(model != null)
            {
                var files = from s in model.FileSpecifications
                            select new FileViewModel
                            {
                                IsAddition = s.IsAddition,
                                IsFolder = s.IsFolder,
                                Path = s.FilePath,
                                IsValid = s.IsValid,
                            };
                foreach(var file in files)
                    this.files.Add(file);
            }
            TargetFolderPath = model.TargetFolderPath;
            CompressOnStart = model.CompressOnStart;
        }

        private void OnTargetFolderPathChanged(string newTargetFolderPath)
        {
            if(model != null)
                model.TargetFolderPath = newTargetFolderPath;
        }

        private void OnCompressOnStartChanged(bool newCompressOnStart)
        {
            if(model != null)
                model.CompressOnStart = newCompressOnStart;
        }

        private void Browse(object parameter)
        {
            var dialog = new System.Windows.Forms.FolderBrowserDialog();
            if(dialog.ShowDialog() == System.Windows.Forms.DialogResult.OK)
                TargetFolderPath = dialog.SelectedPath;
        }

        private void Reset(object parameter)
        {
            model.Reset();
        }

        private void IncludeFile(object parameter)
        {
            AddFile(true);
        }

        private void ExcludeFile(object parameter)
        {
            AddFile(false);
        }

        private void AddFile(bool isAddition)
        {
            var dialog = new OpenFileDialog { Multiselect = true };
            if(dialog.ShowDialog() ?? false)
            {
                foreach(var filePath in dialog.FileNames)
                {
                    var file = new FileViewModel { IsAddition = isAddition, Path = filePath };
                    files.Add(file);
                    model.AddFile(filePath, isAddition);
                }
            }
        }

        private void IncludeFolder(object parameter)
        {
            AddFolder(true);
        }

        private void ExcludeFolder(object parameter)
        {
            AddFolder(false);
        }

        private void AddFolder(bool isAddition)
        {
            var dialog = new System.Windows.Forms.FolderBrowserDialog();
            if(dialog.ShowDialog() == System.Windows.Forms.DialogResult.OK)
            {
                var file = new FileViewModel { IsAddition = isAddition, IsFolder = true, Path = dialog.SelectedPath };
                files.Add(file);
                model.AddFile(dialog.SelectedPath, isAddition);
            }
        }

        private void ExcludePattern(object parameter)
        {
            ExclusionPattern = "*";
            ExclusionPatternVisibility = Visibility.Visible;
            ExcludeCommandVisibility = Visibility.Hidden;
        }

        private void SetExclusionPattern(object parameter)
        {
            string exclusionPattern = ExclusionPattern;
            if(exclusionPattern.ToCharArray().All(c => c == '*' || c == '\\'))
                MessageBox.Show(@"Exclusion pattern must not be only * or \.");
            else
            {
                if(!exclusionPattern.Contains('*'))
                    exclusionPattern = @"*\" + exclusionPattern + @"\*";
                var file = new FileViewModel { Path = exclusionPattern };
                files.Add(file);
                model.AddFile(exclusionPattern, false);
                ExcludeCommandVisibility = Visibility.Visible;
                ExclusionPatternVisibility = Visibility.Collapsed;
            }
        }

        private void CancelExcludePattern(object parameter)
        {
            ExcludeCommandVisibility = Visibility.Visible;
            ExclusionPatternVisibility = Visibility.Collapsed;
        }

        private bool CanInclude(object parameter)
        {
            return !string.IsNullOrEmpty(TargetFolderPath);
        }

        private bool CanRemove(object parameter)
        {
            return SelectedFile != null;
        }

        private void Remove(object parameter)
        {
            while(SelectedFile != null)
            {
                model.RemoveFile(SelectedFile.Path);
                files.Remove(SelectedFile);
            }
        }
    }
}
