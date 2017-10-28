using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading;
using System.Threading.Tasks;

namespace ReverseTCPClient
{
    class Program
    {
        static void Main(string[] args)
        {
            CommandLine cmd = new CommandLine();
            cmd.runCommand("echo hello");
            cmd.runCommand("dir");
            cmd.killCommandLine();
            foreach(string line in cmd.outputRecieved)
            {
                Console.Out.WriteLine(line);
            }
            Thread.Sleep(3000);
        }
    }
}
