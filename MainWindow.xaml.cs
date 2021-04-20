using Microsoft.Win32;
using System;
using System.Collections.Generic;
using System.IO;
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
using System.Windows.Navigation;
using System.Windows.Shapes;
using ADB;
using OTP;
using Classes;
using System.Text.Json;
using Path = System.IO.Path;
using System.Runtime.InteropServices;
using Version = Classes.Version;
using System.Diagnostics;
using Iteedee.ApkReader;
using System.IO.Compression;
using System.Security.Cryptography;
using System.Threading;
using System.Net;

namespace Beat_Saber_downgrader
{
    /// <summary>
    /// Interaktionslogik für MainWindow.xaml
    /// </summary>
    public partial class MainWindow : Window
    {
        [DllImport("Kernel32")]
        public static extern void AllocConsole();

        [DllImport("Kernel32")]
        public static extern void FreeConsole();

        public Versions versions = new Versions();

        public bool CreateFiles = false;
        public string exe = AppDomain.CurrentDomain.BaseDirectory;
        public string appid = "com.beatgames.beatsaber";
        SHA256 Sha256 = SHA256.Create();

        public MainWindow()
        {
            InitializeComponent();

            if (File.Exists(exe + "appid.txt")) appid = File.ReadAllText(exe + "appid.txt");
            else File.WriteAllText(exe + "appid.txt", appid);
            Thread t = new Thread(() =>
            {
                this.Dispatcher.Invoke(() =>
                {
                    txtbox.AppendText("\nAttempting to update downgrade database");
                    txtbox.ScrollToEnd();
                });
                WebClient c = new WebClient();
                try
                {
                    String vD = c.DownloadString("https://raw.githubusercontent.com/ComputerElite/APKDowngrader/main/versions.json");
                    Versions v = JsonSerializer.Deserialize<Versions>(vD);
                    this.Dispatcher.Invoke(() =>
                    {
                        txtbox.AppendText("\nUpdating downgrade database");
                        txtbox.ScrollToEnd();
                    });
                    Versions finished = new Versions();

                    foreach(Version v1 in v.versions)
                    {
                        bool found = false;
                        foreach(Version v2 in versions.versions)
                        {
                            if(v2 == v1)
                            {
                                found = true;
                                finished.versions.Add(v2);
                                break;
                            }
                        }
                        if (!found) finished.versions.Add(v1);
                    }
                    this.Dispatcher.Invoke(() =>
                    {
                        txtbox.AppendText("\nUpdated downgrade Database. Added " + (finished.versions.Count - versions.versions.Count) + " new downgrade entries.");
                        txtbox.ScrollToEnd();
                    });
                    versions = finished;
                    File.WriteAllText(exe + "versions.json", JsonSerializer.Serialize(versions, new JsonSerializerOptions { WriteIndented = true }));
                } catch
                {
                    this.Dispatcher.Invoke(() =>
                    {
                        txtbox.AppendText("\nFailed to update downgrade database");
                        txtbox.ScrollToEnd();
                    });
                }
            });
            t.Start();

            if(File.Exists(exe + "versions.json")) versions = JsonSerializer.Deserialize<Versions>(File.ReadAllText(exe + "versions.json"));
        }

        public void APKChoose(object sender, RoutedEventArgs ev)
        {
            CreateFiles = false;
            OpenFileDialog ofd = new OpenFileDialog();
            ofd.Filter = "APK (*.apk)|*.apk";
            List<string> files = new List<string>();
            if (Keyboard.IsKeyDown(Key.LeftShift))
            {
                ofd.Filter = "Source APK (*.apk)|*.apk";
                if ((bool)ofd.ShowDialog())
                {
                    if(File.Exists(ofd.FileName)) files.Add(ofd.FileName);
                }
                ofd.Filter = "Target APK (*.apk)|*.apk";
                if ((bool)ofd.ShowDialog())
                {
                    if (File.Exists(ofd.FileName)) files.Add(ofd.FileName);
                }
            } else
            {
                ofd.Filter = "APK (*.apk)|*.apk";
                if ((bool)ofd.ShowDialog())
                {
                    if (File.Exists(ofd.FileName)) files.Add(ofd.FileName);
                }
            }
            if (files.Count >= 2)
            {
                if (File.Exists(files[0]) && File.Exists(files[1]))
                {
                    APKPath.Text = files[0] + "|" + files[1];
                    Thread t = new Thread(() =>
                    {
                        String apk1 = GetAPKVersion(files[0]);
                        String apk2 = GetAPKVersion(files[1]);
                        String apkp1 = GetAPKPackageName(files[0]);
                        String apkp2 = GetAPKPackageName(files[1]);
                        if(apkp1 != apkp2 && !Keyboard.IsKeyDown(Key.LeftCtrl))
                        {
                            txtbox.AppendText("\n\nSelect 2 apps with the same package id. Those are not the same app.");
                            txtbox.ScrollToEnd();
                            return;
                        } else if (apkp1 != apkp2)
                        {
                            txtbox.AppendText("\n\nWoah you pressed the secret key. Your apps aren't the same id. This might cause issues but I'll be continuing you ctrl key person.");
                            txtbox.ScrollToEnd();
                        }
                        appid = apkp1;
                        this.Dispatcher.Invoke(() =>
                        {
                            SV.Text = apk1;
                            TV.Text = apk2;
                        });
                        CreateFiles = true;
                    });
                    t.IsBackground = true;
                    t.Start();
                }
                else txtbox.AppendText("\n\nThe APK(s) don't exist.");
            }
            else if (files.Count == 1)
            {
                if (File.Exists(ofd.FileName))
                {
                    APKPath.Text = ofd.FileName;
                    Thread t = new Thread(() =>
                    {
                        String apk = GetAPKVersion(ofd.FileName);
                        appid = GetAPKPackageName(ofd.FileName);
                        this.Dispatcher.Invoke(() =>
                        {
                            SV.Text = apk;
                        });
                    });
                    t.IsBackground = true;
                    t.Start();
                }
                else txtbox.AppendText("\nPlease select a valid APK");
            }
        }

