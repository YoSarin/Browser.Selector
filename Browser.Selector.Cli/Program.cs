namespace Browser.Selector.Cli
{
    using Browser.Selector.Lib;
    using Browser.Selector.Lib.Interfaces;
    using CommandLine;
    using System;
    using System.Collections.Generic;
    using System.Diagnostics;
    using System.Linq;
    using System.Security.Permissions;
    using System.Security.Principal;
    using System.Text.RegularExpressions;
    using static System.Windows.Forms.Design.AxImporter;

    class Program
    {
        internal static Installer Installer { get; } = new Installer();
        internal static BrowserList BrowserList { get; } = new BrowserList();
        internal static IParentProcessFinder ParentProcessFinder { get; } = new ParentProcessFinder();

        internal static GUIConsoleWriter Console { get; } = new GUIConsoleWriter();

        internal static IBrowser DefaultWorkBrowser => BrowserList
                .Where(b => b.Name == "Microsoft Edge")
                .First();

        internal static IBrowser DefaultPersonalBrowser => BrowserList
                .Where(b => b.Name == "Google Chrome")
                .First();

        static void Main(string[] args)
        {
            Parser.Default.ParseArguments<Register, Unregister, Open>(args)
                .WithParsed<Register>(Install)
                .WithParsed<Unregister>(Uninstall)
                .WithParsed<Open>(Open);
        }

        static void Install(Register parameters)
        {
            if (IsAdministrator())
            {
                Installer.Install();
            }
            else
            {
                ProcessStartInfo startInfo = new ProcessStartInfo()
                {
                    FileName = Process.GetCurrentProcess().MainModule.FileName,
                    UseShellExecute = true,
                    Verb = "runas",
                    Arguments = "register"
                };
                Process.Start(startInfo).WaitForExit();
            }
        }

        static void Uninstall(Unregister parameters)
        {
            if (IsAdministrator())
            {
                Installer.Uninstall();
            }
            else
            {
                ProcessStartInfo startInfo = new ProcessStartInfo()
                {
                    FileName = Process.GetCurrentProcess().MainModule.FileName,
                    UseShellExecute = true,
                    Verb = "runas",
                    Arguments = "unregister"
                };
                Process.Start(startInfo).WaitForExit();
            }
        }
        private static bool IsAdministrator()
        {
            WindowsIdentity identity = WindowsIdentity.GetCurrent();
            WindowsPrincipal principal = new WindowsPrincipal(identity);
            return principal.IsInRole(WindowsBuiltInRole.Administrator);
        }

        static void Open(Open parameters)
        {
            Process parent = ParentProcessFinder.GetParent();

            foreach (var process in new List<Process> { parent })
            {
                Console.WriteLine($"#{process?.Id}: {process?.ProcessName} ({process?.MainWindowTitle} | {process?.Site})");
            }
            string processName = parent?.ProcessName;

            try {
                IBrowser browser = PickBrowser(parameters, processName);

                Console.WriteLine($"Opening '{parameters.Url}' in {browser.Name}");

                browser.Open(parameters.Url, parameters.RunAs);
            } catch (Exception e) {
                Console.WriteLine($"Not opening '{parameters.Url}': {e.Message}");
            }
        }

        private static IBrowser PickBrowser(Open options, string processName)
        {
            if (IsPersonal(options, processName))
            {
                return DefaultPersonalBrowser;
            }
            else if (IsWorkRelated(options, processName))
            {
                return DefaultWorkBrowser;
            }
            else
            {
                BrowserPrompt dialog = new BrowserPrompt { Url = options.Url, ProcessName = processName };
                return dialog.Show(BrowserList);
            }
        }

        private static bool IsPersonal(Open options, string processName)
        {
            if (new string[] {
                "Messenger",
                "sihost", // whatsapp, for some reason
                "https://facebook.com"
            }.Any(process => string.Equals(process, processName, StringComparison.InvariantCulture)))
            {
                return true;
            }

            if (new Regex[]{
                    new Regex(@"^https://([^/]+\.)?facebook.com(/.*)?$"),
                    new Regex(@"^https://([^/]+\.)?youtube.com(/.*)?$"),
                    new Regex(@"^https://([^/]+\.)?youtu.be(/.*)?$"),
                    new Regex(@"^https://([^/]+\.)?sreality.cz(/.*)?$"),
                    new Regex(@"^https://([^/]+\.)?bezrealitky.cz(/.*)?$"),
                    new Regex(@"^https://([^/]+\.)?tymy.cz(/.*)?$"),
            }
                .Any(pattern => pattern.IsMatch(options.Url)))
            {
                return true;
            }

            return false;
        }

        private static bool IsWorkRelated(Open options, string processName)
        {
            return false;
        }
    }
}
