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
using System.Diagnostics;

namespace SyncMultipleGoogleDrives
{
    public class GoogleAccount
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
        public void AddToListOnGoogleDrive(List<Item> items)
        {
            _itemListOnGoogleDrive.AddRange(items);
        }
        public void ClearListOnGoogleDrive()
        {
            _itemListOnGoogleDrive.Clear();
        }


        public void FillItemListOnGoogleDriveNew()
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
                        if (lstFolders != null)
                        {
                            // Determine RootFolderID
                            if (string.IsNullOrEmpty(RootFolderID))
                            {
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
                            }

                            if (!string.IsNullOrEmpty(RootFolderID))
                            {
                                GetAllFoldersUnderRootFolder(lstFolders, RootFolderID, RootFolder);
                            }
                            if (_itemListOnGoogleDrive != null && _itemListOnGoogleDrive.Count > 0)
                            {
                                List<Item> newList = _itemListOnGoogleDrive.ToList();
                                GetAllFilesUnderFolder(lstFile, newList);
                            }




                        }



                    }
                }
            }

        }

        private void GetAllFilesUnderFolder(List<File> lstFiles, List<Item> lstFolders)
        {
            if (lstFiles != null && lstFiles.Count > 0)
            {
                if (lstFolders != null && lstFolders.Count > 0)
                {
                    foreach (Item folder in lstFolders)
                    {
                        List<File> lstFilesOnderFolder = lstFiles.FindAll(m => !m.MimeType.EndsWith("apps.folder") && m.Parents != null && m.Parents.Count > 0 && m.Parents[0].Id == folder.GoogleID);
                        if (lstFilesOnderFolder != null && lstFilesOnderFolder.Count > 0)
                        {
                            foreach (File file in lstFilesOnderFolder)
                            {
                                Item item = new Item();
                                item.IsFolder = false;
                                item.Name = file.Title;
                                item.Path = folder.Path + "\\" + file.Title;
                                item.GoogleID = file.Id;
                                item.GoogleParentID = folder.GoogleID;
                                _itemListOnGoogleDrive.Add(item);
                            }

                        }


                    }

                }
            }
        }

        private void GetAllFoldersUnderRootFolder(List<File> lstFolders, string parentid, string parentname)
        {
            List<File> lstFoldersOnderRoot = lstFolders.FindAll(m => m.Parents != null && m.Parents.Count > 0 && m.Parents[0].Id == parentid);
            if (lstFoldersOnderRoot != null && lstFoldersOnderRoot.Count > 0)
            {
                foreach (File fold in lstFoldersOnderRoot)
                {
                    Item item = new Item();
                    item.IsFolder = true;
                    item.Name = fold.Title;
                    item.Path = parentname + "\\" + fold.Title;
                    item.GoogleID = fold.Id;
                    item.GoogleParentID = parentid;
                    _itemListOnGoogleDrive.Add(item);
                    GetAllFoldersUnderRootFolder(lstFolders, fold.Id, item.Path);
                }
            }

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
                        if (lstFolders != null)
                        {
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
                                    AddToListOnGoogleDrive(_foldStruct);
                                    foreach (Item fold in _foldStruct)
                                    {
                                        List<Item> _foldItems = new List<Item>();
                                        GetFolderItems(ref _foldItems, fold.GoogleID);
                                        BrowseAllFiles(lstFile, ref _foldItems, fold.Path);
                                        AddToListOnGoogleDrive(_foldItems);
                                    }
                                }

                                if (_itemListOnGoogleDrive != null && _itemListOnGoogleDrive.Count > 0)
                                {

                                    foreach (Item itm in _itemListOnGoogleDrive)
                                    {
                                        if (itm.IsFolder)
                                        {
                                            Trace.WriteLine("Folder ID:" + itm.GoogleID + ";ParentID:" + itm.GoogleParentID + ";Path:" + itm.Path, "DEVINFO FillItemListOnGoogleDrive");
                                        }
                                        else
                                        {
                                            Trace.WriteLine("File ID:" + itm.GoogleID + ";ParentID:" + itm.GoogleParentID + ";Path:" + itm.Path, "DEVINFO FillItemListOnGoogleDrive");
                                        }

                                    }

                                }

                            }
                        }

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
                    itm.GoogleParentID = rootid;
                    itm.IsFolder = true;
                    lstItems.Add(itm);
                    GetFolderStructure(ref lstItems, fold.Id);
                }

            }
        }

        private void GetFolderItems(ref List<Item> lstItems, string rootid)
        {
            //ChildList listadoFiles = service.Children.List(rootid).Execute();
            //Q = "mimeType='application/vnd.google-apps.folder'

            ChildrenResource.ListRequest request = service.Children.List(rootid);
            request.Q = "mimeType != 'application/vnd.google-apps.folder'";
            ChildList listadoFiles = request.Execute();


            if (listadoFiles != null && listadoFiles.Items.Count > 0)
            {
                foreach (ChildReference fold in listadoFiles.Items)
                {
                    Item itm = new Item();
                    itm.GoogleID = fold.Id;
                    itm.GoogleParentID = rootid;
                    itm.IsFolder = false;
                    lstItems.Add(itm);
                }

            }
        }

        private void BrowseAllFiles(List<File> lstFiles, ref List<Item> lstFileIds, string rootFolder)
        {
            if (lstFiles != null && lstFiles.Count > 0)
            {
                if (lstFileIds != null && lstFileIds.Count > 0)
                {
                    for (int i = 0; i < lstFileIds.Count; i++)
                    {
                        for (int j = 0; j < lstFiles.Count; j++)
                        {
                            if (lstFileIds[i].GoogleID == lstFiles[j].Id)
                            {
                                if (string.IsNullOrEmpty(lstFileIds[i].Name))
                                {
                                    lstFileIds[i].Name = lstFiles[j].Title;
                                    lstFileIds[i].Path = rootFolder + "\\" + lstFiles[j].Title;
                                }
                                break;
                            }
                        }
                    }
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
                            if (lstFolders[j].Parents != null && lstFolders[j].Parents.Count > 0 &&
                                lstFolders[j].Parents[0].Id == rootID &&
                                foldStruct[i].GoogleID == lstFolders[j].Id)
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
                        ClientId = ClientId,
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


        public static List<Google.Apis.Drive.v2.Data.File> retrieveAllFiles(DriveService service, bool IncludingTrashed = false)
        {
            List<Google.Apis.Drive.v2.Data.File> result = new List<Google.Apis.Drive.v2.Data.File>();
            FilesResource.ListRequest request = service.Files.List();
            if (!IncludingTrashed)
            {
                request.Q = "trashed=false";
            }
            request.MaxResults = 1000;

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

        public void CreateFolder(string FullPath, string Name, string RelativePath)
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

                    if (!FolderExists(FullPath, Name, RelativePath))
                    {
                        _CurrentGoogleID = "";
                        File body = new File();
                        body.Title = Name;
                        body.Description = Name;
                        body.MimeType = "application/vnd.google-apps.folder";
                        string parent = GetParentId(RelativePath);
                        if (!string.IsNullOrEmpty(parent))
                        {
                            body.Parents = new List<ParentReference>() { new ParentReference() { Id = parent } };
                        }

                        try
                        {


                            FilesResource.InsertRequest request = service.Files.Insert(body);
                            File file = request.Execute();
                            Item newFold = new Item();
                            newFold.Name = Name;
                            newFold.IsFolder = true;
                            if (!string.IsNullOrEmpty(RelativePath))
                            {
                                newFold.Path = RootFolder + "\\" + RelativePath + "\\" + Name;
                            }
                            else
                            {
                                newFold.Path = RootFolder + "\\" + Name;
                            }
                            newFold.GoogleID = file.Id;
                            newFold.GoogleParentID = parent;
                            AddToListOnGoogleDrive(newFold);
                            _CurrentGoogleID = file.Id;
                        }
                        catch (Exception e)
                        {
                            Console.WriteLine("An error occurred: " + e.Message);
                        }
                    }

                }
            }




        }

        private List<Item> _UploadItems;
        public List<Item> UploadItems
        {
            get { return _UploadItems; }
            set { _UploadItems = value;  }
        }

        public bool FolderExists(string FullPath, string Name, string RelativePath)
        {
            bool _return = false;

            if (_itemListOnGoogleDrive != null && _itemListOnGoogleDrive.Count > 0)
            {
                Item folder = null;
                if (string.IsNullOrEmpty(RelativePath))
                {
                    folder = _itemListOnGoogleDrive.FirstOrDefault(f => f.Path == RootFolder + "\\" + Name && f.Name == Name && f.IsFolder == true);
                }
                else
                {
                    folder = _itemListOnGoogleDrive.FirstOrDefault(f => f.Path == RootFolder + "\\" + RelativePath + "\\" + Name && f.Name == Name && f.IsFolder == true);
                }
                if (folder != null)
                {
                    _return = true;
                }
            }

            return _return;
        }

        public bool FileExists(string FullPath, string Name, string RelativePath)
        {
            bool _return = false;

            if (_itemListOnGoogleDrive != null && _itemListOnGoogleDrive.Count > 0)
            {
                Item file = _itemListOnGoogleDrive.FirstOrDefault(f => f.Path == RootFolder + "\\" + RelativePath + "\\" + Name && f.Name == Name && f.IsFolder == false);
                if (file != null)
                {
                    _return = true;
                }
            }

            return _return;
        }

        private string GetParentId(string RelativePath)
        {
            string _return = "";

            if (_itemListOnGoogleDrive != null && _itemListOnGoogleDrive.Count > 0)
            {
                Item file = null;
                if (string.IsNullOrEmpty(RelativePath))
                {
                    file = _itemListOnGoogleDrive.FirstOrDefault(f => f.Path == RootFolder && f.IsFolder == true);
                }
                else
                {
                    file = _itemListOnGoogleDrive.FirstOrDefault(f => f.Path == RootFolder + "\\" + RelativePath && f.IsFolder == true);
                }
                if (file != null)
                {
                    _return = file.GoogleID;
                }
            }
            if (string.IsNullOrEmpty(_return))
            {
                _return = RootFolderID;
            }


            return _return;
        }

        private string _CurrentGoogleID;
        public string CurrentGoogleID
        {
            get
            {
                return _CurrentGoogleID;
            }
        }

        public void UploadFile(string FullPath, string Name, string RelativePath, int Retries = 0)
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

                    if (!FileExists(FullPath, Name, RelativePath))
                    {
                        try
                        {
                            _CurrentGoogleID = "";
                            File body = new File();
                            body.Title = Name;
                            body.Description = Name;
                            string parent = GetParentId(RelativePath);
                            if (!string.IsNullOrEmpty(parent))
                            {
                                body.Parents = new List<ParentReference>() { new ParentReference() { Id = parent } };
                            }
                            else
                            {
                                if (!string.IsNullOrEmpty(RootFolderID))
                                {
                                    body.Parents = new List<ParentReference>() { new ParentReference() { Id = parent } };
                                }
                            }
                            string mimetype = GetMIMEType(FullPath);
                            if (mimetype != "unknown/unknown")
                            {
                                Trace.WriteLine("FullPath:" + FullPath, "DEVINFO GoogleAccount.UploadFile");
                                Trace.WriteLine("Name:" + Name, "DEVINFO GoogleAccount.UploadFile");
                                Trace.WriteLine("RelativePath:" + RelativePath, "DEVINFO GoogleAccount.UploadFile");

                                body.MimeType = mimetype;
                                byte[] byteArray = System.IO.File.ReadAllBytes(FullPath);
                                _bytesToUpload = byteArray.Length;
                                System.IO.MemoryStream stream = new System.IO.MemoryStream(byteArray);

                                FilesResource.InsertMediaUpload request = service.Files.Insert(body, stream, "text/plain");
                                request.ProgressChanged += request_ProgressChanged;
                                request.Upload();

                                File file = request.ResponseBody;

                                Item newFile = new Item();
                                newFile.Name = Name;
                                newFile.IsFolder = false;
                                if (!string.IsNullOrEmpty(RelativePath))
                                {
                                    newFile.Path = RootFolder + "\\" + RelativePath + "\\" + Name;
                                }
                                else
                                {
                                    newFile.Path = RootFolder + "\\" + Name;
                                }
                                newFile.GoogleID = file.Id;
                                newFile.GoogleParentID = parent;
                                AddToListOnGoogleDrive(newFile);
                                _CurrentGoogleID = file.Id;
                                Trace.WriteLine("File id: " + file.Id, "DEVINFO GoogleAccount.UploadFile");
                            }

                        }
                        catch(Exception ex)
                        {
                            Trace.WriteLine(ex.Message, "EXCEPTION UploadFile");
                            // Retry
                            if (Retries < 3)
                            {
                                Thread.Sleep(5000);
                                UploadFile(FullPath, Name, RelativePath, Retries++);
                            }
                        }



                    }




                }
            }


        }

        private static double _bytesToUpload = 0;
        private static double _bytesUploaded = 0;

        public int ProgressValue
        {
            get
            {
                if (_bytesToUpload != 0)
                {
                    return (int)(_bytesUploaded / _bytesToUpload * 100); 
                }
                else
                {
                    return 0;
                }
            }
        }

        public event EventHandler UploadStart;
        public event EventHandler UploadEnd;
        public event EventHandler UploadBusy;

        protected virtual void OnUploadStart()
        {
            EventHandler handler = UploadStart;
            if (handler != null)
            {
                handler(this, null);
            }
        }
        protected virtual void OnUploadEnd()
        {
            EventHandler handler = UploadEnd;
            if (handler != null)
            {
                handler(this, null);
            }
        }
        protected virtual void OnUploadBusy()
        {
            EventHandler handler = UploadBusy;
            if (handler != null)
            {
                handler(this, null);
            }
        }

        void request_ProgressChanged(Google.Apis.Upload.IUploadProgress obj)
        {
            switch (obj.Status)
            {
                case Google.Apis.Upload.UploadStatus.Starting:
                    OnUploadStart();
                    break;
                case Google.Apis.Upload.UploadStatus.Uploading:
                    _bytesUploaded = obj.BytesSent;
                    OnUploadBusy();
                    break;
                case Google.Apis.Upload.UploadStatus.Completed:
                    OnUploadEnd();
                    break;
            }

            //Trace.WriteLine( obj.Status.ToString() );

        }

        public List<Item> GetListOfItemsToUpload(List<Item> lstLocalFiles, string RootFolder)
        {
            List<Item> lstReturn = new List<Item>();

            List<Item> newList = lstLocalFiles.ToList();
            newList.Sort();

            foreach (Item item in newList)
            {
                string abspath = item.Path.Substring(RootFolder.Length);
                abspath = abspath.Substring(0, abspath.Length - item.Name.Length);
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
                if (item.IsFolder)
                {
                    if (!FolderExists(item.Path, item.Name, abspath))
                    {
                        item.GoogleAccount = this.Name;
                        lstReturn.Add(item);
                    }
                }
                else
                {
                    if (!FileExists(item.Path, item.Name, abspath))
                    {
                        item.GoogleAccount = this.Name;
                        lstReturn.Add(item);
                    }
                }
            }



            return lstReturn;
            
        }


        private static readonly Dictionary<string, string> MIMETypesDictionary = new Dictionary<string, string>
  {
      {"7z", "application/x-7z-compressed"},
    {"ai", "application/postscript"},
    {"aif", "audio/x-aiff"},
    {"aifc", "audio/x-aiff"},
    {"aiff", "audio/x-aiff"},
    {"asc", "text/plain"},
    {"atom", "application/atom+xml"},
    {"au", "audio/basic"},
    {"avi", "video/x-msvideo"},
    {"bcpio", "application/x-bcpio"},
    {"bin", "application/octet-stream"},
    {"bmp", "image/bmp"},
    {"cdf", "application/x-netcdf"},
    {"cgm", "image/cgm"},
    {"class", "application/octet-stream"},
    {"cpio", "application/x-cpio"},
    {"cpt", "application/mac-compactpro"},
    {"csh", "application/x-csh"},
    {"css", "text/css"},
    {"dcr", "application/x-director"},
    {"dif", "video/x-dv"},
    {"dir", "application/x-director"},
    {"djv", "image/vnd.djvu"},
    {"djvu", "image/vnd.djvu"},
    {"dll", "application/octet-stream"},
    {"dmg", "application/octet-stream"},
    {"dms", "application/octet-stream"},
    {"doc", "application/msword"},
    {"docx","application/vnd.openxmlformats-officedocument.wordprocessingml.document"},
    {"dotx", "application/vnd.openxmlformats-officedocument.wordprocessingml.template"},
    {"docm","application/vnd.ms-word.document.macroEnabled.12"},
    {"dotm","application/vnd.ms-word.template.macroEnabled.12"},
    {"dtd", "application/xml-dtd"},
    {"dv", "video/x-dv"},
    {"dvi", "application/x-dvi"},
    {"dxr", "application/x-director"},
    {"eps", "application/postscript"},
    {"etx", "text/x-setext"},
    {"exe", "application/octet-stream"},
    {"ez", "application/andrew-inset"},
    {"gif", "image/gif"},
    {"gram", "application/srgs"},
    {"grxml", "application/srgs+xml"},
    {"gtar", "application/x-gtar"},
    {"hdf", "application/x-hdf"},
    {"hqx", "application/mac-binhex40"},
    {"htm", "text/html"},
    {"html", "text/html"},
    {"ice", "x-conference/x-cooltalk"},
    {"ico", "image/x-icon"},
    {"ics", "text/calendar"},
    {"ief", "image/ief"},
    {"ifb", "text/calendar"},
    {"iges", "model/iges"},
    {"igs", "model/iges"},
    {"jnlp", "application/x-java-jnlp-file"},
    {"jp2", "image/jp2"},
    {"jpe", "image/jpeg"},
    {"jpeg", "image/jpeg"},
    {"jpg", "image/jpeg"},
    {"js", "application/x-javascript"},
    {"kar", "audio/midi"},
    {"latex", "application/x-latex"},
    {"lha", "application/octet-stream"},
    {"log", "text/plain"},
    {"lzh", "application/octet-stream"},
    {"m3u", "audio/x-mpegurl"},
    {"m4a", "audio/mp4a-latm"},
    {"m4b", "audio/mp4a-latm"},
    {"m4p", "audio/mp4a-latm"},
    {"m4u", "video/vnd.mpegurl"},
    {"m4v", "video/x-m4v"},
    {"mac", "image/x-macpaint"},
    {"man", "application/x-troff-man"},
    {"mathml", "application/mathml+xml"},
    {"me", "application/x-troff-me"},
    {"mesh", "model/mesh"},
    {"mid", "audio/midi"},
    {"midi", "audio/midi"},
    {"mif", "application/vnd.mif"},
    {"mov", "video/quicktime"},
    {"movie", "video/x-sgi-movie"},
    {"mp2", "audio/mpeg"},
    {"mp3", "audio/mpeg"},
    {"mp4", "video/mp4"},
    {"mpe", "video/mpeg"},
    {"mpeg", "video/mpeg"},
    {"mpg", "video/mpeg"},
    {"mpga", "audio/mpeg"},
    {"ms", "application/x-troff-ms"},
    {"msh", "model/mesh"},
    {"mxu", "video/vnd.mpegurl"},
    {"nc", "application/x-netcdf"},
    {"oda", "application/oda"},
    {"ogg", "application/ogg"},
    {"pbm", "image/x-portable-bitmap"},
    {"pct", "image/pict"},
    {"pdb", "chemical/x-pdb"},
    {"pdf", "application/pdf"},
    {"pgm", "image/x-portable-graymap"},
    {"pgn", "application/x-chess-pgn"},
    {"pic", "image/pict"},
    {"pict", "image/pict"},
    {"png", "image/png"}, 
    {"pnm", "image/x-portable-anymap"},
    {"pnt", "image/x-macpaint"},
    {"pntg", "image/x-macpaint"},
    {"ppm", "image/x-portable-pixmap"},
    {"ppt", "application/vnd.ms-powerpoint"},
    {"pptx","application/vnd.openxmlformats-officedocument.presentationml.presentation"},
    {"potx","application/vnd.openxmlformats-officedocument.presentationml.template"},
    {"ppsx","application/vnd.openxmlformats-officedocument.presentationml.slideshow"},
    {"ppam","application/vnd.ms-powerpoint.addin.macroEnabled.12"},
    {"pptm","application/vnd.ms-powerpoint.presentation.macroEnabled.12"},
    {"potm","application/vnd.ms-powerpoint.template.macroEnabled.12"},
    {"ppsm","application/vnd.ms-powerpoint.slideshow.macroEnabled.12"},
    {"ps", "application/postscript"},
    {"qt", "video/quicktime"},
    {"qti", "image/x-quicktime"},
    {"qtif", "image/x-quicktime"},
    {"ra", "audio/x-pn-realaudio"},
    {"ram", "audio/x-pn-realaudio"},
    {"ras", "image/x-cmu-raster"},
    {"rdf", "application/rdf+xml"},
    {"rgb", "image/x-rgb"},
    {"rm", "application/vnd.rn-realmedia"},
    {"roff", "application/x-troff"},
    {"rtf", "text/rtf"},
    {"rtx", "text/richtext"},
    {"sgm", "text/sgml"},
    {"sgml", "text/sgml"},
    {"sh", "application/x-sh"},
    {"shar", "application/x-shar"},
    {"silo", "model/mesh"},
    {"sit", "application/x-stuffit"},
    {"skd", "application/x-koan"},
    {"skm", "application/x-koan"},
    {"skp", "application/x-koan"},
    {"skt", "application/x-koan"},
    {"smi", "application/smil"},
    {"smil", "application/smil"},
    {"snd", "audio/basic"},
    {"so", "application/octet-stream"},
    {"spl", "application/x-futuresplash"},
    {"src", "application/x-wais-source"},
    {"sv4cpio", "application/x-sv4cpio"},
    {"sv4crc", "application/x-sv4crc"},
    {"svg", "image/svg+xml"},
    {"swf", "application/x-shockwave-flash"},
    {"t", "application/x-troff"},
    {"tar", "application/x-tar"},
    {"tcl", "application/x-tcl"},
    {"tex", "application/x-tex"},
    {"texi", "application/x-texinfo"},
    {"texinfo", "application/x-texinfo"},
    {"tif", "image/tiff"},
    {"tiff", "image/tiff"},
    {"tr", "application/x-troff"},
    {"tsv", "text/tab-separated-values"},
    {"txt", "text/plain"},
    {"ustar", "application/x-ustar"},
    {"vcd", "application/x-cdlink"},
    {"vrml", "model/vrml"},
    {"vxml", "application/voicexml+xml"},
    {"wav", "audio/x-wav"},
    {"wbmp", "image/vnd.wap.wbmp"},
    {"wbmxl", "application/vnd.wap.wbxml"},
    {"wml", "text/vnd.wap.wml"},
    {"wmlc", "application/vnd.wap.wmlc"},
    {"wmls", "text/vnd.wap.wmlscript"},
    {"wmlsc", "application/vnd.wap.wmlscriptc"},
    {"wrl", "model/vrml"},
    {"xbm", "image/x-xbitmap"},
    {"xht", "application/xhtml+xml"},
    {"xhtml", "application/xhtml+xml"},
    {"xls", "application/vnd.ms-excel"},                        
    {"xml", "application/xml"},
    {"xpm", "image/x-xpixmap"},
    {"xsl", "application/xml"},
    {"xlsx","application/vnd.openxmlformats-officedocument.spreadsheetml.sheet"},
    {"xltx","application/vnd.openxmlformats-officedocument.spreadsheetml.template"},
    {"xlsm","application/vnd.ms-excel.sheet.macroEnabled.12"},
    {"xltm","application/vnd.ms-excel.template.macroEnabled.12"},
    {"xlam","application/vnd.ms-excel.addin.macroEnabled.12"},
    {"xlsb","application/vnd.ms-excel.sheet.binary.macroEnabled.12"},
    {"xslt", "application/xslt+xml"},
    {"xul", "application/vnd.mozilla.xul+xml"},
    {"xwd", "image/x-xwindowdump"},
    {"xyz", "chemical/x-xyz"},
    {"zip", "application/zip"}
  };

        public static string GetMIMEType(string fileName)
        {
            //get file extension
            string extension = System.IO.Path.GetExtension(fileName).ToLowerInvariant();

            if (extension.Length > 0 &&
                MIMETypesDictionary.ContainsKey(extension.Remove(0, 1)))
            {
                return MIMETypesDictionary[extension.Remove(0, 1)];
            }
            return "unknown/unknown";
        }

    }
}
