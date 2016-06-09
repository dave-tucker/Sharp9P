using System.IO;

namespace Sharp9P.Test
{
    public class TestMemoryStream : MemoryStream
    {
        public override void Flush()
        {
            Position = 0;
        }
    }
}