        public String CalculateSHA256(string filename)
        {
            using (FileStream stream = File.OpenRead(filename))
            {
                Stopwatch s = Stopwatch.StartNew();
                txtbox.AppendText("\nCalculating hash (SHA256)");
                string hash = "";
                foreach (byte b in Sha256.ComputeHash(stream)) hash += b.ToString("x2");
                s.Stop();
                txtbox.AppendText("\nCalculated hash (" + hash + ") in " + s.ElapsedMilliseconds + " ms");
                return hash;
            }
        }

        public string GetAPKVersion(String apk)
        {
            Stopwatch s = Stopwatch.StartNew();
            ApkReader apkReader = new ApkReader();
            ZipArchive a = ZipFile.OpenRead(apk);
            this.Dispatcher.Invoke(() =>
            {
                txtbox.AppendText("\n\nGetting APK version");
            });
            a.GetEntry("AndroidManifest.xml").ExtractToFile("Androidmanifest.xml", true);
            a.GetEntry("resources.arsc").ExtractToFile("resources.arsc", true);
            ApkInfo info = apkReader.extractInfo(File.ReadAllBytes("AndroidManifest.xml"), File.ReadAllBytes("resources.arsc"));
            this.Dispatcher.Invoke(() =>
            {
                s.Stop();
                txtbox.AppendText("\nGot APK Version (" + info.versionName + ") in " + s.ElapsedMilliseconds + " ms");
                txtbox.ScrollToEnd();
            });
            return info.versionName;
        }
        public string GetAPKPackageName(String apk)
        {
            Stopwatch s = Stopwatch.StartNew();
            ApkReader apkReader = new ApkReader();
            ZipArchive a = ZipFile.OpenRead(apk);
            this.Dispatcher.Invoke(() =>
            {
                txtbox.AppendText("\n\nGetting APK package name");
            });
            a.GetEntry("AndroidManifest.xml").ExtractToFile("Androidmanifest.xml", true);
            a.GetEntry("resources.arsc").ExtractToFile("resources.arsc", true);
            ApkInfo info = apkReader.extractInfo(File.ReadAllBytes("AndroidManifest.xml"), File.ReadAllBytes("resources.arsc"));
            this.Dispatcher.Invoke(() =>
            {
                s.Stop();
                txtbox.AppendText("\nGot APK package name (" + info.packageName + ") in " + s.ElapsedMilliseconds + " ms");
                txtbox.ScrollToEnd();
            });
            return info.packageName;
        }

