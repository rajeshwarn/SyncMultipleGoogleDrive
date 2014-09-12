using System;
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
using System.Windows.Shapes;
using SyncMultipleGoogleDrives.Model;
using System.Collections.ObjectModel;
using System.Threading;

namespace SyncMultipleGoogleDrives
{
    /// <summary>
    /// Interaction logic for UploadWindow.xaml
    /// </summary>
    public partial class UploadWindow : Window
    {

        public RangeObservableCollection<Item> items;

        private List<GoogleAccount> _gas;
        private string _rootFolder;

        private Item _CurrentItem;
        private GoogleAccount _CurrentGA;

        public UploadWindow()
        {
            InitializeComponent();

        }

        public void SetItems(List<GoogleAccount> gas, string rootFolder)
        {
            if (items == null)
            {
                items = new RangeObservableCollection<Item>();

            }
            _gas = gas;
            _rootFolder = rootFolder;
            if (_gas != null)
            {
                foreach (GoogleAccount ga in _gas)
                {
                    if (ga.UploadItems != null && ga.UploadItems.Count > 0)
                    {
                        items.AddRange(ga.UploadItems);
                    }
                }
            }


            lstFiles.ItemsSource = items;
        }

        private void Button_Click(object sender, RoutedEventArgs e)
        {

            Thread newThread = new Thread(Synch);

            // Use the overload of the Start method that has a 
            // parameter of type Object. You can create an object that 
            // contains several pieces of data, or you can pass any  
            // reference type or value type. The following code passes 
            // the integer value 42. 
            //
            newThread.IsBackground = true;
            newThread.Start();

        }

        private void Synch()
        {
            if (_gas != null && !string.IsNullOrWhiteSpace(_rootFolder) && items != null)
            {

                foreach (GoogleAccount gas in _gas)
                {
                    _CurrentGA = gas;
                    gas.UploadBusy -= ga_UploadBusy;
                    gas.UploadEnd -= ga_UploadEnd;
                    gas.UploadStart -= ga_UploadStart;

                    gas.UploadBusy += ga_UploadBusy;
                    gas.UploadEnd += ga_UploadEnd;
                    gas.UploadStart += ga_UploadStart;

                    foreach (Item i in gas.UploadItems)
                    {
                        _CurrentItem = i;
                        string abspath = i.Path.Substring(_rootFolder.Length);
                        abspath = abspath.Substring(0, abspath.Length - i.Name.Length);
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
                        if (!i.IsFolder)
                        {

                            gas.UploadFile(i.Path, i.Name, abspath);
                            //Thread.Sleep(50);
                        }
                        else
                        {
                            gas.CreateFolder(i.Path, i.Name, abspath);
                            Thread.Sleep(50);
                        }
                        i.GoogleID = gas.CurrentGoogleID;
                        i.UploadProgress = 100;

                        //Thread.Sleep(200);
                    }

                }





            }

        }

        void ga_UploadStart(object sender, EventArgs e)
        {
            if (_CurrentItem != null)
            {
                _CurrentItem.UploadProgress = 0;
            }
        }

        void ga_UploadEnd(object sender, EventArgs e)
        {
            if (_CurrentItem != null)
            {
                _CurrentItem.UploadProgress = 0;
            }
        }

        void ga_UploadBusy(object sender, EventArgs e)
        {
            if (_CurrentItem != null)
            {
                if (_CurrentGA != null)
                {
                    _CurrentItem.UploadProgress = _CurrentGA.ProgressValue;
                }
            }

        }

    }
}
