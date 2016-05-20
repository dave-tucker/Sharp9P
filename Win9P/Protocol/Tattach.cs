using System;
using System.Xml.Xsl;

namespace Win9P.Protocol
{
    public sealed class Tattach : Message
    {
        public uint Fid { get; set; }
        public uint Afid { get; set; }
        public string Uname { get; set; }
        public string Aname { get; set; }

        public Tattach(uint fid, uint afid, string uname, string aname)
        {
            Type = (byte) MessageType.Tattach;
            Fid = fid;
            Afid = afid;
            Uname = uname;
            Aname = aname;
            Length += Protocol.BIT32SZ + Protocol.BIT32SZ +
                      Protocol.GetStringLength(uname) +
                      Protocol.GetStringLength(aname);
        }

        public Tattach(byte[] bytes) : base(bytes)
        {
            var offset = Protocol.HeaderOffset;
            Fid = Protocol.readUInt(bytes, offset);
            offset += Protocol.BIT32SZ;
            Afid = Protocol.readUInt(bytes, offset);
            offset += Protocol.BIT32SZ;
            Uname = Protocol.readString(bytes, offset);
            offset += (int) Protocol.GetStringLength(Uname);
            Aname = Protocol.readString(bytes, offset);
            offset += (int) Protocol.GetStringLength(Aname);
            if (offset < Length)
            {
                throw new Exception("Too much data");
            }
        }

        public override byte[] ToBytes()
        {
            var bytes = new byte[Length];
            var offset = Protocol.writeUint(bytes, Length, 0);

            bytes[offset] = Type;
            offset += Protocol.BIT8SZ;

            offset += Protocol.writeUshort(bytes, Tag, offset);

            offset += Protocol.writeUint(bytes, Fid, offset);
            offset += Protocol.writeUint(bytes, Afid, offset);
            offset += Protocol.writeString(bytes, Uname, offset);
            offset += Protocol.writeString(bytes, Aname, offset);

            if (offset < Length)
            {
                throw new Exception($"Buffer underflow. Len: {Length}, Offset: {offset}");
            }
            return bytes;
        }

        protected bool Equals(Tattach other)
        {
            return Fid == other.Fid && Afid == other.Afid && string.Equals(Uname, other.Uname) &&
                   string.Equals(Aname, other.Aname);
        }

        public override bool Equals(object obj)
        {
            if (ReferenceEquals(null, obj)) return false;
            if (ReferenceEquals(this, obj)) return true;
            return obj.GetType() == GetType() && Equals((Tattach) obj);
        }

        public override int GetHashCode()
        {
            unchecked
            {
                var hashCode = (int) Fid;
                hashCode = (hashCode*397) ^ (int) Afid;
                hashCode = (hashCode*397) ^ (Uname?.GetHashCode() ?? 0);
                hashCode = (hashCode*397) ^ (Aname?.GetHashCode() ?? 0);
                return hashCode;
            }
        }
    }
}