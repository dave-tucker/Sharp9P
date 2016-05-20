using System;
using System.Collections;
using System.IO;
using Win9P.Protocol;

namespace Win9P
{
    public class Client
    {
        public const uint DefaultMsize = 16384;
        public const string DefaultVersion = "9P2000";
        public const uint RootFid = 1;
        private uint _msize;
        private string _version;
        private readonly Queue _tagQueue;
        private readonly Queue _fidQueue;
        private readonly Protocol.Protocol _protocol;

        public Client(Stream stream)
        {
            _msize = DefaultMsize;
            _version = DefaultVersion;
            _protocol = new Protocol.Protocol(stream);
            _tagQueue = new Queue();
            for (ushort i = 1; i < 65535; i++)
            {
                _tagQueue.Enqueue(i);
            }
            _fidQueue = new Queue();
            for (uint i = 2; i < 100; i++)
            {
                _fidQueue.Enqueue(i);
            }
        }

        public void Version(uint msize, string version)
        {
            var request = new Tversion(_msize, _version);
            _protocol.Write(request);

            var r = _protocol.Read();
            Rversion response;
            try { 
                response = (Rversion) r;
            }
            catch (InvalidCastException)
            {
                if (r.Type != (byte) MessageType.Rerror) throw new Exception("Unexpected Message Type");
                var err = (Rerror) r;
                throw new Exception($"Server Error: {err.Ename}");
            }
            /* The server responds with its own maxi-
            mum, msize, which must be less than or equal to the client's
            value */
            if (response.Msize > request.Msize)
            {
                throw new Exception("Server responded with larger Msize");
            }
            _msize = response.Msize;
            if (response.Version != request.Version)
            {
                throw new Exception("Version not supported");
            }
            _version = response.Version;
        }

        public Qid Attach(uint fid, uint afid, string uname, string aname)
        {
            var request = new Tattach(fid, afid, uname, aname)
            {
                Tag = (ushort)_tagQueue.Dequeue()
            };
            _protocol.Write(request);
            var r = _protocol.Read();
            Rattach response;
            try
            {
                response = (Rattach)r;
            }
            catch (InvalidCastException)
            {
                if (r.Type != (byte)MessageType.Rerror) throw new Exception("Unexpected Message Type");
                var err = (Rerror)r;
                throw new Exception($"Server Error: {err.Ename}");
            }
            if (response.Tag != request.Tag)
                throw new Exception("Tag mismatch");
            _tagQueue.Enqueue(request.Tag);
            return response.Qid;
        }

        public Qid Auth(string uname, string aname)
        {
            var fid = (uint)_fidQueue.Dequeue();
            var request = new Tauth(fid, uname, aname)
            {
                Tag = (ushort)_tagQueue.Dequeue()
            };
            _protocol.Write(request);
            var r = _protocol.Read();
            Rauth response;
            try
            {
                response = (Rauth)r;
            }
            catch (InvalidCastException)
            {
                if (r.Type != (byte)MessageType.Rerror) throw new Exception("Unexpected Message Type");
                var err = (Rerror)r;
                throw new Exception($"Server Error: {err.Ename}");
            }
            if (response.Tag != request.Tag)
                throw new Exception("Tag mistmatch");
            _tagQueue.Enqueue(request.Tag);
            return response.Aqid;
        }

        public Qid[] Walk(uint fid, string[] nwnames)
        {
            var newfid = (uint) _fidQueue.Dequeue();
            if (nwnames.Length > Protocol.Protocol.MAXWELEM)
            {
                throw new Exception("No more thatn 16 elements allowed");
            }
            var request = new Twalk(fid, newfid, (ushort) nwnames.Length, nwnames)
            {
                Tag = (ushort)_tagQueue.Dequeue()
            };
            _protocol.Write(request);
            var r = _protocol.Read();
            Rwalk response;
            try
            {
                response = (Rwalk)r;
            }
            catch (InvalidCastException)
            {
                if (r.Type != (byte)MessageType.Rerror) throw new Exception("Unexpected Message Type");
                var err = (Rerror)r;
                throw new Exception($"Server Error: {err.Ename}");
            }
            if (response.Tag != request.Tag)
                throw new Exception("Tag mismatch");
            _tagQueue.Enqueue(request.Tag);
            return response.Wqid;
        }

        public void Clunk(uint fid)
        {
            var request = new Tclunk(fid)
            {
                Tag = (ushort)_tagQueue.Dequeue()
            };
            _protocol.Write(request);
            var r = _protocol.Read();
            Rclunk response;
            try
            {
                response = (Rclunk)r;
            }
            catch (InvalidCastException)
            {
                if (r.Type != (byte)MessageType.Rerror) throw new Exception("Unexpected Message Type");
                var err = (Rerror)r;
                throw new Exception($"Server Error: {err.Ename}");
            }
            if (response.Tag != request.Tag)
                throw new Exception("Tag mismatch");
            _fidQueue.Enqueue(fid);
            _tagQueue.Enqueue(request.Tag);
        }

