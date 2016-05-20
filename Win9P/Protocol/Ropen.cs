using System;

namespace Win9P.Protocol
{
    public class Ropen : Message
    {
        public Qid Qid;
        public uint Iounit;

        public Ropen(Qid qid, uint iounit)
        {
            Type = (byte) MessageType.Ropen;
            Qid = qid;
            Iounit = iounit;
            Length += Protocol.QIDSZ + Protocol.BIT32SZ;
        }

        public Ropen(byte[] bytes) : base(bytes)
        {
            var offset = Protocol.HeaderOffset;
            Qid = Protocol.readQid(bytes, offset);
            offset += Protocol.QIDSZ;
            Iounit = Protocol.readUInt(bytes, offset);
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

            offset += Protocol.writeQid(bytes, Qid, offset);
            offset += Protocol.writeUint(bytes, Iounit, offset);
   
            if (offset < Length)
            {
                throw new Exception($"Buffer underflow. Len: {Length}, Offset: {offset}");
            }
            return bytes;
        }

        protected bool Equals(Ropen other)
        {
            return base.Equals(other) && Equals(Qid, other.Qid) && Iounit == other.Iounit;
        }

        public override bool Equals(object obj)
        {
            if (ReferenceEquals(null, obj)) return false;
            if (ReferenceEquals(this, obj)) return true;
            return obj.GetType() == GetType() && Equals((Ropen) obj);
        }

        public override int GetHashCode()
        {
            unchecked
            {
                var hashCode = base.GetHashCode();
                hashCode = (hashCode*397) ^ (Qid?.GetHashCode() ?? 0);
                hashCode = (hashCode*397) ^ (int) Iounit;
                return hashCode;
            }
        }
    }
}
