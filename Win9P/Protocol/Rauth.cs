using System;

namespace Win9P.Protocol
{
    public class Rauth : Message
    {
        public Qid Aqid { get; set; }

        public Rauth(Qid aqid)
        {
            Type = (byte) MessageType.Rauth;
            Aqid = aqid;
            Length += Protocol.QIDSZ;
        }

        public Rauth(byte[] bytes) : base(bytes)
        {
            Aqid = Protocol.readQid(bytes, Protocol.HeaderOffset);
        }

        public override byte[] ToBytes()
        {
            var bytes = new byte[Length];
            var offset = Protocol.writeUint(bytes, Length, 0);

            bytes[offset] = Type;
            offset += Protocol.BIT8SZ;

            offset += Protocol.writeUshort(bytes, Tag, offset);

            offset += Protocol.writeQid(bytes, Aqid, offset);

            if (offset < Length)
            {
                throw new Exception($"Buffer underflow. Len: {Length}, Offset: {offset}");
            }
            return bytes;
        }

        protected bool Equals(Rauth other)
        {
            return base.Equals(other) && Equals(Aqid, other.Aqid);
        }

        public override bool Equals(object obj)
        {
            if (ReferenceEquals(null, obj)) return false;
            if (ReferenceEquals(this, obj)) return true;
            return obj.GetType() == this.GetType() && Equals((Rauth) obj);
        }

        public override int GetHashCode()
        {
            unchecked
            {
                return (base.GetHashCode()*397) ^ (Aqid?.GetHashCode() ?? 0);
            }
        }
    }
}