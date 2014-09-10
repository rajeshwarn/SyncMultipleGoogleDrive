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

        public void CreateFolder(string FullPath, string Name, string RelativePath)
        {
            Trace.WriteLine("FullPath:" + FullPath, "DEVINFO GoogleAccount.CreateFolder");
            Trace.WriteLine("Name:" + Name, "DEVINFO GoogleAccount.CreateFolder");
            Trace.WriteLine("RelativePath:" + RelativePath, "DEVINFO GoogleAccount.CreateFolder");

        }

        public void UploadFile(string FullPath, string Name, string RelativePath)
        {
            Trace.WriteLine("FullPath:" + FullPath, "DEVINFO GoogleAccount.UploadFile");
            Trace.WriteLine("Name:" + Name, "DEVINFO GoogleAccount.UploadFile");
            Trace.WriteLine("RelativePath:" + RelativePath, "DEVINFO GoogleAccount.UploadFile");

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

                    File body = new File();
                    body.Title = Name;
                    body.Description = Name;
                    string mimetype = GetMIMEType(FullPath);
                    if (mimetype != "unknown/unknown")
                    {
                        body.MimeType = mimetype;
                        byte[] byteArray = System.IO.File.ReadAllBytes(FullPath);
                        System.IO.MemoryStream stream = new System.IO.MemoryStream(byteArray);

                        FilesResource.InsertMediaUpload request = service.Files.Insert(body, stream, "text/plain");
                        request.Upload();

                        File file = request.ResponseBody;
                        Trace.WriteLine("File id: " + file.Id);
                    }



                }
            }


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
