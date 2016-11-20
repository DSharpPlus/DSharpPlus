using System;
using System.Linq;
using System.Text;

namespace DSharpPlus.Voice
{
    public class ByteBuffer
    {
        byte[] buff;
        
        private void ResizeBuffer(int offset, int length)
        {
            if ((offset+length) > buff.Length)
            {
                byte[] tBuff = buff;
                buff = new byte[offset + length];
                Buffer.BlockCopy(tBuff, 0, buff, 0, tBuff.Length);
            }
        }

        public ByteBuffer(int length)
        {
            buff = new byte[length];
        }

        public ByteBuffer(byte[] data, bool reverse = false)
        {
            buff = data;
        }

        public byte[] GetBuffer() => buff;

        public void WriteByteArrayToBuffer(byte[] value, int offset, bool reverse = false)
        {
            if (reverse) value = value.Reverse().ToArray();
            ResizeBuffer(offset, value.Length);
            Buffer.BlockCopy(value, 0, buff, offset, value.Length);
        }

        public byte[] ReadByteArrayFromBuffer(int offset, int length)
        {
            byte[] result = new byte[length];
            Buffer.BlockCopy(buff, offset, result, 0, length);
            return result;
        }

        public void WriteULongToBuffer(ulong value, int offset, bool reverse = false)
        {
            byte[] bValue = BitConverter.GetBytes(value);
            if (reverse) bValue = bValue.Reverse().ToArray();
            ResizeBuffer(offset, bValue.Length);
            Buffer.BlockCopy(bValue, 0, buff, offset, bValue.Length);
        }

        public ulong ReadULongFromBuffer(int offset)
        {
            return BitConverter.ToUInt64(buff, offset);
        }

        public void WriteUIntToBuffer(uint value, int offset, bool reverse = false)
        {
            byte[] bValue = BitConverter.GetBytes(value);
            if (reverse) bValue = bValue.Reverse().ToArray();
            ResizeBuffer(offset, bValue.Length);
            Buffer.BlockCopy(bValue, 0, buff, offset, bValue.Length);
        }

        public uint ReadUIntFromBuffer(int offset, bool reverse = false)
        {
            byte[] result = ReadByteArrayFromBuffer(offset, 4);
            if (reverse) result = result.Reverse().ToArray();

            return BitConverter.ToUInt32(result, 0);
        }

        public void WriteIntToBuffer(int value, int offset, bool reverse = false)
        {
            byte[] bValue = BitConverter.GetBytes(value);
            if (reverse) bValue = bValue.Reverse().ToArray();
            ResizeBuffer(offset, bValue.Length);
            Buffer.BlockCopy(bValue, 0, buff, offset, bValue.Length);
        }

        public int ReadIntFromBuffer(int offset)
        {
            return BitConverter.ToInt32(buff, offset);
        }

        public void WriteUShortToBuffer(ushort value, int offset, bool reverse = false)
        {
            byte[] bValue = BitConverter.GetBytes(value);
            if (reverse) bValue = bValue.Reverse().ToArray();
            ResizeBuffer(offset, bValue.Length);
            Buffer.BlockCopy(bValue, 0, buff, offset, bValue.Length);
        }

        public ushort ReadUShortFromBuffer(int offset, bool reverse = false)
        {
            byte[] result = ReadByteArrayFromBuffer(offset, 2);
            if (reverse) result = result.Reverse().ToArray();

            return BitConverter.ToUInt16(result, 0);
        }

        public void WriteShortToBuffer(short value, int offset, bool reverse = false)
        {
            byte[] bValue = BitConverter.GetBytes(value);
            if (reverse) bValue = bValue.Reverse().ToArray();
            ResizeBuffer(offset, bValue.Length);
            Buffer.BlockCopy(bValue, 0, buff, offset, bValue.Length);
        }

        public short ReadShortFromBuffer(int offset)
        {
            return BitConverter.ToInt16(buff, offset);
        }

        public void WriteCharToBuffer(char value, int offset, bool reverse = false)
        {
            byte[] bValue = BitConverter.GetBytes(value);
            if (reverse) bValue = bValue.Reverse().ToArray();
            ResizeBuffer(offset, bValue.Length);
            Buffer.BlockCopy(bValue, 0, buff, offset, bValue.Length);
        }

        public char ReadCharFromBuffer(int offset)
        {
            return BitConverter.ToChar(buff, offset);
        }

        public void WriteByteToBuffer(byte value, int offset, bool reverse = false)
        {
            byte[] bValue = new byte[1] { value };
            if (reverse) bValue = bValue.Reverse().ToArray();
            ResizeBuffer(offset, bValue.Length);
            Buffer.BlockCopy(bValue, 0, buff, offset, bValue.Length);
        }

        public byte ReadByteFromBuffer(int offset)
        {
            return buff[offset];
        }

        public void WriteStringToBuffer(string value, int offset, bool reverse = false)
        {
            byte[] bValue = Encoding.ASCII.GetBytes(value);
            if (reverse) bValue = bValue.Reverse().ToArray();
            ResizeBuffer(offset, bValue.Length);
            Buffer.BlockCopy(bValue, 0, buff, offset, bValue.Length);
        }

        public string ReadStringFromBuffer(int offset, int length)
        {
            return Encoding.ASCII.GetString(buff, offset, length);
        }

        public bool ByteExists(int offset)
        {
            return (buff.Length > offset);
        }

        public void RemoveByte(int offset)
        {
            buff[offset] = 0x00;
        }
    }
}
