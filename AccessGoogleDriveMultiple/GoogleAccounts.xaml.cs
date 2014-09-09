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

using Google;
using Google.Apis.Auth.OAuth2;
using Google.Apis.Drive.v2;
using Google.Apis.Drive.v2.Data;
using Google.Apis.Services;
using Google.Apis.Util.Store;
using System.Threading;

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
            if (!string.IsNullOrEmpty(this.txtName.Text))
            {

                SaveAccount(txtName.Text, txtEmail.Text, txtClientID.Text, txtClientSecret.Text, txtFileDataStore.Text, txtFileFilter.Text, txtRootFolder.Text);

            }
            this.Close();
        }

        private void SaveAccount(string Name, string Email, string ClientId, string ClientSecret, string FileDataStore, string FileFilter, string RootFolder)
        {
            // checks
            if (!string.IsNullOrEmpty(Name) &&
                !string.IsNullOrEmpty(Email) &&
                !string.IsNullOrEmpty(ClientId) &&
                !string.IsNullOrEmpty(ClientSecret) &&
                !string.IsNullOrEmpty(FileDataStore) &&
                !string.IsNullOrEmpty(FileFilter) &&
                !string.IsNullOrEmpty(RootFolder))
            {

                if (gaXML != null)
                {
                    if (gaXML.HasChildNodes)
                    {
                        XmlNodeList lstNodes = gaXML.GetElementsByTagName("GoogleAccount");
                        if (lstNodes != null && lstNodes.Count > 0)
                        {
                            XmlNode nodeToUpdate = null;
                            foreach (XmlNode oNode in lstNodes)
                            {
                                XmlNodeList lstNodesPerAccount = oNode.ChildNodes;
                                if (lstNodesPerAccount != null && lstNodesPerAccount.Count > 0)
                                {
                                    foreach (XmlNode oNodeChild in lstNodesPerAccount)
                                    {
                                        if (oNodeChild.Name == "Name")
                                        {
                                            if (oNodeChild.InnerText == Name)
                                            {
                                                nodeToUpdate = oNode;
                                                break;
                                            }
                                        }
                                    }
                                }
                                if (nodeToUpdate != null)
                                {
                                    break;
                                }
                            }
                            if (nodeToUpdate != null)
                            {
                                UpdateNode(ref nodeToUpdate, Name, Email, ClientId, ClientSecret, FileDataStore, FileFilter);
                            }
                            else
                            {
                                AddNode(Name, Email, ClientId, ClientSecret, FileDataStore, FileFilter, RootFolder);
                            }
                        }
                    }
                }

            }


        }

        private void UpdateNode(ref XmlNode nodeToUpdate, string Name, string Email, string ClientId, string ClientSecret, string FileDataStore, string FileFilter)
        {
            XmlNodeList lstNodesPerAccount = nodeToUpdate.ChildNodes;
            if (lstNodesPerAccount != null && lstNodesPerAccount.Count > 0)
            {
                foreach (XmlNode oNodeChild in lstNodesPerAccount)
                {
                    switch (oNodeChild.Name)
                    {
                        case "Name":
                            oNodeChild.InnerText = Name;
                            break;
                        case "Email":
                            oNodeChild.InnerText = Email;
                            break;
                        case "ClientId":
                            oNodeChild.InnerText = ClientId;
                            break;
                        case "ClientSecret":
                            oNodeChild.InnerText = ClientSecret;
                            break;
                        case "FileDataStore":
                            oNodeChild.InnerText = FileDataStore ;
                            break;
                        case "FileFilter":
                            oNodeChild.InnerText = FileFilter;
                            break;
                    }
                }
            }

        }
        private void AddNode(string Name, string Email, string ClientId, string ClientSecret, string FileDataStore, string FileFilter, string RootFolder)
        {

            XmlNode rootNode;
            
            XmlNodeList lstNodes = gaXML.GetElementsByTagName("GoogleAccounts");
            if (lstNodes != null && lstNodes.Count == 1)
            {
                rootNode = lstNodes[0];

                XmlNode accountNode = gaXML.CreateElement("GoogleAccount");

                XmlNode nameNode = gaXML.CreateElement("Name");
                nameNode.InnerText = Name;
                XmlNode emailNode = gaXML.CreateElement("Email");
                emailNode.InnerText = Email;
                XmlNode clientidNode = gaXML.CreateElement("ClientId");
                clientidNode.InnerText = ClientId;
                XmlNode clientsecretNode = gaXML.CreateElement("ClientSecret");
                clientsecretNode.InnerText = ClientSecret;
                XmlNode filedatastoreNode = gaXML.CreateElement("FileDataStore");
                filedatastoreNode.InnerText = FileDataStore;
                XmlNode filefilterNode = gaXML.CreateElement("FileFilter");
                filefilterNode.InnerText = FileFilter;
                XmlNode rootfolderNode = gaXML.CreateElement("RootFolder");
                rootfolderNode.InnerText = RootFolder;

                accountNode.AppendChild(nameNode);
                accountNode.AppendChild(emailNode);
                accountNode.AppendChild(clientidNode);
                accountNode.AppendChild(clientsecretNode);
                accountNode.AppendChild(filedatastoreNode);
                accountNode.AppendChild(filefilterNode);

                rootNode.AppendChild(accountNode);


            }


        }

        private void comboGoogleAccounts_SelectionChanged(object sender, SelectionChangedEventArgs e)
        {
            if (SelectedcbItem != null)
            {
                if (SelectedcbItem.Content.ToString() != "<-- Select Account -->")
                {

                    if (SelectedcbItem.Content.ToString() == "<-- Add new account -->")
                    {
                        txtName.Text = "";
                        txtEmail.Text = "";
                        txtClientID.Text = "";
                        txtClientSecret.Text = "";
                        txtFileDataStore.Text = "";
                        txtFileFilter.Text = "";
                        txtRootFolder.Text = "";


                        return;
                    }

                    if (gaXML != null)
                    {
                        XmlNodeList nodes = gaXML.GetElementsByTagName("GoogleAccount");
                        if (nodes != null)
                        {
                            foreach (XmlNode oNode in nodes)
                            {
                                XmlNodeList lstNodesPerAccount = oNode.ChildNodes;
                                if (lstNodesPerAccount != null && lstNodesPerAccount.Count > 0)
                                {
                                    bool _nodefound = false;
                                    foreach (XmlNode oNodeChild in lstNodesPerAccount)
                                    {
                                        switch (oNodeChild.Name)
                                        {
                                            case "Name":
                                                if (oNodeChild.InnerText == SelectedcbItem.Content.ToString())
                                                {
                                                    txtName.Text = oNodeChild.InnerText;
                                                    _nodefound = true;
                                                }
                                                break;
                                            case "Email":
                                                if (_nodefound)
                                                {
                                                    txtEmail.Text = oNodeChild.InnerText;
                                                }
                                                break;
                                            case "ClientId":
                                                if (_nodefound)
                                                {
                                                    txtClientID.Text = oNodeChild.InnerText;
                                                }
                                                break;
                                            case "ClientSecret":
                                                if (_nodefound)
                                                {
                                                    txtClientSecret.Text = oNodeChild.InnerText;
                                                }
                                                break;
                                            case "FileDataStore":
                                                if (_nodefound)
                                                {
                                                    txtFileDataStore.Text = oNodeChild.InnerText;
                                                }
                                                break;
                                            case "FileFilter":
                                                if (_nodefound)
                                                {
                                                    txtFileFilter.Text = oNodeChild.InnerText;
                                                }
                                                break;
                                            case "RootFolder":
                                                if (_nodefound)
                                                {
                                                    txtRootFolder.Text = oNodeChild.InnerText;
                                                }
                                                break;
                                            default:
                                                break;
                                        }
                                    }
                                    if (_nodefound)
                                    {
                                        break;
                                    }

                                }

                            }
                        }

                    }
                }
            }
        }

        private void btnAccountTest_Click(object sender, RoutedEventArgs e)
        {
            if (!string.IsNullOrEmpty(txtName.Text) &&
                !string.IsNullOrEmpty(txtEmail.Text) &&
                !string.IsNullOrEmpty(txtClientID.Text ) &&
                !string.IsNullOrEmpty(txtClientSecret.Text ) &&
                !string.IsNullOrEmpty(txtFileDataStore.Text))
            {
                UserCredential credential = GoogleWebAuthorizationBroker.AuthorizeAsync(
                    new ClientSecrets
                    {
                        ClientId = txtClientID.Text,
                        ClientSecret = txtClientSecret.Text,
                    },
                    new[] { DriveService.Scope.Drive },
                    "user",
                    CancellationToken.None ,
                    new FileDataStore(txtFileDataStore.Text)).Result;
            }
        }




    }
}
