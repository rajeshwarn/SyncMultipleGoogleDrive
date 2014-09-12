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
using System.Windows.Navigation;
using System.Windows.Shapes;

using Google;
using Google.Apis.Auth.OAuth2;
using Google.Apis.Drive.v2;
using Google.Apis.Drive.v2.Data;
using Google.Apis.Services;
using Google.Apis.Util.Store;
using System.Threading;

using System.Diagnostics;

using System.Windows.Forms;
using SyncMultipleGoogleDrives.Binding;
using SyncMultipleGoogleDrives.Model;
using System.Xml;
using System.IO.IsolatedStorage;
using System.IO;

namespace SyncMultipleGoogleDrives
{
    /// <summary>
    /// Interaction logic for MainWindow.xaml
    /// </summary>
    public partial class MainWindow : Window
    {

        private XmlDocument _xmlGoogleAccountSettings = null;

        private List<GoogleAccount> _GoogleAccounts = null;

        private ItemProvider itemProvider;
        private List<Item> items;

        private CurrentSynchro cs;

        private static string _rootFolder = "";

        public MainWindow()
        {
            InitializeComponent();

            cs = new CurrentSynchro();

            DataContext = cs;

            if (string.IsNullOrEmpty(Properties.Settings.Default.rootFolder))
            {
                if (System.IO.Directory.Exists(Environment.GetFolderPath(Environment.SpecialFolder.MyDocuments)))
                {
                    txtRootFolder.Text = Environment.GetFolderPath(Environment.SpecialFolder.MyDocuments);
                    Properties.Settings.Default.rootFolder = txtRootFolder.Text;
                    Properties.Settings.Default.Save();
                }
            }
            else
            {
                txtRootFolder.Text = Properties.Settings.Default.rootFolder;
            }
            _rootFolder = txtRootFolder.Text;

            itemProvider = new ItemProvider();
            items = itemProvider.GetItems(txtRootFolder.Text);
            tvFilesFolders.DataContext = items;

            txtRootFolder.TextChanged += txtRootFolder_TextChanged;

            GetGoogleAccountSettings();
            AdaptGoogleAccountList();
            SetColorsOfItemList(items);
        }

        void txtRootFolder_TextChanged(object sender, TextChangedEventArgs e)
        {
            _rootFolder = txtRootFolder.Text;
            var itemProvider = new ItemProvider();

            var items = itemProvider.GetItems(txtRootFolder.Text);

            tvFilesFolders.DataContext = items;

            if (_GoogleAccounts != null && _GoogleAccounts.Count > 0)
            {
                foreach (GoogleAccount ga in _GoogleAccounts)
                {
                    ga.ClearListToUpload();
                }
            }
            SetColorsOfItemList(items);
        }

        private void Button_Click(object sender, RoutedEventArgs e)
        {

            GoogleAccounts gAW = new GoogleAccounts(_xmlGoogleAccountSettings);
            gAW.ShowDialog();
            SaveGoogleAccountsXML();

            AdaptGoogleAccountList();

            if (_GoogleAccounts != null && _GoogleAccounts.Count > 0)
            {
                foreach (GoogleAccount ga in _GoogleAccounts)
                {
                    ga.ClearListToUpload();
                }
            }
            SetColorsOfItemList(items);

            //        //UserCredential credential = GoogleWebAuthorizationBroker.AuthorizeAsync(
            //        //    new ClientSecrets
            //        //    {
            //        //        ClientId = "846715596453-539dui7i3rf5kqmmkenfnkk1i2d5dhlj.apps.googleusercontent.com",
            //        //        ClientSecret = "U-V5GMN3qk5Z6ubSql1xYD9M",
            //        //    },
            //        //    new[] { DriveService.Scope.Drive },
            //        //    "user",
            //        //    CancellationToken.None, 
            //        //    new FileDataStore("credentials_koen.mestdagh")).Result;

            //        UserCredential credential = GoogleWebAuthorizationBroker.AuthorizeAsync(
            //new ClientSecrets
            //{
            //    ClientId = "610519876157-svk4euh3mk2bt37j43engm1uciid0jc9.apps.googleusercontent.com",
            //    ClientSecret = "2x4HYu0kucG6wXqPYNBZ8dmV",
            //},
            //new[] { DriveService.Scope.Drive },
            //"user",
            //CancellationToken.None,
            //new FileDataStore("credentials_koen.mestdagh2014")).Result;

            //        // Create the service.
            //        var service = new DriveService(new BaseClientService.Initializer()
            //        {
            //            HttpClientInitializer = credential,
            //            ApplicationName = "Drive API Sample",
            //        });

            //        List<File> lstFile = retrieveAllFiles(service);
            //        if (lstFile != null && lstFile.Count > 0)
            //        {
            //            foreach (File f in lstFile)
            //            {
            //                if (f.MimeType.EndsWith("apps.folder"))
            //                {
            //                    Trace.WriteLine(f.Title);
            //                }
            //                else if (f.Title.Contains("pellikaan"))
            //                {
            //                    Trace.WriteLine(f.Title);
            //                }
            //                else
            //                {
            //                    Trace.WriteLine(f.Title);
            //                }
            //            }
            //        }

            //        //File body = new File();
            //        //body.Title = "My document";
            //        //body.Description = "A test document";
            //        //body.MimeType = "text/plain";

            //        //byte[] byteArray = System.IO.File.ReadAllBytes("document.txt");
            //        //System.IO.MemoryStream stream = new System.IO.MemoryStream(byteArray);

            //        //FilesResource.InsertMediaUpload request = service.Files.Insert(body, stream, "text/plain");
            //        //request.Upload();

            //        //File file = request.ResponseBody;
            //        //Console.WriteLine("File id: " + file.Id);
        }



