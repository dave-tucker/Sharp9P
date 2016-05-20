using System;

namespace Win9P.Protocol
{
    public sealed class Tread : Message
    {
        public uint Fid { get; set; }
        public ulong Offset { get; set; }
        public uint Count { get; set; }

        public Tread(uint fid, ulong offset, uint count)
        {
            Type = (byte) MessageType.Tread;
            Fid = fid;
            Offset = offset;
            Count = count;
            Length += Protocol.BIT32SZ + Protocol.BIT64SZ + Protocol.BIT32SZ;
        }

        public Tread(byte[] bytes) : base(bytes)
        {
            var offset = Protocol.HeaderOffset;
            Fid = Protocol.readUInt(bytes, offset);
            offset += Protocol.BIT32SZ;
            Offset = Protocol.readULong(bytes, offset);
            offset += Protocol.BIT64SZ;
            Count = Protocol.readUInt(bytes, offset);
            offset += Protocol.BIT32SZ;
            if (offset < Length)
            {
                throw new Exception($"Too much data: Offset={offset}, Length={Length}");
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
            offset += Protocol.writeUlong(bytes, Offset, offset);
            offset += Protocol.writeUint(bytes, Count, offset);

            if (offset < Length)
            {
                throw new Exception($"Buffer underflow. Len: {Length}, Offset: {offset}");
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
                int hashCode = base.GetHashCode();
                hashCode = (hashCode*397) ^ (int) Fid;
                hashCode = (hashCode*397) ^ Offset.GetHashCode();
                hashCode = (hashCode*397) ^ (int) Count;
                return hashCode;
            }
        }
    }
}