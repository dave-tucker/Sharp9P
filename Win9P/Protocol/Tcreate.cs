using System;

namespace Win9P.Protocol
{
    public sealed class Tcreate : Message
    {
        public uint Fid { get; set; }

        public string Name { get; set; }
        public uint Perm { get; set; }
        public byte Mode { get; set; }

        public Tcreate(uint fid, string name, uint perm, byte mode)
        {
            Type = (byte) MessageType.Tcreate;
            Fid = fid;
            Name = name;
            Perm = perm;
            Mode = mode;
            Length += Protocol.BIT32SZ + Protocol.GetStringLength(Name) + Protocol.BIT32SZ + Protocol.BIT8SZ;
        }

        public Tcreate(byte[] bytes) : base(bytes)
        {
            var offset = Protocol.HeaderOffset;
            Fid = Protocol.readUInt(bytes, offset);
            offset += Protocol.BIT32SZ;
            Name = Protocol.readString(bytes, offset);
            offset += (int)Protocol.GetStringLength(Name);
            Perm = Protocol.readUInt(bytes, offset);
            offset += Protocol.BIT32SZ;
            Mode = bytes[offset];
            offset += Protocol.BIT8SZ;
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

            offset += Protocol.writeUint(bytes, Fid, offset);
            offset += Protocol.writeString(bytes, Name, offset);
            offset += Protocol.writeUint(bytes, Perm, offset);
            bytes[offset] = Mode;
            offset += Protocol.BIT8SZ;

            if (offset < Length)
            {
                throw new Exception($"Buffer underflow. Len: {Length}, Offset: {offset}");
            }
            return bytes;
        }

        private bool Equals(Tcreate other)
        {
            return base.Equals(other) && Fid == other.Fid && Name == other.Name && Perm == other.Perm && Mode == other.Mode;
        }

        public override bool Equals(object obj)
        {
            if (ReferenceEquals(null, obj)) return false;
            if (ReferenceEquals(this, obj)) return true;
            return obj is Tcreate && Equals((Tcreate) obj);
        }

        public override int GetHashCode()
        {
            unchecked
            {
                int hashCode = base.GetHashCode();
                hashCode = (hashCode*397) ^ (int) Fid;
                hashCode = (hashCode*397) ^ Name.GetHashCode();
                hashCode = (hashCode*397) ^ (int) Perm;
                hashCode = (hashCode*397) ^ Mode.GetHashCode();
                return hashCode;
            }
        }
    }
}