using System;
using System.Collections.Concurrent;
using System.IO;
using System.Text;
using System.Threading;
using System.Threading.Tasks;
using Sharp9P.Protocol.Messages;

namespace Sharp9P.Protocol
{
    public interface IProtocol
    {
        uint Msize { get; set; }
        void Send(Message message);
        Task<Message> Receive(ushort tag);
        void Start();
        void Stop();
    }

    public class Protocol : IProtocol
    {
        private readonly ConcurrentDictionary<uint, Task> _rxCallbacks;
        private readonly ConcurrentDictionary<uint, Message> _rxQueue;
        private readonly Stream _stream;
        private readonly ConcurrentQueue<Message> _txQueue;
        private readonly CancellationTokenSource _tokenSource;

        public Protocol(Stream stream)
        {
            _stream = stream;
            _rxCallbacks = new ConcurrentDictionary<uint, Task>();
            _rxQueue = new ConcurrentDictionary<uint, Message>();
            _txQueue = new ConcurrentQueue<Message>();
            _tokenSource = new CancellationTokenSource();
        }

        public uint Msize { get; set; } = Constants.DefaultMsize;

        public void Send(Message message)
        {
            _txQueue.Enqueue(message);
        }

        public async Task<Message> Receive(ushort tag)
        {
            var t = new Task<Message>(delegate
            {
                Message result;
                var ok = _rxQueue.TryRemove(tag, out result);
                return result;
            });
            _rxCallbacks.TryAdd(tag, t);
            return await t;
        }

        public void Start()
        {
            Task.Factory.StartNew(UpdateLoop, _tokenSource.Token, TaskCreationOptions.LongRunning, TaskScheduler.Default);
        }
    
        public void Stop()
        {
            _tokenSource.Cancel();
        }

        public async void UpdateLoop()
        {
            while (!_tokenSource.IsCancellationRequested)
            {

                if (_txQueue.Count <= 0) continue;
                Message outboundMsg;
                _txQueue.TryDequeue(out outboundMsg);
                Write(outboundMsg);

                await Read();

                // Process Callbacks
                foreach (var c in _rxCallbacks)
                {
                    if (!_rxQueue.ContainsKey(c.Key)) continue;
                    Task t;
                    var ok = _rxCallbacks.TryRemove(c.Key, out t);
                    if (!ok)
                    {
                        // ignore
                    }
                    t.Start();
                }
            }
        }
        
        private async Task<byte[]> ReadBytes(int n)
        {
            var data = new byte[n];
            var r = await _stream.ReadAsync(data, 0, n);
            if (r < n)
            {
                throw new Exception("Failed to read enough bytes");
            }
            return data;
        }

        /*
        private void Read()
        {
            var data = new byte[Constants.Bit32Sz];
            var callBack = new AsyncCallback(MessageCallback);
            _reading = true;
            _stream.BeginRead(data, 0, Constants.Bit32Sz, callBack, data);
        }
        */

        internal static uint ReadUInt(byte[] data, int offset)
        {
            return BitConverter.ToUInt32(data, offset);
        }

        internal static ulong ReadULong(byte[] data, int offset)
        {
            return BitConverter.ToUInt64(data, offset);
        }

        internal static ushort ReadUShort(byte[] data, int offset)
        {
            return BitConverter.ToUInt16(data, offset);
        }

        internal static string ReadString(byte[] data, int offset)
        {
            var utf8 = new UTF8Encoding();
            var len = ReadUShort(data, offset);
            offset += Constants.Bit16Sz;
            var strdata = new char[utf8.GetCharCount(data, offset, len)];
            utf8.GetChars(data, offset, len, strdata, 0);
            return new string(strdata);
        }

        internal static Qid ReadQid(byte[] bytes, int offset)
        {
            var b = new byte[Constants.Qidsz];
            Array.Copy(bytes, offset, b, 0, Constants.Qidsz);
            return new Qid(b);
        }

        internal static Stat ReadStat(byte[] bytes, int offset)
        {
            var length = ReadUShort(bytes, offset);
            var b = new byte[length];
            Array.Copy(bytes, offset, b, 0, length);
            return new Stat(b);
        }

        private async Task Read()
        {
            var length = await ReadBytes(Constants.Bit32Sz);
            // Read length uint
            var pktlen = ReadUInt(length, 0);
            if (pktlen - Constants.Bit32Sz > Msize)
                throw new Exception("Message too large!");

            // Read the remainder of the packet (minus the uint length)
            var data = await ReadBytes((int) pktlen - Constants.Bit32Sz);

            var pkt = new byte[pktlen];
            length.CopyTo(pkt, 0);
            data.CopyTo(pkt, Constants.Bit32Sz);
            var message = ProcessMessage(pkt);
            _rxQueue.TryAdd(message.Tag, message);
       }

