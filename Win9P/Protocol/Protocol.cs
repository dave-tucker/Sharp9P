using System;
using System.IO;
using System.Security.Cryptography;
using System.Text;

namespace Win9P.Protocol
{
    class Protocol
    {
        internal const int BIT8SZ = 1;
        internal const int BIT16SZ = 2;
        internal const int BIT32SZ = 4;
        internal const int BIT64SZ = 8;

        internal const int HeaderOffset = 7;

        internal const int QIDSZ = BIT8SZ + BIT32SZ + BIT64SZ;

        internal const ushort NoTag = 0;
        public const uint NoFid = 0;
        internal const int NOUID = -1;
        internal const int IODHDRSIZE = 24;

        internal const int MAXWELEM = 16;
        internal const int STATFIXLEN = (BIT16SZ + QIDSZ + 5*BIT16SZ + 4*BIT32SZ + BIT64SZ);
   
        private const int MAXPKTSIZE = 8192;
        private const int IOHDRSIZE = (BIT8SZ + BIT16SZ + 3*BIT32SZ + BIT64SZ);

        private const uint DMDIR = 0x80000000;
        private const uint DMAPPEND = 0x40000000;
        private const uint DMEXCL = 0x20000000;
        private const uint DMMOUNT = 0x10000000;
        private const uint DMAUTH = 0x08000000;
        private const uint DMTMP = 0x04000000;
        private const uint DMNONE = 0xFC000000;
        
        private Stream _stream;
        private int _msize = 8192;

        public Protocol(Stream stream)
        {
            _stream = stream;
        }

        private byte[] readBytes(int n)
        {
            var data = new byte[n];
            var r = _stream.Read(data, 0, n);
            if (r < n)
            {
                throw new Exception("Failed to read enough bytes");
            }
            return data;
        }

        internal static uint readUInt(byte[] data, int offset)
        {
            return BitConverter.ToUInt32(data, offset);
        }

        internal static ulong readULong(byte[] data, int offset)
        {
            return BitConverter.ToUInt64(data, offset);
        }

        internal static ushort readUShort(byte[] data, int offset)
        {
            return BitConverter.ToUInt16(data, offset);
        }

        internal static string readString(byte[] data, int offset)
        {
            var utf8 = new UTF8Encoding();
            var len = readUShort(data, offset);
            Console.WriteLine($"String Length: {len}, Offset: {offset}");
            offset += BIT16SZ;
            var strdata = new char[utf8.GetCharCount(data, offset, (int) len)];
            utf8.GetChars(data, offset, (int) len, strdata, 0);
            return new string(strdata);
        }

        internal static Qid readQid(byte[] bytes, int offset)
        {
            var b = new byte[QIDSZ];
            Array.Copy(bytes, offset, b, 0, QIDSZ);
            return new Qid(b);
        }

        internal static Stat readStat(byte[] bytes, int offset)
        {
            var length = readUShort(bytes, offset);
            var b = new byte[length];
            Array.Copy(bytes, offset, b, 0, length);
            return new Stat(b);
        }

        private byte[] readMessage()
        {
            // Read length uint
            var length = readBytes(BIT32SZ);
            var pktlen = readUInt(length,0);
            if (pktlen - BIT32SZ > _msize)
                throw new Exception("Message too large!");

            // Read the remainder of the packet (minus the uint length)
            var data = readBytes((int)pktlen - BIT32SZ);

            var pkt = new byte[pktlen];
            length.CopyTo(pkt, 0);
            data.CopyTo(pkt, BIT32SZ);
            return pkt;
        }

