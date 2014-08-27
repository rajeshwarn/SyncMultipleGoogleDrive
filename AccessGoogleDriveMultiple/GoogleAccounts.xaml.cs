using System;
using System.Collections.Generic;
using System.Collections.ObjectModel;
using System.Collections.Specialized;
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
using System.Xml;

namespace SyncMultipleGoogleDrives
{
    /// <summary>
    /// Interaction logic for GoogleAccounts.xaml
    /// </summary>
    public partial class GoogleAccounts : Window
    {

        public ObservableCollection<ComboBoxItem> cbItems { get; set; }
        public ComboBoxItem SelectedcbItem { get; set; }

        private XmlDocument gaXML;

        public GoogleAccounts(XmlDocument doc)
        {
            InitializeComponent();
            gaXML = doc;

            DataContext = this;

            cbItems = new ObservableCollection<ComboBoxItem>();
            
            var cbItem = new ComboBoxItem { Content = "<-- Select Account -->" };
            cbItems.Add(cbItem);
            SelectedcbItem = cbItem;

            LoadItemList();
        }

        private void LoadItemList()
        {
            if (gaXML != null)
            {
                if (gaXML.HasChildNodes)
                {
                    XmlNodeList lstNodes = gaXML.GetElementsByTagName("GoogleAccount");
                    if (lstNodes != null && lstNodes.Count > 0)
                    {
                        foreach (XmlNode oNode in lstNodes)
                        {
                            XmlNodeList lstNodesPerAccount = oNode.ChildNodes;
                            if (lstNodesPerAccount != null && lstNodesPerAccount.Count > 0)
                            {
                                foreach (XmlNode oNodeChild in lstNodesPerAccount)
                                {
                                    if (oNodeChild.Name == "Name")
                                    {
                                        var cbItem = new ComboBoxItem { Content = oNodeChild.InnerText };
                                        cbItems.Add(cbItem);
                                    }
                                }
                            }
                        }
                    }
                }
            }
            var cbItemNew = new ComboBoxItem { Content = "<-- Add new account -->" };
            cbItems.Add(cbItemNew);
        }


        private void btnSave_Click(object sender, RoutedEventArgs e)
        {
            if (!string.IsNullOrEmpty(this.txtName.Text ))
            {
            }
        }

        private void SaveAccount(string Name, string ClientId, string ClientSecret, string clientStore)
        {


        }


    }
}
