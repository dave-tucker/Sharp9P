using System;
using System.Runtime.InteropServices;
using System.Runtime.Remoting.Messaging;

namespace Win9P.Protocol
{
    public abstract class Message
    {
        public uint Length { get; protected set; }
        public byte Type { get; protected set; }
        public ushort Tag { get; set; }

        protected Message(byte[] bytes)
        {
            Length = Protocol.readUInt(bytes, 0);
            var offset = Protocol.BIT32SZ;

            Type = bytes[offset];
            offset += Protocol.BIT8SZ;

            Tag = Protocol.readUShort(bytes, offset);
            offset += Protocol.BIT16SZ;
        }
        protected Message()
        {
            Length = Protocol.BIT32SZ + Protocol.BIT8SZ + Protocol.BIT16SZ;
        }

        public virtual byte[] ToBytes()
        {
            var bytes = new byte[Length];
            var offset = Protocol.writeUint(bytes, Length, 0);

            bytes[offset] = Type;
            offset += Protocol.BIT8SZ;

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