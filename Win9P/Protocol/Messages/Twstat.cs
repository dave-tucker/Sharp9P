using Win9P.Exceptions;

namespace Win9P.Protocol.Messages
{
    public class Twstat : Message
    {
        public Twstat(uint fid, Stat stat)
        {
            Type = (byte) MessageType.Twstat;
            Fid = fid;
            Stat = stat;
            Length += Constants.BIT32SZ + (uint) Stat.Size;
        }

        public Twstat(byte[] bytes) : base(bytes)
        {
            var offset = Constants.HeaderOffset;
            Fid = Protocol.readUInt(bytes, offset);
            offset += Constants.BIT32SZ;
            Stat = Protocol.readStat(bytes, offset);
            offset += Stat.Size;
            if (offset < Length)
            {
                throw new InsufficientDataException(Length, offset);
            }
        }

        public uint Fid { get; set; }
        public Stat Stat { get; set; }

        public override byte[] ToBytes()
        {
            var bytes = new byte[Length];
            var offset = Protocol.writeUint(bytes, Length, 0);
            bytes[offset] = Type;
            offset += Constants.BIT8SZ;
            offset += Protocol.writeUshort(bytes, Tag, offset);

            offset += Protocol.writeUint(bytes, Fid, offset);
            offset += Protocol.writeStat(bytes, Stat, offset);

            if (offset < Length)
            {
                throw new InsufficientDataException(Length, offset);
            }
            return bytes;
        }

        protected bool Equals(Twstat other)
        {
            return base.Equals(other) && Fid == other.Fid && Equals(Stat, other.Stat);
        }

        public override bool Equals(object obj)
        {
            if (ReferenceEquals(null, obj)) return false;
            if (ReferenceEquals(this, obj)) return true;
            return obj.GetType() == GetType() && Equals((Twstat) obj);
        }

        public override int GetHashCode()
        {
            unchecked
            {
                var hashCode = base.GetHashCode();
                hashCode = (hashCode*397) ^ (int) Fid;
                hashCode = (hashCode*397) ^ (Stat?.GetHashCode() ?? 0);
                return hashCode;
            }
        }
    }
}