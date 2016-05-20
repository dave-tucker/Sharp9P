using System;

namespace Win9P.Protocol
{
    public class Rwstat : Message
    {
        public Rwstat()
        {
            Type = (byte) MessageType.Rwstat;
        }

        public Rwstat(byte[] bytes) : base(bytes)
        {
            
        }
    }
}