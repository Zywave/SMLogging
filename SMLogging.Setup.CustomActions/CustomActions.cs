using System;
using System.IO;
using System.Linq;
using System.Security.AccessControl;
using System.Security.Principal;
using System.Xml.Linq;
using System.Xml.XPath;
using Microsoft.Deployment.WindowsInstaller;

namespace SMLogging.Setup.CustomActions
{
    public class CustomActions
    {
        #region Request Logging

        [CustomAction]
        public static ActionResult InstallRequestLoggingMachinConfig(Session session)
        {
            session.Log($"Begin {nameof(InstallRequestLoggingMachinConfig)}");

            RegisterRequestLogging(MachineConfig32v2Path, session);
            RegisterRequestLogging(MachineConfig32v4Path, session);
            if (Environment.Is64BitOperatingSystem)
            {
                RegisterRequestLogging(MachineConfig64v2Path, session);
                RegisterRequestLogging(MachineConfig64v4Path, session);
            }

            session.Log($"End {nameof(InstallRequestLoggingMachinConfig)}");
            return ActionResult.Success;
        }

        [CustomAction]
        public static ActionResult UninstallRequestLoggingMachinConfig(Session session)
        {
            session.Log($"Begin {nameof(InstallRequestLoggingMachinConfig)}");

            UnregisterRequestLogging(MachineConfig32v2Path, session);
            UnregisterRequestLogging(MachineConfig32v4Path, session);
            if (Environment.Is64BitOperatingSystem)
            {
                UnregisterRequestLogging(MachineConfig64v2Path, session);
                UnregisterRequestLogging(MachineConfig64v4Path, session);
            }

            session.Log($"End {nameof(InstallRequestLoggingMachinConfig)}");
            return ActionResult.Success;
        }

