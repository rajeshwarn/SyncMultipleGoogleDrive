using System;
using System.Collections.Generic;
using System.ComponentModel;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace SyncMultipleGoogleDrives
{
    class CurrentSynchro : INotifyPropertyChanged
    {

        private string _CurrentAccount;
        public string CurrentAccount
        {
            get
            {
                return _CurrentAccount;
            }
            set
            {
                _CurrentAccount = value;
                NotifyPropertyChanged();
            }
        }

        private string _CurrentFolder;
        public string CurrentFolder
        {
            get
            {
                return _CurrentFolder;
            }
            set
            {
                _CurrentFolder = value;
                NotifyPropertyChanged();
            }
        }

        private string _CurrentFile;
        public string CurrentFile
        {
            get
            {
                return _CurrentFile;
            }
            set
            {
                _CurrentFile = value;
                NotifyPropertyChanged();
            }
        }

        private int _CurrentFileUploadValue;
        public int CurrentFileUploadValue
        {
            get
            {
                return _CurrentFileUploadValue;
            }
            set
            {
                _CurrentFileUploadValue = value;
                NotifyPropertyChanged();
            }
        }

        private int _TotalFileUploadValue;
        public int TotalFileUploadValue
        {
            get
            {
                return _TotalFileUploadValue;
            }
            set
            {
                _TotalFileUploadValue = value;
                NotifyPropertyChanged();
            }
        }


        public event PropertyChangedEventHandler PropertyChanged;

        protected virtual void NotifyPropertyChanged(String propertyName = "")
        {
            var handler = PropertyChanged;
            if (handler != null) handler(this, new PropertyChangedEventArgs(propertyName));
        }
    }
}
