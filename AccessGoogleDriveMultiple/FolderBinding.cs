using System;
using System.Collections.Generic;
using System.IO;
using SyncMultipleGoogleDrives.Model;
using System.Diagnostics;


namespace SyncMultipleGoogleDrives.Binding
{
    public class ItemProvider
    {
        public List<Item> GetItems(string path)
        {
            if (!System.IO.Directory.Exists(path))
            {
                return null;
            }

            var items = new List<Item>();

            var dirInfo = new DirectoryInfo(path);

            try
            {
                foreach (var directory in dirInfo.GetDirectories())
                {
                    var item = new DirectoryItem
                    {
                        Name = directory.Name,
                        Path = directory.FullName,
                        Items = GetItems(directory.FullName),
                        IsFolder = true
                    };

                    items.Add(item);
                }


            }
            catch(Exception ex)
            {
                Trace.WriteLine(ex.Message + Environment.NewLine + ex.StackTrace, "EXCEPTION GetItems");
            }


            try
            {
                foreach (var file in dirInfo.GetFiles())
                {
                    if (file.Name.ToLower() != "thumbs.db" && file.Name.ToLower() != "desktop.ini")
                    {
                        var item = new FileItem
                        {
                            Name = file.Name,
                            Path = file.FullName,
                            IsFolder = false
                        };

                        items.Add(item);

                    }
                }

            }
            catch (Exception ex)
            {
                Trace.WriteLine(ex.Message + Environment.NewLine + ex.StackTrace, "EXCEPTION GetItems");
            }


            return items;
        }
    }
}