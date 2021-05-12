using Classes;
using System;
using System.Collections.Generic;
using System.Linq;
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
using Version = Classes.Version;

namespace Beat_Saber_downgrader
{
    /// <summary>
    /// Interaktionslogik für VersionShower.xaml
    /// </summary>
    public partial class VersionShower : Window
    {
        public Versions versions = new Versions();
        public List<string> appids = new List<string>();
        public List<Version> appVersions = new List<Version>();

        public VersionShower(Versions availableVersions)
        {
            InitializeComponent();
            versions = availableVersions;
            UpdateList();
        }

        public List<Version> Sort(List<Version> input)
        {
            input = input.OrderByDescending(x => x.SV).ToList();
            List<Version> outp = new List<Version>();
            Dictionary<string, List<Version>> sorter = new Dictionary<string, List<Version>>();
            foreach(Version v in input)
            {
                if (!sorter.ContainsKey(v.SV)) sorter.Add(v.SV, new List<Version>());
                sorter[v.SV].Add(v);
            }
            sorter.OrderBy(x => x.Key);
            foreach(String s in sorter.Keys)
            {
                Console.WriteLine(s);
                List<Version> tmp = new List<Version>(sorter[s]);
                tmp = tmp.OrderByDescending(x => x.TV).ToList();
                foreach (Version v in tmp)
                {
                    Console.WriteLine(v.TV);
                    outp.Add(v);
                }
            }
            return outp;
        }

        public void UpdateList()
        {
            if(Apps.Items.Count <= 0)
            {
                appids = versions.GetAppIDs();
                foreach(string s in appids)
                {
                    Apps.Items.Add(new Version { appid = s });
                }
                if (appids.Count == 0)
                {
                    Apps.Items.Add(new Version { appid = "None" });
                }
            }
            else if(Apps.SelectedIndex >= 0 && Apps.SelectedIndex < appids.Count)
            {
                Versions.Items.Clear();
                appVersions = Sort(versions.GetVersionsByAppID(appids[Apps.SelectedIndex]));
                foreach(Version v in appVersions)
                {
                    Versions.Items.Add(v);
                }
            }
        }

        private void Reload(object sender, SelectionChangedEventArgs e)
        {
            UpdateList();
        }
    }
}
