/*********************************************
 *  File:       MainPage.xaml.cs
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
using System.Runtime.InteropServices;
using Windows.System.Threading;
using System.Runtime.InteropServices.WindowsRuntime;
using Windows.Devices.Bluetooth;
using Windows.Devices.Bluetooth.GenericAttributeProfile;
using Windows.Devices.Enumeration;
using Windows.Foundation;
using Windows.Foundation.Collections;
using Windows.Storage.Search;
using Windows.UI.Xaml;
using Windows.UI.Xaml.Controls;
using Windows.UI.Xaml.Controls.Primitives;
using Windows.Security.Cryptography;
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

            //StartBleDeviceWatcher();


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
                if (L[i].SelectedValue == null) 
                {
                    storageBuffer[i] = 0;
                } else {
                    storageBuffer[i] = (byte)L[i].SelectedValue;
                }
            }
            for (int j = 0; j < R.Count; j++)
            {
                if (R[j].SelectedValue == null)
                {
                    storageBuffer[i + j] = 0;
                }
                else
                {
                    storageBuffer[i + j] = (byte)R[j].SelectedValue;
                }
            }
        }

        internal void ParseKeys2(byte[] storageBuffer, List<ComboBox> L, int offset)
        {
            int i = 0;
            for (; i < storageBuffer.Length; i++)
            {
                if (L[i + offset].SelectedValue == null)
                {
                    storageBuffer[i] = 0;
                }
                else
                {
                    storageBuffer[i] = (byte)L[i + offset].SelectedValue;
                }
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

        private void UpdateKeymapControls(byte[] keyBuffer)
        {
            int i = 0;
            for (; i < LeftHKeys.Count; i++)
            {
                // attempts to select the item in the combobox itemssource that matches the int
                LeftHKeys[i].SelectedValue = keyBuffer[i];
            }
            for (int j = 0; j < RightHKeys.Count; j++)
            {
                RightHKeys[j].SelectedValue = keyBuffer[i + j];
            }
        }

        private async void LoadButton_Click(object sender, RoutedEventArgs e)
        {
            // try to open file
            try
            {
                if (CBProfiles.SelectedValue == null) { return; }
                var fstream = await Windows.Storage.ApplicationData.Current.LocalFolder.OpenStreamForReadAsync( 
                    CBProfiles.SelectedValue.ToString() + ".keyprofile");
                byte[] keyBuf = new byte[C.nKeys];

                // read key array
                fstream.Read(keyBuf, 0, C.nKeys);
                await fstream.FlushAsync();
                fstream.Dispose();

                // (TODO: if valid) update ui controls
                UpdateKeymapControls(keyBuf);


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

                // get selected keys
                byte[] keyBuffer = new byte[C.nKeys];
                ParseKeys(keyBuffer, LeftHKeys, RightHKeys);

                // write keys to file
                var fstream = await Windows.Storage.ApplicationData.Current.LocalFolder.OpenStreamForWriteAsync( 
                    CBProfiles.SelectedValue.ToString() + ".keyprofile", Windows.Storage.CreationCollisionOption.ReplaceExisting);

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
        
        private async void UploadButton_Click(object sender, RoutedEventArgs e)
        {
            string errormsg;
            /// device BLEdevice;
            /// 
            // get  keys
            byte[] keyBuffer1 = new byte[13];
            byte[] keyBuffer2 = new byte[13];
            Windows.Storage.Streams.IBuffer buffer;


            // check button
            BluetoothLEDevice BLEDevice = null;
            switch ((sender as FrameworkElement).Tag)
            {
                case "LeftHand":
                    /// device = left
                    BLEDevice = await BluetoothLEDevice.FromIdAsync(C.DeviceAddressLeft);
                    ParseKeys2(keyBuffer1, LeftHKeys, 0);
                    ParseKeys2(keyBuffer2, LeftHKeys, 12);

                    break;
                case "RightHand":
                    /// device = right
                    BLEDevice = await BluetoothLEDevice.FromIdAsync(C.DeviceAddressRight);
                    ParseKeys2(keyBuffer1, RightHKeys, 0);
                    ParseKeys2(keyBuffer2, RightHKeys, 12);
                    break;
                default:
                    break;
            }

            if (BLEDevice != null)
            {
                var Sresult = await BLEDevice.GetGattServicesForUuidAsync(new Guid(C.ServiceUUID));

                if (Sresult.Status == GattCommunicationStatus.Success)
                {
                    GattDeviceService BLEserv = Sresult.Services[0];

                    // get frist half of keyboard data
                    var Cresult = await BLEserv.GetCharacteristicsForUuidAsync(new Guid(C.Characteristic1UUID));
                    if (Cresult.Status == GattCommunicationStatus.Success)
                    {
                        GattCharacteristic BLEchar = Cresult.Characteristics[0];

                        buffer = keyBuffer1.AsBuffer();
                        GattWriteResult result = await BLEchar.WriteValueWithResultAsync(buffer);
                        if (result.Status == GattCommunicationStatus.Success)
                        {
                            Cresult = await BLEserv.GetCharacteristicsForUuidAsync(new Guid(C.Characteristic2UUID));
                            if (Cresult.Status == GattCommunicationStatus.Success)
                            {
                                BLEchar = Cresult.Characteristics[0];

                                buffer = keyBuffer2.AsBuffer();
                                result = await BLEchar.WriteValueWithResultAsync(buffer);
                                if (result.Status == GattCommunicationStatus.Success)
                                {
                                    Debug.WriteLine("write good");
                                }
                                else
                                {
                                    Debug.WriteLine("write failed");
                                }
                            }
                        }
                    }
                }
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

        private async void DownloadButton_Click(object sender, RoutedEventArgs e)
        {
            string errormsg;
            /// device BLEdevice;
            // check button
            BluetoothLEDevice BLEDevice = null;
            switch ((sender as FrameworkElement).Tag)
            {
                case "LeftHand":
                    /// device = left
                    BLEDevice = await BluetoothLEDevice.FromIdAsync(C.DeviceAddressLeft);                  
                    break;
                case "RightHand":
                    /// device = right
                    BLEDevice = await BluetoothLEDevice.FromIdAsync(C.DeviceAddressRight);    
                    break;
                default:
                    break;
            }

            if (BLEDevice != null)
            {
                var Sresult = await BLEDevice.GetGattServicesForUuidAsync(new Guid(C.ServiceUUID));

                if (Sresult.Status == GattCommunicationStatus.Success)
                {
                    GattDeviceService BLEserv = Sresult.Services[0];

                    // get frist half of keyboard data
                    var Cresult = await BLEserv.GetCharacteristicsForUuidAsync(new Guid(C.Characteristic1UUID));
                    if (Cresult.Status == GattCommunicationStatus.Success)
                    {
                        GattCharacteristic BLEchar = Cresult.Characteristics[0];
                        GattReadResult result = await BLEchar.ReadValueAsync(BluetoothCacheMode.Uncached);
                        if (result.Status == GattCommunicationStatus.Success)
                        {
                            byte[] KeyBuffer1 = result.Value.ToArray();
                            //CryptographicBuffer.CopyToByteArray(, out KeyBuffer1);
                            Debug.WriteLine("Read result: {0}", KeyBuffer1.Length);

                            // get second half of keyboard data
                            Cresult = await BLEserv.GetCharacteristicsForUuidAsync(new Guid(C.Characteristic2UUID));
                            if (Cresult.Status == GattCommunicationStatus.Success)
                            {
                                BLEchar = Cresult.Characteristics[0];
                                result = await BLEchar.ReadValueAsync(BluetoothCacheMode.Uncached);
                                if (result.Status == GattCommunicationStatus.Success)
                                {
                                    byte[] KeyBuffer2;
                                    CryptographicBuffer.CopyToByteArray(result.Value, out KeyBuffer2);
                                    Debug.WriteLine("Read result 2: {0}", KeyBuffer2.Length);

                                    byte[] KeyBuffer = KeyBuffer1.Concat(KeyBuffer2).ToArray();

                                    for (int i = 0; i < LeftHKeys.Count; i++)
                                    {
                                        // attempts to select the item in the combobox itemssource that matches the int
                                        LeftHKeys[i].SelectedValue = KeyBuffer[i];
                                    }
                                }
                            }
                        }
                        else
                        {
                            Debug.WriteLine("Read failed");
                        }
                    }
                }

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
