using System;

namespace Win9P.Protocol
{
    public class Rstat : Message
    {
        public Stat Stat { get; set; }

        public Rstat(Stat stat)
        {
            Type = (byte) MessageType.Rstat;
            Stat = stat;
            Length += Stat.Size;
        }

        public Rstat(byte[] bytes) : base(bytes)
        {
            var offset = Protocol.HeaderOffset;
            Stat = Protocol.readStat(bytes, offset);
            offset += Stat.Size;
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

            offset += Protocol.writeStat(bytes, Stat, offset);

            if (offset < Length)
            {
                throw new Exception($"Buffer underflow. Len: {Length}, Offset: {offset}");
            }
            return bytes;
        }

        protected bool Equals(Rstat other)
        {
            return base.Equals(other) && Equals(Stat, other.Stat);
        }

        public override bool Equals(object obj)
        {
            if (ReferenceEquals(null, obj)) return false;
            if (ReferenceEquals(this, obj)) return true;
            if (obj.GetType() != this.GetType()) return false;
            return Equals((Rstat) obj);
        }

        public override int GetHashCode()
        {
            unchecked
            {
                return (base.GetHashCode()*397) ^ (Stat != null ? Stat.GetHashCode() : 0);
            }
        }
    }
}