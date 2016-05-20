using System;

namespace Win9P.Protocol
{
    public sealed class Tflush : Message
    {
        public ushort Oldtag { get; set; }

        public Tflush(ushort oldTag)
        {
            Type = (byte) MessageType.Tflush;
            Oldtag = oldTag;
            Length += Protocol.BIT16SZ;
        }

        public Tflush(byte[] bytes) : base(bytes)
        {
            var offset = Protocol.HeaderOffset;
            Oldtag = Protocol.readUShort(bytes, offset);
            offset += Protocol.BIT16SZ;
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
            offset += Protocol.writeUshort(bytes, Oldtag, offset);
            if (offset < Length)
            {
                throw new Exception($"Buffer underflow. Len: {Length}, Offset: {offset}");
            }
            return bytes;
        }

        private bool Equals(Tflush other)
        {
            return base.Equals(other) && Oldtag == other.Oldtag;
        }

        public override bool Equals(object obj)
        {
            if (ReferenceEquals(null, obj)) return false;
            if (ReferenceEquals(this, obj)) return true;
            return obj is Tflush && Equals((Tflush) obj);
        }

        public override int GetHashCode()
        {
            unchecked
            {
                return (base.GetHashCode()*397) ^ Oldtag.GetHashCode();
            }
        }
    }
}