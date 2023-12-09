namespace Browser.Selector.Cli
{
    using System;
    using System.IO;
    using System.Runtime.InteropServices;

    // This always writes to the parent console window and also to a redirected stdout if there is one.
    // It would be better to do the relevant thing (eg write to the redirected file if there is one, otherwise
    // write to the console) but it doesn't seem possible.
    public class GUIConsoleWriter
    {
        [DllImport("kernel32.dll")]
        private static extern bool AttachConsole(int dwProcessId);

        [DllImport("kernel32.dll", SetLastError = true)]
        [return: MarshalAs(UnmanagedType.Bool)]
        static extern bool AllocConsole();

        private const int ATTACH_PARENT_PROCESS = -1;

        StreamWriter _stdOutWriter;
    
        // this must be called early in the program
        public GUIConsoleWriter(bool enforceLogging = false)
        {
#if DEBUG
            enforceLogging = true;
#endif
            // this needs to happen before attachconsole.
            // If the output is not redirected we still get a valid stream but it doesn't appear to write anywhere
            // I guess it probably does write somewhere, but nowhere I can find out about
            var stdout = Console.OpenStandardOutput();
            _stdOutWriter = new StreamWriter(stdout)
            {
                AutoFlush = true
            };

            if (!AttachConsole(ATTACH_PARENT_PROCESS) && enforceLogging)
            {
                AllocConsole();
            }
        }

        public void WriteLine(string line)
        {
            _stdOutWriter.WriteLine(line);
            Console.WriteLine(line);
        }
    }
}
