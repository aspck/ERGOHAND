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

namespace UWtest
{    
    public sealed partial class MainPage : Page
    {
        // USB HID spec https://usb.org/document-library/hid-usage-tables-121
        private Dictionary<byte, string> KeyList { get; } = new Dictionary<byte, string>() {
            { 0x4, "A" },            { 0x5, "B" },            { 0x6, "C" },            { 0x7, "D" },            { 0x8, "E" },
            { 0x9, "F" },            { 0xA, "G" },            { 0xB, "H" },            { 0xC, "I" },            { 0xD, "J" },
            { 0xE, "K" },            { 0xF, "L" },            { 0x10, "M" },            { 0x11, "N" },            { 0x12, "O" },
            { 0x13, "P" },            { 0x14, "Q" },            { 0x15, "R" },            { 0x16, "S" },            { 0x17, "T" },
            { 0x18, "U" },            { 0x19, "V" },            { 0x1A, "W" },            { 0x1B, "X" },            { 0x1C, "Y" },
            { 0x1D, "Z" },            { 0x1E, "1" },            { 0x1F, "2" },            { 0x20, "3" },            { 0x21, "4" },
            { 0x22, "5" },            { 0x23, "6" },            { 0x24, "7" },            { 0x25, "8" },            { 0x26, "9" },
            { 0x27, "0" },            { 0x28, "ENTER" },            { 0x29, "ESC" },            { 0x2A, "BACK\nSPACE" },            { 0x2B, "TAB" },
            { 0x2C, "SPACE" },            { 0xE0, "LEFT\nCTRL" },            { 0xE1, "LEFT\nSHIFT" },            { 0xE2, "LEFT\nALT" },            { 0xE3, "LEFT\nMETA" },
            { 0xE4, "RIGHT\nCTRL" },            { 0xE5, "RIGHT\nSHIFT" },            { 0xE6, "RIGHT\nALT" },            { 0xE7, "RIGHT\nMETA" },            { 0x2D, "-" },
            { 0x2E, "=" },            { 0x2F, "[" },            { 0x30, "]" },            { 0x31, "\\" },            { 0x32, "`" },
            { 0x33, ";" },            { 0x34, "'" },            { 0x35, "GRAVE" },            { 0x36, "," },            { 0x37, "." },
            { 0x38, "/" },            { 0x3A, "F1" },            { 0x3B, "F2" },            { 0x3C, "F3" },            { 0x3D, "F4" },
            { 0x3E, "F5" },            { 0x3F, "F6" },            { 0x40, "F7" },            { 0x41, "F8" },            { 0x42, "F9" },
            { 0x43, "F10" },            { 0x44, "F11" },            { 0x45, "F12" },            { 0x46, "SYSRQ" },            { 0x47, "SCROLL\nLOCK" },
            { 0x53, "NUM\nLOCK" },            { 0x39, "CAPS\nLOCK" },            { 0x48, "PAUSE" },            { 0x49, "INSERT" },            { 0x4A, "HOME" },
            { 0x4B, "PAGEUP" },            { 0x4C, "DELETE" },            { 0x4D, "END" },            { 0x4E, "PAGE\nDOWN" },            { 0x4F, "RIGHT" },
            { 0x50, "LEFT" },            { 0x51, "DOWN" },            { 0x52, "UP" },            { 0x7F, "MUTE" },            { 0x80, "VOL UP" },
            { 0x81, "VOL\nDOWN" },            { 0x54, "KP \\" },            { 0x55, "KP *" },            { 0x56, "KP -" },            { 0x57, "KP +" },
            { 0x58, "KP\nENTER" },            { 0x59, "KP1" },            { 0x5A, "KP2" },            { 0x5B, "KP3" },            { 0x5C, "KP4" },
            { 0x5D, "KP5" },            { 0x5E, "KP6" },            { 0x5F, "KP7" },            { 0x60, "KP8" },            { 0x61, "KP9" },
            { 0x62, "KP0" },            { 0x63, "KP ." } };

        private int nKeys = 50;  // 25 per device

        // bluetooth constants, these should be read from an app config file in the future
        private int KeyReportSize = 13;
        private string Characteristic1UUID = "6AF7ACFD-F66B-4932-8975-41F512990077";
        private string Characteristic2UUID = "4F229A78-0344-430A-B203-CF8685E77E2C";
        private long DeviceAddressLeft = 0x01fffa50a010; // 10A050-FAFF01
        private long DeviceAddressRightt = 0x02fffa50a010; // 10A050-FAFF02

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
                byte[] keyBuffer = new byte[nKeys];

                // read key array
                fstream.Read(keyBuffer, 0, nKeys);
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
               
                byte[] keyBuffer = new byte[nKeys];

                // get selected keys
                ParseKeys(keyBuffer, LeftHKeys, RightHKeys);

                // write keys to file
                fstream.Write(keyBuffer, 0, nKeys);
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
