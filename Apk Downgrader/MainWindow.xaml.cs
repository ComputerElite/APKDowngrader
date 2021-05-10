using Microsoft.Win32;
using System;
using System.Collections.Generic;
using System.IO;
using System.Windows;
using System.Windows.Input;
using ADB;
using OTP;
using Classes;
using System.Text.Json;
using System.Runtime.InteropServices;
using Version = Classes.Version;
using System.Diagnostics;
using Iteedee.ApkReader;
using System.IO.Compression;
using System.Security.Cryptography;
using System.Threading;
using System.Net;
using System.Threading.Tasks;
using ComputerUtils.RegxTemplates;
using APKDowngrader;

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
        public string repo = "github.com/ComputerElite/APKDowngrader";
        public string supportedVersions = "github.com/ComputerElite/wiki/wiki/APK-Downgrader#officially-supported-app-downgrades";
        public string wiki = "https://GitHub.com/ComputerElite/wiki/wiki/APK-Downgrader";
        public string versionTag = "1.1.8";
        bool draggable = true;
        SHA256 Sha256 = SHA256.Create();

        public MainWindow()
        {
            InitializeComponent();
            SetupExceptionHandlers();
            UpdateB.Visibility = Visibility.Hidden;
            if (!Directory.Exists(exe + "DowngradeFiles")) Directory.CreateDirectory(exe + "DowngradeFiles");
            if (!Directory.Exists(exe + "DowngradedAPKs")) Directory.CreateDirectory(exe + "DowngradedAPKs");
            if (File.Exists(exe + "appid.txt")) appid = File.ReadAllText(exe + "appid.txt");
            else File.WriteAllText(exe + "appid.txt", appid);
            if (File.Exists(exe + "versions.json")) versions = JsonSerializer.Deserialize<Versions>(File.ReadAllText(exe + "versions.json"));
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
                    Versions finished = new Versions();
                    foreach(Version v1 in v.versions)
                    {
                        finished.versions.Add(v1);
                    }
                    foreach (Version v1 in versions.versions)
                    {
                        
                        bool found = false;
                        foreach (Version v2 in finished.versions)
                        {
                            if (v2.Equals(v1))
                            {
                                found = true;
                                break;
                            }
                        }
                        if (!found) finished.versions.Add(v1);
                    }
                    this.Dispatcher.Invoke(() =>
                    {
                        txtbox.AppendText("\nUpdated downgrade Database. Added " + (finished.versions.Count - versions.versions.Count) + " new downgrade entrie(s).");
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
                Update();
                if (!File.Exists(exe + "xdelta3.exe"))
                {
                    this.Dispatcher.Invoke(() =>
                    {
                        txtbox.AppendText("\nDownloading XDelta3.exe");
                        txtbox.ScrollToEnd();
                    });
                    try
                    {
                        c.DownloadFile("https://github.com/jmacd/xdelta-gpl/releases/download/v3.0.10/xdelta3-x86_64-3.0.10.exe.zip", exe + "xdelta3.exe.zip");
                        foreach(ZipArchiveEntry e in ZipFile.OpenRead(exe + "xdelta3.exe.zip").Entries) if(e.Name.ToLower().Contains("xdelta")) e.ExtractToFile(exe + "xdelta3.exe", true);
                        this.Dispatcher.Invoke(() =>
                        {
                            txtbox.AppendText("\nDownloaded XDelta3.exe");
                            txtbox.ScrollToEnd();
                        });
                    } catch
                    {
                        this.Dispatcher.Invoke(() =>
                        {
                            txtbox.AppendText("\nUnable to download XDelta3.exe");
                            txtbox.ScrollToEnd();
                        });
                    }
                    
                    try
                    {
                        File.Delete(exe + "xdelta3.exe.zip");
                    } catch { }
                }
            });
            t.IsBackground = true;
            t.Start();
        }

        public void SetupExceptionHandlers()
        {
            AppDomain.CurrentDomain.UnhandledException += (s, e) =>
            HandleExeption((Exception)e.ExceptionObject, "AppDomain.CurrentDomain.UnhandledException");

            Application.Current.DispatcherUnhandledException += (s, e) =>
            {
                HandleExeption(e.Exception, "Application.Current.DispatcherUnhandledException");
                e.Handled = true;
            };

            TaskScheduler.UnobservedTaskException += (s, e) =>
            {
                HandleExeption(e.Exception, "TaskScheduler.UnobservedTaskException");
                e.SetObserved();
            };
        }

        public void HandleExeption(Exception e, string source)
        {
            DateTime t = DateTime.Now;
            String Save = "\n\nCrash of APK Downgrader has been catched at " + t.Day.ToString("d2") + "." + t.Month.ToString("d2") + "." + t.Year.ToString("d4") + "   " + t.Hour.ToString("d2") + ":" + t.Minute.ToString("d2") + ":" + t.Second.ToString("d2") + "." + t.Millisecond.ToString("d5");
            Save += "\nUseful information:";
            Save += "\n- Version: " + versionTag;
            Save += "\n- Execution directory (Usernames Removed): " + RegexTemplates.RemoveUserName(exe);
            Save += "\n\nException (Usernames Removed):\n   " + RegexTemplates.RemoveUserName(e.ToString());
            File.AppendAllText(exe + "\\Crash.log", Save);
            MessageBoxResult r = MessageBox.Show("Oops. Something has gone wrong as you shouldn't see that window. Please contect ComputerElite via GitHub issues or Discord and send then the file names Crash.log next to the exe. Exception:\n\n" + RegexTemplates.RemoveUserName(e.ToString()), "APK Downgrader - Exception Reporter", MessageBoxButton.OK, MessageBoxImage.Error);
            Process.GetCurrentProcess().Kill();
        }

        public void OpenWiki(object sender, RoutedEventArgs ev)
        {
            Process.Start(wiki);
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
                        if (apkp1 != apkp2 && !Keyboard.IsKeyDown(Key.LeftCtrl))
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
                            txtbox.AppendText("\n\nYou can now start downgrade creation via Start Downgrading");
                            txtbox.ScrollToEnd();
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
            this.Dispatcher.Invoke(() =>
            {
                txtbox.AppendText("\n\nGetting APK version");
            });
            Stopwatch s = Stopwatch.StartNew();
            ApkReader apkReader = new ApkReader();
            ApkInfo info = new ApkInfo();
            try
            {
                ZipArchive a = ZipFile.OpenRead(apk);
                a.GetEntry("AndroidManifest.xml").ExtractToFile(exe + "Androidmanifest.xml", true);
                a.GetEntry("resources.arsc").ExtractToFile(exe + "resources.arsc", true);
                info = apkReader.extractInfo(File.ReadAllBytes(exe + "AndroidManifest.xml"), File.ReadAllBytes(exe + "resources.arsc"));
            } catch (Exception e)
            {
                this.Dispatcher.Invoke(() =>
                {
                    txtbox.AppendText("\n\nAn Error occured while getting the APK Version:\n" + e.ToString() + "\n\nYour APK may be corrupted. Please get a fresh copy of it and try again. If you used Auto downgrade then try again.");
                    txtbox.ScrollToEnd();
                });
            };
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
            this.Dispatcher.Invoke(() =>
            {
                txtbox.AppendText("\n\nGetting APK package name");
            });
            Stopwatch s = Stopwatch.StartNew();
            ApkReader apkReader = new ApkReader();
            ApkInfo info = new ApkInfo();
            try
            {
                ZipArchive a = ZipFile.OpenRead(apk);
                a.GetEntry("AndroidManifest.xml").ExtractToFile(exe + "Androidmanifest.xml", true);
                a.GetEntry("resources.arsc").ExtractToFile(exe + "resources.arsc", true);
                info = apkReader.extractInfo(File.ReadAllBytes(exe + "AndroidManifest.xml"), File.ReadAllBytes(exe + "resources.arsc"));
            }
            catch (Exception e)
            {
                this.Dispatcher.Invoke(() =>
                {
                    txtbox.AppendText("\n\nAn Error occured while getting the APK Version:\n" + e.ToString() + "\n\nYour APK may be corrupted.Please get a fresh copy of it and try again. If you used Auto downgrade then try again.");
                    txtbox.ScrollToEnd();
                });
            };
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
            try
            {
                foreach (String f in APKPath.Text.Split('|'))
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
                    txtbox.AppendText("\n\nThe Version downgrade isn't available for those versions. Please check that your versions are following the version names. e. g. 1.14.0 or 1.13.2 for Beat Saber\n\nTo see supported versions vist " + supportedVersions);
                    txtbox.ScrollToEnd();
                    return false;
                }
                Version v = versions.GetVersion(SV.Text, TV.Text, appid);
                if (!CreateFiles && !ignoreSV && !File.Exists(exe + "DowngradeFiles\\" + v.GetDecrName()))
                {
                    if (!showDownload || v.download == "")
                    {
                        txtbox.AppendText("\n\nYou haven't got " + v.GetDecrName() + " to downgrade your App. " + (v.download == "" ? "No download link has been provided." : "You can download it once you click Start Downgrade"));
                        txtbox.ScrollToEnd();
                    }
                    else
                    {
                        if (!Directory.Exists(exe + "DowngradeFiles"))
                        {
                            Directory.CreateDirectory(exe + "DowngradeFiles");
                        }
                        MessageBoxResult r = MessageBox.Show("You haven't got " + v.GetDecrName() + " to downgrade your App. Do you want to download it?", "APK Downgrader", MessageBoxButton.YesNo, MessageBoxImage.Question);
                        switch (r)
                        {
                            case MessageBoxResult.Yes:
                                if (v.isDirectDownload)
                                {
                                    txtbox.AppendText("\n\nDownloading File. Press Start Downgrade again once the download finished");
                                    txtbox.ScrollToEnd();
                                    Downloadprogress DP = new Downloadprogress();
                                    DP.Show();
                                    DP.Download(v.download, v.GetDecrName(), exe + "DowngradeFiles\\" + v.GetDecrName());
                                }
                                else
                                {
                                    MessageBox.Show("I'll open the download page in your webbrowser once you clicked ok. Please download the files there and then put it in the folder named \"DowngradeFiles\" (I'll open that folder for you too). Once you finished click Start Downgrade again", "APK Downgrader");
                                    Process.Start(exe + "DowngradeFiles");
                                    Process.Start(v.download);
                                    txtbox.AppendText("\n\nPress Start Downgrade once you downloaded and moved the downgrade file intoy \"DowngradeFiles\".");
                                    txtbox.ScrollToEnd();
                                }
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
            } catch (Exception e)
            {
                txtbox.AppendText("An Error occurred while checking if you can downgrade. Please contact ComputerElite:\n" + e.ToString());
                txtbox.ScrollToEnd();
                return false;
            }
        }

        private void Check(object sender, RoutedEventArgs ev)
        {
            if (!CheckVersions()) return;
            txtbox.AppendText("\n\nYup you can downgrade to this version.");
            txtbox.ScrollToEnd();
        }

        private void Start(object sender, RoutedEventArgs ev)
        {
            StartDowngrade();
        }



        private void AutoPull(object sender, RoutedEventArgs ev)
        {
            if (!CheckVersions(true)) return;
            MessageBoxResult r = MessageBox.Show("Do you really want to proceed? If so I'll uninstall the app and hopefully install a downgraded version. This needs ADB (which is included with SideQuest) and your Android Device being connected to your PC via USB and detectable via ADB/Sidequest.", "APK Downgrader", MessageBoxButton.YesNo, MessageBoxImage.Question);
            if(r == MessageBoxResult.No)
            {
                txtbox.AppendText("\n\nAborted");
                txtbox.ScrollToEnd();
                return;
            }
            ADBInteractor i = new ADBInteractor();
            txtbox.AppendText("\n\nPulling APK (that might take a minute)");
            Stopwatch s = Stopwatch.StartNew();
            if (!i.adb("pull " + i.adbS("shell pm path " + appid, txtbox).Replace("package:", "").Replace(Environment.NewLine, "") + " \"" + exe + "apk.apk\"", txtbox))
            {
                txtbox.AppendText("\n\nAborted");
                txtbox.ScrollToEnd();
                return;
            }
            if(!File.Exists(exe + "apk.apk"))
            {
                txtbox.AppendText("\n\nI was unable to pull the APK from your Android Device. Make sure your Android Device is connected via USB and developer mode enabled as well as USB debugging enabled (on some android devices USB Debugging will get enabled with Developer mode. e. g. on the Oculus Quest)");
                txtbox.ScrollToEnd();
                return;
            }
            APKPath.Text = exe + "apk.apk";
            SV.Text = GetAPKVersion(exe + "apk.apk");
            StartDowngrade();
            //txtbox.AppendText("\n\nDeleting pulled APK");
            //if (File.Exists(exe + "apk.apk")) File.Delete(exe + "apk.apk");
            txtbox.AppendText("\n\nUninstalling app");
            i.adb("uninstall " + appid, txtbox);
            txtbox.AppendText("\n\nInstalling downgraded apk");
            i.adb("install \"" + exe + "DowngradedAPKs\\" + appid + "_" + TV.Text + ".apk\"", txtbox);
            s.Stop();
            txtbox.AppendText("\n\nFinished downgrading in " + s.ElapsedMilliseconds + " ms");
        }

        private void StartDowngrade()
        {
            bool highRam = false; //Keyboard.IsKeyDown(Key.LeftShift);
            if (!CheckVersions(false, true)) return;
            try
            {
                Stopwatch s = Stopwatch.StartNew();
                Decrypter d = new Decrypter();
                if (!Directory.Exists(exe + "DowngradedAPKs")) Directory.CreateDirectory(exe + "DowngradedAPKs");
                String outputAPK = exe + "DowngradedAPKs\\" + appid + "_" + TV.Text + ".apk";
                if (CreateFiles)
                {
                    txtbox.AppendText("\n\nCreating downgrade file");
                    
                    string[] files = APKPath.Text.Split('|');
                    Version v = new Version();
                    MessageBoxResult res = MessageBox.Show("Do you want to use XDelta3 to create the downgrade file (smaller end file; yes) or the old XOR method (big file size; no)", "APK Downgrader", MessageBoxButton.YesNo, MessageBoxImage.Question);
                    bool useXDelta = res == MessageBoxResult.Yes;
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
                    v.isXDelta3 = useXDelta;
                    v.appid = appid;
                    String downgrFile = exe + "DowngradeFiles\\" + v.GetDecrName();
                    if (useXDelta)
                    {
                        if (!XDeltaPresent()) return;
                        txtbox.AppendText("\n\nRunning cmd.exe /c \"\"" + exe + "xdelta3.exe\" -c -s \"" + files[0] + "\" " + "\"" + files[1] + "\" > \"" + downgrFile + "\"\"\nPlease be patient");
                        txtbox.ScrollToEnd();
                        if (File.Exists(downgrFile)) File.Delete(downgrFile);
                        Process p = Process.Start("cmd.exe", "/c \"\"" + exe + "xdelta3.exe\" -c -s \"" + files[0] + "\" " + "\"" + files[1] + "\" > \"" + downgrFile + "\"\"");
                        p.WaitForExit();
                        //File.WriteAllText(downgrFile, p.StandardOutput.ReadToEnd());
                        if(!File.Exists(downgrFile) || new FileInfo(downgrFile).Length <= 0)
                        {
                            MessageBox.Show("XDelta3 was unable to create a downgrade. This isn't supposed to happen.", "APK Downgrader", MessageBoxButton.OK, MessageBoxImage.Error);
                            txtbox.AppendText("\n\nAn Error occurred");
                            txtbox.ScrollToEnd();
                            return;
                        }
                    } else
                    {
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
                            File.Copy(files[1], exe + "tmp_APKAppended.apk", true);
                            FileStream fs = new FileStream(exe + "tmp_APKAppended.apk", FileMode.Append);
                            files[1] = exe + "tmp_APKAppended.apk";
                            fs.Write(random, 0, random.Length);
                            fs.Flush();
                            fs.Close();
                            txtbox.AppendText("\nAdjusted File size. Appended " + random.Length + " bytes to Target APK copy");
                        }
                        AllocConsole();
                        if (File.Exists(exe + "DowngradeFiles\\" + v.GetDecrName())) File.Delete(exe + "DowngradeFiles\\" + v.GetDecrName());
                        d.DecryptOTPFile(files[0], files[1], exe + "DowngradeFiles\\" + v.GetDecrName(), !highRam);
                    }
                    txtbox.AppendText("\nCalculating SHA256 for downgrade file");
                    txtbox.ScrollToEnd();
                    v.DSHA256 = CalculateSHA256(exe + "DowngradeFiles\\" + v.GetDecrName());
                    versions.versions.Add(v);
                    File.WriteAllText(exe + "versions.json", JsonSerializer.Serialize(versions, new JsonSerializerOptions { WriteIndented = true }));
                    txtbox.AppendText("\nFeel free to add a download link to the json.");
                    txtbox.ScrollToEnd();
                    FreeConsole();
                }
                else
                {
                    txtbox.AppendText("\n\nDowngrading APK");
                    txtbox.ScrollToEnd();
                    if(!File.Exists(APKPath.Text))
                    {
                        txtbox.AppendText("\nThe APK you put in doesn't exist.");
                        txtbox.ScrollToEnd();
                        return;
                    }
                    appid = GetAPKPackageName(APKPath.Text);
                    Version v = versions.GetVersion(SV.Text, TV.Text, appid);
                    String hash = CalculateSHA256(exe + "DowngradeFiles\\" + v.GetDecrName());
                    bool otherhash = false;
                    if(hash != v.DSHA256 && v.DSHA256 != "")
                    {
                        MessageBoxResult r = MessageBox.Show("Your Downloaded downgrade file doesn't match the hash (hashes are a unique ideifier for files which can be clculated. Same content = same hash) from the downgrade file the person who made the it has. Do you want to continue (in worst case the downgraded file just won't work or not exist)?", "APK Downgrader", MessageBoxButton.YesNo, MessageBoxImage.Warning);
                        if (r == MessageBoxResult.No)
                        {
                            txtbox.AppendText("\n\nAborted");
                            txtbox.ScrollToEnd();
                            return;
                        }
                        otherhash = true;
                    }
                    hash = CalculateSHA256(APKPath.Text);
                    if (hash.ToLower() != v.SSHA256.ToLower() && v.SSHA256 != "")
                    {
                        if (v.isXDelta3)
                        {
                            MessageBox.Show("Your APK doesn't match the hash (hashes are a unique ideifier for files which can be clculated. Same content = same hash) from the apk the person who made the downgrade file has. Since it uses XDelta3 to downgrade your APK I can't continue.", "APK Downgrader", MessageBoxButton.OK, MessageBoxImage.Warning);
                            txtbox.AppendText("\n\nAborted");
                            txtbox.ScrollToEnd();
                            return;
                        }
                        MessageBoxResult r = MessageBox.Show("Your APK doesn't match the hash (hashes are a unique ideifier for files which can be clculated. Same content = same hash) from the apk the person who made the downgrade file has. Do you want to continue (in worst case the downgraded file just won't work)?", "APK Downgrader", MessageBoxButton.YesNo, MessageBoxImage.Warning);
                        if(r == MessageBoxResult.No)
                        {
                            txtbox.AppendText("\n\nAborted");
                            txtbox.ScrollToEnd();
                            return;
                        }
                        otherhash = true;
                    }
                    if (v.isXDelta3)
                    {
                        if (!XDeltaPresent()) return;
                        txtbox.AppendText("\nExecuting cmd.exe /c \"\"" + exe + "xdelta3.exe\" -d -f -s \"" + APKPath.Text + "\" \"" + exe + "DowngradeFiles\\" + v.GetDecrName() + "\" \"" + outputAPK + "\"\"");
                        txtbox.ScrollToEnd();
                        Process p = Process.Start("cmd.exe", "/c \"\"" + exe + "xdelta3.exe\" -d -f -s \"" + APKPath.Text + "\" \"" + exe + "DowngradeFiles\\" + v.GetDecrName() + "\" \"" + outputAPK + "\"\"");
                        p.WaitForExit();
                        if (!File.Exists(outputAPK) || File.Exists(outputAPK) && new FileInfo(outputAPK).Length <= 0)
                        {
                            MessageBox.Show("XDelta3 was unable to downgrade your APK. This isn't supposed to happen.", "APK Downgrader", MessageBoxButton.OK, MessageBoxImage.Error);
                            txtbox.AppendText("\n\nAn Error occurred");
                            txtbox.ScrollToEnd();
                            return;
                        }
                    } else
                    {
                        txtbox.AppendText("\nXOR-ing APK with downgrade file");
                        txtbox.ScrollToEnd();
                        AllocConsole();
                        d.DecryptOTPFile(APKPath.Text, exe + "DowngradeFiles\\" + v.GetDecrName(), outputAPK, !highRam);
                        FreeConsole();
                        txtbox.AppendText("\nRemoving tailing bytes");
                        txtbox.ScrollToEnd();
                        FileInfo fi = new FileInfo(outputAPK);
                        FileStream fs = fi.Open(FileMode.Open);

                        fs.SetLength(v.TargetByteSize);
                        fs.Close();
                    }
                    txtbox.AppendText("\nChecking hash");
                    txtbox.ScrollToEnd();
                    hash = CalculateSHA256(outputAPK);
                    if (hash.ToLower() != v.TSHA256.ToLower() && v.TSHA256 != "")
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
                    txtbox.AppendText("\nSelecting downgraded APK in explorer");
                    txtbox.ScrollToEnd();
                    Process.Start("explorer.exe", "/select," + outputAPK);
                }
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

        private bool XDeltaPresent()
        {
            if(!File.Exists(exe + "xdelta3.exe"))
            {
                txtbox.AppendText("\n\nXDelta3.exe doesn't exist. Please restart the program to download it automatically.\n\nAborted.");
                txtbox.ScrollToEnd();
                return false;
            }
            return true;
        }

        public void Update()
        {
            try
            {
                bool exists = File.Exists(exe + "Update.exe");
                if (exists) File.Delete(exe + "Update.exe");
                WebClient c = new WebClient();
                this.Dispatcher.Invoke(() =>
                {
                    txtbox.AppendText("\n\nTrying to check for updates");
                    txtbox.ScrollToEnd();
                });
                c.Headers.Add("user-agent", "APKDowngrader/" + versionTag);
                String json = c.DownloadString("https://raw.githubusercontent.com/ComputerElite/APKDowngrader/main/update.json");
                UpdateFile updates = JsonSerializer.Deserialize<UpdateFile>(json);
                if (updates.Updates.Count <= 0)
                {
                    this.Dispatcher.Invoke(() =>
                    {
                        txtbox.AppendText("\n\nNo updates could be found");
                        txtbox.ScrollToEnd();
                    });
                    return;
                }
                int comparison = updates.Updates[0].GetVersion().CompareTo(new System.Version(versionTag));
                String text = "";
                String btext = "";
                bool visible = false;
                if (comparison == 0)
                {
                    text = "You are on the newest Version";
                    if(exists)
                    {
                        text += "\n\nApparently you just updated (to Version " + this.versionTag + ")\nUpdate created by " + String.Join(", ", updates.Updates[0].Creators) + "\nChangelog:\n\n" + updates.Updates[0].Changelog;
                    }
                } else if(comparison == -1)
                {
                    text = "You have an newer version than exists? woah congrats. Downgrade now xD";
                    visible = true;
                    btext = "Downgrade now xD";
                } else if(comparison == 1)
                {
                    text = "An Update is available. Please press Update now";
                    visible = true;
                    btext = "Update now";
                }
                this.Dispatcher.Invoke(() =>
                {
                    UpdateB.Content = btext;
                    UpdateB.Visibility = visible ? Visibility.Visible : Visibility.Hidden;
                    txtbox.AppendText("\n\n" + text);
                    txtbox.ScrollToEnd();
                });
            }
            catch (Exception e)
            {
                this.Dispatcher.Invoke(() =>
                {
                    txtbox.AppendText("\nAn Error occurred while checking for updates:\n" + e.ToString());
                    txtbox.ScrollToEnd();
                });
            }
        }

        private void DoUpdate(object sender, RoutedEventArgs ev)
        {
            txtbox.AppendText("\n\nUpdating");
            txtbox.ScrollToEnd();
            try
            {
                WebClient c = new WebClient();
                c.DownloadFile("https://github.com/ComputerElite/APKDowngrader/raw/main/APK%20Downgrader%20Update.exe", exe + "Update.exe");
                Process.Start(exe + "Update.exe");
                Process.GetCurrentProcess().Kill();
            } catch
            {
                txtbox.AppendText("\n\nUnable to downgrade");
                txtbox.ScrollToEnd();
            }
        }

        private void Drag(object sender, RoutedEventArgs e)
        {
            bool mouseIsDown = System.Windows.Input.Mouse.LeftButton == MouseButtonState.Pressed;


            if (mouseIsDown)
            {
                if (draggable)
                {
                    this.DragMove();
                }

            }

        }

        public void noDrag(object sender, MouseEventArgs e)
        {
            draggable = false;
        }

        public void doDrag(object sender, MouseEventArgs e)
        {
            draggable = true;
        }
    }
}
