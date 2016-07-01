using System;
using System.IO;
using System.Linq;
using System.Security.AccessControl;
using System.Security.Principal;
using System.Xml.Linq;
using System.Xml.XPath;
using Microsoft.Deployment.WindowsInstaller;

namespace SMRequestLogging.Setup.CustomActions
{
    public class CustomActions
    {
        [CustomAction]
        public static ActionResult InstallRequestLoggingMachinConfig(Session session)
        {
            session.Log($"Begin {nameof(InstallRequestLoggingMachinConfig)}");

            RegisterRequestLogging(MachineConfig32Path, session);
            if (Environment.Is64BitOperatingSystem)
            {
                RegisterRequestLogging(MachineConfig64Path, session);
            }

            session.Log($"End {nameof(InstallRequestLoggingMachinConfig)}");
            return ActionResult.Success;
        }

        [CustomAction]
        public static ActionResult UninstallRequestLoggingMachinConfig(Session session)
        {
            session.Log($"Begin {nameof(InstallRequestLoggingMachinConfig)}");

            UnregisterRequestLogging(MachineConfig32Path, session);
            if (Environment.Is64BitOperatingSystem)
            {
                UnregisterRequestLogging(MachineConfig64Path, session);
            }

            session.Log($"End {nameof(InstallRequestLoggingMachinConfig)}");
            return ActionResult.Success;
        }

        private static void RegisterRequestLogging(string path, Session session)
        {
            var data = session.CustomActionData;

            session.Log($"**CUSTOMACTIONDATA = {session["CustomActionData"]}");

            if (!File.Exists(path))
            {
                session.Log($"Machine.config file ({path}) not found");
                return;
            }

            var document = XDocument.Load(path);
            
            var behaviorExtensionsElement = GetOrAddElement(document.Root, "system.serviceModel", "extensions", "behaviorExtensions");
            if (behaviorExtensionsElement.XPathSelectElement($"add[@name='{RequestLoggingBehaviorName}']") == null)
            {
                behaviorExtensionsElement.Add(new XElement("add", new XAttribute("name", RequestLoggingBehaviorName), new XAttribute("type", RequestLoggingBehaviorType)));
            }

            var behaviorElement = GetOrAddElement(document.Root, "system.serviceModel", "behaviors", "serviceBehaviors", "behavior");
            if (behaviorElement.Element(RequestLoggingBehaviorName) == null)
            {
                behaviorElement.Add(new XElement(RequestLoggingBehaviorName));
            }
            
            var sourcesElement = GetOrAddElement(document.Root, "system.diagnostics", "sources");
            if (sourcesElement.XPathSelectElement($"source[@name='{RequestLoggingSourceName}']") == null)
            {
                sourcesElement.Add(new XElement("source", 
                    new XAttribute("name", RequestLoggingSourceName), 
                    new XAttribute("switchValue", RequestLoggingSourceSwitchValue),
                    new XElement("listeners", 
                        new XElement("remove", 
                            new XAttribute("name", "Default")),
                        new XElement("add",
                            new XAttribute("name", FileTraceListenerName),
                            new XAttribute("type", BackgroundFileTraceListenerType),
                            new XAttribute("initializeData", Path.Combine(data["RequestLoggingPathRoot"], data["RequestLoggingPath"])),
                            new XAttribute("rollingMode", data["RequestLoggingRollingMode"]),
                            new XAttribute("rollingInterval", data["RequestLoggingRollingInterval"]),
                            new XAttribute("maximumFileSize", data["RequestLoggingMaximumFileSize"]),
                            new XAttribute("maximumFileIndex", data["RequestLoggingMaximumFileIndex"])))));
            }
            
            document.Save(path);
            
            SetupDirectory(data["RequestLoggingPathRoot"]);
        }

        private static void UnregisterRequestLogging(string path, Session session)
        {
            if (!File.Exists(path))
            {
                session.Log($"Machine.config file ({path}) not found");
                return;
            }

            var document = XDocument.Load(path);

            document.Root?.XPathSelectElement($"system.serviceModel/extensions/behaviorExtensions/add[@name='{RequestLoggingBehaviorName}']")?.Remove(); 
            document.Root?.XPathSelectElement($"system.serviceModel/behaviors/serviceBehaviors/behavior/{RequestLoggingBehaviorName}")?.Remove();
            document.Root?.XPathSelectElement($"system.diagnostics/sources/source[@name='{RequestLoggingSourceName}']")?.Remove();

            document.Save(path);
        }

        private static XElement GetOrAddElement(XElement parent, params string[] names)
        {
            if (names == null || names.Length == 0)
            {
                return parent;
            }

            var element = parent.Element(names[0]);
            if (element == null)
            {
                element = new XElement(names[0]);
                parent.Add(element);
            }

            return GetOrAddElement(element, names.Skip(1).ToArray());
        }

        private static void SetupDirectory(string path)
        {
            path = Environment.ExpandEnvironmentVariables(path);

            var directory = Directory.CreateDirectory(path);
            var dac = directory.GetAccessControl();
            dac.AddAccessRule(new FileSystemAccessRule(new SecurityIdentifier(WellKnownSidType.WorldSid, null), FileSystemRights.FullControl, InheritanceFlags.ObjectInherit | InheritanceFlags.ContainerInherit, PropagationFlags.NoPropagateInherit, AccessControlType.Allow));
            directory.SetAccessControl(dac);
        }

        private const string FrameworkVersion = @"v4.0.30319";
        private static readonly string WindowsPath = Environment.GetFolderPath(Environment.SpecialFolder.Windows);
        private static readonly string MachineConfig32Path = Path.Combine(WindowsPath, "Microsoft.NET", "Framework", FrameworkVersion, "Config", "machine.config");
        private static readonly string MachineConfig64Path = Path.Combine(WindowsPath, "Microsoft.NET", "Framework64", FrameworkVersion, "Config", "machine.config");

        private const string RequestLoggingBehaviorName = "requestLogging";
        private const string RequestLoggingBehaviorType = "SMRequestLogging.RequestLoggingBehaviorExtension, SMRequestLogging, Version=1.0.0.0, Culture=neutral, PublicKeyToken=e8230e20520e9f79";
        private const string RequestLoggingSourceName = "System.ServiceModel.RequestLogging";
        private const string RequestLoggingSourceSwitchValue = "Information";
        private const string FileTraceListenerName = "File";
        private const string BackgroundFileTraceListenerType = "SMRequestLogging.BackgroundFileTraceListener, SMRequestLogging, Version=1.0.0.0, Culture=neutral, PublicKeyToken=e8230e20520e9f79";


    }
}