        private void Button_Click_1(object sender, RoutedEventArgs e)
        {

            FolderBrowserDialog folderBrowserDialog1;
            folderBrowserDialog1 = new FolderBrowserDialog();
            folderBrowserDialog1.ShowNewFolderButton = true;
            folderBrowserDialog1.SelectedPath = txtRootFolder.Text;
            DialogResult result = folderBrowserDialog1.ShowDialog();
            if (result == System.Windows.Forms.DialogResult.OK)
            {
                txtRootFolder.Text = folderBrowserDialog1.SelectedPath;
                Properties.Settings.Default.rootFolder = txtRootFolder.Text;
                Properties.Settings.Default.Save();
            }

        }

        private void GetGoogleAccountSettings()
        {
            IsolatedStorageFile isoStore = IsolatedStorageFile.GetStore(IsolatedStorageScope.User | IsolatedStorageScope.Assembly, null, null);

            if (isoStore.FileExists("SyncMultipleGoogleDrives.xml"))
            {
                Trace.WriteLine("The file already exists!");
                using (IsolatedStorageFileStream isoStream = new IsolatedStorageFileStream("SyncMultipleGoogleDrives.xml", FileMode.Open, isoStore))
                {
                    using (StreamReader reader = new StreamReader(isoStream))
                    {
                        _xmlGoogleAccountSettings = new XmlDocument();
                        _xmlGoogleAccountSettings.Load(reader);
                        //Console.WriteLine("Reading contents:");
                        //Console.WriteLine(reader.ReadToEnd());
                    }
                }
            }
            else
            {
                using (IsolatedStorageFileStream isoStream = new IsolatedStorageFileStream("SyncMultipleGoogleDrives.xml", FileMode.CreateNew, isoStore))
                {
                    using (StreamWriter writer = new StreamWriter(isoStream))
                    {
                        _xmlGoogleAccountSettings = new XmlDocument();
                        // Write down the XML declaration
                        XmlDeclaration xmlDeclaration = _xmlGoogleAccountSettings.CreateXmlDeclaration("1.0", "utf-8", null);

                        // Create the root element
                        XmlElement rootNode = _xmlGoogleAccountSettings.CreateElement("GoogleAccounts");
                        _xmlGoogleAccountSettings.InsertBefore(xmlDeclaration, _xmlGoogleAccountSettings.DocumentElement);
                        _xmlGoogleAccountSettings.AppendChild(rootNode);
                        writer.Write(_xmlGoogleAccountSettings.OuterXml);

                        //writer.WriteLine("Hello Isolated Storage");
                        //Console.WriteLine("You have written to the file.");
                    }
                }
            }
        }

        private void SaveGoogleAccountsXML()
        {
            if (_xmlGoogleAccountSettings != null)
            {
                IsolatedStorageFile isoStore = IsolatedStorageFile.GetStore(IsolatedStorageScope.User | IsolatedStorageScope.Assembly, null, null);

                using (IsolatedStorageFileStream isoStream = new IsolatedStorageFileStream("SyncMultipleGoogleDrives.xml", FileMode.Create, isoStore))
                {
                    using (StreamWriter writer = new StreamWriter(isoStream))
                    {

                        writer.Write(_xmlGoogleAccountSettings.OuterXml);

                    }
                }

            }
        }

