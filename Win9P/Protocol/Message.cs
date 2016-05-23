using System;

namespace Win9P.Protocol
{
    public abstract class Message
    {
        protected Message(byte[] bytes)
        {
            Length = Protocol.readUInt(bytes, 0);
            var offset = Constants.BIT32SZ;

            Type = bytes[offset];
            offset += Constants.BIT8SZ;

            Tag = Protocol.readUShort(bytes, offset);
            offset += Constants.BIT16SZ;
        }

        protected Message()
        {
            Length = Constants.BIT32SZ + Constants.BIT8SZ + Constants.BIT16SZ;
        }

        public uint Length { get; protected set; }
        public byte Type { get; protected set; }
        public ushort Tag { get; set; }

        public virtual byte[] ToBytes()
        {
            var bytes = new byte[Length];
            var offset = Protocol.writeUint(bytes, Length, 0);

            bytes[offset] = Type;
            offset += Constants.BIT8SZ;

            offset += Protocol.writeUshort(bytes, Tag, offset);

            if (offset < Length)
            {
                throw new Exception($"Buffer underflow. Len: {Length}, Offset: {offset}");
            }
            return bytes;
        }

        protected bool Equals(Message other)
        {
            return Length == other.Length && Type == other.Type && Tag == other.Tag;
        }

        public override bool Equals(object obj)
        {
            if (ReferenceEquals(null, obj)) return false;
            if (ReferenceEquals(this, obj)) return true;
            return obj.GetType() == GetType() && Equals((Message) obj);
        }

        public override int GetHashCode()
        {
            unchecked
            {
                var hashCode = (int) Length;
                hashCode = (hashCode*397) ^ Type.GetHashCode();
                hashCode = (hashCode*397) ^ Tag.GetHashCode();
                return hashCode;
            }
        }
    }
}