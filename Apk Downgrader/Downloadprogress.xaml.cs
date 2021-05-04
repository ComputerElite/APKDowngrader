using System;
using System.Collections.Generic;
using System.ComponentModel;
using System.Linq;
using System.Net;
using System.Text;
using System.Threading.Tasks;
using System.Windows;
using System.Windows.Controls;
using System.Windows.Data;
using System.Windows.Documents;
using System.Windows.Input;
using System.Windows.Media;
using System.Windows.Media.Imaging;
using System.Windows.Shapes;

namespace Beat_Saber_downgrader
{
    /// <summary>
    /// Interaktionslogik für Downloadprogress.xaml
    /// </summary>
    public partial class Downloadprogress : Window
    {
        public String uRL = "";
        public String name = "";
        public String path = "";

        public Downloadprogress()
        {
            InitializeComponent();
        }

        public void Download(String URL, String Name, String path)
        {
            text.Text = "Downloading " + Name;
            this.uRL = URL;
            this.name = Name;
            this.path = path;
            WebClient c = new WebClient();
            c.DownloadFileCompleted += new AsyncCompletedEventHandler(finished_download);
            c.DownloadProgressChanged += new DownloadProgressChangedEventHandler(client_DownloadProgressChanged);
            c.DownloadFileAsync(new Uri(URL), path);
        }

        private void client_DownloadProgressChanged(object sender, DownloadProgressChangedEventArgs e)
        {
            float bytesIn = float.Parse(e.BytesReceived.ToString());
            float totalBytes = float.Parse(e.TotalBytesToReceive.ToString());
            float percentage = bytesIn / totalBytes * 100;
            MB.Text = (bytesIn / 1024 / 1024) + " / " + (totalBytes / 1024 / 1024) + " MB (" + percentage + " %)";
            progress.Value = Math.Truncate(percentage);
        }

        private void finished_download(object sender, AsyncCompletedEventArgs e)
        {
            MessageBox.Show("Download of " + name + " from " + uRL + " finished.", "Download Finished - APK Downgrader", MessageBoxButton.OK, MessageBoxImage.Information); ;
            this.Close();
        }
    }
}
