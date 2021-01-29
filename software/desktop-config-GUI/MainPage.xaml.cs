using System;
using System.Collections.Generic;
using System.Collections.ObjectModel;
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
using System.Diagnostics;

// The Blank Page item template is documented at https://go.microsoft.com/fwlink/?LinkId=402352&clcid=0x409

namespace UWtest
{    
    /// <summary>
    /// An empty page that can be used on its own or navigated to within a Frame.
    /// </summary>
    public sealed partial class MainPage : Page
    {
        public Dictionary<string, int> KeyList { get; } = new Dictionary<string, int>() {
                {"A", 0x04 },
                {"B", 0x05 },
                {"C", 0x06 },
                {"D", 0x07 },
                {"E", 0x08 },
                {"F", 0x09 },
                {"G", 0x0a },
                {"H", 0x0b },
                {"I", 0x0c },
                {"J", 0x0d },
                {"K", 0x0e },
                {"L", 0x0f },
                {"M", 0x10 },
                {"N", 0x11 },
                {"O", 0x12 },
                {"P", 0x13 },
                {"Q", 0x14 },
                {"R", 0x15 },
                {"S", 0x16 },
                {"T", 0x17 },
                {"U", 0x18 },
                {"V", 0x19 },
                {"W", 0x1a },
                {"X", 0x1b },
                {"Y", 0x1c },
                {"Z", 0x1d },
                {"1", 0x1e },
                {"2", 0x1f },
                {"3", 0x20 },
                {"4", 0x21 },
                {"5", 0x22 },
                {"6", 0x23 },
                {"7", 0x24 },
                {"8", 0x25 },
                {"9", 0x26 },
                {"0", 0x27 },
                {"ENTER", 0x28 },
                {"ESC", 0x29 },
                {"BACK\nSPACE", 0x2a },
                {"TAB", 0x2b },
                {"SPACE", 0x2c },
                {"LEFT\nCTRL", 0xe0 },
                {"LEFT\nSHIFT", 0xe1 },
                {"LEFT\nALT", 0xe2 },
                {"LEFT\nMETA", 0xe3 },
                {"RIGHT\nCTRL", 0xe4 },
                {"RIGHT\nSHIFT", 0xe5 },
                {"RIGHT\nALT", 0xe6 },
                {"RIGHT\nMETA", 0xe7 },
                {"-", 0x2d },
                {"=", 0x2e },
                {"[", 0x2f },
                {"]", 0x30 },
                {"\\", 0x31 },
                {"`", 0x32 },
                {";", 0x33 },
                {"'", 0x34 },
                {"GRAVE", 0x35 },
                {",", 0x36 },
                {".", 0x37 },
                {"/", 0x38 },

                {"F1", 0x3a },
                {"F2", 0x3b },
                {"F3", 0x3c },
                {"F4", 0x3d },
                {"F5", 0x3e },
                {"F6", 0x3f },
                {"F7", 0x40 },
                {"F8", 0x41 },
                {"F9", 0x42 },
                {"F10", 0x43 },
                {"F11", 0x44 },
                {"F12", 0x45 },
                {"SYSRQ", 0x46 },
                {"SCROLL\nLOCK", 0x47 },
                {"NUM\nLOCK", 0x53 },
                {"CAPS\nLOCK", 0x39 },
                {"PAUSE", 0x48 },
                {"INSERT", 0x49 },
                {"HOME", 0x4a },
                {"PAGEUP", 0x4b },
                {"DELETE", 0x4c },
                {"END", 0x4d },
                {"PAGE\nDOWN", 0x4e },
                {"RIGHT", 0x4f },
                {"LEFT", 0x50 },
                {"DOWN", 0x51 },
                {"UP", 0x52 },
                {"MUTE", 0x7f },
                {"VOL\nUP", 0x80 },
                {"VOL\nDOWN", 0x81 },
                {"KP\n\\", 0x54 },
                {"KP\n*", 0x55 },
                {"KP\n-", 0x56 },
                {"KP\n+", 0x57 },
                {"KP\nENTER", 0x58 },
                {"KP1", 0x59 },
                {"KP2", 0x5a },
                {"KP3", 0x5b },
                {"KP4", 0x5c },
                {"KP5", 0x5d },
                {"KP6", 0x5e },
                {"KP7", 0x5f },
                {"KP8", 0x60 },
                {"KP9", 0x61 },
                {"KP0", 0x62 },
                {"KP\n.", 0x63 }
                //{"OPEN", 0x74 },
                //{"HELP", 0x75 },
                //{"PROPS", 0x76 },
                //{"FRONT", 0x77 },
                //{"STOP", 0x78 },
                //{"AGAIN", 0x79 },
                //{"UNDO", 0x7a },
                //{"CUT", 0x7b },
                //{"COPY", 0x7c },
                //{"PASTE", 0x7d },
                //{"FIND", 0x7e },

};

        public MainPage()
        {
            this.InitializeComponent();
            Debug.WriteLine("ggg");

        }


        internal static void FindChildren<T>(List<T> results, DependencyObject startNode)
where T : DependencyObject
        {
            int count = VisualTreeHelper.GetChildrenCount(startNode);
            for (int i = 0; i < count; i++)
            {
                DependencyObject current = VisualTreeHelper.GetChild(startNode, i);
                if (current.GetType().Equals(typeof(T)))
                {
                    T asType = (T)current;
                    results.Add(asType);
                }
                FindChildren<T>(results, current);
            }
        }
    }

}
