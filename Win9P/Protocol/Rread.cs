using System;
using System.Linq;

namespace Win9P.Protocol
{
    public class Rread : Message
    {
        public uint Count { get; set; }
        public byte[] Data { get; set; }

        public Rread(uint count, byte[] data)
        {
            Type = (byte)MessageType.Rread;
            Count = count;
            Data = data;
            Length += Protocol.BIT32SZ + Count;
        }

        public Rread(byte[] bytes) : base(bytes)
        {
            var offset = Protocol.HeaderOffset;
            Count = Protocol.readUInt(bytes, offset);
            offset += Protocol.BIT32SZ;
            Data = new byte[Count];
            Array.Copy(bytes, offset, Data, 0, Count);
            offset += (int)Count;
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
            Array.Copy(Data,0,bytes,offset,Count);
            offset += (int) Count;

            if (offset < Length)
            {
                throw new Exception($"Buffer underflow. Len: {Length}, Offset: {offset}");
            }
            return bytes;
        }

        protected bool Equals(Rread other)
        {
            return base.Equals(other) && Count == other.Count && Data.SequenceEqual(other.Data);
        }

        public override bool Equals(object obj)
        {
            if (ReferenceEquals(null, obj)) return false;
            if (ReferenceEquals(this, obj)) return true;
            return obj.GetType() == this.GetType() && Equals((Rread) obj);
        }

        public override int GetHashCode()
        {
            unchecked
            {
                var hashCode = base.GetHashCode();
                hashCode = (hashCode*397) ^ (int) Count;
                hashCode = (hashCode*397) ^ (Data?.GetHashCode() ?? 0);
                return hashCode;
            }
        }
    }
}