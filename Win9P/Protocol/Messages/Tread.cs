using System;
using Win9P.Exceptions;

namespace Win9P.Protocol.Messages
{
    public sealed class Tread : Message
    {
        public Tread(uint fid, ulong offset, uint count)
        {
            Type = (byte) MessageType.Tread;
            Fid = fid;
            Offset = offset;
            Count = count;
            Length += Constants.BIT32SZ + Constants.BIT64SZ + Constants.BIT32SZ;
        }

        public Tread(byte[] bytes) : base(bytes)
        {
            var offset = Constants.HeaderOffset;
            Fid = Protocol.readUInt(bytes, offset);
            offset += Constants.BIT32SZ;
            Offset = Protocol.readULong(bytes, offset);
            offset += Constants.BIT64SZ;
            Count = Protocol.readUInt(bytes, offset);
            offset += Constants.BIT32SZ;
            if (offset < Length)
            {
                throw new InsufficientDataException(Length, offset);
            }
        }

        public uint Fid { get; set; }
        public ulong Offset { get; set; }
        public uint Count { get; set; }

        public override byte[] ToBytes()
        {
            var bytes = new byte[Length];
            var offset = Protocol.writeUint(bytes, Length, 0);
            bytes[offset] = Type;
            offset += Constants.BIT8SZ;
            offset += Protocol.writeUshort(bytes, Tag, offset);

            offset += Protocol.writeUint(bytes, Fid, offset);
            offset += Protocol.writeUlong(bytes, Offset, offset);
            offset += Protocol.writeUint(bytes, Count, offset);

            if (offset < Length)
            {
                throw new InsufficientDataException(Length, offset);
            }
            return bytes;
        }

        private bool Equals(Tread other)
        {
            return base.Equals(other) && Fid == other.Fid && Offset == other.Offset && Count == other.Count;
        }

        public override bool Equals(object obj)
        {
            if (ReferenceEquals(null, obj)) return false;
            if (ReferenceEquals(this, obj)) return true;
            return obj is Tread && Equals((Tread) obj);
        }

        public override int GetHashCode()
        {
            unchecked
            {
                var hashCode = base.GetHashCode();
                hashCode = (hashCode*397) ^ (int) Fid;
                hashCode = (hashCode*397) ^ Offset.GetHashCode();
                hashCode = (hashCode*397) ^ (int) Count;
                return hashCode;
            }
        }
    }
}