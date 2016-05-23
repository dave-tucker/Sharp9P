using System;

namespace Win9P.Protocol
{
    public class Qid
    {
        public readonly ulong Path;
        public readonly byte Type;
        public readonly uint Vers;

        public Qid(byte type, uint vers, ulong path)
        {
            Type = type;
            Vers = vers;
            Path = path;
        }

        public Qid(byte[] bytes)
        {
            var offset = 0;
            Type = bytes[offset];
            offset += Constants.BIT8SZ;
            Vers = Protocol.readUInt(bytes, offset);
            offset += Constants.BIT32SZ;
            Path = Protocol.readULong(bytes, offset);
        }

        public byte[] ToBytes()
        {
            var bytes = new byte[Constants.QIDSZ];
            var offset = 0;
            bytes[offset] = Type;
            offset += Constants.BIT8SZ;
            offset += Protocol.writeUint(bytes, Vers, offset);
            offset += Protocol.writeUlong(bytes, Path, offset);

            if (offset < Constants.QIDSZ)
            {
                throw new Exception($"Buffer underflow. Len: {Constants.QIDSZ}, Offset: {offset}");
            }
            return bytes;
        }

        protected bool Equals(Qid other)
        {
            return Type == other.Type && Vers == other.Vers && Path == other.Path;
        }

        public override bool Equals(object obj)
        {
            if (ReferenceEquals(null, obj)) return false;
            if (ReferenceEquals(this, obj)) return true;
            return obj.GetType() == GetType() && Equals((Qid) obj);
        }

        public override int GetHashCode()
        {
            unchecked
            {
                var hashCode = Type.GetHashCode();
                hashCode = (hashCode*397) ^ (int) Vers;
                hashCode = (hashCode*397) ^ Path.GetHashCode();
                return hashCode;
            }
        }
    }
}