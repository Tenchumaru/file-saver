using System;
using System.ComponentModel;
using System.Drawing;
using System.Linq;
using System.Text;
using System.Text.RegularExpressions;

namespace FileSaver
{
    public partial class NotificationIcon : Component
    {
        public event EventHandler Configure = delegate { };
        public event EventHandler Update = delegate { };
        public event EventHandler Display = delegate { };
        public event EventHandler Exit = delegate { };
        private static readonly Regex rx = new Regex("([a-z])([A-Z])", RegexOptions.Compiled | RegexOptions.CultureInvariant);

        public NotificationIcon()
        {
            InitializeComponent();
            ConfigureComponent();
        }

        public NotificationIcon(IContainer container)
        {
            container.Add(this);

            InitializeComponent();
            ConfigureComponent();
        }

        private void ConfigureComponent()
        {
            var boldFont = new Font(configureMenuItem.Font, FontStyle.Bold);
            configureMenuItem.Font = boldFont; // This is the default item.
            configureMenuItem.Click += configureMenuItem_Click;
            updateMenuItem.Click += updateMenuItem_Click;
            displayMenuItem.Click += displayMenuItem_Click;
            exitMenuItem.Click += exitMenuItem_Click;
            UpdateState(NotificationState.Enabled);
        }

        public void UpdateState(NotificationState state)
        {
            notifyIcon.Text = GetStateText(state);
            var iconUri = new Uri(string.Format("/Icons/{0}.ico", state), UriKind.Relative);
            notifyIcon.Icon = new Icon(App.GetResourceStream(iconUri).Stream);
        }

        private static string GetStateText(NotificationState state)
        {
            string stateString = state.ToString();
            MatchCollection matches = rx.Matches(stateString);
            var sb = new StringBuilder(stateString.ToLowerInvariant());
            foreach(var match in matches.OfType<Match>().OrderByDescending(m => m.Index))
                sb.Insert(match.Index + 1, ' ');
            return string.Format("File Saver ({0})", sb);
        }

        private void FireConfigure()
        {
            Configure(this, EventArgs.Empty);
        }

        private void FireUpdate()
        {
            Update(this, EventArgs.Empty);
        }

        private void FireDisplay()
        {
            Display(this, EventArgs.Empty);
        }

        private void FireExit()
        {
            Exit(this, EventArgs.Empty);
        }

        private void configureMenuItem_Click(object sender, EventArgs e)
        {
            FireConfigure();
        }

        private void updateMenuItem_Click(object sender, EventArgs e)
        {
            FireUpdate();
        }

        private void displayMenuItem_Click(object sender, EventArgs e)
        {
            FireDisplay();
        }

        private void exitMenuItem_Click(object sender, EventArgs e)
        {
            FireExit();
        }

        private void notifyIcon_DoubleClick(object sender, EventArgs e)
        {
            FireConfigure();
        }
    }

    public enum NotificationState { Enabled, ReadyToCopy, FilesChanged, Compressing, Copying, Error, Disabled }
}