        public Message Read()
        {
            Message message;
            var bytes = readMessage();
            var offset = BIT32SZ;
            var type = bytes[offset];
            switch (type)
            {
                case (byte) MessageType.Tversion:
                    message = new Tversion(bytes);
                    break;
                case (byte) MessageType.Rversion:
                    message = new Rversion(bytes);
                    break;
                case (byte) MessageType.Tauth:
                    message = new Tauth(bytes);
                    break;
                case (byte) MessageType.Rauth:
                    message = new Rauth(bytes);
                    break;
                case (byte)MessageType.Tattach:
                    message = new Tattach(bytes);
                    break;
                case (byte)MessageType.Rattach:
                    message = new Rattach(bytes);
                    break;
                case (byte) MessageType.Rerror:
                    message = new Rerror(bytes);
                    break;
                case (byte) MessageType.Tflush:
                    message = new Tflush(bytes);
                    break;
                case (byte) MessageType.Rflush:
                    message = new Rflush(bytes);
                    break;
                case (byte) MessageType.Twalk:
                    message = new Twalk(bytes);
                    break;
                case (byte) MessageType.Rwalk:
                    message = new Rwalk(bytes);
                    break;
                case (byte) MessageType.Topen:
                    message = new Topen(bytes);
                    break;
                case (byte) MessageType.Ropen:
                    message = new Ropen(bytes);
                    break;
                case (byte) MessageType.Tcreate:
                    message = new Tcreate(bytes);
                    break;
                case (byte) MessageType.Rcreate:
                    message = new Rcreate(bytes);
                    break;
                case (byte) MessageType.Tread:
                    message = new Tread(bytes);
                    break;
                case (byte) MessageType.Rread:
                    message = new Rread(bytes);
                    break;
                case (byte) MessageType.Twrite:
                    message = new Twrite(bytes);
                    break;
                case (byte) MessageType.Rwrite:
                    message = new Rwrite(bytes);
                    break;
                case (byte) MessageType.Tclunk:
                    message = new Tclunk(bytes);
                    break;
                case (byte) MessageType.Rclunk:
                    message = new Rclunk(bytes);
                    break;
                case (byte) MessageType.Tremove:
                    message = new Tremove(bytes);
                    break;
                case (byte) MessageType.Rremove:
                    message = new Rremove(bytes);
                    break;
                case (byte) MessageType.Tstat:
                    message = new Tstat(bytes);
                    break;
                case (byte) MessageType.Rstat:
                    message = new Rstat(bytes);
                    break;
                case (byte) MessageType.Twstat:
                    message = new Twstat(bytes);
                    break;
                case (byte) MessageType.Rwstat:
                    message = new Rwstat(bytes);
                    break;
                default:
                    throw new Exception("Unsupported Message Type");
            }
            return message;
        }

        internal static int writeUlong(byte[] data, ulong var, int offset)
        {
            var bytes = BitConverter.GetBytes(var);
            Array.Copy(bytes, 0, data, offset, bytes.Length);
            return BIT64SZ;
        }

        internal static int writeUint(byte[] data, uint var, int offset)
        {
            var bytes = BitConverter.GetBytes(var);
            Array.Copy(bytes, 0, data, offset, bytes.Length);
            return BIT32SZ;
        }

        internal static int writeUshort(byte[] data, ushort var, int offset)
        {
            var bytes = BitConverter.GetBytes(var);
            Array.Copy(bytes, 0, data, offset, bytes.Length);
            return BIT16SZ;
        }

        internal static int writeString(byte[] data, string var, int offset)
        {
            var utf8 = new UTF8Encoding();

            writeUshort(data, (ushort)var.Length, offset);
            offset += BIT16SZ;

            var bytes = utf8.GetBytes(var);
            Array.Copy(bytes, 0, data, offset, utf8.GetByteCount(var));
            return BIT16SZ + utf8.GetByteCount(var);
        }

        internal static int writeQid(byte[] data, Qid qid, int offset)
        {
            var bytes = qid.ToBytes();
            Array.Copy(bytes, 0, data, offset, QIDSZ);
            return QIDSZ;
        }

        internal static int writeStat(byte[] data, Stat stat, int offset)
        {
            var bytes = stat.ToBytes();
            Array.Copy(bytes, 0, data, offset, stat.Size);
            return stat.Size;
        }


        internal static uint GetStringLength(string var)
        {
            var utf8 = new UTF8Encoding();
            return (uint)(BIT16SZ + utf8.GetByteCount(var));
        }

        public void Write(Message message)
        {
            var bytes = message.ToBytes();
            _stream.Write(bytes,0,bytes.Length);
            _stream.Flush();
        }
    }
}