        public bool CheckVersions(bool ignoreSV = false, bool showDownload = false)
        {
            foreach(String f in APKPath.Text.Split('|'))
            {
                if (!ignoreSV && !File.Exists(f))
                {
                    txtbox.AppendText("\n\nPlease put in/choose your apk");
                    txtbox.ScrollToEnd();
                    return false;
                }
            }
            if (SV.Text == "" && !ignoreSV)
            {
                txtbox.AppendText("\n\nPlease put in your APK Version");
                txtbox.ScrollToEnd();
                return false;
            }
            if (TV.Text == "")
            {
                txtbox.AppendText("\n\nPlease put in your target Version");
                txtbox.ScrollToEnd();
                return false;
            }
            if (!CreateFiles && !versions.IsPresent(SV.Text, TV.Text, appid) && !ignoreSV || ignoreSV && !versions.IsPresent(TV.Text, appid))
            {
                txtbox.AppendText("\n\nThe Version downgrade isn't available for those versions. Please check that your versions are following the version names. e. g. 1.14.0 or 1.13.2 for Beat Saber\n\nTo see supported versions vists github.com/ComputerElite/wiki/");
                txtbox.ScrollToEnd();
                return false;
            }
            Version v = versions.GetVersion(SV.Text, TV.Text, appid);
            if (!CreateFiles && !ignoreSV && !File.Exists(exe + "DowngradeFiles\\" + v.GetDecrName()))
            {
                if(!showDownload || versions.GetVersion(SV.Text, TV.Text, appid).download == "")
                {
                    txtbox.AppendText("\n\nYou haven't got " + v.GetDecrName() + " to downgrade your App. Please search it e. g. under github.com/ComputerElite/wiki/");
                    txtbox.ScrollToEnd();
                } else
                {
                    if(!Directory.Exists(exe + "DowngradeFiles"))
                    {
                        Directory.CreateDirectory(exe + "DowngradeFiles");
                    }
                    MessageBoxResult r = MessageBox.Show("You haven't got " + versions.GetVersion(SV.Text, TV.Text, appid).GetDecrName() + " to downgrade your App. Do you want to download it?", "APK Downgrader", MessageBoxButton.YesNo, MessageBoxImage.Question);
                    switch(r)
                    {
                        case MessageBoxResult.Yes:
                            MessageBox.Show("I'll open the download page in your webbrowser once you clicked ok. Please download the files there and then put it in the folder named \"DowngradeFiles\" (I'll open that folder for you too). Once you finished click Start Downgrade again", "APK Downgrader");
                            Process.Start(exe + "DowngradeFiles");
                            Process.Start(v.download);
                            txtbox.AppendText("\n\nPress Start Downgrade once you downloaded the downgrade file.");
                            txtbox.ScrollToEnd();
                            break;
                        case MessageBoxResult.No:
                            txtbox.AppendText("\n\nYou can't continue without the files. Aborting.");
                            txtbox.ScrollToEnd();
                            break;
                    }
                }
                
                return false;
            }
            return true;
        }

        private void Check(object sender, RoutedEventArgs ev)
        {
            if (!CheckVersions()) return;
            txtbox.AppendText("\n\nYup you can downgrade this version as you wanted.");
            txtbox.ScrollToEnd();
        }

        private void Start(object sender, RoutedEventArgs ev)
        {
            StartDowngrade();
        }



        private void AutoPull(object sender, RoutedEventArgs ev)
        {

            if (!CheckVersions(true)) return;
            ADBInteractor i = new ADBInteractor();
            txtbox.AppendText("\n\nPulling APK");
            Stopwatch s = Stopwatch.StartNew();
            if(!i.adb("pull " + i.adbS("shell pm path " + appid, txtbox) + " \"" + exe + "\\apk.apk\"", txtbox))
            {
                txtbox.AppendText("\n\nAborted");
                txtbox.ScrollToEnd();
                return;
            }
            APKPath.Text = exe + "\\apk.apk";
            SV.Text = GetAPKVersion(exe + "\\apk.apk");
            StartDowngrade();
            txtbox.AppendText("\n\nInstalling app");
            i.adb("uninstall " + appid, txtbox);
            txtbox.AppendText("\n\nInstalling downgraded apk");
            i.adb("install \"" + exe + "\\" + TV.Text + ".apk\"", txtbox);
            s.Stop();
            txtbox.AppendText("\n\nFinished downgrading in " + s.ElapsedMilliseconds + " ms");
        }

