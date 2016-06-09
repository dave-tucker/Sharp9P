using System;
using System.Collections.Concurrent;
using System.IO;
using System.Threading.Tasks;
using Sharp9P.Exceptions;
using Sharp9P.Protocol;
using Sharp9P.Protocol.Messages;

namespace Sharp9P
{
    public class Client
    {
        private readonly ConcurrentQueue<uint> _fidQueue;
        private readonly IProtocol _protocol;
        private readonly ConcurrentQueue<ushort> _tagQueue;
        private uint _msize;
        private string _version;
       
        public Client(IProtocol protocol)
        {
            _msize = Constants.DefaultMsize;
            _version = Constants.DefaultVersion;
            _protocol = protocol;
            _tagQueue = new ConcurrentQueue<ushort>();
            for (ushort i = 1; i < 65535; i++)
            {
                _tagQueue.Enqueue(i);
            }
            _fidQueue = new ConcurrentQueue<uint>();
            for (uint i = 2; i < 500; i++)
            {
                _fidQueue.Enqueue(i);
            }
         }

        public void Start()
        {
            _protocol.Start();
        }

        public void Stop()
        {
            _protocol.Stop();
        }

        public static Client FromStream (Stream stream)
        {
            var p = new Protocol.Protocol(stream);
            return new Client(p);
        }

        public async Task<uint> AllocateFid(uint parent)
        {
            uint fid;
            var ok = _fidQueue.TryDequeue(out fid);
            if (!ok)
            {
                throw new Exception("Unable to dequeue a Fid");
            }
            await Walk(parent, fid, new string[0]);
            return fid;
        }

        private ushort AllocateTag()
        {
            ushort tag;
            var ok = _tagQueue.TryDequeue(out tag);
            if (!ok)
            {
                throw new Exception("Unable to dequeue a Tag");
            }
            return tag;
        }

        public async Task FreeFid(uint fid)
        {
            await Clunk(fid);
        }

        public async Task Version(uint msize, string version)
        {
            var request = new Tversion(_msize, _version);
            _protocol.Send(request);

            var r = await _protocol.Receive(Constants.NoTag);
            Rversion response;
            try
            {
                response = (Rversion) r;
            }
            catch (InvalidCastException)
            {
                if (r.Type != (byte) MessageType.Rerror) throw new UnexpectedMessageException(request.Type, r.Type);
                var err = (Rerror) r;
                throw new ServerErrorException(err.Ename);
            }
            /* The server responds with its own maxi-
            mum, msize, which must be less than or equal to the client's
            value */
            if (response.Msize > request.Msize)
            {
                throw new MsizeNegotiationException(request.Msize, response.Msize);
            }
            _msize = response.Msize;
            _protocol.Msize = _msize;
            if (response.Version != request.Version)
            {
                throw new UnsupportedVersionException(response.Version);
            }
            _version = response.Version;
        }

        public async Task<Qid> Attach(uint fid, uint afid, string uname, string aname)
        {
            var tag = AllocateTag();
            var request = new Tattach(fid, afid, uname, aname)
            {
                Tag = tag
            };
            _protocol.Send(request);
            var r = await _protocol.Receive(tag);
            Rattach response;
            try
            {
                response = (Rattach) r;
            }
            catch (InvalidCastException)
            {
                if (r.Type != (byte) MessageType.Rerror) throw new UnexpectedMessageException(request.Type, r.Type);
                var err = (Rerror) r;
                throw new ServerErrorException(err.Ename);
            }
            _tagQueue.Enqueue(request.Tag);
            return response.Qid;
        }

        public async Task<Qid> Auth(uint fid, string uname, string aname)
        {
            var tag = AllocateTag();
            var request = new Tauth(fid, uname, aname)
            {
                Tag = tag
            };
            _protocol.Send(request);
            var r = await _protocol.Receive(tag);
            Rauth response;
            try
            {
                response = (Rauth) r;
            }
            catch (InvalidCastException)
            {
                if (r.Type != (byte) MessageType.Rerror) throw new UnexpectedMessageException(request.Type, r.Type);
                var err = (Rerror) r;
                throw new ServerErrorException(err.Ename);
            }
            _tagQueue.Enqueue(request.Tag);
            return response.Aqid;
        }

        public async Task<Qid[]> Walk(uint fid, uint newFid, string[] nwnames)
        {
            var tag = AllocateTag();
            if (nwnames.Length > Constants.Maxwelem)
            {
                throw new Exception("No more thatn 16 elements allowed");
            }
            var request = new Twalk(fid, newFid, (ushort) nwnames.Length, nwnames)
            {
                Tag = tag
            };
            _protocol.Send(request);
            var r = await _protocol.Receive(tag);
            Rwalk response;
            try
            {
                response = (Rwalk) r;
            }
            catch (InvalidCastException)
            {
                if (r.Type != (byte) MessageType.Rerror) throw new UnexpectedMessageException(request.Type, r.Type);
                var err = (Rerror) r;
                throw new ServerErrorException(err.Ename);
            }
            _tagQueue.Enqueue(request.Tag);
            return response.Wqid;
        }