        public Tuple<Qid, uint> Create(uint fid, string name, uint perm, byte mode)
        {
            var request = new Tcreate(fid, name, perm, mode)
            {
                Tag = (ushort)_tagQueue.Dequeue()
            };
            _protocol.Write(request);
            var r = _protocol.Read();
            Rcreate response;
            try
            {
                response = (Rcreate)r;
            }
            catch (InvalidCastException)
            {
                if (r.Type != (byte)MessageType.Rerror) throw new Exception("Unexpected Message Type");
                var err = (Rerror)r;
                throw new Exception($"Server Error: {err.Ename}");
            }
            if (response.Tag != request.Tag)
                throw new Exception("Tag mismatch");
            _tagQueue.Enqueue(request.Tag);
            return new Tuple<Qid, uint>(response.Qid, response.Iounit);
        }

        public Tuple<Qid, uint> Open(uint fid, byte mode)
        {
            var request = new Topen(fid, mode)
            {
                Tag = (ushort)_tagQueue.Dequeue()
            };
            _protocol.Write(request);
            var r = _protocol.Read();
            Ropen response;
            try
            {
                response = (Ropen)r;
            }
            catch (InvalidCastException)
            {
                if (r.Type != (byte)MessageType.Rerror) throw new Exception("Unexpected Message Type");
                var err = (Rerror)r;
                throw new Exception($"Server Error: {err.Ename}");
            }
            if (response.Tag != request.Tag)
                throw new Exception("Tag mismatch");
            _tagQueue.Enqueue(request.Tag);
            return new Tuple<Qid, uint>(response.Qid, response.Iounit);
        }

        public Tuple<uint, byte[]> Read(uint fid, ulong offset, uint count)
        {
            var request = new Tread(fid, offset,count)
            {
                Tag = (ushort)_tagQueue.Dequeue()
            };
            _protocol.Write(request);
            var r = _protocol.Read();
            Rread response;
            try
            {
                response = (Rread)r;
            }
            catch (InvalidCastException)
            {
                if (r.Type != (byte)MessageType.Rerror) throw new Exception("Unexpected Message Type");
                var err = (Rerror)r;
                throw new Exception($"Server Error: {err.Ename}");
            }
            if (response.Tag != request.Tag)
                throw new Exception("Tag mismatch");
            _tagQueue.Enqueue(request.Tag);
            return new Tuple<uint, byte[]>(response.Count, response.Data);
        }

        public uint Write(uint fid, ulong offset, uint count, byte[] data)
        {
            var request = new Twrite(fid, offset, count, data)
            {
                Tag = (ushort)_tagQueue.Dequeue()
            };
            _protocol.Write(request);
            var r = _protocol.Read();
            Rwrite response;
            try
            {
                response = (Rwrite)r;
            }
            catch (InvalidCastException)
            {
                if (r.Type != (byte)MessageType.Rerror) throw new Exception("Unexpected Message Type");
                var err = (Rerror)r;
                throw new Exception($"Server Error: {err.Ename}");
            }
            if (response.Tag != request.Tag)
                throw new Exception("Tag mismatch");
            _tagQueue.Enqueue(request.Tag);
            return response.Count;
        }

        public Stat Stat(uint fid)
        {
            var request = new Tstat(fid)
            {
                Tag = (ushort)_tagQueue.Dequeue()
            };
            _protocol.Write(request);
            var r = _protocol.Read();
            Rstat response;
            try
            {
                response = (Rstat)r;
            }
            catch (InvalidCastException)
            {
                if (r.Type != (byte)MessageType.Rerror) throw new Exception("Unexpected Message Type");
                var err = (Rerror)r;
                throw new Exception($"Server Error: {err.Ename}");
            }
            if (response.Tag != request.Tag)
                throw new Exception("Tag mismatch");
            _tagQueue.Enqueue(request.Tag);
            return response.Stat;
        }

        public void Wstat(uint fid, Stat stat)
        {
            var request = new Twstat(fid,stat)
            {
                Tag = (ushort)_tagQueue.Dequeue()
            };
            _protocol.Write(request);
            var r = _protocol.Read();
            Rwstat response;
            try
            {
                response = (Rwstat)r;
            }
            catch (InvalidCastException)
            {
                if (r.Type != (byte)MessageType.Rerror) throw new Exception("Unexpected Message Type");
                var err = (Rerror)r;
                throw new Exception($"Server Error: {err.Ename}");
            }
            if (response.Tag != request.Tag)
                throw new Exception("Tag mismatch");
            _tagQueue.Enqueue(request.Tag);
        }

        public void Flush(ushort tag)
        {
            var request = new Tflush(tag)
            {
                Tag = (ushort)_tagQueue.Dequeue()
            };
            _protocol.Write(request);
            var r = _protocol.Read();
            Rflush response;
            try
            {
                response = (Rflush)r;
            }
            catch (InvalidCastException)
            {
                if (r.Type != (byte)MessageType.Rerror) throw new Exception("Unexpected Message Type");
                var err = (Rerror)r;
                throw new Exception($"Server Error: {err.Ename}");
            }
            if (response.Tag != request.Tag)
                throw new Exception("Tag mismatch");
            _tagQueue.Enqueue(request.Tag);
            _tagQueue.Enqueue(tag);
        }
    }
}
