using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.IO;

namespace AccessGoogleDriveMultiple.Model
{
    public class Item
    {
        public string Name { get; set; }
        public string Path { get; set; }
        public bool Checked { get; set; }
    }
}

namespace AccessGoogleDriveMultiple.Model
{
    public class FileItem : Item
    {

    }
}

namespace AccessGoogleDriveMultiple.Model
{
    public class DirectoryItem : Item
    {
        public List<Item> Items { get; set; }

        public DirectoryItem()
        {
            Items = new List<Item>();
        }
    }
}