        public async Task Clunk(uint fid)
        {
            var tag = AllocateTag();
            var tcs = new TaskCompletionSource<object>();
            var request = new Tclunk(fid)
            {
                Tag = tag
            };
            _protocol.Send(request);
            var r = await _protocol.Receive(tag);
            Rclunk response;
            try
            {
                response = (Rclunk) r;
            }
            catch (InvalidCastException)
            {
                if (r.Type != (byte) MessageType.Rerror) throw new UnexpectedMessageException(request.Type, r.Type);
                var err = (Rerror) r;
                throw new ServerErrorException(err.Ename);
            }
            _fidQueue.Enqueue(fid);
            _tagQueue.Enqueue(request.Tag);
            tcs.SetResult(null);
        }

        public async Task<Tuple<Qid, uint>> Create(uint fid, string name, uint perm, byte mode)
        {
            var tag = AllocateTag();
            var request = new Tcreate(fid, name, perm, mode)
            {
                Tag = tag
            };
            _protocol.Send(request);
            var r = await _protocol.Receive(tag);
            Rcreate response;
            try
            {
                response = (Rcreate) r;
            }
            catch (InvalidCastException)
            {
                if (r.Type != (byte) MessageType.Rerror) throw new UnexpectedMessageException(request.Type, r.Type);
                var err = (Rerror) r;
                throw new ServerErrorException(err.Ename);
            }
            _tagQueue.Enqueue(request.Tag);
            return new Tuple<Qid, uint>(response.Qid, response.Iounit);
        }

        public async Task<Tuple<Qid, uint>> Open(uint fid, byte mode)
        {
            var tag = AllocateTag();
            var request = new Topen(fid, mode)
            {
                Tag = tag
            };
            _protocol.Send(request);
            var r = await _protocol.Receive(tag);
            Ropen response;
            try
            {
                response = (Ropen) r;
            }
            catch (InvalidCastException)
            {
                if (r.Type != (byte) MessageType.Rerror) throw new UnexpectedMessageException(request.Type, r.Type);
                var err = (Rerror) r;
                throw new ServerErrorException(err.Ename);
            }
            _tagQueue.Enqueue(request.Tag);
            return new Tuple<Qid, uint>(response.Qid, response.Iounit);
        }

        public async Task<Tuple<uint, byte[]>> Read(uint fid, ulong offset, uint count)
        {
            var tag = AllocateTag();
            var request = new Tread(fid, offset, count)
            {
                Tag = tag
            };
            _protocol.Send(request);
            var r = await _protocol.Receive(tag);
            Rread response;
            try
            {
                response = (Rread) r;
            }
            catch (InvalidCastException)
            {
                if (r.Type != (byte) MessageType.Rerror) throw new UnexpectedMessageException(request.Type, r.Type);
                var err = (Rerror) r;
                throw new ServerErrorException(err.Ename);
            }
            _tagQueue.Enqueue(request.Tag);
            return new Tuple<uint, byte[]>(response.Count, response.Data);
        }

        public async Task<uint> Write(uint fid, ulong offset, uint count, byte[] data)
        {
            var tag = AllocateTag();
            var request = new Twrite(fid, offset, count, data)
            {
                Tag = tag
            };
            _protocol.Send(request);
            var r = await _protocol.Receive(tag);
            Rwrite response;
            try
            {
                response = (Rwrite) r;
            }
            catch (InvalidCastException)
            {
                if (r.Type != (byte) MessageType.Rerror) throw new UnexpectedMessageException(request.Type, r.Type);
                var err = (Rerror) r;
                throw new ServerErrorException(err.Ename);
            }
            _tagQueue.Enqueue(request.Tag);
            return response.Count;
        }

        public async Task<Stat> Stat(uint fid)
        {
            var tag = AllocateTag();
            var request = new Tstat(fid)
            {
                Tag = tag
            };
            _protocol.Send(request);
            var r = await _protocol.Receive(tag);
            Rstat response;
            try
            {
                response = (Rstat) r;
            }
            catch (InvalidCastException)
            {
                if (r.Type != (byte) MessageType.Rerror) throw new UnexpectedMessageException(request.Type, r.Type);
                var err = (Rerror) r;
                throw new ServerErrorException(err.Ename);
            }
            _tagQueue.Enqueue(request.Tag);
            return response.Stat;
        }

        public async Task Wstat(uint fid, Stat stat)
        {
            var tag = AllocateTag();
            var request = new Twstat(fid, stat)
            {
                Tag = tag
            };
            _protocol.Send(request);
            var r = await _protocol.Receive(tag);
            Rwstat response;
            try
            {
                response = (Rwstat) r;
            }
            catch (InvalidCastException)
            {
                if (r.Type != (byte) MessageType.Rerror) throw new UnexpectedMessageException(request.Type, r.Type);
                var err = (Rerror) r;
                throw new ServerErrorException(err.Ename);
            }
            _tagQueue.Enqueue(request.Tag);
        }

        public async Task Flush(ushort tag)
        {
            var requestTag = AllocateTag();
            var request = new Tflush(tag)
            {
                Tag = requestTag
            };
            _protocol.Send(request);
            var r = await _protocol.Receive(requestTag);
            Rflush response;
            try
            {
                response = (Rflush) r;
            }
            catch (InvalidCastException)
            {
                if (r.Type != (byte) MessageType.Rerror) throw new UnexpectedMessageException(request.Type, r.Type);
                var err = (Rerror) r;
                throw new ServerErrorException(err.Ename);
            }
            _tagQueue.Enqueue(request.Tag);
            _tagQueue.Enqueue(tag);
        }
    }
}