using System;

namespace Win9P.Protocol
{
    public sealed class Rversion : Message
    {
        public uint Msize { get; set; }
        public string Version { get; set; }

        public Rversion(uint msize, string version)
        {
            Type = (byte)MessageType.Rversion;
            Msize = msize;
            Version = version;
            Length += Protocol.BIT32SZ + Protocol.GetStringLength(version);
        }

        public Rversion(byte[] bytes) : base (bytes)
        {
            var offset = 7;
            Msize = Protocol.readUInt(bytes, offset);
            offset += Protocol.BIT32SZ;

            Version = Protocol.readString(bytes, offset);
        }

        public override byte[] ToBytes()
        {
            var bytes = new byte[Length];
            var offset = Protocol.writeUint(bytes, Length, 0);
           
            bytes[offset] = Type;
            offset += Protocol.BIT8SZ;
           
            offset += Protocol.writeUshort(bytes, Tag, offset);
           
            offset += Protocol.writeUint(bytes, Msize, offset);
           
            offset += Protocol.writeString(bytes, Version, offset);
           
            if (offset < Length)
            {
                throw new Exception($"Buffer underflow. Len: {Length}, Offset: {offset}");
            }
            return bytes;
        }

        private bool Equals(Rversion other)
        {
            return base.Equals(other) && Msize == other.Msize && string.Equals(Version, other.Version);
        }

        public override bool Equals(object obj)
        {
            if (ReferenceEquals(null, obj)) return false;
            if (ReferenceEquals(this, obj)) return true;
            return obj is Rversion && Equals((Rversion) obj);
        }

        public override int GetHashCode()
        {
            unchecked
            {
                var hashCode = base.GetHashCode();
                hashCode = (hashCode*397) ^ (int) Msize;
                hashCode = (hashCode*397) ^ (Version?.GetHashCode() ?? 0);
                return hashCode;
            }
        }
    }
}