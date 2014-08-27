﻿using System;
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
using AccessGoogleDriveMultiple.Binding;
using AccessGoogleDriveMultiple.Model;

namespace AccessGoogleDriveMultiple
{
    /// <summary>
    /// Interaction logic for MainWindow.xaml
    /// </summary>
    public partial class MainWindow : Window
    {
        public MainWindow()
        {
            InitializeComponent();

            if (string.IsNullOrEmpty(Properties.Settings.Default.rootFolder))
            {
                if(System.IO.Directory.Exists(Environment.GetFolderPath(Environment.SpecialFolder.MyDocuments))){
                    txtRootFolder.Text = Environment.GetFolderPath(Environment.SpecialFolder.MyDocuments);
                    Properties.Settings.Default.rootFolder = txtRootFolder.Text;
                    Properties.Settings.Default.Save();
                }
            }
            else
            {
                txtRootFolder.Text = Properties.Settings.Default.rootFolder;
            }


            var itemProvider = new ItemProvider();

            var items = itemProvider.GetItems(txtRootFolder.Text);

            DataContext = items;

            txtRootFolder.TextChanged += txtRootFolder_TextChanged;

        }

        void txtRootFolder_TextChanged(object sender, TextChangedEventArgs e)
        {

            var itemProvider = new ItemProvider();

            var items = itemProvider.GetItems(txtRootFolder.Text);

            DataContext = items;


        }

        private void Button_Click(object sender, RoutedEventArgs e)
        {
            //UserCredential credential = GoogleWebAuthorizationBroker.AuthorizeAsync(
            //    new ClientSecrets
            //    {
            //        ClientId = "846715596453-539dui7i3rf5kqmmkenfnkk1i2d5dhlj.apps.googleusercontent.com",
            //        ClientSecret = "U-V5GMN3qk5Z6ubSql1xYD9M",
            //    },
            //    new[] { DriveService.Scope.Drive },
            //    "user",
            //    CancellationToken.None, 
            //    new FileDataStore("credentials_koen.mestdagh")).Result;

            UserCredential credential = GoogleWebAuthorizationBroker.AuthorizeAsync(
    new ClientSecrets
    {
        ClientId = "610519876157-svk4euh3mk2bt37j43engm1uciid0jc9.apps.googleusercontent.com",
        ClientSecret = "2x4HYu0kucG6wXqPYNBZ8dmV",
    },
    new[] { DriveService.Scope.Drive },
    "user",
    CancellationToken.None,
    new FileDataStore("credentials_koen.mestdagh2014")).Result;

            // Create the service.
            var service = new DriveService(new BaseClientService.Initializer()
            {
                HttpClientInitializer = credential,
                ApplicationName = "Drive API Sample",
            });

            List<File> lstFile = retrieveAllFiles(service);
            if (lstFile != null && lstFile.Count > 0)
            {
                foreach (File f in lstFile)
                {
                    if (f.MimeType.EndsWith("apps.folder"))
                    {
                        Trace.WriteLine(f.Title);
                    }
                    else if (f.Title.Contains("pellikaan"))
                    {
                        Trace.WriteLine(f.Title);
                    }
                    else
                    {
                        Trace.WriteLine(f.Title);
                    }
                }
            }

            //File body = new File();
            //body.Title = "My document";
            //body.Description = "A test document";
            //body.MimeType = "text/plain";

            //byte[] byteArray = System.IO.File.ReadAllBytes("document.txt");
            //System.IO.MemoryStream stream = new System.IO.MemoryStream(byteArray);

            //FilesResource.InsertMediaUpload request = service.Files.Insert(body, stream, "text/plain");
            //request.Upload();

            //File file = request.ResponseBody;
            //Console.WriteLine("File id: " + file.Id);
        }


        public static List<File> retrieveAllFiles(DriveService service)
        {
            List<File> result = new List<File>();
            FilesResource.ListRequest request = service.Files.List();

            do
            {
                try
                {
                    FileList files = request.Execute();

                    result.AddRange(files.Items);
                    request.PageToken = files.NextPageToken;
                }
                catch (Exception e)
                {
                    Console.WriteLine("An error occurred: " + e.Message);
                    request.PageToken = null;
                }
            } while (!String.IsNullOrEmpty(request.PageToken));
            return result;
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
    }
}