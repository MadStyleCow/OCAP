// Define build type
#define WIN64

// Includes
using RGiesecke.DllExport;
using System;
using System.Collections.Concurrent;
using System.IO;
using System.Runtime.InteropServices;
using System.Text;
using System.Threading;

// DLL Entry
namespace OcapReplaySaver2
{
    public class EntryPoint
    {
        /* Private variables */
        /// <summary>
        /// A value, indicating where the debug file should be located at.
        /// </summary>
        private static readonly String _debugFile = @"D:\wog_ocap_debug.txt";

        /// <summary>
        /// A concurrent queue, used for storing messages, until the thread retrieves them.
        /// </summary>
        private static readonly ConcurrentQueue<Command> _queue = new ConcurrentQueue<Command>();

        /// <summary>
        /// A background thread for processing.
        /// </summary>
        private static readonly Thread _thread = new Thread(new ThreadStart(ProcessingThread))
        {
            IsBackground = true
        };

        /* Private methods */
        /// <summary>
        /// A method which is executed by the background-thread.
        /// </summary>
        static void ProcessingThread()
        {
            while (true)
            {
                try
                {
                    // Do we have messages to process?
                    if (_queue.Count > 0)
                    {
                        // Define a command
                        Command _command;

                        // Try dequeueing the message
                        if (_queue.TryDequeue(out _command))
                        {
                            // Log(_debugFile, String.Format("Parsing command: {0}", _command.CommandName));

                            CommandParser.ParseCommand(_command.CommandName, _command.Arguments);
                        }
                    }
                    else
                    {
                        // Log(_debugFile, "Nothing to process...");
                        Thread.Sleep(500);
                        
                    }
                }
                catch (Exception ex)
                {
                    Log(_debugFile, ex.ToString());
                }
            }
        }

        /// <summary>
        /// A static logger function
        /// </summary>
        /// <param name="pFile">Location of the output file</param>
        /// <param name="pOutput">Text to output</param>
        static void Log(String pFile, String pOutput)
        {
            File.AppendAllText(pFile, String.Format("\n===============\n{0}\n{1}", DateTime.Now.ToString(), pOutput));
        }

        /* Arma calls */
        #region ArmA calls
        /// <summary>
        /// Gets called when arma starts up and loads all extension.
        /// It's perfect to load in static objects in a seperate thread so that the extension doesn't needs any seperate initalization
        /// </summary>
        /// <param name="output">The string builder object that contains the result of the function</param>
        /// <param name="outputSize">The maximum size of bytes that can be returned</param>
#if WIN64
        [DllExport("RVExtensionVersion", CallingConvention = CallingConvention.Winapi)]
#else
        [DllExport("_RVExtensionVersion@8", CallingConvention = CallingConvention.Winapi)]
#endif
        public static void RvExtensionVersion(StringBuilder output, int outputSize)
        {
            // Check if thread is alive
            if (!_thread.IsAlive)
            {
                // And if it isn't start it
                _thread.Start();
            }

            output.Append("0.0.1.0");
        }

        /// <summary>
        /// The entry point for the default callExtension command.
        /// </summary>
        /// <param name="output">The string builder object that contains the result of the function</param>
        /// <param name="outputSize">The maximum size of bytes that can be returned</param>
        /// <param name="function">The string argument that is used along with callExtension</param>
#if WIN64
        [DllExport("RVExtension", CallingConvention = CallingConvention.Winapi)]
#else
        [DllExport("_RVExtension@12", CallingConvention = CallingConvention.Winapi)]
#endif
        public static void RvExtension(StringBuilder output, int outputSize, [MarshalAs(UnmanagedType.LPStr)] string function)
        {
            // This is not supported by this extension, therefore return an error message and thats all.
            outputSize--; // Ensure that we don't exceed the maximum output size - it's a bit paranoid but you should keep it there
            output.Append("This extension does not support this method.");
        }

        /// <summary>
        /// The entry point for the callExtensionArgs command.
        /// </summary>
        /// <param name="output">The string builder object that contains the result of the function</param>
        /// <param name="outputSize">The maximum size of bytes that can be returned</param>
        /// <param name="function">The string argument that is used along with callExtension</param>
        /// <param name="args">The args passed to callExtension as a string array</param>
        /// <param name="argsCount">The size of the string array args</param>
        /// <returns>The result code</returns>
#if WIN64
        [DllExport("RVExtensionArgs", CallingConvention = CallingConvention.Winapi)]
#else
        [DllExport("_RVExtensionArgs@20", CallingConvention = CallingConvention.Winapi)]
#endif
        public static int RvExtensionArgs(StringBuilder output, int outputSize, [MarshalAs(UnmanagedType.LPStr)] string function, [MarshalAs(UnmanagedType.LPArray, ArraySubType = UnmanagedType.LPStr, SizeParamIndex = 4)] string[] args, int argCount)
        {
            try
            {
                // Add the command to the queue
                _queue.Enqueue(new Command()
                {
                    CommandName = function,
                    Arguments = args
                });

                return 0;
            }
            catch(Exception ex)
            {
                outputSize--;
                output.Append(ex);
                return 1;
            }
        }
        #endregion
    }
}
