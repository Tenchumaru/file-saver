using System.Windows;
using System.Windows.Controls;
using System.Windows.Media;

namespace FileSaver
{
    class FileViewModel : DependencyObject
    {
        public bool IsAddition
        {
            get { return Background == inclusionBrush; }
            set { Background = value ? inclusionBrush : exclusionBrush; }
        }

        public bool IsFolder
        {
            get { return ImageSource == folderImageSource; }
            set { ImageSource = value ? folderImageSource : fileImageSource; }
        }

        public bool IsValid
        {
            get { return string.IsNullOrEmpty((string)GetValue(IsValidIndicatorProperty)); }
            set { SetValue(IsValidIndicatorProperty, value ? "" : "X"); }
        }

        public Brush Background
        {
            get { return (Brush)GetValue(BackgroundProperty); }
            set { SetValue(BackgroundProperty, value); }
        }

        public ImageSource ImageSource
        {
            get { return (ImageSource)GetValue(ImageSourceProperty); }
            set { SetValue(ImageSourceProperty, value); }
        }

        public string Path
        {
            get { return (string)GetValue(PathProperty); }
            set { SetValue(PathProperty, value); }
        }

        public static readonly DependencyProperty IsValidIndicatorProperty =
            DependencyProperty.Register("IsValidIndicator", typeof(string), typeof(FileViewModel));
        public static readonly DependencyProperty BackgroundProperty =
            DependencyProperty.Register("Background", typeof(Brush), typeof(FileViewModel));
        public static readonly DependencyProperty ImageSourceProperty =
            DependencyProperty.Register("ImageSource", typeof(ImageSource), typeof(FileViewModel));
        public static readonly DependencyProperty PathProperty =
            DependencyProperty.Register("Path", typeof(string), typeof(FileViewModel));
        private static readonly Brush inclusionBrush = new SolidColorBrush(Color.FromRgb(103, 241, 124));
        private static readonly Brush exclusionBrush = new SolidColorBrush(Color.FromRgb(255, 125, 148));
        private ImageSource fileImageSource;
        private ImageSource folderImageSource;

        public FileViewModel()
        {
            Background = exclusionBrush;
#if DEBUG
            if(App.IsDesigning)
                return;
#endif
            var image = (Image)Application.Current.Resources["FileIcon"];
            ImageSource = fileImageSource = image.Source;
            image = (Image)Application.Current.Resources["FolderIcon"];
            folderImageSource = image.Source;
        }

        private void OnIsFolderChanged(bool isFolder)
        {
            ImageSource = isFolder ? folderImageSource : fileImageSource;
        }
    }
}