        public Message ProcessMessage(byte[] bytes)
        {
            Message message;
            var offset = Constants.Bit32Sz;
            var type = bytes[offset];
            switch (type)
            {
                case (byte) MessageType.Tversion:
                    message = new Tversion(bytes);
                    break;
                case (byte) MessageType.Rversion:
                    message = new Rversion(bytes);
                    break;
                case (byte) MessageType.Tauth:
                    message = new Tauth(bytes);
                    break;
                case (byte) MessageType.Rauth:
                    message = new Rauth(bytes);
                    break;
                case (byte) MessageType.Tattach:
                    message = new Tattach(bytes);
                    break;
                case (byte) MessageType.Rattach:
                    message = new Rattach(bytes);
                    break;
                case (byte) MessageType.Rerror:
                    message = new Rerror(bytes);
                    break;
                case (byte) MessageType.Tflush:
                    message = new Tflush(bytes);
                    break;
                case (byte) MessageType.Rflush:
                    message = new Rflush(bytes);
                    break;
                case (byte) MessageType.Twalk:
                    message = new Twalk(bytes);
                    break;
                case (byte) MessageType.Rwalk:
                    message = new Rwalk(bytes);
                    break;
                case (byte) MessageType.Topen:
                    message = new Topen(bytes);
                    break;
                case (byte) MessageType.Ropen:
                    message = new Ropen(bytes);
                    break;
                case (byte) MessageType.Tcreate:
                    message = new Tcreate(bytes);
                    break;
                case (byte) MessageType.Rcreate:
                    message = new Rcreate(bytes);
                    break;
                case (byte) MessageType.Tread:
                    message = new Tread(bytes);
                    break;
                case (byte) MessageType.Rread:
                    message = new Rread(bytes);
                    break;
                case (byte) MessageType.Twrite:
                    message = new Twrite(bytes);
                    break;
                case (byte) MessageType.Rwrite:
                    message = new Rwrite(bytes);
                    break;
                case (byte) MessageType.Tclunk:
                    message = new Tclunk(bytes);
                    break;
                case (byte) MessageType.Rclunk:
                    message = new Rclunk(bytes);
                    break;
                case (byte) MessageType.Tremove:
                    message = new Tremove(bytes);
                    break;
                case (byte) MessageType.Rremove:
                    message = new Rremove(bytes);
                    break;
                case (byte) MessageType.Tstat:
                    message = new Tstat(bytes);
                    break;
                case (byte) MessageType.Rstat:
                    message = new Rstat(bytes);
                    break;
                case (byte) MessageType.Twstat:
                    message = new Twstat(bytes);
                    break;
                case (byte) MessageType.Rwstat:
                    message = new Rwstat(bytes);
                    break;
                default:
                    throw new Exception("Unsupported Message Type");
            }
            return message;
        }

        internal static int WriteUlong(byte[] data, ulong var, int offset)
        {
            var bytes = BitConverter.GetBytes(var);
            Array.Copy(bytes, 0, data, offset, bytes.Length);
            return Constants.Bit64Sz;
        }

        internal static int WriteUint(byte[] data, uint var, int offset)
        {
            var bytes = BitConverter.GetBytes(var);
            Array.Copy(bytes, 0, data, offset, bytes.Length);
            return Constants.Bit32Sz;
        }

        internal static int WriteUshort(byte[] data, ushort var, int offset)
        {
            var bytes = BitConverter.GetBytes(var);
            Array.Copy(bytes, 0, data, offset, bytes.Length);
            return Constants.Bit16Sz;
        }

        internal static int WriteString(byte[] data, string var, int offset)
        {
            var utf8 = new UTF8Encoding();

            WriteUshort(data, (ushort) var.Length, offset);
            offset += Constants.Bit16Sz;

            var bytes = utf8.GetBytes(var);
            Array.Copy(bytes, 0, data, offset, utf8.GetByteCount(var));
            return Constants.Bit16Sz + utf8.GetByteCount(var);
        }

        internal static int WriteQid(byte[] data, Qid qid, int offset)
        {
            var bytes = qid.ToBytes();
            Array.Copy(bytes, 0, data, offset, Constants.Qidsz);
            return Constants.Qidsz;
        }

        internal static int WriteStat(byte[] data, Stat stat, int offset)
        {
            var bytes = stat.ToBytes();
            Array.Copy(bytes, 0, data, offset, stat.Size);
            return stat.Size;
        }

        internal static uint GetStringLength(string var)
        {
            var utf8 = new UTF8Encoding();
            return (uint) (Constants.Bit16Sz + utf8.GetByteCount(var));
        }

        private void Write(Message message)
        {
            var bytes = message.ToBytes();
            /*
            var c = new AsyncCallback(delegate
            {
                _stream.FlushAsync();
            });
            _stream.BeginWrite(bytes, 0, bytes.Length, c, null);
            */
            _stream.Write(bytes, 0, bytes.Length);
            _stream.Flush();
        }
    }
}