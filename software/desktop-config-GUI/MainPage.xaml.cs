/*********************************************
 *  File:       MainPage.xaml.cs
 *  Author:     ***REMOVED***
 *  Created:    22-Jan-2021 
 *
 *  Bluetooth Low Energy keyboard project. 
 *  Full documentation at:
 *  https://github.com/aspck/ErgoHand 
 *
 ********************************************/

using System;
using System.Collections.Generic;
using System.Collections.ObjectModel;
using System.IO;
using System.Linq;
using System.Runtime.InteropServices.WindowsRuntime;
using Windows.Foundation;
using Windows.Foundation.Collections;
using Windows.Storage.Search;
using Windows.UI.Xaml;
using Windows.UI.Xaml.Controls;
using Windows.UI.Xaml.Controls.Primitives;
using Windows.UI.Xaml.Data;
using Windows.UI.Xaml.Input;
using Windows.UI.Xaml.Media;
using Windows.UI.Xaml.Navigation;
using System.Diagnostics;

namespace desktop_config_GUI
{    
    public sealed partial class MainPage : Page
    {
        private Constants C = new Constants();

        private List<ComboBox> LeftHKeys = new List<ComboBox>();
        private List<ComboBox> RightHKeys = new List<ComboBox>();

        public MainPage()
        {
            // TODO: load app config file          

            this.InitializeComponent();

            CBProfiles_RefreshFiles(CBProfiles);

            // get all key selection controls
            FindChildren<ComboBox>(LeftHKeys, leftkeymapcontainer, "CBKey");            
            FindChildren<ComboBox>(RightHKeys, rightkeymapcontainer, "CBKey");

            // TODO: set default profile
        }

        internal static void FindChildren<T>(List<T> results, DependencyObject startNode, string Tag)
where T : DependencyObject
        {
            int count = VisualTreeHelper.GetChildrenCount(startNode);
            for (int i = 0; i < count; i++)
            {
                DependencyObject current = VisualTreeHelper.GetChild(startNode, i);
                if (current.GetType().Equals(typeof(T)))
                {
                    if ((string)(current as FrameworkElement).Tag == Tag)
                    {
                        T asType = (T)current;

                        results.Add(asType);
                    } 
                }
                FindChildren<T>(results, current, Tag);
            }
        }
        
        internal void ParseKeys(byte[] storageBuffer, List<ComboBox> L, List<ComboBox> R)
        {
            int i = 0;
            for (; i < L.Count; i++)
            {
                storageBuffer[i] = (byte)L[i].SelectedValue;
            }
            for (int j = 0; j <R.Count; j++)
            {
                storageBuffer[i+j] = (byte)R[j].SelectedValue;
            }
        }

        internal async void CBProfiles_RefreshFiles(object _combobox)
        {
            // get config files in directory
            IReadOnlyList<Windows.Storage.StorageFile> xmlFiles = await Windows.Storage.ApplicationData.Current.LocalFolder.CreateFileQueryWithOptions(
                new QueryOptions(CommonFileQuery.OrderByName, new List<string>() { ".keyprofile" })).GetFilesAsync();
            
            if (xmlFiles != null)
            {
                // list of filenames for combobox
                ObservableCollection<string> xmlList = new ObservableCollection<string>(xmlFiles.Select(p => p.DisplayName).ToList());
                // add new profile action to end of list
                xmlList.Add("< Create new profile >");
                // update item source with file list
                (_combobox as ComboBox).ItemsSource = xmlList;            
            }      
            
        }

        private void CBProfiles_DropDownClosed(object sender, object e)
        {
            var s = (sender as ComboBox).SelectedItem;
            // just handles the "create new profile" action
            if (s != null && s.ToString() == "< Create new profile >"){
                // show popup with text dialog
                if (!NewProfilePopup.IsOpen) { NewProfilePopup.IsOpen = true; }
            }

        }
       
