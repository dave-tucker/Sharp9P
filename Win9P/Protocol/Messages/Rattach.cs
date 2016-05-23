using System;
using Win9P.Exceptions;

namespace Win9P.Protocol.Messages
{
    public sealed class Rattach : Message
    {
        public Rattach(Qid qid)
        {
            Type = (byte) MessageType.Rattach;
            Qid = qid;
            Length += Constants.QIDSZ;
        }

        public Rattach(byte[] bytes) : base(bytes)
        {
            var offset = Constants.HeaderOffset;
            Qid = Protocol.readQid(bytes, offset);
            offset += Constants.QIDSZ;
            if (offset < Length)
            {
                throw new InsufficientDataException(Length,offset);
            }
        }

        public Qid Qid { get; set; }

        public override byte[] ToBytes()
        {
            var bytes = new byte[Length];
            var offset = Protocol.writeUint(bytes, Length, 0);
            bytes[offset] = Type;
            offset += Constants.BIT8SZ;
            offset += Protocol.writeUshort(bytes, Tag, offset);

            offset += Protocol.writeQid(bytes, Qid, offset);

            if (offset < Length)
            {
                throw new InsufficientDataException(Length, offset);
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