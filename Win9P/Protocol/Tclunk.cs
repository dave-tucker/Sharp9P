﻿using System;

namespace Win9P.Protocol
{
    public class Tclunk : Message
    {
        public uint Fid { get; set; }

        public Tclunk(uint fid)
        {
            Type = (byte) MessageType.Tclunk;
            Fid = fid;
            Length += Protocol.BIT32SZ;
        }

        public Tclunk(byte[] bytes) : base(bytes)
        {
            var offset = Protocol.HeaderOffset;
            Fid = Protocol.readUInt(bytes, offset);
            offset += Protocol.BIT32SZ;
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
            if (offset < Length)
            {
                throw new Exception($"Buffer underflow. Len: {Length}, Offset: {offset}");
            }
            return bytes;
        }

        protected bool Equals(Tclunk other)
        {
            return base.Equals(other) && Fid == other.Fid;
        }

        public override bool Equals(object obj)
        {
            if (ReferenceEquals(null, obj)) return false;
            if (ReferenceEquals(this, obj)) return true;
            return obj.GetType() == GetType() && Equals((Tclunk) obj);
        }

        public override int GetHashCode()
        {
            unchecked
            {
                return (base.GetHashCode()*397) ^ (int) Fid;
            }
        }
    }
}