using System;
using Win9P.Exceptions;

namespace Win9P.Protocol.Messages
{
    public sealed class Tauth : Message
    {
        public Tauth(uint afid, string uname, string aname)
        {
            Type = (byte) MessageType.Tauth;
            Afid = afid;
            Uname = uname;
            Aname = aname;
            Length += Constants.BIT32SZ + Protocol.GetStringLength(Uname) + Protocol.GetStringLength(Aname);
        }

        public Tauth(byte[] bytes) : base(bytes)
        {
            var offset = Constants.HeaderOffset;
            Afid = Protocol.readUInt(bytes, offset);
            offset += Constants.BIT32SZ;
            Uname = Protocol.readString(bytes, offset);
            offset += (int) Protocol.GetStringLength(Uname);
            Aname = Protocol.readString(bytes, offset);
            offset += (int) Protocol.GetStringLength(Aname);
            if (offset < Length)
            {
                throw new InsufficientDataException(Length, offset);
            }
        }

        public uint Afid { get; set; }
        public string Uname { get; set; }
        public string Aname { get; set; }

        public override byte[] ToBytes()
        {
            var bytes = new byte[Length];
            var offset = Protocol.writeUint(bytes, Length, 0);

            bytes[offset] = Type;
            offset += Constants.BIT8SZ;

            offset += Protocol.writeUshort(bytes, Tag, offset);

            offset += Protocol.writeUint(bytes, Afid, offset);

            offset += Protocol.writeString(bytes, Uname, offset);
            offset += Protocol.writeString(bytes, Aname, offset);

            if (offset < Length)
            {
                throw new InsufficientDataException(Length, offset);
            }
            return bytes;
        }

        private bool Equals(Tauth other)
        {
            return base.Equals(other) && Afid == other.Afid && string.Equals(Uname, other.Uname) &&
                   string.Equals(Aname, other.Aname);
        }

        public override bool Equals(object obj)
        {
            if (ReferenceEquals(null, obj)) return false;
            if (ReferenceEquals(this, obj)) return true;
            return obj is Tauth && Equals((Tauth) obj);
        }

        public override int GetHashCode()
        {
            unchecked
            {
                var hashCode = base.GetHashCode();
                hashCode = (hashCode*397) ^ (int) Afid;
                hashCode = (hashCode*397) ^ (Uname?.GetHashCode() ?? 0);
                hashCode = (hashCode*397) ^ (Aname?.GetHashCode() ?? 0);
                return hashCode;
            }
        }
    }
}