        private static void RegisterRequestLogging(string path, Session session)
        {
            var data = session.CustomActionData;

            if (!File.Exists(path))
            {
                session.Log($"Machine.config file ({path}) not found");
                return;
            }

            var document = XDocument.Load(path);
            
            var behaviorExtensionsElement = GetOrAddElement(document.Root, "system.serviceModel", "extensions", "behaviorExtensions");
            if (behaviorExtensionsElement.XPathSelectElement($"add[@name='{RequestLoggingBehaviorName}']") == null)
            {
                behaviorExtensionsElement.Add(new XElement("add", new XAttribute("name", RequestLoggingBehaviorName), new XAttribute("type", String.Format(RequestLoggingBehaviorTypeFormat, data["ProductVersion"]))));
            }

            var serviceBehaviorsElement = GetOrAddElement(document.Root, "system.serviceModel", "commonBehaviors", "serviceBehaviors");
            if (serviceBehaviorsElement.Element(RequestLoggingBehaviorName) == null)
            {
                serviceBehaviorsElement.Add(new XElement(RequestLoggingBehaviorName, 
                    new XAttribute("createBufferedMessageCopy", data["CreateBufferedMessageCopy"]),
                    new XAttribute("ignoreDispatchReplyMessage", data["IgnoreDispatchReplyMessage"])));
            }

            var endpointBehaviorsElement = GetOrAddElement(document.Root, "system.serviceModel", "commonBehaviors", "endpointBehaviors");
            if (endpointBehaviorsElement.Element(RequestLoggingBehaviorName) == null)
            {
                endpointBehaviorsElement.Add(new XElement(RequestLoggingBehaviorName,
                    new XAttribute("createBufferedMessageCopy", data["CreateBufferedMessageCopy"])));
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
                            new XAttribute("type", String.Format(BackgroundFileTraceListenerTypeFormat, data["ProductVersion"])),
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
            document.Root?.XPathSelectElement($"system.serviceModel/commonBehaviors/serviceBehaviors/{RequestLoggingBehaviorName}")?.Remove();
            document.Root?.XPathSelectElement($"system.serviceModel/commonBehaviors/endpointBehaviors/{RequestLoggingBehaviorName}")?.Remove();
            document.Root?.XPathSelectElement($"system.diagnostics/sources/source[@name='{RequestLoggingSourceName}']")?.Remove();

            document.Save(path);
        }

        #endregion

        #region Error Logging

        [CustomAction]
        public static ActionResult InstallErrorLoggingMachinConfig(Session session)
        {
            session.Log($"Begin {nameof(InstallErrorLoggingMachinConfig)}");

            RegisterErrorLogging(MachineConfig32v2Path, session);
            RegisterErrorLogging(MachineConfig32v4Path, session);
            if (Environment.Is64BitOperatingSystem)
            {
                RegisterErrorLogging(MachineConfig64v2Path, session);
                RegisterErrorLogging(MachineConfig64v4Path, session);
            }

            session.Log($"End {nameof(InstallErrorLoggingMachinConfig)}");
            return ActionResult.Success;
        }

        [CustomAction]
        public static ActionResult UninstallErrorLoggingMachinConfig(Session session)
        {
            session.Log($"Begin {nameof(InstallErrorLoggingMachinConfig)}");

            UnregisterErrorLogging(MachineConfig32v2Path, session);
            UnregisterErrorLogging(MachineConfig32v4Path, session);
            if (Environment.Is64BitOperatingSystem)
            {
                UnregisterErrorLogging(MachineConfig64v2Path, session);
                UnregisterErrorLogging(MachineConfig64v4Path, session);
            }

            session.Log($"End {nameof(InstallErrorLoggingMachinConfig)}");
            return ActionResult.Success;
        }

        private static void RegisterErrorLogging(string path, Session session)
        {
            var data = session.CustomActionData;

            if (!File.Exists(path))
            {
                session.Log($"Machine.config file ({path}) not found");
                return;
            }

            var document = XDocument.Load(path);

            var behaviorExtensionsElement = GetOrAddElement(document.Root, "system.serviceModel", "extensions", "behaviorExtensions");
            if (behaviorExtensionsElement.XPathSelectElement($"add[@name='{ErrorLoggingBehaviorName}']") == null)
            {
                behaviorExtensionsElement.Add(new XElement("add", new XAttribute("name", ErrorLoggingBehaviorName), new XAttribute("type", String.Format(ErrorLoggingBehaviorTypeFormat, data["ProductVersion"]))));
            }

            var behaviorsElement = GetOrAddElement(document.Root, "system.serviceModel", "commonBehaviors", "serviceBehaviors");
            if (behaviorsElement.Element(ErrorLoggingBehaviorName) == null)
            {
                behaviorsElement.Add(new XElement(ErrorLoggingBehaviorName));
            }

            var sourcesElement = GetOrAddElement(document.Root, "system.diagnostics", "sources");
            if (sourcesElement.XPathSelectElement($"source[@name='{ErrorLoggingSourceName}']") == null)
            {
                sourcesElement.Add(new XElement("source",
                    new XAttribute("name", ErrorLoggingSourceName),
                    new XAttribute("switchValue", ErrorLoggingSourceSwitchValue),
                    new XElement("listeners",
                        new XElement("remove",
                            new XAttribute("name", "Default")),
                        new XElement("add",
                            new XAttribute("name", FileTraceListenerName),
                            new XAttribute("type", String.Format(BackgroundFileTraceListenerTypeFormat, data["ProductVersion"])),
                            new XAttribute("initializeData", Path.Combine(data["ErrorLoggingPathRoot"], data["ErrorLoggingPath"])),
                            new XAttribute("rollingMode", data["ErrorLoggingRollingMode"]),
                            new XAttribute("rollingInterval", data["ErrorLoggingRollingInterval"]),
                            new XAttribute("maximumFileSize", data["ErrorLoggingMaximumFileSize"]),
                            new XAttribute("maximumFileIndex", data["ErrorLoggingMaximumFileIndex"])))));
            }

            document.Save(path);

            SetupDirectory(data["ErrorLoggingPathRoot"]);
        }

        private static void UnregisterErrorLogging(string path, Session session)
        {
            if (!File.Exists(path))
            {
                session.Log($"Machine.config file ({path}) not found");
                return;
            }

            var document = XDocument.Load(path);

            document.Root?.XPathSelectElement($"system.serviceModel/extensions/behaviorExtensions/add[@name='{ErrorLoggingBehaviorName}']")?.Remove();
            document.Root?.XPathSelectElement($"system.serviceModel/commonBehaviors/serviceBehaviors/{ErrorLoggingBehaviorName}")?.Remove();
            document.Root?.XPathSelectElement($"system.diagnostics/sources/source[@name='{ErrorLoggingSourceName}']")?.Remove();

            document.Save(path);
        }

        #endregion

        #region Activity Propagation

        [CustomAction]
        public static ActionResult EnableEndToEndTracingMachineConfig(Session session)
        {
            session.Log($"Begin {nameof(EnableEndToEndTracingMachineConfig)}");

            EnableEndToEndTracing(MachineConfig32v4Path, session);
            if (Environment.Is64BitOperatingSystem)
            {
                EnableEndToEndTracing(MachineConfig64v4Path, session);
            }

            session.Log($"End {nameof(EnableEndToEndTracingMachineConfig)}");
            return ActionResult.Success;
        }

        private static void EnableEndToEndTracing(string path, Session session)
        {
            if (!File.Exists(path))
            {
                session.Log($"Machine.config file ({path}) not found");
                return;
            }

            var document = XDocument.Load(path);
            
            var endToEndTracingElement = GetOrAddElement(document.Root, "system.serviceModel", "diagnostics", "endToEndTracing");
            endToEndTracingElement.SetAttributeValue("propagateActivity", "true");
            endToEndTracingElement.SetAttributeValue("activityTracing", "true");
            endToEndTracingElement.SetAttributeValue("messageFlowTracing", "true");

            document.Save(path);
        }

        #endregion

        #region Helpers

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

        #endregion

        #region Constants

        private static readonly string WindowsPath = Environment.GetFolderPath(Environment.SpecialFolder.Windows);
        private static readonly string MachineConfig32v2Path = Path.Combine(WindowsPath, @"Microsoft.NET\Framework\v2.0.50727\Config\machine.config");
        private static readonly string MachineConfig64v2Path = Path.Combine(WindowsPath, @"Microsoft.NET\Framework64\v2.0.50727\Config\machine.config");
        private static readonly string MachineConfig32v4Path = Path.Combine(WindowsPath, @"Microsoft.NET\Framework\v4.0.30319\Config\machine.config");
        private static readonly string MachineConfig64v4Path = Path.Combine(WindowsPath, @"Microsoft.NET\Framework64\v4.0.30319\Config\machine.config");

        private const string RequestLoggingBehaviorName = "requestLogging";
        private const string RequestLoggingBehaviorTypeFormat = "SMLogging.RequestLoggingBehaviorExtension, SMLogging, Version={0}.0, Culture=neutral, PublicKeyToken=ddc81ec55fc35caf";
        private const string RequestLoggingSourceName = "System.ServiceModel.RequestLogging";
        private const string RequestLoggingSourceSwitchValue = "Information";
        private const string ErrorLoggingBehaviorName = "errorLogging";
        private const string ErrorLoggingBehaviorTypeFormat = "SMLogging.ErrorLoggingBehaviorExtension, SMLogging, Version={0}.0, Culture=neutral, PublicKeyToken=ddc81ec55fc35caf";
        private const string ErrorLoggingSourceName = "System.ServiceModel.ErrorLogging";
        private const string ErrorLoggingSourceSwitchValue = "Error";
        private const string FileTraceListenerName = "File";
        private const string BackgroundFileTraceListenerTypeFormat = "SMLogging.BackgroundFileTraceListener, SMLogging, Version={0}.0, Culture=neutral, PublicKeyToken=ddc81ec55fc35caf";

        #endregion
    }
}
