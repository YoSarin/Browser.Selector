namespace Browser.Selector.Lib
{
    using global::Browser.Selector.Lib.Interfaces;
    using System.Collections.Generic;
    using System.Linq;

    class Browser : IBrowser
    {
        public string Name { get; }
        public string ExecutablePath { get; }
        private readonly IEnumerable<string> Parameters = new List<string>();
        private readonly IEnumerable<string> RunAsParameters = new List<string>();

        public Browser(string name, string executablePath, string command, string runasCommand)
        {
            Name = name;
            ExecutablePath = executablePath;
            Parameters = command.Replace(ExecutablePath, null).Trim().Split();
            if (runasCommand != null)
            {
                RunAsParameters = runasCommand.Replace(ExecutablePath, null).Trim().Split().Except(command.Replace(ExecutablePath, null).Trim().Split());
            }
        }

        public void Open(string Url, bool runAs = false)
        {
            var args = Parameters
                .Select(p => p.Replace("%1", Url))
                .ToList();

            if (runAs)
            {
                args.AddRange(RunAsParameters);
            }

            System.Diagnostics.Process.Start(ExecutablePath, string.Join(" ", args));
        }
    }
}
