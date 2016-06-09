using System;
using System.Collections.Generic;
using System.Threading.Tasks;
using Moq;
using NUnit.Framework;
using Sharp9P.Protocol;
using Sharp9P.Protocol.Messages;

namespace Sharp9P.Test
{
    [TestFixture]
    public class ClientTest
    {
        [Test]
        public async Task TestFidAllocation()
        {
            var protocol = new Mock<IProtocol>();
            Message nextMessage = null;

            protocol.Setup(p => p.Send(It.IsAny<Message>()))
                .Callback((Message m) => nextMessage = CreateReturnMessage(m));
            protocol.Setup(p => p.Receive(It.IsAny<ushort>()))
                .Returns(() => Task.FromResult(nextMessage));

            var c = new Client(protocol.Object);

            var fids = new List<uint>();

            for (var i = 0; i < 200; i++)
            {
                var fid = await c.AllocateFid(Constants.RootFid);
                Assert.That(!fids.Contains(fid));
                // Hold the Fid if it's divisible by 3
                if (fid % 3 == 0)
                {
                    fids.Add(fid);
                }
                else
                {
                    await c.FreeFid(fid);
                }
            }
        }

        private static Message CreateReturnMessage(Message m)
        {
            switch (m.Type)
            {
                case (byte)MessageType.Twalk:
                    var qid = new Qid((byte)QidType.QtFile, 1, 0x111L);
                    return new Rwalk(1, new[] { qid }) { Tag = m.Tag };
                case (byte)MessageType.Tclunk:
                    return new Rclunk { Tag = m.Tag };
                default:
                    throw new Exception("Unexpected Message");
            }
        }
    }
}