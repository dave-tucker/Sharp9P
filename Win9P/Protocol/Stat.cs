using System;

namespace Win9P.Protocol
{
    // http://man.cat-v.org/plan_9/5/stat
    public class Stat
    {
        public ushort Size { get; set; }
        public ushort Type { get; set; }
        public uint Dev { get; set; }
        public Qid Qid { get; set; }
        public uint Mode { get; set; }
        public uint Atime { get; set; }
        public uint Mtime { get; set; }
        public ulong Length { get; set; }
        public string Name { get; set; }
        public string Uid { get; set; }
        public string Gid { get; set; }
        public string Muid { get; set; }

        public Stat(
            ushort type,
            uint dev,
            Qid qid,
            uint mode,
            uint atime,
            uint mtime,
            ulong length,
            string name,
            string uid,
            string gid,
            string muid)
        {
            Type = type;
            Dev = dev;
            Qid = qid;
            Mode = mode;
            Atime = atime;
            Mtime = mtime;
            Length = length;
            Name = name;
            Uid = uid;
            Gid = gid;
            Muid = muid;
            Size = (ushort) (Protocol.BIT16SZ + Protocol.BIT16SZ +
                Protocol.BIT32SZ + Protocol.QIDSZ +
                Protocol.BIT32SZ + Protocol.BIT32SZ +
                Protocol.BIT32SZ + Protocol.BIT64SZ +
                Protocol.GetStringLength(Name) +
                Protocol.GetStringLength(Uid) +
                Protocol.GetStringLength(Gid) +
                Protocol.GetStringLength(Muid));
        }

        public Stat(byte[] bytes)
        {
            var offset = 0;
            Size = Protocol.readUShort(bytes, offset);
            offset += Protocol.BIT16SZ;
            Type = Protocol.readUShort(bytes, offset);
            offset += Protocol.BIT16SZ;
            Dev = Protocol.readUInt(bytes, offset);
            offset += Protocol.BIT32SZ;
            Qid = Protocol.readQid(bytes, offset);
            offset += Protocol.QIDSZ;
            Mode = Protocol.readUInt(bytes, offset);
            offset += Protocol.BIT32SZ;
            Atime = Protocol.readUInt(bytes, offset);
            offset += Protocol.BIT32SZ;
            Mtime = Protocol.readUInt(bytes, offset);
            offset += Protocol.BIT32SZ;
            Length = Protocol.readULong(bytes, offset);
            offset += Protocol.BIT64SZ;
            Name = Protocol.readString(bytes, offset);
            offset += (int)Protocol.GetStringLength(Name);
            Uid = Protocol.readString(bytes, offset);
            offset += (int)Protocol.GetStringLength(Uid);
            Gid = Protocol.readString(bytes, offset);
            offset += (int)Protocol.GetStringLength(Gid);
            Muid = Protocol.readString(bytes, offset);
            offset += (int)Protocol.GetStringLength(Muid);
            if (offset < Size)
            {
                throw new Exception("Too much data");
            }
        }

        public byte[] ToBytes()
        {
            var bytes = new byte[Size];
            var offset = 0;

            offset += Protocol.writeUshort(bytes, Size, offset);
            offset += Protocol.writeUshort(bytes, Type, offset);
            offset += Protocol.writeUint(bytes, Dev, offset);
            offset += Protocol.writeQid(bytes, Qid, offset);
            offset += Protocol.writeUint(bytes, Mode, offset);
            offset += Protocol.writeUint(bytes, Atime, offset);
            offset += Protocol.writeUint(bytes, Mtime, offset);
            offset += Protocol.writeUlong(bytes, Length, offset);
            offset += Protocol.writeString(bytes, Name, offset);
            offset += Protocol.writeString(bytes, Uid, offset);
            offset += Protocol.writeString(bytes, Gid, offset);
            offset += Protocol.writeString(bytes, Muid, offset);
            
            if (offset < Size)
            {
                throw new Exception($"Buffer underflow. Len: {Size}, Offset: {offset}");
            }
            return bytes;
        }

        protected bool Equals(Stat other)
        {
            return Size == other.Size && Type == other.Type && Dev == other.Dev && Equals(Qid, other.Qid) && Mode == other.Mode && Atime == other.Atime && Mtime == other.Mtime && Length == other.Length && string.Equals(Name, other.Name) && string.Equals(Uid, other.Uid) && string.Equals(Gid, other.Gid) && string.Equals(Muid, other.Muid);
        }

        public override bool Equals(object obj)
        {
            if (ReferenceEquals(null, obj)) return false;
            if (ReferenceEquals(this, obj)) return true;
            if (obj.GetType() != this.GetType()) return false;
            return Equals((Stat) obj);
        }

        public override int GetHashCode()
        {
            unchecked
            {
                var hashCode = Size.GetHashCode();
                hashCode = (hashCode*397) ^ Type.GetHashCode();
                hashCode = (hashCode*397) ^ (int) Dev;
                hashCode = (hashCode*397) ^ (Qid != null ? Qid.GetHashCode() : 0);
                hashCode = (hashCode*397) ^ (int) Mode;
                hashCode = (hashCode*397) ^ (int) Atime;
                hashCode = (hashCode*397) ^ (int) Mtime;
                hashCode = (hashCode*397) ^ Length.GetHashCode();
                hashCode = (hashCode*397) ^ (Name != null ? Name.GetHashCode() : 0);
                hashCode = (hashCode*397) ^ (Uid != null ? Uid.GetHashCode() : 0);
                hashCode = (hashCode*397) ^ (Gid != null ? Gid.GetHashCode() : 0);
                hashCode = (hashCode*397) ^ (Muid != null ? Muid.GetHashCode() : 0);
                return hashCode;
            }
        }
    }
}
