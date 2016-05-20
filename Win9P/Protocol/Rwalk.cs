using System;

namespace Win9P.Protocol
{
    public sealed class Rwalk : Message
    {
        public ushort Nwqid { get; set; }
        public Qid[] Wqid { get; set; }

        public Rwalk(ushort nwqid, Qid[] wqid)
        {
            Type = (byte) MessageType.Rwalk;
            Nwqid = nwqid;
            Wqid = wqid;
            Length += Protocol.BIT16SZ + (Nwqid * (uint)Protocol.QIDSZ);
        }

        public Rwalk(byte[] bytes) : base(bytes)
        {
            var offset = Protocol.HeaderOffset;
            Nwqid = Protocol.readUShort(bytes, offset);
            offset += Protocol.BIT16SZ;
            Wqid = new Qid[Nwqid];
            for (var i = 0; i < Nwqid; i++)
            {
                Wqid[i] = Protocol.readQid(bytes, offset);
                offset += Protocol.QIDSZ;
            }
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
            offset += Protocol.writeUshort(bytes, Nwqid, offset);
            foreach (var qid in Wqid)
            {
                offset += Protocol.writeQid(bytes, qid, offset);
            }
            if (offset < Length)
            {
                throw new Exception($"Buffer underflow. Len: {Length}, Offset: {offset}");
            }
            return bytes;
        }
    }
}