        private void AdaptGoogleAccountList()
        {

            if (_GoogleAccounts != null)
            {
                _GoogleAccounts.Clear();
            }
            else
            {
                _GoogleAccounts = new List<GoogleAccount>();
            }

            if (_xmlGoogleAccountSettings != null)
            {
                if (_xmlGoogleAccountSettings.HasChildNodes)
                {
                    XmlNodeList lstNodes = _xmlGoogleAccountSettings.GetElementsByTagName("GoogleAccount");
                    if (lstNodes != null && lstNodes.Count > 0)
                    {
                        foreach (XmlNode oNode in lstNodes)
                        {
                            GoogleAccount ga = new GoogleAccount();
                            XmlNodeList lstNodesPerAccount = oNode.ChildNodes;
                            if (lstNodesPerAccount != null && lstNodesPerAccount.Count > 0)
                            {
                                foreach (XmlNode oNodeChild in lstNodesPerAccount)
                                {
                                    switch (oNodeChild.Name)
                                    {
                                        case "Name":
                                            ga.Name = oNodeChild.InnerText;
                                            break;
                                        case "Email":
                                            ga.Email = oNodeChild.InnerText;
                                            break;
                                        case "ClientId":
                                            ga.ClientId = oNodeChild.InnerText;
                                            break;
                                        case "ClientSecret":
                                            ga.ClientSecret = oNodeChild.InnerText;
                                            break;
                                        case "FileDataStore":
                                            ga.FileDataStore = oNodeChild.InnerText;
                                            break;
                                        case "FileFilter":
                                            ga.FileFilter = oNodeChild.InnerText;
                                            break;
                                        case "RootFolder":
                                            ga.RootFolder = oNodeChild.InnerText;
                                            break;
                                    }
                                }
                            }
                            _GoogleAccounts.Add(ga);
                        }
                    }
                }


            }


        }

        private void SetColorsOfItemList(List<Item> itemsToSet)
        {
            if (itemsToSet != null && itemsToSet.Count > 0)
            {
                if (_GoogleAccounts != null && _GoogleAccounts.Count > 0)
                {
                    foreach (Item item in itemsToSet)
                    {
                        if (item.IsFolder)
                        {
                            string pathminushome = item.Path.Replace(txtRootFolder.Text, "");
                            string _forecolor = "Gray";
                            if (!string.IsNullOrEmpty(item.Name))
                            {
                                foreach (GoogleAccount ga in _GoogleAccounts)
                                {
                                    if (ga.FileFilter == "*")
                                    {
                                        _forecolor = "Green";
                                        ga.AddToListToUpload(item);
                                    }
                                    else
                                    {
                                        if (ga.FileFilter.EndsWith("*") && pathminushome.StartsWith(ga.FileFilter.Replace("*", "")))
                                        {
                                            _forecolor = "Green";
                                            ga.AddToListToUpload(item);
                                        }
                                        else if (ga.FileFilter.StartsWith("*") && pathminushome.EndsWith(ga.FileFilter.Replace("*", "")))
                                        {
                                            _forecolor = "Green";
                                            ga.AddToListToUpload(item);
                                        }
                                        else if (ga.FileFilter.Contains("*") && pathminushome.Contains(ga.FileFilter.Replace("*", "")))
                                        {
                                            _forecolor = "Green";
                                            ga.AddToListToUpload(item);
                                        }
                                    }
                                }
                                item.ForeColorString = _forecolor;
                            }
                            SetColorsOfItemList(((DirectoryItem)item).Items);
                        }
                        else
                        {
                            string pathminushome = item.Path.Replace(txtRootFolder.Text, "");
                            string _forecolor = "Gray";
                            foreach (GoogleAccount ga in _GoogleAccounts)
                            {
                                if (ga.FileFilter == "*")
                                {
                                    _forecolor = "Green";
                                    ga.AddToListToUpload(item);
                                }
                                else
                                {
                                    if (pathminushome.StartsWith("\\" + ga.FileFilter.Replace("*", "")))
                                    {
                                        _forecolor = "Green";
                                        ga.AddToListToUpload(item);
                                    }
                                }
                            }
                            item.ForeColorString = _forecolor;

                        }

                    }
                }
            }
        }


        private void btnSynch_Click(object sender, RoutedEventArgs e)
        {
            Thread newThread = new Thread(Synch);

            // Use the overload of the Start method that has a 
            // parameter of type Object. You can create an object that 
            // contains several pieces of data, or you can pass any  
            // reference type or value type. The following code passes 
            // the integer value 42. 
            //
            newThread.IsBackground = true;
            newThread.Start();
        }

        private void btnSynch_WithLog_Click(object sender, RoutedEventArgs e)
        {
            Thread newThread = new Thread(SynchWithLogging);

            // Use the overload of the Start method that has a 
            // parameter of type Object. You can create an object that 
            // contains several pieces of data, or you can pass any  
            // reference type or value type. The following code passes 
            // the integer value 42. 
            //
            newThread.SetApartmentState(ApartmentState.STA);
            newThread.IsBackground = true;
            newThread.Start();
        }
        

