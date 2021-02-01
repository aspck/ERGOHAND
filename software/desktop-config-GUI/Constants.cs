using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace desktop_config_GUI
{
    internal class Constants
    {
        // USB HID spec https://usb.org/document-library/hid-usage-tables-121
        internal Dictionary<byte, string> KeyList { get; } = new Dictionary<byte, string>() {
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

        internal int nKeys = 50;  // 25 per device

        // bluetooth constants, these should be read from an app config file in the future
        internal int KeyReportSize = 13;
        internal string ServiceUUID = "72450222-C69E-485B-82AC-083A3EAFB3AC";
        internal string Characteristic1UUID = "6AF7ACFD-F66B-4932-8975-41F512990077";
        internal string Characteristic2UUID = "4F229A78-0344-430A-B203-CF8685E77E2C";
        internal string DeviceAddressLeft = "BluetoothLE#BluetoothLE5c:f3:70:99:86:ef-10:a0:50:fa:ff:01"; // 10A050-FAFF01
        internal string DeviceAddressRight = "BluetoothLE#BluetoothLE5c:f3:70:99:86:ef-10:a0:50:fa:ff:02"; // 10A050-FAFF02
    }
}
