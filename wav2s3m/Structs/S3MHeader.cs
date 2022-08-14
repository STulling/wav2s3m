using System;
using System.Collections.Generic;
using System.Linq;
using System.Runtime.InteropServices;
using System.Text;
using System.Threading.Tasks;

namespace wav2s3m.Structs
{
    [StructLayout(LayoutKind.Sequential, Pack = 1, Size = 0x60)]
    internal struct S3MHeader
    {
        [MarshalAs(UnmanagedType.ByValArray, SizeConst = 28)]
        public char[] title;
        public byte sig1;
        public byte type;
        public ushort reserved;
        public ushort orderCount;
        public ushort instrumentCount;
        public ushort patternPtrCount;
        public ushort flags;
        public ushort trackerVersion;
        public ushort sampleType;
        [MarshalAs(UnmanagedType.ByValArray, SizeConst = 4)]
        public char[] sig2;
        public byte globalVolume;
        public byte initialSpeed;
        public byte initialTempo;
        public byte masterVolume;
        public byte ultraClickRemoval;
        public byte defaultPan;
        [MarshalAs(UnmanagedType.ByValArray, SizeConst = 8)]
        private byte[] reserved2;
        public ushort ptrSpecial;
        [MarshalAs(UnmanagedType.ByValArray, SizeConst = 32)]
        public byte[] channelSettings;

    }
}