        private GoogleAccount _currentGA = null;

        private void Synch()
        {
            foreach (GoogleAccount ga in _GoogleAccounts)
            {
                _currentGA = ga;
                ga.UploadBusy -= ga_UploadBusy;
                ga.UploadEnd -= ga_UploadEnd;
                ga.UploadStart -= ga_UploadStart;

                ga.UploadBusy += ga_UploadBusy;
                ga.UploadEnd += ga_UploadEnd;
                ga.UploadStart += ga_UploadStart;

                cs.CurrentAccount = ga.Name;
                cs.TotalFileUploadValue = 0;
                cs.CurrentFileUploadValue = 0;
                cs.CurrentFile = "";
                cs.CurrentFolder = "";

                if (ga.ItemListOnGoogleDrive == null || ga.ItemListOnGoogleDrive.Count == 0)
                {
                    //ga.FillItemListOnGoogleDrive();
                    ga.FillItemListOnGoogleDriveNew();
                }
                if (ga.ItemListToUpload != null && ga.ItemListToUpload.Count > 0)
                {
                    double breuk = 100 / (double)ga.ItemListToUpload.Count;
                    double huidigtotaal = 0.0;
                    foreach (Item i in ga.ItemListToUpload)
                    {
                        huidigtotaal += breuk;
                        string abspath = i.Path.Substring(_rootFolder.Length);
                        abspath = abspath.Substring(0, abspath.Length - i.Name.Length);
                        if (abspath == "\\")
                        {
                            abspath = "";
                        }
                        if (abspath.StartsWith("\\"))
                        {
                            abspath = abspath.Substring(1);
                        }
                        if (abspath.EndsWith("\\"))
                        {
                            abspath = abspath.Substring(0, abspath.Length - 1);
                        }
                        if (!i.IsFolder)
                        {

                            cs.CurrentFile = i.Name;

                            ga.UploadFile(i.Path, i.Name, abspath);
                            //for (int tel = 0; tel <= 100; tel++)
                            //{
                            //    cs.CurrentFileUploadValue = tel;
                            //    Thread.Sleep(50);
                            //}
                        }
                        else
                        {
                            cs.CurrentFolder = i.Path;
                            ga.CreateFolder(i.Path, i.Name, abspath);
                            Thread.Sleep(50);
                        }
                        cs.CurrentFileUploadValue = 0;
                        cs.TotalFileUploadValue = (int)huidigtotaal;
                    }
                    cs.TotalFileUploadValue = 100;
                }
            }

        }

        private void SynchWithLogging()
        {
            foreach (GoogleAccount ga in _GoogleAccounts)
            {
                _currentGA = ga;
                ga.UploadBusy -= ga_UploadBusy;
                ga.UploadEnd -= ga_UploadEnd;
                ga.UploadStart -= ga_UploadStart;

                ga.UploadBusy += ga_UploadBusy;
                ga.UploadEnd += ga_UploadEnd;
                ga.UploadStart += ga_UploadStart;

                cs.CurrentAccount = ga.Name;
                cs.TotalFileUploadValue = 0;
                cs.CurrentFileUploadValue = 0;
                cs.CurrentFile = "";
                cs.CurrentFolder = "";

                if (ga.ItemListOnGoogleDrive == null || ga.ItemListOnGoogleDrive.Count == 0)
                {
                    //ga.FillItemListOnGoogleDrive();
                    ga.FillItemListOnGoogleDriveNew();
                }
                if (ga.ItemListToUpload != null && ga.ItemListToUpload.Count > 0)
                {

                    List<Item> lstItemsToUpload = ga.GetListOfItemsToUpload(ga.ItemListToUpload, _rootFolder);
                    if (lstItemsToUpload != null && lstItemsToUpload.Count > 0)
                    {
                        ga.UploadItems = lstItemsToUpload;
                    }




                }
            }
            UploadWindow wind = new UploadWindow();
            wind.Show();
            wind.SetItems(_GoogleAccounts, _rootFolder);
            System.Windows.Threading.Dispatcher.Run();


        }

        void ga_UploadStart(object sender, EventArgs e)
        {
            cs.CurrentFileUploadValue = 0;
        }

        void ga_UploadEnd(object sender, EventArgs e)
        {
            cs.CurrentFileUploadValue = 100;
        }

        void ga_UploadBusy(object sender, EventArgs e)
        {
            if (_currentGA != null)
            {
                cs.CurrentFileUploadValue = _currentGA.ProgressValue;
            }

        }
    }

}
