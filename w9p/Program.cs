using System;
using System.IO.Pipes;
using System.Linq;
using System.Net.Sockets;
using System.Security.Principal;
using System.Text;
using Win9P;
using Win9P.Protocol;

namespace w9p
{
    internal class Program
    {
        private static Client client;

        static void Main(string[] args)
        {
            //var stream = new NamedPipeClientStream(".", "datakit",
            //    PipeDirection.InOut, PipeOptions.None, TokenImpersonationLevel.None);

            var tcpClient = new TcpClient();
            
            Console.WriteLine("Connecting to server...\n");
            //stream.Connect();
            tcpClient.Connect("10.0.75.2", 5640);
            var stream = tcpClient.GetStream();
            Console.WriteLine("Creating new client...\n");
            client = new Win9P.Client(stream);
            Console.WriteLine("Checking Version...\n");
            client.Version(Win9P.Client.DefaultMsize, Win9P.Client.DefaultVersion);
            Console.WriteLine("Attaching...\n");
            client.Attach(Client.RootFid, Protocol.NoFid, "Dave", "/");
            Mkdir(new[] {"branch", "master", "transactions", "test", "rw", "com.docker.driver.amd64-linux" });
            Create(new[] {"branch", "master", "transactions", "test", "rw", "com.docker.driver.amd64-linux", "network"});
            Commit(new[] { "branch", "master", "transactions", "test", "ctl" });
        }

        private const uint rwx = Protocol.DMREAD | Protocol.DMWRITE | Protocol.DMEXEC;
        private const uint rw = Protocol.DMREAD | Protocol.DMEXEC;
        private const uint r = Protocol.DMREAD;
        private const uint DirPerm = rwx << 6 | rw << 3 | r | Protocol.DMDIR;
        private const uint FilePerm = rw << 6 | r << 3 | r;

        static void Mkdir(string[] path)
        {
            var fid = client.AllocateFid(Client.RootFid); 
            foreach (var dir in path)
            {
                var dirFid = client.AllocateFid(fid);
                try
                {
                    client.Create(dirFid, dir, DirPerm, Protocol.OREAD);
                }
                catch (Exception)
                {
                    //Ignoring errors doesn't seem right, but we do this in Go...
                }
                client.FreeFid(dirFid);
                client.Walk(fid, fid, new[] { dir});
            }
        }

        static void Create(string[] path)
        {
            var fid = client.AllocateFid(Client.RootFid);
            var dirs = path.Take(path.Length-1).ToArray();
            var utf8 = new UTF8Encoding();
            const string value = "hybrid";
            client.Walk(fid,fid,dirs);
            client.Create(fid, path.Last(), FilePerm, Protocol.ORDWR);
            client.Write(fid, 0, (uint)utf8.GetByteCount(value), utf8.GetBytes(value));
            client.FreeFid(fid);
        }

        static void Commit(string[] path)
        {
            var fid = client.AllocateFid(Client.RootFid);
            var utf8 = new UTF8Encoding();
            const string value = "commit";
            client.Walk(fid, fid, path);
            client.Open(fid, Protocol.ORDWR);
            client.Write(fid, 0, (uint)utf8.GetByteCount(value), utf8.GetBytes(value));
            client.FreeFid(fid);
        }
    }
}
