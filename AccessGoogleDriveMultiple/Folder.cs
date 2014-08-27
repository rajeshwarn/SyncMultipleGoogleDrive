using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.IO;
using System.Drawing;
using System.Windows.Media;

namespace SyncMultipleGoogleDrives.Model
{
    public class Item
    {
        public string Name { get; set; }
        public string Path { get; set; }

        //private Color _ForeColor = (Color)ColorConverter.ConvertFromString("AliceBlue");
        //public Color ForeColor
        //{
        //    get
        //    {
        //        return _ForeColor;
        //    }
        //    set{
        //        _ForeColor = value;
        //    }
        //}

        private string _ForeColorString = "Gray";
        public string ForeColorString
        {
            get
            {
                return _ForeColorString;
            }
            set
            {
                _ForeColorString = value;
            }
        }
    }
}

namespace SyncMultipleGoogleDrives.Model
{
    public class FileItem : Item
    {

    }
}

namespace SyncMultipleGoogleDrives.Model
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