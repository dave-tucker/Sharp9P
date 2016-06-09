using System;
using System.Text;
using System.Threading.Tasks;
using NUnit.Framework;
using Sharp9P.Protocol;
using Sharp9P.Protocol.Messages;

namespace Sharp9P.Test
{
    [TestFixture]
    public class ProtocolTest
    {

        private Protocol.Protocol _protocol;
        private ushort _tag;

        [OneTimeSetUp]
        public void SetUpFixture()
        {
            var stream = new TestMemoryStream();
            _protocol = new Protocol.Protocol(stream);
            _protocol.Start();
            _tag = 1234;
        }

        [OneTimeTearDown]
        public void TearDownFixture()
        {
            _protocol.Stop();
        }

        [TearDown]
        public void TearDown()
        {
            _tag++;
        }

        [Test]
        public void TestReadWriteQid()
        {
            var qid = new Qid((byte)QidType.QtFile, 1, 0x111L);
            var bytes = qid.ToBytes();
            var qid2 = new Qid(bytes);
            Assert.That(qid, Is.EqualTo(qid2));
        }

        [Test]
        public async Task TestReadWriteRattach()
        {
            var qid = new Qid((byte)QidType.QtFile, 1, 0x111L);
            var message = new Rattach(qid)
            {
                Tag = _tag
            };

            _protocol.Send(message);
            var message2 = (Rattach)await _protocol.Receive(_tag);
            Assert.That(message, Is.EqualTo(message2));
        }

        [Test]
        public async Task TestReadWriteRauth()
        {
            var qid = new Qid((byte)QidType.QtFile, 1, 0x111L);
            var message = new Rauth(qid)
            {
                Tag = _tag
            };

            _protocol.Send(message);
            var message2 = (Rauth)await _protocol.Receive(_tag);
            Assert.That(qid, Is.EqualTo(message2.Aqid));
        }

        [Test]
        public async Task TestReadWriteRclunk()
        {
            var message = new Rclunk
            {
                Tag = _tag
            };

            _protocol.Send(message);
            var message2 = (Rclunk)await _protocol.Receive(_tag);
            Assert.That(message, Is.EqualTo(message2));
        }

        [Test]
        public async Task TestReadWriteRcreate()
        {
            var qid = new Qid((byte)QidType.QtFile, 1, 0x111L);
            var message = new Rcreate(qid, 1)
            {
                Tag = _tag
            };

            _protocol.Send(message);
            var message2 = (Rcreate)await _protocol.Receive(_tag);
            Assert.That(message, Is.EqualTo(message2));
        }

        [Test]
        public async Task TestReadWriteRerror()
        {
            var message = new Rerror("ETEST")
            {
                Tag = _tag
            };

            _protocol.Send(message);
            var message2 = (Rerror)await _protocol.Receive(_tag);
            Assert.That(message, Is.EqualTo(message2));
        }

        [Test]
        public async Task TestReadWriteRflush()
        {
            var message = new Rflush
            {
                Tag = _tag
            };

            _protocol.Send(message);
            var message2 = (Rflush)await _protocol.Receive(_tag);
            Assert.That(message, Is.EqualTo(message2));
        }

        [Test]
        public async Task TestReadWriteRopen()
        {
            var qid = new Qid((byte)QidType.QtFile, 1, 0x111L);
            var message = new Ropen(qid, 1)
            {
                Tag = _tag
            };

            _protocol.Send(message);
            var message2 = (Ropen)await _protocol.Receive(_tag);
            Assert.That(message, Is.EqualTo(message2));
        }

        [Test]
        public async Task TestReadWriteRread()
        {
            var utf8 = new UTF8Encoding();

            var message = new Rread((uint)utf8.GetByteCount("test"),
                utf8.GetBytes("test"))
            {
                Tag = _tag
            };

            _protocol.Send(message);
            var message2 = (Rread)await _protocol.Receive(_tag);
            Assert.That(message, Is.EqualTo(message2));
        }

        [Test]
        public async Task TestReadWriteRremove()
        {
            var message = new Rremove
            {
                Tag = _tag
            };

            _protocol.Send(message);
            var message2 = (Rremove)await _protocol.Receive(_tag);
            Assert.That(message, Is.EqualTo(message2));
        }

        [Test]
        public async Task TestReadWriteRstat()
        {
            var qid = new Qid((byte)QidType.QtFile, 1, 0x111L);
            var stat = new Stat(
                1, 2, qid, 4, 5, 6, 65535L, "foo", "root", "root", "root");
            var message = new Rstat(stat)
            {
                Tag = _tag
            };

            _protocol.Send(message);
            var message2 = (Rstat)await _protocol.Receive(_tag);
            Assert.That(message, Is.EqualTo(message2));
        }

        [Test]
        public async Task TestReadWriteRversion()
        {
            var message = new Rversion(16384, "9P2000")
            {
                Tag = _tag
            };

            _protocol.Send(message);
            var message2 = (Rversion)await _protocol.Receive(_tag);
            Assert.That(message2, Is.EqualTo(message));
        }

        [Test]
        public async Task TestReadWriteRwalk()
        {
            var qid = new Qid((byte)QidType.QtFile, 1, 0x111L);
            var message = new Rwalk(1, new[] { qid })
            {
                Tag = _tag
            };

            _protocol.Send(message);
            var message2 = (Rwalk)await _protocol.Receive(_tag);
            Assert.That(message, Is.EqualTo(message2));
        }

