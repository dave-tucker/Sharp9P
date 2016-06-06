using System;
using System.IO.Pipes;
using System.Linq;
using System.Security.Principal;
using System.Text;
using Sharp9P;

namespace Example
{
     /* A sample client for talking to Datakit. http:\\github.com\docker\datakit */
    internal class Program
    {
        private const uint Rwx = Constants.Dmread | Constants.Dmwrite | Constants.Dmexec;
        private const uint Rw = Constants.Dmread | Constants.Dmexec;
        private const uint R = Constants.Dmread;
        private const uint DirPerm = Rwx << 6 | Rw << 3 | R | Constants.Dmdir;
        private const uint FilePerm = Rw << 6 | R << 3 | R;
        private static Client _client;

        private static void Main(string[] args)
        {
            Console.WriteLine("Connecting to server...\n");
            var stream = new NamedPipeClientStream(".", "datakit",
                PipeDirection.InOut, PipeOptions.None, TokenImpersonationLevel.None);
            stream.Connect();

            // You can also use a TCP Connection, for example
            // var tcpClient = new TcpClient();
            // tcpClient.Connect("10.0.75.2", 5640);
            // var stream = tcpClient.GetStream();

            Console.WriteLine("Creating new client...\n");
            _client = Client.FromStream(stream);
            Console.WriteLine("Checking Version...\n");
            _client.Version(Constants.DefaultMsize, Constants.DefaultVersion);
            Console.WriteLine("Attaching...\n");
            _client.Attach(Constants.RootFid, Constants.NoFid, "anybody", "/");

            for (var i = 0; i < 200; i++) { 
                Console.WriteLine($"Writing {i}...");
                Mkdir(new[] {"branch", "master", "transactions", "test", "rw", "foo"});
                Create(new[] {"branch", "master", "transactions", "test", "rw", "foo", "{i}"});
            }
            Commit(new[] {"branch", "master", "transactions", "test", "ctl"});
        }

        private static void Mkdir(string[] path)
        {
            var fid = _client.AllocateFid(Constants.RootFid);
            foreach (var dir in path)
            {
                var dirFid = _client.AllocateFid(fid);
                try
                {
                    _client.Create(dirFid, dir, DirPerm, Constants.Oread);
                }
                catch (Exception)
                {
                    //Ignoring errors doesn't seem right, but we do this in Go...
                }
                _client.FreeFid(dirFid);
                _client.Walk(fid, fid, new[] {dir});
            }
            _client.FreeFid(fid);
        }

        private static void Create(string[] path)
        {
            var fid = _client.AllocateFid(Constants.RootFid);
            var dirs = path.Take(path.Length - 1).ToArray();
            var utf8 = new UTF8Encoding();
            const string value = "baz";
            _client.Walk(fid, fid, dirs);
            _client.Create(fid, path.Last(), FilePerm, Constants.Ordwr);
            _client.Write(fid, 0, (uint) utf8.GetByteCount(value), utf8.GetBytes(value));
            _client.FreeFid(fid);
        }

        private static void Commit(string[] path)
        {
            var fid = _client.AllocateFid(Constants.RootFid);
            var utf8 = new UTF8Encoding();
            const string value = "commit";
            _client.Walk(fid, fid, path);
            _client.Open(fid, Constants.Ordwr);
            _client.Write(fid, 0, (uint) utf8.GetByteCount(value), utf8.GetBytes(value));
            _client.FreeFid(fid);
        }
    }
}