        private async void CreateProfilePopupClicked(object sender, RoutedEventArgs e)
        {
            string fname = CreateProfileText.Text;

            // try to create file
            Windows.Storage.StorageFolder storageFolder =
                Windows.Storage.ApplicationData.Current.LocalFolder;

            Windows.Storage.StorageFile newFile =
                await storageFolder.CreateFileAsync(fname + ".keyprofile", Windows.Storage.CreationCollisionOption.ReplaceExisting);
 
            // update combobox selection
            CBProfiles_RefreshFiles(CBProfiles);
            CBProfiles.SelectedItem = fname;

            // if the Popup is open, then close it 
            if (NewProfilePopup.IsOpen) { NewProfilePopup.IsOpen = false; }
        }
        
        private void CancelProfilePopupClicked(object sender, RoutedEventArgs e)
        {
            CBProfiles.SelectedItem = null;
            // if the Popup is open, then close it 
            if (NewProfilePopup.IsOpen) { NewProfilePopup.IsOpen = false; }
        }
        
        private async void LoadButton_Click(object sender, RoutedEventArgs e)
        {
            // try to open file
            try
            {
                if (CBProfiles.SelectedValue == null) { return; }
                var fstream = await Windows.Storage.ApplicationData.Current.LocalFolder.OpenStreamForReadAsync( 
                    CBProfiles.SelectedValue.ToString() + ".keyprofile");
                byte[] keyBuffer = new byte[C.nKeys];

                // read key array
                fstream.Read(keyBuffer, 0, C.nKeys);
                await fstream.FlushAsync();
                fstream.Dispose();


                // (TODO: if valid) update ui controls
                int i = 0;
                for (; i < LeftHKeys.Count; i++)
                {
                    // attempts to select the item in the combobox itemssource that matches the int
                    LeftHKeys[i].SelectedValue = keyBuffer[i];
                }
                for (int j = 0; j < RightHKeys.Count; j++)
                {
                    RightHKeys[j].SelectedValue = keyBuffer[i+j];
                }

            } // not sure what to do with these
            catch (NullReferenceException)
            {

            }
            catch (ArgumentNullException)
            {

            }
            catch (ArgumentException)
            {

            }
            catch (IOException)
            {

            }


        }
        
        private async void SaveButton_Click(object sender, RoutedEventArgs e)
        {
            //TODO: try to create backup of current profile

            try
            {
                if (CBProfiles.SelectedValue == null) { return; }
                var fstream = await Windows.Storage.ApplicationData.Current.LocalFolder.OpenStreamForWriteAsync( 
                    CBProfiles.SelectedValue.ToString() + ".keyprofile", Windows.Storage.CreationCollisionOption.ReplaceExisting);
               
                byte[] keyBuffer = new byte[C.nKeys];

                // get selected keys
                ParseKeys(keyBuffer, LeftHKeys, RightHKeys);

                // write keys to file
                fstream.Write(keyBuffer, 0, C.nKeys);
                await fstream.FlushAsync();
                fstream.Dispose();
            }
            catch (ArgumentNullException)
            {

            }
            catch (IOException)
            {

            }

        }
        
        private void UploadButton_Click(object sender, RoutedEventArgs e)
        {
            string errormsg;
            /// device BLEdevice;
            // check button
            switch ((sender as FrameworkElement).Tag)
            {
                case "LeftHand":
                    /// device = left
                    break;
                case "RightHand":
                    /// device = right
                    break;
                default:
                    break;
            }
            // check device
            /// if (device is paired)
            // write config
            /// errormsg = "config uploaded to device"
            /// else
            /// errormsg = "upload failed: device not found"

            // display result of operation
            Popup popup = new Popup();

        }
        
        private void DownloadButton_Click(object sender, RoutedEventArgs e)
        {
            string errormsg;
            /// device BLEdevice;
            // check button
            switch ((sender as FrameworkElement).Tag)
            {
                case "LeftHand":
                    /// device = left
                    break;
                case "RightHand":
                    /// device = right
                    break;
                default:
                    break;
            }
            // check device
            /// if (device is paired)
            /// read config and udpate keys
            /// else
            /// display error
            Popup popup = new Popup();



        }
    
    }

}
