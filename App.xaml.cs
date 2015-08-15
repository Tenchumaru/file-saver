using System;
using System.ComponentModel;
using System.Linq;
using System.Windows;
using FileSaver.Models;
using FileSaver.Properties;

namespace FileSaver
{
    /// <summary>
    /// Interaction logic for App.xaml
    /// </summary>
    public partial class App : Application
    {
        new public static App Current { get { return (App)Application.Current; } }
        private readonly NotificationIcon notificationIcon = new NotificationIcon();
        private readonly MainModel model = new MainModel(Settings.Default);
        private ChangedFileDisplayWindow displayWindow;

        static App()
        {
            var runningCount = System.Diagnostics.Process.GetProcesses().Count(p => p.ProcessName == "FileSaver");
            if(runningCount > 1)
                Environment.Exit(1);
            EmbeddedAssemblyResolver.HookResolve();
        }

        public static bool IsDesigning
        {
            get
            {
                try
                {
                    if(!(Application.Current is App))
                        return true;
                    if(DesignerProperties.GetIsInDesignMode(Application.Current.MainWindow))
                        return true;
                    return Application.Current.StartupUri == null;
                }
                catch
                {
                    return true;
                }
            }
        }

        private SessionTracker tracker;

        public void Configure(Window window)
        {
            window.DataContext = new MainViewModel(model);
            tracker = new SessionTracker();
        }

        protected override void OnStartup(StartupEventArgs e)
        {
            base.OnStartup(e);
            model.Changed += model_Changed;
            notificationIcon.Configure += notificationIcon_Configure;
            notificationIcon.Update += notificationIcon_Update;
            notificationIcon.Display += notificationIcon_Display;
            notificationIcon.Exit += notificationIcon_Exit;
        }

        private void model_Changed(object sender, EventArgs e)
        {
            Action action = UpdateNotifyIcon;
            Dispatcher.BeginInvoke(action, null);
        }

        private void UpdateNotifyIcon()
        {
            var state = (NotificationState)Enum.Parse(typeof(NotificationState), model.State.ToString());
            string reason = null;
            switch(state)
            {
            case NotificationState.ReadyToCopy:
                reason = "Files are ready to back up.";
                break;
            case NotificationState.FilesChanged:
                reason = "Files have changed.";
                break;
            case NotificationState.Compressing:
                reason = "Files are compressing.";
                break;
            case NotificationState.Copying:
                reason = "Files are copying.";
                break;
            }
            if(reason != null)
                tracker.BlockShutdown(reason);
            else
                tracker.UnblockShutdown();
            notificationIcon.UpdateState(state);
        }

        private void notificationIcon_Configure(object sender, EventArgs e)
        {
            MainWindow.Visibility = Visibility.Visible;
        }

        private void notificationIcon_Update(object sender, EventArgs e)
        {
            model.Update();
        }

        private void notificationIcon_Display(object sender, EventArgs e)
        {
            if(displayWindow == null)
                displayWindow = new ChangedFileDisplayWindow();
            displayWindow.DataContext = model.ChangedFiles;
            displayWindow.Show();
        }

        private void notificationIcon_Exit(object sender, EventArgs e)
        {
            model.Changed -= model_Changed;
            model.Dispose();
            notificationIcon.Dispose();
            Shutdown();
        }

        private void Application_DispatcherUnhandledException(object sender, System.Windows.Threading.DispatcherUnhandledExceptionEventArgs e)
        {
            var server = new Microsoft.VisualBasic.Devices.ServerComputer();
            string folderPath = server.FileSystem.SpecialDirectories.CurrentUserApplicationData;
            string fileName = DateTime.Now.ToString("yyyyMMddHHmmss'.log'");
            string filePath = System.IO.Path.Combine(folderPath, fileName);
            System.IO.File.WriteAllText(filePath, e.Exception.ToString());
        }
    }
}
