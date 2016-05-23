using System;
using System.Linq;
using Win9P.Exceptions;

namespace Win9P.Protocol.Messages
{
    public class Twrite : Message
    {
        public Twrite(uint fid, ulong offset, uint count, byte[] data)
        {
            Type = (byte) MessageType.Twrite;
            Fid = fid;
            Offset = offset;
            Count = count;
            Data = data;
            Length += Constants.BIT32SZ + Constants.BIT64SZ +
                      Constants.BIT32SZ + Count;
        }

        public Twrite(byte[] bytes) : base(bytes)
        {
            var offset = Constants.HeaderOffset;
            Fid = Protocol.readUInt(bytes, offset);
            offset += Constants.BIT32SZ;
            Offset = Protocol.readULong(bytes, offset);
            offset += Constants.BIT64SZ;
            Count = Protocol.readUInt(bytes, offset);
            offset += Constants.BIT32SZ;
            Data = new byte[Count];
            Array.Copy(bytes, offset, Data, 0, Count);
            offset += (int) Count;
            if (offset < Length)
            {
                throw new InsufficientDataException(Length, offset);
            }
        }

        public uint Fid { get; set; }
        public ulong Offset { get; set; }
        public uint Count { get; set; }
        public byte[] Data { get; set; }

        public override byte[] ToBytes()
        {
            var bytes = new byte[Length];
            var offset = Protocol.writeUint(bytes, Length, 0);
            bytes[offset] = Type;
            offset += Constants.BIT8SZ;
            offset += Protocol.writeUshort(bytes, Tag, offset);

            offset += Protocol.writeUint(bytes, Fid, offset);
            offset += Protocol.writeUlong(bytes, Offset, offset);
            offset += Protocol.writeUint(bytes, Count, offset);
            Array.Copy(Data, 0, bytes, offset, Count);
            offset += (int) Count;

            if (offset < Length)
            {
                throw new InsufficientDataException(Length, offset);
            }
            return bytes;
        }

        protected bool Equals(Twrite other)
        {
            return base.Equals(other) && Fid == other.Fid && Offset == other.Offset && Count == other.Count &&
                   Data.SequenceEqual(other.Data);
        }

        public override bool Equals(object obj)
        {
            if (ReferenceEquals(null, obj)) return false;
            if (ReferenceEquals(this, obj)) return true;
            return obj.GetType() == GetType() && Equals((Twrite) obj);
        }

        public override int GetHashCode()
        {
            unchecked
            {
                var hashCode = base.GetHashCode();
                hashCode = (hashCode*397) ^ (int) Fid;
                hashCode = (hashCode*397) ^ Offset.GetHashCode();
                hashCode = (hashCode*397) ^ (int) Count;
                hashCode = (hashCode*397) ^ (Data?.GetHashCode() ?? 0);
                return hashCode;
            }
        }
    }
}