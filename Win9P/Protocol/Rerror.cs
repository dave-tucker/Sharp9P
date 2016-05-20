using System;

namespace Win9P.Protocol
{
    public sealed class Rerror : Message
    {
        public string Ename { get; set; }

        public Rerror(string ename)
        {
            Type = (byte) MessageType.Rerror;
            Ename = ename;
            Length += Protocol.GetStringLength(Ename);
        }

        public Rerror(byte[] bytes) : base(bytes)
        {
            var offset = Protocol.HeaderOffset;
            Ename = Protocol.readString(bytes, offset);
            offset += (int)Protocol.GetStringLength(Ename);
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
            offset += Protocol.writeString(bytes, Ename, offset);
            if (offset < Length)
            {
                throw new Exception($"Buffer underflow. Len: {Length}, Offset: {offset}");
            }
            return bytes;
        }

        private bool Equals(Rerror other)
        {
            return base.Equals(other) && string.Equals(Ename, other.Ename);
        }

        public override bool Equals(object obj)
        {
            if (ReferenceEquals(null, obj)) return false;
            if (ReferenceEquals(this, obj)) return true;
            return obj is Rerror && Equals((Rerror) obj);
        }

        public override int GetHashCode()
        {
            unchecked
            {
                return (base.GetHashCode()*397) ^ (Ename?.GetHashCode() ?? 0);
            }
        }
    }
}