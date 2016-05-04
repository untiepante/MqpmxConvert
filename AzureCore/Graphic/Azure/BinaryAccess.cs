using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.IO;
using SharpDX;

namespace AzureCore.Graphic.Azure
{

    public static class BinaryAccess
    {
        public static int ReadInt32(Stream s)
        {
            byte[] buffer = new byte[4];
            s.Read(buffer, 0, 4);
            return BitConverter.ToInt32(buffer, 0);
        }
        public static uint ReadUInt32(Stream s)
        {
            byte[] buffer = new byte[4];
            s.Read(buffer, 0, 4);
            return BitConverter.ToUInt32(buffer, 0);
        }
        public static short ReadInt16(Stream s)
        {
            byte[] buffer = new byte[2];
            s.Read(buffer, 0, 2);
            return BitConverter.ToInt16(buffer, 0);
        }
        public static ushort ReadUInt16(Stream s)
        {
            byte[] buffer = new byte[2];
            s.Read(buffer, 0, 2);
            return BitConverter.ToUInt16(buffer, 0);
        }
        public static byte ReadByte(Stream s)
        {
            byte[] buffer = new byte[1];
            s.Read(buffer, 0, 1);
            return buffer[0];
        }
        public static long ReadInt64(Stream s)
        {
            byte[] buffer = new byte[8];
            s.Read(buffer, 0, 8);
            return BitConverter.ToInt64(buffer, 0);
        }
        public static ulong ReadUInt64(Stream s)
        {
            byte[] buffer = new byte[8];
            s.Read(buffer, 0, 8);
            return BitConverter.ToUInt64(buffer, 0);
        }
        public static float ReadFloat(Stream s)
        {
            byte[] buffer = new byte[4];
            s.Read(buffer, 0, 4);
            return BitConverter.ToSingle(buffer, 0);
        }
        public static double ReadDouble(Stream s)
        {
            byte[] buffer = new byte[8];
            s.Read(buffer, 0, 8);
            return BitConverter.ToDouble(buffer, 0);
        }
        public static bool ReadBoolean(Stream s)
        {
            byte[] buffer = new byte[1];
            s.Read(buffer, 0, 1);
            return BitConverter.ToBoolean(buffer, 0);
        }
        public static string ReadString(Stream s)
        {
            byte[] buffer = new byte[4];
            s.Read(buffer, 0, 4);
            buffer = new byte[BitConverter.ToInt32(buffer, 0)];
            s.Read(buffer, 0, buffer.Length);
            return System.Text.Encoding.Default.GetString(buffer);
        }
        public static string ReadString(Stream s, System.Text.Encoding encoding)
        {
            byte[] buffer = new byte[4];
            s.Read(buffer, 0, 4);
            buffer = new byte[BitConverter.ToInt32(buffer, 0)];
            s.Read(buffer, 0, buffer.Length);
            return encoding.GetString(buffer);
        }
        public static byte[] ReadBinary(Stream s)
        {
            byte[] buffer = new byte[4];
            s.Read(buffer, 0, 4);
            buffer = new byte[BitConverter.ToInt32(buffer, 0)];
            s.Read(buffer, 0, buffer.Length);
            return buffer;
        }
        public static int[] ReadInt32Array(Stream s)
        {
            byte[] buffer = new byte[8];
            s.Read(buffer, 0, 8);
            int[] result = new int[BitConverter.ToInt64(buffer, 0)];
            for (int i = 0; i < result.Length; i++)
            {
                result[i] = BinaryAccess.ReadInt32(s);
            }
            return result;
        }
        public static string[] ReadStringArray(Stream s)
        {
            byte[] buffer = new byte[8];
            s.Read(buffer, 0, 8);
            string[] result = new string[BitConverter.ToInt64(buffer, 0)];
            for (int i = 0; i < result.Length; i++)
            {
                result[i] = BinaryAccess.ReadString(s);
            }
            return result;
        }
        public static bool[] ReadBooleanArray(Stream s)
        {
            byte[] buffer = new byte[8];
            s.Read(buffer, 0, 8);
            bool[] result = new bool[BitConverter.ToInt64(buffer, 0)];
            for (int i = 0; i < result.Length; i++)
            {
                result[i] = BinaryAccess.ReadBoolean(s);
            }
            return result;
        }
        public static Vector3 ReadVector3(Stream s)
        {
            byte[] buffer = new byte[12];
            Vector3 res = new Vector3();
            s.Read(buffer, 0, 12);
            res.X = BitConverter.ToSingle(buffer, 0);
            res.Y = BitConverter.ToSingle(buffer, 4);
            res.Z = BitConverter.ToSingle(buffer, 8);
            return res;
        }

