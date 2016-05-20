using System;

namespace Win9P.Protocol
{
    public class Rclunk : Message
    {
        public Rclunk()
        {
            Type = (byte)MessageType.Rclunk;
        }

        public Rclunk(byte[] bytes) : base(bytes)
        {
            
        }
    }
}