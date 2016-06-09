using System;
using System.Diagnostics;
using System.IO.Pipes;
using System.Linq;
using System.Security.Principal;
using System.Text;
using System.Threading;
using System.Threading.Tasks;
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

        static void Main(string[] args)
        {
<<<<<<< Updated upstream
            Stopwatch sw1 = new Stopwatch();
            sw1.Start();
            Console.WriteLine("Connecting to server...\n");
            var stream = new NamedPipeClientStream(".", "datakit",
                PipeDirection.InOut, PipeOptions.None, TokenImpersonationLevel.None);
            stream.Connect();
=======
            var t = new Task(async () =>
            {
                Console.WriteLine("Connecting to server...\n");
                var stream = new NamedPipeClientStream(".", "datakit",
                    PipeDirection.InOut, PipeOptions.None, TokenImpersonationLevel.None);
                stream.Connect();
>>>>>>> Stashed changes

                // You can also use a TCP Connection, for example
                // var tcpClient = new TcpClient();
                // tcpClient.Connect("10.0.75.2", 5640);
                // var stream = tcpClient.GetStream();

<<<<<<< Updated upstream
            Console.WriteLine("Creating new client...\n");
            _client = Client.FromStream(stream);
            Console.WriteLine("Checking Version...\n");
            _client.Version(Constants.DefaultMsize, Constants.DefaultVersion);
            Console.WriteLine("Attaching...\n");
            _client.Attach(Constants.RootFid, Constants.NoFid, "anybody", "/");
            sw1.Stop();
            Console.WriteLine($"Connection took ${sw1.Elapsed}");

            Mkdir(new[] { "branch", "master", "transactions", "test", "rw", "foo" });
            for (var i = 0; i < 200; i++) {
                Stopwatch sw = new Stopwatch();
                sw.Start();
                Console.WriteLine($"Writing {i}...");
                Create(new[] {"branch", "master", "transactions", "test", "rw", "foo", "{i}"});
                sw.Stop();
                Console.WriteLine($"Write took={sw.Elapsed}");
            }
            Commit(new[] {"branch", "master", "transactions", "test", "ctl"});
=======
                Console.WriteLine("Creating new client...\n");
                _client = Client.FromStream(stream);
                _client.Start();
                Console.WriteLine("Checking Version...\n");
                _client.Version(Constants.DefaultMsize, Constants.DefaultVersion).Wait();
                Console.WriteLine("Attaching...\n");
                _client.Attach(Constants.RootFid, Constants.NoFid, "anybody", "/").Wait();

                for (var i = 0; i < 200; i++)
                {
                    Console.WriteLine($"Writing {i}...");
                    await Mkdir(new[] {"branch", "master", "transactions", "test", "rw", "foo"});
                    await Create(new[] {"branch", "master", "transactions", "test", "rw", "foo", $"{i}"});
                }
                await Commit(new[] {"branch", "master", "transactions", "test", "ctl"});
                Thread.Sleep(2000);
                for (var i = 0; i < 200; i++)
                {
                    Console.WriteLine($"Writing {i}...");
                    await Mkdir(new[] {"branch", "master", "transactions", "test2", "rw", "foo"});
                    await Create(new[] {"branch", "master", "transactions", "test2", "rw", "foo", $"{i}"});
                }
                await Commit(new[] {"branch", "master", "transactions", "test2", "ctl"});
                Task.WaitAll();
            });
            t.Start();
            t.Wait();
            Task.WaitAll();
>>>>>>> Stashed changes
        }

        private static async Task Mkdir(string[] path)
        {
            var fid = await _client.AllocateFid(Constants.RootFid);
            foreach (var dir in path)
            {
                var dirFid = await _client.AllocateFid(fid);
                try
                {
                    await _client.Create(dirFid, dir, DirPerm, Constants.Oread);
                }
                catch (Exception)
                {
                    //Ignoring errors doesn't seem right, but we do this in Go...
                }
                await _client.FreeFid(dirFid);
                await _client.Walk(fid, fid, new[] {dir});
            }
            await _client.FreeFid(fid);
            Task.WaitAll();
        }

        private static async Task Create(string[] path)
        {
            var fid = await _client.AllocateFid(Constants.RootFid);
            var dirs = path.Take(path.Length - 1).ToArray();
            var utf8 = new UTF8Encoding();
            const string value = "baz";
            await _client.Walk(fid, fid, dirs);
            await _client.Create(fid, path.Last(), FilePerm, Constants.Ordwr);
            await _client.Write(fid, 0, (uint) utf8.GetByteCount(value), utf8.GetBytes(value));
            await _client.FreeFid(fid);
            Task.WaitAll();
        }

        private static async Task Commit(string[] path)
        {
            var fid = await _client.AllocateFid(Constants.RootFid);
            var utf8 = new UTF8Encoding();
            const string value = "commit";
            await _client.Walk(fid, fid, path);
            await _client.Open(fid, Constants.Ordwr);
            await _client.Write(fid, 0, (uint) utf8.GetByteCount(value), utf8.GetBytes(value));
            await _client.FreeFid(fid);
        }
    }
}