        public static void Write(Stream s, int v)
        {
            byte[] buffer = BitConverter.GetBytes(v);
            s.Write(buffer, 0, buffer.Length);
        }
        public static void Write(Stream s, uint v)
        {
            byte[] buffer = BitConverter.GetBytes(v);
            s.Write(buffer, 0, buffer.Length);
        }
        public static void Write(Stream s, short v)
        {
            byte[] buffer = BitConverter.GetBytes(v);
            s.Write(buffer, 0, buffer.Length);
        }
        public static void Write(Stream s, ushort v)
        {
            byte[] buffer = BitConverter.GetBytes(v);
            s.Write(buffer, 0, buffer.Length);
        }
        public static void Write(Stream s, byte v)
        {
            s.WriteByte(v);
        }
        public static void Write(Stream s, long v)
        {
            byte[] buffer = BitConverter.GetBytes(v);
            s.Write(buffer, 0, buffer.Length);
        }
        public static void Write(Stream s, ulong v)
        {
            byte[] buffer = BitConverter.GetBytes(v);
            s.Write(buffer, 0, buffer.Length);
        }
        public static void Write(Stream s, float v)
        {
            byte[] buffer = BitConverter.GetBytes(v);
            s.Write(buffer, 0, buffer.Length);
        }
        public static void Write(Stream s, double v)
        {
            byte[] buffer = BitConverter.GetBytes(v);
            s.Write(buffer, 0, buffer.Length);
        }
        public static void Write(Stream s, bool v)
        {
            byte[] buffer = BitConverter.GetBytes(v);
            s.Write(buffer, 0, buffer.Length);
        }
        public static void Write(Stream s, string v)
        {
            byte[] buffer = System.Text.Encoding.Default.GetBytes(v);
            s.Write(BitConverter.GetBytes((int)buffer.Length), 0, 4);
            s.Write(buffer, 0, buffer.Length);
        }
        public static void Write(Stream s, string v, System.Text.Encoding encoding)
        {
            byte[] buffer = encoding.GetBytes(v);
            s.Write(BitConverter.GetBytes((int)buffer.Length), 0, 4);
            s.Write(buffer, 0, buffer.Length);
        }
        public static void Write(Stream s, byte[] v)
        {
            s.Write(BitConverter.GetBytes((int)v.Length), 0, 4);
            s.Write(v, 0, v.Length);
        }
        public static void Write(Stream s, int[] v)
        {
            byte[] buffer = BitConverter.GetBytes(v.LongLength);
            s.Write(buffer, 0, 8);
            for (int i = 0; i < v.Length; i++)
                BinaryAccess.Write(s, v[i]);
        }
        public static void Write(Stream s, string[] v)
        {
            byte[] buffer = BitConverter.GetBytes(v.LongLength);
            s.Write(buffer, 0, 8);
            for (int i = 0; i < v.Length; i++)
                BinaryAccess.Write(s, v[i]);
        }
        public static void Write(Stream s, bool[] v)
        {
            byte[] buffer = BitConverter.GetBytes(v.LongLength);
            s.Write(buffer, 0, 8);
            for (int i = 0; i < v.Length; i++)
                BinaryAccess.Write(s, v[i]);
        }
        public static void Write(Stream s, Vector3 v)
        {
            byte[] buffer = BitConverter.GetBytes(v.X);
            s.Write(buffer, 0, buffer.Length);
            buffer = BitConverter.GetBytes(v.Y);
            s.Write(buffer, 0, buffer.Length);
            buffer = BitConverter.GetBytes(v.Z);
            s.Write(buffer, 0, buffer.Length);
        }

        public const int BufferSize = 1024 * 1024 * 5;

        public static void CopyBlock(Stream input, Stream output, long blockSize)
        {
            long rest = blockSize;
            byte[] Buffer = new byte[BufferSize];

            while (rest > BufferSize)
            {
                input.Read(Buffer, 0, BufferSize);
                output.Write(Buffer, 0, BufferSize);
                rest -= BufferSize;
            }
            if (rest != 0)
            {
                Buffer = new byte[rest];
                input.Read(Buffer, 0, (int)rest);
                output.Write(Buffer, 0, (int)rest);
            }

            Buffer = null;
        }
        public static void CopyBlock(Stream input, Stream output, long inputStart, long outputStart, long blockSize)
        {
            input.Seek(inputStart, SeekOrigin.Begin);
            output.Seek(outputStart, SeekOrigin.Begin);

            CopyBlock(input, output, blockSize);
        }
    }
}
