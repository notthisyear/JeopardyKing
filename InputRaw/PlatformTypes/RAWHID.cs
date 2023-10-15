using System;
using System.Runtime.InteropServices;

namespace JeopardyKing.InputRaw
{
#pragma warning disable IDE1006 // Naming Styles
    [StructLayout(LayoutKind.Sequential)]
    internal struct RAWHID
    {
        /// <summary>
        /// The size, in bytes, of each HID input in RawData.
        /// </summary>
        public uint SizeHid;

        /// <summary>
        /// The number of HID inputs in RawData.
        /// </summary>
        public uint Count;

        /// <summary>
        /// The raw input data, as an array of bytes.
        /// </summary>
        public IntPtr RawData;
    }
#pragma warning restore IDE1006 // Naming Styles
}
