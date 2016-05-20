using System;
using System.Runtime.InteropServices;

namespace Win9P.Protocol
{
    public enum QidType
    {
        QTDIR  = 0x80, // type bit for directories
        QTAPPEND = 0x40, // type bit for append only files
        QTEXCL = 0x20, // type bit for exclusive use files
        QTMOUNT = 0x10, // type bit for mounted channel
        QTAUTH = 0x08, // type bit for authentication file
        QTTMP = 0x04, // type bit for not-backed-up file
        QTFILE = 0x00, // plain file
    }

    public class Qid
    {
        public readonly byte Type;
        public readonly uint Vers;
        public readonly ulong Path;

        public Qid(byte type, uint vers, ulong path)
        {
            Type = type;
            Vers = vers;
            Path = path;
        }

        public Qid(byte[] bytes)
        {
            var offset = 0;
            Type = bytes[offset];
            offset += Protocol.BIT8SZ;
            Vers = Protocol.readUInt(bytes, offset);
            offset += Protocol.BIT32SZ;
            Path = Protocol.readULong(bytes, offset);
        }

        public byte[] ToBytes()
        {
            var bytes = new byte[Protocol.QIDSZ];
            var offset = 0;
            bytes[offset] = Type;
            offset += Protocol.BIT8SZ;
            offset += Protocol.writeUint(bytes, Vers, offset);
            offset += Protocol.writeUlong(bytes, Path, offset);

            if (offset < Protocol.QIDSZ)
            {
                throw new Exception($"Buffer underflow. Len: {Protocol.QIDSZ}, Offset: {offset}");
            }
            return bytes;
        }

        protected bool Equals(Qid other)
        {
            return Type == other.Type && Vers == other.Vers && Path == other.Path;
        }

        public override bool Equals(object obj)
        {
            if (ReferenceEquals(null, obj)) return false;
            if (ReferenceEquals(this, obj)) return true;
            return obj.GetType() == GetType() && Equals((Qid) obj);
        }

        public override int GetHashCode()
        {
            unchecked
            {
                var hashCode = Type.GetHashCode();
                hashCode = (hashCode*397) ^ (int) Vers;
                hashCode = (hashCode*397) ^ Path.GetHashCode();
                return hashCode;
            }
        }
    }
}
