using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using System.IO;
using OcapReplaySaver2;
using System.Text.RegularExpressions;

namespace DLL_Tester
{
    class Program
    {
        static void Main(string[] args)
        {
            Console.OutputEncoding = Encoding.UTF8;

            StringBuilder _builder = new StringBuilder();
            String file = @"C:\Users\moroz\Desktop\OCAP_dev\test_ocap_output.txt";

            // Launch the processing thread
            OcapReplaySaver2.EntryPoint.RvExtensionVersion(_builder, 9999);

            // Read file and create a list of commands to be executed
            string fileContents = File.ReadAllText(file);

            // Match the contents to a regular expression
            var _matchResults = Regex.Matches(fileContents, "Command:\n^(?<command>.*)$\nArguments:(?<arguments>\n^[^=]*$){1,}", RegexOptions.Multiline);

            // Iterate through matches
            foreach (Match _match in _matchResults)
            {
                String _command = _match.Groups["command"].Value.ToString();
                List<String> _arguments = new List<String>(_match.Groups["arguments"].Value.ToString().Replace("\n", "|").Trim().Split('|'));

                // Remove the first element
                _arguments.RemoveAt(0);

                // Call the extension
                Console.WriteLine(_command);
                Console.WriteLine(string.Join(",", _arguments.ToArray()));
                Console.WriteLine(OcapReplaySaver2.EntryPoint.RvExtensionArgs(_builder, 99999, _command, _arguments.ToArray(), _arguments.Count));
                Console.WriteLine(_builder.ToString());
                Console.WriteLine("===\n");

                _builder.Clear();
            }

            Console.ReadLine();
        }
    }
}
