using System;
using Win9P.Exceptions;

namespace Win9P.Protocol.Messages
{
    public sealed class Rauth : Message
    {
        public Rauth(Qid aqid)
        {
            Type = (byte) MessageType.Rauth;
            Aqid = aqid;
            Length += Constants.QIDSZ;
        }

        public Rauth(byte[] bytes) : base(bytes)
        {
            var offset = Constants.HeaderOffset;
            Aqid = Protocol.readQid(bytes, Constants.HeaderOffset);
            offset += Constants.QIDSZ;
            if (offset < Length)
            {
                throw new InsufficientDataException(Length, offset);
            }
        }

        public Qid Aqid { get; set; }

        public override byte[] ToBytes()
        {
            var bytes = new byte[Length];
            var offset = Protocol.writeUint(bytes, Length, 0);

            bytes[offset] = Type;
            offset += Constants.BIT8SZ;

            offset += Protocol.writeUshort(bytes, Tag, offset);

            offset += Protocol.writeQid(bytes, Aqid, offset);

            if (offset < Length)
            {
                throw new InsufficientDataException(Length, offset);
            }
            return bytes;
        }

        private bool Equals(Rauth other)
        {
            return base.Equals(other) && Equals(Aqid, other.Aqid);
        }

        public override bool Equals(object obj)
        {
            if (ReferenceEquals(null, obj)) return false;
            if (ReferenceEquals(this, obj)) return true;
            return obj.GetType() == GetType() && Equals((Rauth) obj);
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