        [Test]
        public async Task TestReadWriteRwrite()
        {
            var message = new Rwrite(8192)
            {
                Tag = _tag
            };

            _protocol.Send(message);
            var message2 = (Rwrite)await _protocol.Receive(_tag);
            Assert.That(message, Is.EqualTo(message2));
        }

        [Test]
        public async Task TestReadWriteRwstat()
        {
            var message = new Rwstat
            {
                Tag = _tag
            };

            _protocol.Send(message);
            var message2 = (Rwstat)await _protocol.Receive(_tag);
            Assert.That(message, Is.EqualTo(message2));
        }

        [Test]
        public async Task TestReadWriteTattach()
        {
            var message = new Tattach(1, 2, "uname", "aname")
            {
                Tag = _tag
            };

            _protocol.Send(message);
            var message2 = (Tattach)await _protocol.Receive(_tag);
            Assert.That(message, Is.EqualTo(message2));
        }

        [Test]
        public async Task TestReadWriteTauth()
        {
            var message = new Tauth(Constants.NoFid, "user", "tree")
            {
                Tag = _tag
            };

            _protocol.Send(message);
            var message2 = (Tauth)await _protocol.Receive(_tag);
            Assert.That(message2, Is.EqualTo(message));
        }

        [Test]
        public async Task TestReadWriteTclunk()
        {
            var message = new Tclunk(1)
            {
                Tag = _tag
            };

            _protocol.Send(message);
            var message2 = (Tclunk)await _protocol.Receive(_tag);
            Assert.That(message, Is.EqualTo(message2));
        }

        [Test]
        public async Task TestReadWriteTcreate()
        {
            var message = new Tcreate(1, "test", 1, 1)
            {
                Tag = _tag
            };

            _protocol.Send(message);
            var message2 = (Tcreate)await _protocol.Receive(_tag);
            Console.WriteLine(message.ToString());
            Console.WriteLine(message2.ToString());
            Assert.That(message, Is.EqualTo(message2));
        }

        [Test]
        public async Task TestReadWriteTflush()
        {
            var message = new Tflush(1234)
            {
                Tag = _tag
            };

            _protocol.Send(message);
            var message2 = (Tflush)await _protocol.Receive(_tag);
            Assert.That(message, Is.EqualTo(message2));
        }

        [Test]
        public async Task TestReadWriteTopen()
        {
            var message = new Topen(1, 1)
            {
                Tag = _tag
            };

            _protocol.Send(message);
            var message2 = (Topen)await _protocol.Receive(_tag);
            Assert.That(message, Is.EqualTo(message2));
        }

        [Test]
        public async Task TestReadWriteTread()
        {
            var message = new Tread(1, 65535L, 1)
            {
                Tag = _tag
            };

            _protocol.Send(message);
            var message2 = (Tread)await _protocol.Receive(_tag);
            Assert.That(message, Is.EqualTo(message2));
        }

        [Test]
        public async Task TestReadWriteTremove()
        {
            var message = new Tremove(1)
            {
                Tag = _tag
            };

            _protocol.Send(message);
            var message2 = (Tremove)await _protocol.Receive(_tag);
            Assert.That(message, Is.EqualTo(message2));
        }

        [Test]
        public async Task TestReadWriteTstat()
        {
            var message = new Tstat(1)
            {
                Tag = _tag
            };

            _protocol.Send(message);
            var message2 = (Tstat)await _protocol.Receive(_tag);
            Assert.That(message, Is.EqualTo(message2));
        }

        [Test]
        public async Task TestReadWriteTversion()
        {
            var message = new Tversion(16384, "9P2000")
            {
                Tag = _tag
            };

            _protocol.Send(message);
            var message2 = (Tversion)await _protocol.Receive(_tag);
            Assert.That(message2, Is.EqualTo(message));
        }

        [Test]
        public async Task TestReadWriteTwalk()
        {
            var message = new Twalk(3, 4, 2, new[] { "hello", "world" })
            {
                Tag = _tag
            };

            _protocol.Send(message);
            var message2 = (Twalk)await _protocol.Receive(_tag);
            Assert.That(message, Is.EqualTo(message2));
        }

        [Test]
        public async Task TestReadWriteTwrite()
        {
            var utf8 = new UTF8Encoding();

            var message = new Twrite(
                1, 65535L,
                (uint)utf8.GetByteCount("test"),
                utf8.GetBytes("test"))
            {
                Tag = _tag
            };

            _protocol.Send(message);
            var message2 = (Twrite)await _protocol.Receive(_tag);
            Assert.That(message, Is.EqualTo(message2));
        }

        [Test]
        public async Task TestReadWriteTwstat()
        {
            var qid = new Qid((byte)QidType.QtFile, 1, 0x111L);
            var stat = new Stat(
                1, 2, qid, 4, 5, 6, 65535L, "foo", "root", "root", "root");
            var message = new Twstat(1, stat)
            {
                Tag = _tag
            };

            _protocol.Send(message);
            var message2 = (Twstat)await _protocol.Receive(_tag);
            Assert.That(message, Is.EqualTo(message2));
        }
    }
}