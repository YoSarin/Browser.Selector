namespace Browser.Selector.Lib
{
    using global::Browser.Selector.Lib.Interfaces;
    using Microsoft.Win32;
    using System.Collections;
    using System.Collections.Generic;

    public class BrowserList : IEnumerable<IBrowser>
    {
        private readonly IList<IBrowser> Browsers = new List<IBrowser> { };

        private string RegistryRoot = @"SOFTWARE\Clients\StartMenuInternet";

        public BrowserList()
        {
            using RegistryKey registryKey = Registry.LocalMachine.OpenSubKey(RegistryRoot);
            string[] subkeys = registryKey.GetSubKeyNames();
            foreach (string subkey in subkeys)
            {
                var name = registryKey.OpenSubKey(subkey).GetValue(null).ToString();
                var associatedClass = registryKey.OpenSubKey($@"{subkey}\Capabilities\URLAssociations")?.GetValue("https");
                // who cares about browses without https support? o.O
                // and we do not want cycles, ofc
                if (associatedClass == null || name == Installer.ApplicationName)
                {
                    continue;
                }

                var executable = registryKey.OpenSubKey($@"{subkey}\shell\open\command").GetValue(null).ToString().Trim('"');

                var command = Registry.LocalMachine.OpenSubKey(@$"SOFTWARE\Classes\{associatedClass}\shell\open\command").GetValue(null).ToString();
                var runas = Registry.LocalMachine.OpenSubKey(@$"SOFTWARE\Classes\{associatedClass}\shell\runas\command")?.GetValue(null).ToString();
                Browsers.Add(new Browser(name, executable, command, runas));
            }
        }

        public IEnumerator<IBrowser> GetEnumerator()
        {
            return Browsers.GetEnumerator();
        }

        IEnumerator IEnumerable.GetEnumerator()
        {
            return Browsers.GetEnumerator();
        }
    }
}
