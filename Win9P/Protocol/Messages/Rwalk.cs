using System;
using Win9P.Exceptions;

namespace Win9P.Protocol.Messages
{
    public sealed class Rwalk : Message
    {
        public Rwalk(ushort nwqid, Qid[] wqid)
        {
            Type = (byte) MessageType.Rwalk;
            Nwqid = nwqid;
            Wqid = wqid;
            Length += Constants.BIT16SZ + Nwqid*(uint) Constants.QIDSZ;
        }

        public Rwalk(byte[] bytes) : base(bytes)
        {
            var offset = Constants.HeaderOffset;
            Nwqid = Protocol.readUShort(bytes, offset);
            offset += Constants.BIT16SZ;
            Wqid = new Qid[Nwqid];
            for (var i = 0; i < Nwqid; i++)
            {
                Wqid[i] = Protocol.readQid(bytes, offset);
                offset += Constants.QIDSZ;
            }
            if (offset < Length)
            {
                throw new InsufficientDataException(Length, offset);
            }
        }

        public ushort Nwqid { get; set; }
        public Qid[] Wqid { get; set; }

        public override byte[] ToBytes()
        {
            var bytes = new byte[Length];
            var offset = Protocol.writeUint(bytes, Length, 0);
            bytes[offset] = Type;
            offset += Constants.BIT8SZ;
            offset += Protocol.writeUshort(bytes, Tag, offset);
            offset += Protocol.writeUshort(bytes, Nwqid, offset);
            foreach (var qid in Wqid)
            {
                offset += Protocol.writeQid(bytes, qid, offset);
            }
            if (offset < Length)
            {
                throw new InsufficientDataException(Length, offset);
            }
            return bytes;
        }
    }
}