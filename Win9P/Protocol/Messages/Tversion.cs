using System;
using Win9P.Exceptions;

namespace Win9P.Protocol.Messages
{
    public sealed class Tversion : Message
    {
        public Tversion(uint msize, string version)
        {
            Type = (byte) MessageType.Tversion;
            Msize = msize;
            Version = version;
            Length += Constants.BIT32SZ + Protocol.GetStringLength(version);
        }

        public Tversion(byte[] bytes) : base(bytes)
        {
            var offset = Constants.HeaderOffset;
            Msize = Protocol.readUInt(bytes, offset);
            offset += Constants.BIT32SZ;
            Version = Protocol.readString(bytes, offset);
            offset += (int) Protocol.GetStringLength(Version);
            if (offset < Length)
            {
                throw new InsufficientDataException(Length, offset);
            }
        }

        public uint Msize { get; set; }
        public string Version { get; set; }

        public override byte[] ToBytes()
        {
            var bytes = new byte[Length];
            var offset = Protocol.writeUint(bytes, Length, 0);

            bytes[offset] = Type;
            offset += Constants.BIT8SZ;

            offset += Protocol.writeUshort(bytes, Tag, offset);

            offset += Protocol.writeUint(bytes, Msize, offset);

            offset += Protocol.writeString(bytes, Version, offset);

            if (offset < Length)
            {
                throw new InsufficientDataException(Length, offset);
            }
            return bytes;
        }

        private bool Equals(Tversion other)
        {
            return Msize == other.Msize && string.Equals(Version, other.Version);
        }

        public override bool Equals(object obj)
        {
            if (ReferenceEquals(null, obj)) return false;
            if (ReferenceEquals(this, obj)) return true;
            return obj is Tversion && Equals((Tversion) obj);
        }

        public override int GetHashCode()
        {
            unchecked
            {
                return ((int) Msize*397) ^ (Version?.GetHashCode() ?? 0);
            }
        }
    }
}