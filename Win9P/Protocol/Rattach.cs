using System;
using System.Diagnostics;

namespace Win9P.Protocol
{
    public sealed class Rattach : Message
    {
        public Qid Qid { get; set; }

        public Rattach(Qid qid)
        {
            Type = (byte) MessageType.Rattach;
            Qid = qid;
            Length += Protocol.QIDSZ;
        }

        public Rattach(byte[] bytes) : base(bytes)
        {
            var offset = Protocol.HeaderOffset;
            Qid = Protocol.readQid(bytes, offset);
            offset += Protocol.QIDSZ;
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

            if (offset < Length)
            {
                throw new Exception($"Buffer underflow. Len: {Length}, Offset: {offset}");
            }
            return bytes;
        }

        private bool Equals(Rattach other)
        {
            return base.Equals(other) && Equals(Qid, other.Qid);
        }

        public override bool Equals(object obj)
        {
            if (ReferenceEquals(null, obj)) return false;
            if (ReferenceEquals(this, obj)) return true;
            return obj is Rattach && Equals((Rattach) obj);
        }

        public override int GetHashCode()
        {
            unchecked
            {
                return (base.GetHashCode()*397) ^ (Qid?.GetHashCode() ?? 0);
            }
        }
    }
}