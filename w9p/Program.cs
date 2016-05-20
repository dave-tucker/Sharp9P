using System;
using System.Diagnostics;
using System.IO.Pipes;
using System.Security.Principal;

namespace w9p
{
    internal class Program
    {
        static void Main(string[] args)
        {
            var stream = new NamedPipeClientStream(".", "datakit",
                PipeDirection.InOut, PipeOptions.None, TokenImpersonationLevel.None);

            Console.WriteLine("Connecting to server...\n");
            stream.Connect();
            Console.WriteLine("Creating new client...\n");
            var client = new Win9P.Client(stream);
            try
            {
                Console.WriteLine("Checking Version...\n");
                client.Version(Win9P.Client.DefaultMsize, Win9P.Client.DefaultVersion);
                Console.WriteLine("Attaching...\n");
                var qid = client.Attach(Win9P.Client.RootFid, 0, "anyone", ".");
                client.Create(Win9P.Client.RootFid, "test", 755, 2);
            }
            catch (Exception ex)
            {
                Console.WriteLine(ex);
                client.Clunk(Win9P.Client.RootFid);
                stream.Close();
            }
        }
    }
}
