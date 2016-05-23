using System;
using System.Linq;
using Win9P.Exceptions;

namespace Win9P.Protocol.Messages
{
    public sealed class Twalk : Message
    {
        public Twalk(uint fid, uint newFid, ushort nwname, string[] wname)
        {
            Type = (byte) MessageType.Twalk;
            Fid = fid;
            NewFid = newFid;
            Nwname = nwname;
            Wname = wname;
            Length += Constants.BIT32SZ + Constants.BIT32SZ +
                      Constants.BIT16SZ;
            foreach (var name in wname)
            {
                Length += Protocol.GetStringLength(name);
            }
        }

        public Twalk(byte[] bytes) : base(bytes)
        {
            var offset = Constants.HeaderOffset;
            Fid = Protocol.readUInt(bytes, offset);
            offset += Constants.BIT32SZ;
            NewFid = Protocol.readUInt(bytes, offset);
            offset += Constants.BIT32SZ;
            Nwname += Protocol.readUShort(bytes, offset);
            offset += Constants.BIT16SZ;
            Wname = new string[Nwname];
            for (var i = 0; i < Nwname; i++)
            {
                Wname[i] = Protocol.readString(bytes, offset);
                offset += (int) Protocol.GetStringLength(Wname[i]);
            }
            if (offset < Length)
            {
                throw new InsufficientDataException(Length, offset);
            }
        }

        public uint Fid { get; set; }
        public uint NewFid { get; set; }
        public ushort Nwname { get; set; }
        public string[] Wname { get; set; }

        public override byte[] ToBytes()
        {
            var bytes = new byte[Length];
            var offset = Protocol.writeUint(bytes, Length, 0);
            bytes[offset] = Type;
            offset += Constants.BIT8SZ;
            offset += Protocol.writeUshort(bytes, Tag, offset);
            offset += Protocol.writeUint(bytes, Fid, offset);
            offset += Protocol.writeUint(bytes, NewFid, offset);
            offset += Protocol.writeUshort(bytes, Nwname, offset);
            offset = Wname.Aggregate(offset, (current, name) => current + Protocol.writeString(bytes, name, current));
            if (offset < Length)
            {
                throw new InsufficientDataException(Length, offset);
            }
            return bytes;
        }

        private bool Equals(Twalk other)
        {
            return base.Equals(other) && Fid == other.Fid && NewFid == other.NewFid && Nwname == other.Nwname &&
                   Wname.SequenceEqual(other.Wname);
        }

        public override bool Equals(object obj)
        {
            if (ReferenceEquals(null, obj)) return false;
            if (ReferenceEquals(this, obj)) return true;
            return obj is Twalk && Equals((Twalk) obj);
        }

        public override int GetHashCode()
        {
            unchecked
            {
                var hashCode = base.GetHashCode();
                hashCode = (hashCode*397) ^ (int) Fid;
                hashCode = (hashCode*397) ^ (int) NewFid;
                hashCode = (hashCode*397) ^ Nwname.GetHashCode();
                hashCode = (hashCode*397) ^ (Wname?.GetHashCode() ?? 0);
                return hashCode;
            }
        }
    }
}