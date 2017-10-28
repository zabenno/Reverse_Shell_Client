using System;
using System.Collections.Generic;
using System.Diagnostics;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace ReverseTCPClient
{
    class CommandLine
    {
        //Variable containg data output by the current process line by line.
        public List<string> outputRecieved = new List<string>();
        private Process CommandInterface = new Process();

        public CommandLine(string filename = "CMD.exe")
        {
            //Setting up Process to be run.
            ProcessStartInfo startInfo = new ProcessStartInfo();
            startInfo.CreateNoWindow = true;
            startInfo.FileName = filename;
            startInfo.RedirectStandardInput = true;
            startInfo.RedirectStandardOutput = true;
            startInfo.RedirectStandardError = true;
            startInfo.UseShellExecute = false;
            CommandInterface.StartInfo = startInfo;
            //Setting up event listeners to output data to global variable when received. 
            CommandInterface.OutputDataReceived += (sender, lineReceived) => outputRecieved.Add(lineReceived.Data);
            CommandInterface.ErrorDataReceived += (sender, lineReceived) => outputRecieved.Add(lineReceived.Data);
            CommandInterface.Start();
            //Beginning event listeners
            CommandInterface.BeginOutputReadLine();
        }
        /// <summary>
        /// Runs a given command in the application that is running.
        /// </summary>
        /// <param name="command">Command to be run</param>
        public void runCommand(string command)
        {
            CommandInterface.StandardInput.WriteLine(command);
        }

        /// <summary>
        /// Kill the running process. Can not be restarted. 
        /// </summary>
        public void killCommandLine(string commandToClose = "exit")
        {
            runCommand(commandToClose);
            CommandInterface.WaitForExit();
            CommandInterface.CancelOutputRead();
        }

    }
}
