namespace Browser.Selector.Cli
{
    using CommandLine;

    [Verb("register", HelpText = "Create registry keys")]
    class Register
    {
    }

    [Verb("unregister", HelpText = "Remove registry keys")]
    class Unregister
    {
    }

    [Verb("open", HelpText = "Open web page")]
    class Open
    {
        [Option("url", Required = true, HelpText = "Url to open")]
        public string Url { get; set; }

        [Option("runas", Required = false, HelpText = "Indicator when run with different user", Default = false)]
        public bool RunAs { get; set; } = false;
    }
}
