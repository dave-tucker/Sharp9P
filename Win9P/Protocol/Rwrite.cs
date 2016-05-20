using System;

namespace Win9P.Protocol
{
    public class Rwrite : Message
    {
        public uint Count { get; set; }

        public Rwrite(uint count)
        {
            Type = (byte) MessageType.Rwrite;
            Count = count;
            Length += Protocol.BIT32SZ;
        }

        public Rwrite(byte[] bytes) : base(bytes)
        {
            var offset = Protocol.HeaderOffset;
            Count = Protocol.readUInt(bytes, offset);
            offset += Protocol.BIT32SZ;
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

            offset += Protocol.writeUint(bytes, Count, offset);

            if (offset < Length)
            {
                throw new Exception($"Buffer underflow. Len: {Length}, Offset: {offset}");
            }
            return bytes;
        }

        protected bool Equals(Rwrite other)
        {
            return base.Equals(other) && Count == other.Count;
        }

        public override bool Equals(object obj)
        {
            if (ReferenceEquals(null, obj)) return false;
            if (ReferenceEquals(this, obj)) return true;
            return obj.GetType() == GetType() && Equals((Rwrite) obj);
        }

        public override int GetHashCode()
        {
            unchecked
            {
                return (base.GetHashCode()*397) ^ (int) Count;
            }
        }
    }
}