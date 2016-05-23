using System;
using Win9P.Exceptions;

namespace Win9P.Protocol.Messages
{
    public sealed class Ropen : Message
    {
        public uint Iounit;
        public Qid Qid;

        public Ropen(Qid qid, uint iounit)
        {
            Type = (byte) MessageType.Ropen;
            Qid = qid;
            Iounit = iounit;
            Length += Constants.QIDSZ + Constants.BIT32SZ;
        }

        public Ropen(byte[] bytes) : base(bytes)
        {
            var offset = Constants.HeaderOffset;
            Qid = Protocol.readQid(bytes, offset);
            offset += Constants.QIDSZ;
            Iounit = Protocol.readUInt(bytes, offset);
            offset += Constants.BIT32SZ;
            if (offset < Length)
            {
                throw new InsufficientDataException(Length, offset);
            }
        }

        public override byte[] ToBytes()
        {
            var bytes = new byte[Length];
            var offset = Protocol.writeUint(bytes, Length, 0);
            bytes[offset] = Type;
            offset += Constants.BIT8SZ;
            offset += Protocol.writeUshort(bytes, Tag, offset);

            offset += Protocol.writeQid(bytes, Qid, offset);
            offset += Protocol.writeUint(bytes, Iounit, offset);

            if (offset < Length)
            {
               throw new InsufficientDataException(Length, offset);
            }
            return bytes;
        }

        private bool Equals(Ropen other)
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