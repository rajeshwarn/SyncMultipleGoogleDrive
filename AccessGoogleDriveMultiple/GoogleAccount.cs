using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using SyncMultipleGoogleDrives.Model;
using Google.Apis.Drive.v2;
using Google.Apis.Drive.v2.Data;
using Google.Apis.Auth.OAuth2;
using System.Threading;
using Google.Apis.Util.Store;
using Google.Apis.Services;

namespace SyncMultipleGoogleDrives
{
    class GoogleAccount
    {

        UserCredential credential;
        DriveService service;

        private string RootFolderID;

        public string Name;

        public string Email;

        public string ClientId;

        public string ClientSecret;

        public string FileDataStore;

        public string FileFilter;

        public string RootFolder;

        private List<Item> _itemListToUpload = new List<Item>();

        private List<Item> _itemListOnGoogleDrive = new List<Item>();

        public void AddToListToUpload(Item item)
        {
            _itemListToUpload.Add(item);
        }

        public List<Item> ItemListToUpload
        {
            get
            {
                return _itemListToUpload;
            }
        }
        public void ClearListToUpload()
        {
            _itemListToUpload.Clear();
        }

        public List<Item> ItemListOnGoogleDrive
        {
            get
            {
                return _itemListOnGoogleDrive;
            }
        }

        public void AddToListOnGoogleDrive(Item item)
        {
            _itemListOnGoogleDrive.Add(item);
        }
        public void ClearListOnGoogleDrive()
        {
            _itemListOnGoogleDrive.Clear();
        }


        public void FillItemListOnGoogleDrive()
        {

            if (credential == null)
            {
                SetCredential();
            }
            if (credential != null)
            {
                if (service == null)
                {
                    SetService();
                }
                if (service != null)
                {
                    List<File> lstFile = retrieveAllFiles(service);
                    if (lstFile != null && lstFile.Count > 0)
                    {

                        List<File> lstFolders = lstFile.FindAll(m => m.MimeType.EndsWith("apps.folder"));
                        if(lstFolders != null){
                            foreach (File fold in lstFolders)
                            {
                                if (fold.Title.ToLower() == RootFolder.ToLower())
                                {
                                    RootFolderID = fold.Id;
                                    Item item = new Item();
                                    item.IsFolder = true;
                                    item.Name = fold.Title;
                                    item.Path = fold.Title;
                                    item.GoogleID = fold.Id;
                                    _itemListOnGoogleDrive.Add(item);
                                    break;
                                }
                            }


                            if (!string.IsNullOrEmpty(RootFolderID))
                            {
                                List<Item> _foldStruct = new List<Item>();
                                GetFolderStructure(ref _foldStruct, RootFolderID);

                                if (_foldStruct != null && _foldStruct.Count > 0)
                                {
                                    BrowseAllFolders(lstFolders, RootFolderID, RootFolder, ref _foldStruct);
                                    if (_foldStruct != null && _foldStruct.Count > 0)
                                    {
                                        foreach (Item fold in _foldStruct)
                                        {
                                            BrowseAllFolders(lstFolders, fold.GoogleID, fold.Path, ref _foldStruct);
                                        }
                                    }
                                }

                                if (_foldStruct != null && _foldStruct.Count > 0)
                                {
                                    foreach (Item fold in _foldStruct)
                                    {
                                        System.Diagnostics.Trace.WriteLine(fold.Name + " : " + fold.Path);
                                    }
                                }

                            }
                        }

                        //foreach (File f in lstFile)
                        //{
                        //    if (f.MimeType.EndsWith("apps.folder"))
                        //    {
                        //        System.Diagnostics.Trace.WriteLine(f.Title);
                        //        System.Diagnostics.Trace.WriteLine(f.Parents )
                        //    }
                        //    else if (f.Title.Contains("pellikaan"))
                        //    {
                        //        System.Diagnostics.Trace.WriteLine(f.Title);
                        //    }
                        //    else
                        //    {
                        //        System.Diagnostics.Trace.WriteLine(f.Title);
                        //    }
                        //}
                    }
                }
            }


        }

        private void GetFolderStructure(ref List<Item> lstItems, string rootid)
        {
            //ChildList listadoFiles = service.Children.List(rootid).Execute();
            //Q = "mimeType='application/vnd.google-apps.folder'

            ChildrenResource.ListRequest request = service.Children.List(rootid);
            request.Q = "mimeType='application/vnd.google-apps.folder'";
            ChildList listadoFiles = request.Execute();


            if (listadoFiles != null && listadoFiles.Items.Count > 0)
            {
                foreach (ChildReference fold in listadoFiles.Items)
                {
                    Item itm = new Item();
                    itm.GoogleID = fold.Id;
                    itm.IsFolder = true;
                    lstItems.Add(itm);
                    GetFolderStructure(ref lstItems, fold.Id);
                }

            }
        }

        private void BrowseAllFolders(List<File> lstFolders, string rootID, string rootFolder, ref List<Item> foldStruct)
        {

            if (lstFolders != null && lstFolders.Count > 0)
            {
                if (foldStruct != null && foldStruct.Count > 0)
                {

                    for (int i = 0; i < foldStruct.Count; i++)
                    {
                        for (int j = 0; j < lstFolders.Count; j++)
                        {
                            if (foldStruct[i].GoogleID == lstFolders[j].Id)
                            {
                                if (string.IsNullOrEmpty(foldStruct[i].Name))
                                {
                                    foldStruct[i].Name = lstFolders[j].Title;
                                    foldStruct[i].Path = rootFolder + "\\" + lstFolders[j].Title;
                                }
                                break;
                            }
                        }
                    }

                }
            }


        }

        private void SetCredential()
        {
            credential = GoogleWebAuthorizationBroker.AuthorizeAsync(
                    new ClientSecrets
                    {
                        ClientId = ClientId ,
                        ClientSecret = ClientSecret,
                    },
                    new[] { DriveService.Scope.Drive },
                    "user",
                    CancellationToken.None,
                    new FileDataStore(FileDataStore)).Result;
        }
        private void SetService()
        {
            // Create the service.
            service = new DriveService(new BaseClientService.Initializer()
            {
                HttpClientInitializer = credential,
                ApplicationName = "Drive API Sample",
            });
        }


        public static List<Google.Apis.Drive.v2.Data.File> retrieveAllFiles(DriveService service)
        {
            List<Google.Apis.Drive.v2.Data.File> result = new List<Google.Apis.Drive.v2.Data.File>();
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

    }
}
