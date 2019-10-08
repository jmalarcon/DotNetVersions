using System;
using Microsoft.Win32;

namespace DotNetVersion
{
    class Program
    {
        static void Main(string[] args)
        {
            bool batchMode = false;

            if (args.Length > 0 && (args[0] == "/help" || args[0] == "-help" || args[0] == "--help" || args[0] == "/?" || args[0] == "-?" || args[0] == "--?"))
            {
                Console.Write("Writes all the currently installed versions of \"classic\" .NET platform in the system.\r\nUse --b, -b or /b to use in a batch, showing only the installed versions, without any extra informational lines.");
            }
            else
            {
                if (args.Length > 0 && (args[0] == "/b" || args[0] == "-b" || args[0] == "--b"))
                {
                    batchMode = true;
                }

                if (!batchMode)
                    Console.WriteLine("Currently installed \"classic\" .NET Versions in the system:");

                //Show all the installed versions
                Get1To45VersionFromRegistry();
                Get45PlusFromRegistry();
            }

            if (!batchMode)
                Console.ReadLine();
        }

        //Writes the version
        private static void WriteVersion(string version, string spLevel = "")
        {
            version = version.Trim();
            if (string.IsNullOrEmpty(version))
                return;

            string spLevelString = "";
            if (!string.IsNullOrEmpty(spLevel))
                spLevelString = " Service Pack " + spLevel;

            Console.WriteLine($"{version}{spLevelString}");
        }
        private static void Get1To45VersionFromRegistry()
        {
            // Opens the registry key for the .NET Framework entry.
            using (RegistryKey ndpKey = Registry.LocalMachine.OpenSubKey(@"SOFTWARE\Microsoft\NET Framework Setup\NDP\"))
            {
                foreach (string versionKeyName in ndpKey.GetSubKeyNames())
                {
                    // Skip .NET Framework 4.5 version information.
                    if (versionKeyName == "v4")
                    {
                        continue;
                    }

                    if (versionKeyName.StartsWith("v"))
                    {

                        RegistryKey versionKey = ndpKey.OpenSubKey(versionKeyName);
                        // Get the .NET Framework version value.
                        string name = (string)versionKey.GetValue("Version", "");
                        // Get the service pack (SP) number.
                        string sp = versionKey.GetValue("SP", "").ToString();

                        // Get the installation flag, or an empty string if there is none.
                        string install = versionKey.GetValue("Install", "").ToString();
                        if (string.IsNullOrEmpty(install)) // No install info; it must be in a child subkey.
                            WriteVersion(name);
                        else
                        {
                            if (!(string.IsNullOrEmpty(sp)) && install == "1")
                            {
                                WriteVersion(name, sp);
                            }
                        }
                        if (!string.IsNullOrEmpty(name))
                        {
                            continue;
                        }
                        foreach (string subKeyName in versionKey.GetSubKeyNames())
                        {
                            RegistryKey subKey = versionKey.OpenSubKey(subKeyName);
                            name = (string)subKey.GetValue("Version", "");
                            if (!string.IsNullOrEmpty(name))
                                sp = subKey.GetValue("SP", "").ToString();

                            install = subKey.GetValue("Install", "").ToString();
                            if (string.IsNullOrEmpty(install)) //No install info; it must be later.
                                WriteVersion(name);
                            else
                            {
                                if (!(string.IsNullOrEmpty(sp)) && install == "1")
                                {
                                    WriteVersion(name, sp);
                                }
                                else if (install == "1")
                                {
                                    WriteVersion(name);
                                }
                            }
                        }
                    }
                }
            }
        }
        private static void Get45PlusFromRegistry()
        {
            const string subkey = @"SOFTWARE\Microsoft\NET Framework Setup\NDP\v4\Full\";

            using (RegistryKey ndpKey = Registry.LocalMachine.OpenSubKey(subkey))
            {
                if (ndpKey != null && ndpKey.GetValue("Release") != null)
                {
                    WriteVersion(
                        CheckFor45PlusVersion(
                                (int)ndpKey.GetValue("Release")
                            )
                    );
                }
            }

            // Checking the version using >= enables forward compatibility.
            string CheckFor45PlusVersion(int releaseKey)
            {
                if (releaseKey >= 528040)
                    return "4.8";
                if (releaseKey >= 461808)
                    return "4.7.2";
                if (releaseKey >= 461308)
                    return "4.7.1";
                if (releaseKey >= 460798)
                    return "4.7";
                if (releaseKey >= 394802)
                    return "4.6.2";
                if (releaseKey >= 394254)
                    return "4.6.1";
                if (releaseKey >= 393295)
                    return "4.6";
                if (releaseKey >= 379893)
                    return "4.5.2";
                if (releaseKey >= 378675)
                    return "4.5.1";
                if (releaseKey >= 378389)
                    return "4.5";
                // This code should never execute. A non-null release key should mean
                // that 4.5 or later is installed.
                return "";
            }
        }
    }
}