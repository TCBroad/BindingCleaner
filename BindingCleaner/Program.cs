namespace BindingCleaner
{
    using System;
    using System.Collections.Generic;
    using System.IO;
    using System.Linq;
    using System.Text;
    using System.Xml;
    using System.Xml.Linq;
    using System.Xml.Serialization;

    using BindingCleaner.Annotations;

    [UsedImplicitly]
    internal class Program
    {
        private static readonly XNamespace Ns = "urn:schemas-microsoft-com:asm.v1";

        private static void Main(string[] args)
        {
            Splash();

            if (args.Length == 0)
            {
                Error("Usage: BindingCleaner <path to config file>");

                return;
            }

            var xml = ReadXml(args[0]);
            if (ProcessRedirects(xml))
            {
                SaveXml(xml, args[0]);
            }
        }

        private static void SaveXml(XDocument xml, string filename)
        {
            try
            {
                Log("backing up old config...", true, ConsoleColor.Cyan);
                File.Copy(filename, filename + ".bak");

                Log("saving new config...", true, ConsoleColor.Magenta);
                using (var file = File.Create(filename))
                {
                    xml.Save(file, SaveOptions.None);
                    file.Flush();
                }
            }
            catch (Exception ex)
            {
                Error(string.Format("Unable to save file: {0}", ex.Message), true);
            }
        }

        private static bool ProcessRedirects(XDocument xml)
        {
            var root = xml.Root;
            if (root == null)
            {
                Error("Unable to parse XML - no root element found", true);

                return false;
            }

            var runtime = root.Element("runtime");
            if (runtime == null)
            {
                Error("No runtime element found, exiting", true);

                return false;
            }

            var bindings = runtime.Element(Ns + "assemblyBinding");
            if (bindings == null)
            {
                Error("No assemblyBinding element found, exiting", true);

                return false;
            }

            var redirects = GetRedirectList(bindings);
            RemoveDuplicates(redirects);

            var fixedRedirects = SerializeBinding(redirects);
            bindings.ReplaceWith(fixedRedirects);

            return true;
        }

        private static void RemoveDuplicates(AssemblyBinding redirects)
        {
            var cleaned = new List<DependentAssembly>();
            var groups = redirects.DependentAssemblies.GroupBy(x => x.AssemblyIdentity.Name);
            foreach (var @group in groups)
            {
                Log(string.Format("Found {0} versions of {1}...", group.Count(), group.Key));
                if (group.Count() == 1)
                {
                    var single = group.Single();
                    Log(string.Format("{0}", single.BindingRedirect.NewVersion), true, ConsoleColor.Yellow);
                    cleaned.Add(single);

                    continue;
                }

                var highestVersion = group.OrderByDescending(x => x.BindingRedirect.NewVersion).First();
                
                Log(string.Format("{0}", highestVersion.BindingRedirect.NewVersion), true, ConsoleColor.Green);
                cleaned.Add(highestVersion);
            }

            redirects.DependentAssemblies = cleaned;
        }

        private static AssemblyBinding GetRedirectList(XElement element)
        {
            var serializer = new XmlSerializer(typeof(AssemblyBinding));
            if (!serializer.CanDeserialize(element.CreateReader()))
            {
                Error("Unable to deserialize assembly bindings, please ensure xml is valid", true);

                return null;
            }

            return serializer.Deserialize(element.CreateReader()) as AssemblyBinding;
        }

        private static XElement SerializeBinding(AssemblyBinding binding)
        {
            var xmlBindings =
                binding.DependentAssemblies.Select(
                                                   x =>
                                                       new XElement(
                                                       Ns + "dependentAssembly",
                                                       GetAssemblyIdentityXElement(x.AssemblyIdentity),
                                                       GetBindingRedirectXElement(x.BindingRedirect)));


            var xml = new XElement(Ns + "assemblyBinding", xmlBindings);

            return xml;
        }

        private static XElement GetAssemblyIdentityXElement(AssemblyIdentity identity)
        {
            return new XElement(
                Ns + "assemblyIdentity",
                new XAttribute("name", identity.Name),
                new XAttribute("publicKeyToken", identity.PublicKeyToken),
                string.IsNullOrEmpty(identity.Culture) ? null : new XAttribute("culture", identity.Culture));
        }

        private static XElement GetBindingRedirectXElement(BindingRedirect redirect)
        {
            return new XElement(Ns + "bindingRedirect", new XAttribute("oldVersion", redirect.OldVersion), new XAttribute("newVersion", redirect.NewVersion));
        }

        private static XDocument ReadXml(string filename)
        {
            var xml = File.ReadAllText(filename);
            
            return XDocument.Parse(xml);
        }

        private static void Error(string message, bool fatal = false)
        {
            Console.ForegroundColor = ConsoleColor.Red;
            Console.WriteLine(message);

            Console.ForegroundColor = ConsoleColor.Gray;

            if (fatal)
            {
                Environment.Exit(1);
            }
        }

        private static void Log(string message, bool writeLine = false, ConsoleColor colour = ConsoleColor.Gray)
        {
            Console.ForegroundColor = colour;
            if (writeLine)
            {
                Console.WriteLine(message);
            }
            else
            {
                Console.Write(message);
            }

            Console.ForegroundColor = ConsoleColor.Gray;
        }

        private static void Splash()
        {
            Console.WindowWidth = 140;
            Console.BufferWidth = 140;
            Console.WindowHeight = 50;
            Console.BufferHeight = 50;

            Log("\n ▀█████████▄   ▄█  ███▄▄▄▄   ████████▄   ▄█  ███▄▄▄▄  *   ▄██████▄     *          *            *              *            *         ", true, ConsoleColor.Magenta);
            Log("  *███    ███ ███  ███▀▀▀██▄ ███   ▀███ ███  ███▀▀▀██▄   ███    ███                                      *            *          *   ", true, ConsoleColor.Red);
            Log("   ███   *███ ███▌ ███ * ███ ███    ███ ███▌ ███   ███   ███    █▀                             *                                     ", true, ConsoleColor.Yellow);
            Log("  ▄███▄▄▄██▀  ███▌ ███   ███ ███    ███ ███▌ ███   ███  ▄███ *                        *                                              ", true, ConsoleColor.White);
            Log(" ▀▀███▀▀▀██▄  ███▌ ███   ███ ███   *███ ███▌ ███   ███ ▀▀███ ████▄           *                                                       ", true, ConsoleColor.Cyan);
            Log("   ███    ██▄ ███  ███   ███ ███    ███ ███  ███   ███   ███    ███  *                                *          *                   ", true, ConsoleColor.Blue);
            Log("   ███    ███ ███  ███   ███ ███   ▄███ ███  ███   ███   ███    ███                      *                                  *       *", true, ConsoleColor.Gray);
            Log(" ▄█████████▀  █▀    ▀█   █▀  ████████▀  █▀    ▀█   █▀    ████████▀                                           *                       \n", true, ConsoleColor.DarkGray);

            Log("               *         *        *                     ▄████████  ▄█          ▄████████    ▄████████ ███▄▄▄▄    * ▄████████    ▄████████", true, ConsoleColor.DarkRed);
            Log("     *                                         *       ███    ███ ███    *    ███    ███   ███    ███ ███▀▀▀██▄   ███    ███   ███    ███", true, ConsoleColor.DarkRed);
            Log("  *                *                   *               ███    █▀  ███         ███    █▀ *  ███    ███ ███   ███   ███    █▀    ███    ███", true, ConsoleColor.DarkMagenta);
            Log("                                *                      ███        ███   *    ▄███▄▄▄       ███    ███ ███   ███  ▄███▄▄▄      ▄███▄▄▄▄██▀", true, ConsoleColor.Magenta);
            Log("       *              *                  *             ███  *     ███       ▀▀███▀▀▀     ▀███████████ ███   ███ ▀▀███▀▀▀     ▀▀███▀▀▀▀▀  ", true, ConsoleColor.DarkMagenta);
            Log("  *           *                                 *      ███    █▄  ███         ███    █▄    ███    ███ ███   ███   ███    █▄  ▀███████████", true, ConsoleColor.Red);
            Log("                                   *                   ███    ███ ███▌    ▄   ███    ███   ███    ███ ███   ███  *███    ███   ███    ███", true, ConsoleColor.Red);
            Log("                                                       ████████▀  █████▄▄██   ██████████   ███    █▀   ▀█   █▀    ██████████   ███    ███", true, ConsoleColor.DarkRed);
            Log("      *        *        *                   *                     ▀                 *                                        * ███    ███", true, ConsoleColor.DarkRed);  
        }
    }
}