        private void StartDowngrade()
        {
            if (!CheckVersions(false, true)) return;
            try
            {
                Stopwatch s = Stopwatch.StartNew();
                Decrypter d = new Decrypter();
                String outputAPK = appid + "_" + TV.Text + ".apk";
                if (CreateFiles)
                {
                    txtbox.AppendText("\n\nCreating downgrade file");
                    
                    string[] files = APKPath.Text.Split('|');
                    Version v = new Version();
                    txtbox.AppendText("\nCreating Version info");
                    txtbox.ScrollToEnd();
                    v.SV = SV.Text;
                    v.TV = TV.Text;
                    v.SourceByteSize = (int)new FileInfo(files[0]).Length;
                    v.TargetByteSize = (int)new FileInfo(files[1]).Length;
                    txtbox.AppendText("\nCalculating SHA256 hashes for apks");
                    txtbox.ScrollToEnd();
                    v.SSHA256 = CalculateSHA256(files[0]);
                    v.TSHA256 = CalculateSHA256(files[1]);
                    v.appid = appid;
                    txtbox.AppendText("\nXOR-ing " + SV.Text + " with " + TV.Text);
                    txtbox.ScrollToEnd();
                    if (v.SourceByteSize < v.TargetByteSize)
                    {
                        txtbox.AppendText("\n\nI'm sorry. Due to the source file having to be as big as the target one or bigger to not distribute game code I can't do that for you");
                        txtbox.ScrollToEnd();
                        FreeConsole();
                        return;
                    }
                    if (v.SourceByteSize != v.TargetByteSize)
                    {
                        txtbox.AppendText("\n\nCopying and adjusting File size.");
                        Random r = new Random();
                        byte[] random = new byte[v.SourceByteSize - v.TargetByteSize];
                        r.NextBytes(random);
                        File.Copy(files[1], "tmp_APKAppended.apk", true);
                        FileStream fs = new FileStream("tmp_APKAppended.apk", FileMode.Append);
                        files[1] = "tmp_APKAppended.apk";
                        fs.Write(random, 0, random.Length);
                        fs.Flush();
                        fs.Close();
                        txtbox.AppendText("\nAdjusted File size. Appended " + random.Length + " bytes to Target APK copy");
                    }
                    AllocConsole();
                    if (File.Exists(v.GetDecrName())) File.Delete(v.GetDecrName());
                    d.DecryptOTPFile(files[0], files[1], exe + "DowngradeFiles\\" + v.GetDecrName(), true);
                    versions.versions.Add(v);
                    File.WriteAllText("versions.json", JsonSerializer.Serialize(versions, new JsonSerializerOptions { WriteIndented = true }));
                    txtbox.AppendText("\nFeel free to add a download link to the json.");
                    txtbox.ScrollToEnd();
                    FreeConsole();
                }
                else
                {
                    txtbox.AppendText("\n\nDowngrading APK");
                    txtbox.ScrollToEnd();
                    Version v = versions.GetVersion(SV.Text, TV.Text, appid);
                    String hash = CalculateSHA256(APKPath.Text);
                    bool otherhash = false;
                    if(hash.ToLower() != v.SSHA256.ToLower())
                    {
                        MessageBoxResult r = MessageBox.Show("Your APK doesn't match the hash (hashes are a unique ideifier for files which can be clculated. Same content = same hash) from the apk the person who made the downgrade file has. Do you want to continue (in worst case the downgraded file just won't work)?", "APK Downgrader", MessageBoxButton.YesNo, MessageBoxImage.Warning);
                        if(r == MessageBoxResult.No)
                        {
                            txtbox.AppendText("\n\nAborted");
                            txtbox.ScrollToEnd();
                            return;
                        }
                        otherhash = true;
                    }
                    txtbox.AppendText("\nXOR-ing APK with downgrade file");
                    txtbox.ScrollToEnd();
                    AllocConsole();
                    d.DecryptOTPFile(APKPath.Text, exe + "DowngradeFiles\\" + v.GetDecrName(), outputAPK, true);
                    FreeConsole();
                    txtbox.AppendText("\nRemoving tailing bytes");
                    txtbox.ScrollToEnd();
                    FileInfo fi = new FileInfo(outputAPK);
                    FileStream fs = fi.Open(FileMode.Open);

                    fs.SetLength(v.TargetByteSize);
                    fs.Close();
                    txtbox.AppendText("\nChecking hash");
                    txtbox.ScrollToEnd();
                    hash = CalculateSHA256(outputAPK);
                    if (hash.ToLower() != v.TSHA256.ToLower())
                    {
                        if(otherhash)
                        {
                            txtbox.AppendText("\nAgain the downgraded APK has another hash as the person who created the downgrade file. Hope for the best.");
                            txtbox.ScrollToEnd();
                        } else
                        {
                            MessageBox.Show("Apparently an error occurred. The downgrade apk doesn't match the hash (hashes are a unique ideifier for files which can be clculated. Same content = same hash) of the person who created the downgrade files. Hope for the best.", "APK Downgrader", MessageBoxButton.OK, MessageBoxImage.Warning);
                        }
                    }
                }


                //List<byte> file = File.ReadAllBytes(outputAPK).ToList<byte>();

                //file.RemoveRange(v.TargetByteSize, file.Count - v.TargetByteSize);
                //File.WriteAllBytes(outputAPK, file.ToArray());
                s.Stop();
                txtbox.AppendText("\n\nFinished in " + s.ElapsedMilliseconds + " ms");
                txtbox.ScrollToEnd();
            }
            catch (Exception e)
            {
                txtbox.AppendText("\n\nAn Error occurred:\n" + e.ToString());
                txtbox.ScrollToEnd();
            }
        }
    }
}
