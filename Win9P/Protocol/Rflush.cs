namespace Win9P.Protocol
{
    public class Rflush : Message
    {
        public Rflush()
        {
            Type = (byte) MessageType.Rflush;
        }
        public Rflush(byte[] bytes) : base(bytes)
        {
            
        }
    }
}