using System;
using System.Collections.Generic;
using System.Linq;
using System.Runtime.InteropServices;
using System.Text;
using System.Threading.Tasks;

namespace wav2s3m.Structs
{
    static internal class utils
    {
        public static T ReadType<T>(BinaryReader binaryReader)
        {
            int size = Marshal.SizeOf(typeof(T));
            return (T)BytesToDataStruct(binaryReader.ReadBytes(size), typeof(T));
        }
        public static object BytesToDataStruct(byte[] bytes, Type type)
        {
            //DataStruct data = new DataStruct();

            int size = Marshal.SizeOf(type);

            if (size > bytes.Length)
            {
                return null;
            }

            IntPtr structPtr = Marshal.AllocHGlobal(size);
            Marshal.Copy(bytes, 0, structPtr, size);
            object obj = Marshal.PtrToStructure(structPtr, type);
            Marshal.FreeHGlobal(structPtr);
            return obj;
        }

        public static byte[] StructToBytes(object anyStruct)
        {
            int size = Marshal.SizeOf(anyStruct);
            IntPtr bytesPtr = Marshal.AllocHGlobal(size);
            Marshal.StructureToPtr(anyStruct, bytesPtr, false);
            byte[] bytes = new byte[size];
            Marshal.Copy(bytesPtr, bytes, 0, size);
            Marshal.FreeHGlobal(bytesPtr);

            return bytes;
        }

        public static long Align(BinaryWriter bw)
        {
            long currentPos = bw.BaseStream.Position;
            long offset = 16 - (currentPos % 16);
            if (offset == 0 || offset == 16) return (currentPos / 16);
            for (int i = 0; i < offset; i++)
            {
                bw.Write((byte)0x80);
            }
            return (bw.BaseStream.Position / 16);
        }
        internal static uint ReadUInt24(this BinaryReader reader)
        {
            try
            {
                var b1 = reader.ReadByte();
                var b2 = reader.ReadUInt16();
                return (uint)((b1 << 16) | b2);
            }
            catch
            {
                return 0u;
            }
        }

        internal static uint WriteUInt24(this BinaryWriter writer, uint value)
        {
            writer.Write((byte)(value >> 16));
            writer.Write((ushort)value);
            return value;
        }

        internal static byte PeekByte(this BinaryReader reader)
        {
            long position = reader.BaseStream.Position;
            byte value = reader.ReadByte();
            reader.BaseStream.Position = position;
            return value;
        }
    }
}
