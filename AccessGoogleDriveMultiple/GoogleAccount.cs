using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using SyncMultipleGoogleDrives.Model;

namespace SyncMultipleGoogleDrives
{
    class GoogleAccount
    {

        public string Name;

        public string Email;

        public string ClientId;

        public string ClientSecret;

        public string FileDataStore;

        public string FileFilter;

        private List<Item> _itemList = new List<Item>();

        public void AddToList(Item item)
        {
            _itemList.Add(item);
        }

        public List<Item> ItemList
        {
            get
            {
                return _itemList;
            }
        }
        public void ClearList()
        {
            _itemList.Clear();
        }

    }
}
