using System;
using System.Collections.Generic;
using System.IO;
using System.Linq;
using System.Runtime.InteropServices.WindowsRuntime;
using Windows.Foundation;
using Windows.Foundation.Collections;
using Windows.UI.Xaml;
using Windows.UI.Xaml.Controls;
using Windows.UI.Xaml.Controls.Primitives;
using Windows.UI.Xaml.Data;
using Windows.UI.Xaml.Input;
using Windows.UI.Xaml.Media;
using Windows.UI.Xaml.Navigation;

// The Blank Page item template is documented at https://go.microsoft.com/fwlink/?LinkId=234238

namespace UWtest
{

    public class KeyCode
    { 
            public int ID
        {
            get;
            set;
        }
        public string DisplayName
        {
            get;
            set;
        }
        public override string ToString()
        {
            return this.DisplayName;
        }
        public KeyCode(int x, string a)
        {
            ID = x;
            DisplayName = a;
        }
    }
    public sealed partial class KeyboardPage : Page
    {
        List<KeyCode> KeyList = new List<KeyCode>();
        public KeyboardPage()
        {
            this.InitializeComponent();

            int[] keyIDs = { 0x01, 0x02, 0x03 };
            string[] keyNames = { "A", "B", "C" };
            //populate keycode list
            for (int x = 0; x < keyIDs.Length; x++)
            {
                KeyList.Add(new KeyCode(keyIDs[x], keyNames[x]));
            }
        }
    }
}
