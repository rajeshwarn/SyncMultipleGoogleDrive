using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.IO;
using System.Drawing;
using System.Windows.Media;
using System.ComponentModel;
using System.Runtime.CompilerServices;

namespace SyncMultipleGoogleDrives.Model
{
    public class Item : IComparable<Item>, INotifyPropertyChanged
    {

        public event PropertyChangedEventHandler PropertyChanged;

        // This method is called by the Set accessor of each property. 
        // The CallerMemberName attribute that is applied to the optional propertyName 
        // parameter causes the property name of the caller to be substituted as an argument. 
        private void NotifyPropertyChanged([CallerMemberName] String propertyName = "")
        {
            if (PropertyChanged != null)
            {
                PropertyChanged(this, new PropertyChangedEventArgs(propertyName));
            }
        }

        private string _Name;
        private string _Path;
        private bool _IsFolder;
        private string _GoogleID;
        private string _GoogleParentID;
        private int _UploadProgress;
        private string _GoogleAccount;

        public string Name
        {
            get
            {
                return _Name;
            }
            set
            {
                _Name = value;
                NotifyPropertyChanged();
            }
        }
        public string Path {
            get
            {
                return _Path;
            }
            set
            {
                _Path = value;
                NotifyPropertyChanged();
            }
        }
        public bool IsFolder {
            get
            {
                return _IsFolder;
            }
            set
            {
                _IsFolder = value;
                NotifyPropertyChanged();
            }
        }
        public string GoogleID {
            get
            {
                return _GoogleID;
            }
            set
            {
                _GoogleID = value;
                NotifyPropertyChanged();
            }
        }
        public string GoogleParentID {
            get
            {
                return _GoogleParentID;
            }
            set
            {
                _GoogleParentID = value;
                NotifyPropertyChanged();
            }
        }
        public int UploadProgress {
            get
            {
                return _UploadProgress;
            }
            set
            {
                _UploadProgress = value;
                NotifyPropertyChanged();
            }
        }
        public string GoogleAccount
        {
            get
            {
                return _GoogleAccount;
            }
            set
            {
                _GoogleAccount = value;
                NotifyPropertyChanged();
            }
        }


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

        //public override bool Equals(object obj)
        //{
        //    if (obj == null) return false;
        //    Item objAsPart = obj as Item;
        //    if (objAsPart == null) return false;
        //    else return Equals(objAsPart);
        //}

        // Default comparer for Part type. 
        public int CompareTo(Item compareItem)
        {
            // A null value means that this object is greater. 
            if (compareItem == null)
                return 1;

            else
                return this.Path.CompareTo(compareItem.Path);
        }
        //public override bool Equals(Item other)
        //{
        //    if (other == null) return false;
        //    return (this.Name.Equals(other.Name) && this.Path.Equals(other.Path) && this.IsFolder.Equals(other.IsFolder));
        //}

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