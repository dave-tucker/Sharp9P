using System;

namespace Win9P.Protocol
{
    public class Rremove : Message
    {
        public Rremove()
        {
            Type = (byte) MessageType.Rremove;
        }

        public Rremove(byte[] bytes) : base(bytes)
        {
            
        }
    }
}