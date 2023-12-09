namespace Browser.Selector.Lib
{
    using global::Browser.Selector.Lib.Exceptions;
    using Microsoft.Win32;
    using System.Collections.Generic;
    using System.Diagnostics;
    using System.Linq;

    public class Installer
    {
        internal static readonly string ApplicationName = "Browser Selector";
        internal static readonly string RegistryClassName = "Browser.Selector.HTML";
        private static readonly string Icon = @$"{Process.GetCurrentProcess().MainModule.FileName},0";
        private static string InstallPath { get; } = Process.GetCurrentProcess().MainModule.FileName;

        private static Dictionary<string, Dictionary<string, string>> RegistrationData = new Dictionary<string, Dictionary<string, string>>
        {
            { @$"SOFTWARE\Clients\StartMenuInternet\Browser.Selector", new Dictionary<string, string> {
                { "@$", ApplicationName}
            }},
            { @$"SOFTWARE\Clients\StartMenuInternet\Browser.Selector\Capabilities", new Dictionary<string, string> {
                { "ApplicationDescription", $"{ApplicationName} allows you to choose which browser to use when new link is opened"},
                { "ApplicationIcon", Icon},
                { "ApplicationName", ApplicationName}
            }},
            { @$"SOFTWARE\Clients\StartMenuInternet\Browser.Selector\Capabilities\FileAssociations", new Dictionary<string, string> {
                // not necessary, i hope?
            }},
            { @$"SOFTWARE\Clients\StartMenuInternet\Browser.Selector\Capabilities\StartMenu", new Dictionary<string, string> {
                { "StartMenuInternet", ApplicationName}
            }},
            { @$"SOFTWARE\Clients\StartMenuInternet\Browser.Selector\Capabilities\URLAssociations", new Dictionary<string, string> {
                { "ftp", RegistryClassName },
                { "http", RegistryClassName },
                { "https", RegistryClassName },
                { "irc", RegistryClassName },
                { "mailto", RegistryClassName },
                { "mms", RegistryClassName },
                { "news" ,  RegistryClassName },
                { "nntp" ,  RegistryClassName },
                { "sms" ,  RegistryClassName },
                { "smsto" ,  RegistryClassName },
                { "snews" ,  RegistryClassName },
                { "tel" ,  RegistryClassName },
                { "urn" ,  RegistryClassName },
                { "webcal" , RegistryClassName }
            }},
            { @$"SOFTWARE\Clients\StartMenuInternet\Browser.Selector\DefaultIcon", new Dictionary<string, string> {
                { "@$", Icon }
            }},
            { @$"SOFTWARE\Clients\StartMenuInternet\Browser.Selector\InstallInfo", new Dictionary<string, string> {
                // { "ReinstallCommand", "\"C:\\Program Files\\Google\\Chrome\\Application\\chrome.exe\" --make-default-browser" },
                // { "HideIconsCommand", "\"C:\\Program Files\\Google\\Chrome\\Application\\chrome.exe\" --hide-icons" },
                // { "ShowIconsCommand", "\"C:\\Program Files\\Google\\Chrome\\Application\\chrome.exe\" --show-icons" },
                // { "IconsVisible", "dword:00000001" }
            }},
            { @$"SOFTWARE\Clients\StartMenuInternet\Browser.Selector\shell\open\command", new Dictionary<string, string> {
                { "@$", InstallPath}
            }},
            { @$"SOFTWARE\RegisteredApplications", new Dictionary<string, string> {
                { ApplicationName, @$"SOFTWARE\Clients\StartMenuInternet\Browser.Selector\Capabilities" }
            }},
            { @$"SOFTWARE\Classes\{RegistryClassName}", new Dictionary<string, string> {
                { "@$", ApplicationName },
                { "AppUserModelId", "Browser.Selector" }
            }},
            { @$"SOFTWARE\Classes\{RegistryClassName}\Application", new Dictionary<string, string> {
                { "AppUserModelId", "Browser.Selector" },
                { "ApplicationIcon", Icon },
                { "ApplicationName", ApplicationName },
                { "ApplicationDescription", "Choose your way how to browse the web" },
                { "ApplicationCompany", "YoSarin" }
            }},
            { @$"SOFTWARE\Classes\{RegistryClassName}\DefaultIcon", new Dictionary<string, string> {
                { "@$", Icon }
            }},
            { @$"SOFTWARE\Classes\{RegistryClassName}\shell\open\command", new Dictionary<string, string> {
                { "@$", $"{InstallPath} open --url %1"}
            }},
            { @$"SOFTWARE\Classes\{RegistryClassName}\shell\runas\command", new Dictionary<string, string> {
                { "@$", $"{InstallPath} open --runas --url %1"}
            }}
        };

        private static IList<string> UnregisterKeys = new List<string> {
            @$"SOFTWARE\Clients\StartMenuInternet\Browser.Selector\shell\open\command",
            @$"SOFTWARE\Clients\StartMenuInternet\Browser.Selector\shell\open",
            @$"SOFTWARE\Clients\StartMenuInternet\Browser.Selector\shell",
            @$"SOFTWARE\Clients\StartMenuInternet\Browser.Selector\InstallInfo",
            @$"SOFTWARE\Clients\StartMenuInternet\Browser.Selector\DefaultIcon",
            @$"SOFTWARE\Clients\StartMenuInternet\Browser.Selector\Capabilities\URLAssociations",
            @$"SOFTWARE\Clients\StartMenuInternet\Browser.Selector\Capabilities\StartMenu",
            @$"SOFTWARE\Clients\StartMenuInternet\Browser.Selector\Capabilities\FileAssociations",
            @$"SOFTWARE\Clients\StartMenuInternet\Browser.Selector\Capabilities",
            @$"SOFTWARE\Clients\StartMenuInternet\Browser.Selector",
            @$"SOFTWARE\Classes\{RegistryClassName}\shell\runas\command",
            @$"SOFTWARE\Classes\{RegistryClassName}\shell\open\command",
            @$"SOFTWARE\Classes\{RegistryClassName}\shell\runas",
            @$"SOFTWARE\Classes\{RegistryClassName}\shell\open",
            @$"SOFTWARE\Classes\{RegistryClassName}\shell",
            @$"SOFTWARE\Classes\{RegistryClassName}\DefaultIcon",
            @$"SOFTWARE\Classes\{RegistryClassName}\Application",
            @$"SOFTWARE\Classes\{RegistryClassName}"
        };

        public void Install()
        {
            foreach (KeyValuePair<string, Dictionary<string, string>> path in RegistrationData)
            {
                using RegistryKey registryKey = Registry.LocalMachine.CreateSubKey(path.Key);
                foreach (KeyValuePair<string, string> entry in path.Value)
                {
                    var key = entry.Key == "@$" ? null : entry.Key;
                    registryKey.SetValue(key, entry.Value);
                }
            }
        }

        public void Uninstall()
        {
            foreach (KeyValuePair<string, Dictionary<string, string>> path in RegistrationData)
            {
                using RegistryKey registryKey = Registry.LocalMachine.OpenSubKey(path.Key, true);
                if (registryKey == null)
                {
                    continue;
                }
                foreach (KeyValuePair<string, string> entry in path.Value)
                {
                    var key = entry.Key == "@$" ? "" : entry.Key;
                    registryKey.DeleteValue(key, false);
                }
            }
            foreach (string path in UnregisterKeys)
            {
                try
                {
                    using (RegistryKey registryKey = Registry.LocalMachine.OpenSubKey(path))
                    {
                        if (registryKey.GetValueNames().Count() > 0)
                        {
                            throw new GenericException($"Registry key '${path}' does have ${registryKey.GetValueNames().Count()} values. Delete failed.");
                        }
                    }
                    Registry.LocalMachine.DeleteSubKeyTree(path);
                } catch
                {
                    // pass
                }
            }
        }
    }
}
