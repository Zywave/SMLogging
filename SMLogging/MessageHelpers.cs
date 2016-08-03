using System;
using System.Diagnostics;
using System.ServiceModel.Channels;
using System.Text.RegularExpressions;
using System.Xml.Linq;

namespace SMLogging
{
    internal class MessageHelpers
    {
        public static bool ExtractMessageId(Message message, out Guid messageId)
        {
            if (message == null) throw new ArgumentNullException(nameof(message));

            messageId = Guid.Empty;

            if (message.State != MessageState.Closed && message.Headers != null)
            {
                return ExtractMessageId(message.Headers, out messageId);
            }

            return false;
        }

        public static bool ExtractMessageId(MessageHeaders headers, out Guid messageId)
        {
            if (headers == null) throw new ArgumentNullException(nameof(headers));

            messageId = Guid.Empty;

            if (headers.MessageId != null)
            {
                return headers.MessageId.TryGetGuid(out messageId);
            }

            return false;
        }

        public static bool ExtractActivityAndCorrelationId(Message message, out Guid activityId, out Guid correlationId)
        {
            if (message == null) throw new ArgumentNullException(nameof(message));

            activityId = Guid.Empty;
            correlationId = Guid.Empty;
            
            if (message.State != MessageState.Closed && message.Headers != null)
            {
                return ExtractActivityAndCorrelationId(message.Headers, out activityId, out correlationId);
            }

            return false;
        }

        public static bool ExtractActivityAndCorrelationId(MessageHeaders headers, out Guid activityId, out Guid correlationId)
        {
            if (headers == null) throw new ArgumentNullException(nameof(headers));

            activityId = Guid.Empty;
            correlationId = Guid.Empty;

            try
            {
                var index = headers.FindHeader("ActivityId", "http://schemas.microsoft.com/2004/09/ServiceModel/Diagnostics");
                if (index >= 0)
                {
                    using (var reader = headers.GetReaderAtHeader(index))
                    {
                        correlationId = new Guid(reader.GetAttribute("CorrelationId", null));
                        activityId = reader.ReadElementContentAsGuid();
                        return true;
                    }
                }
            }
            catch
            {
                if (Debugger.IsAttached)
                {
                    throw;
                }
            }

            return false;
        }

        public static bool ExtractRemoteEndpointAddress(Message message, out string address)
        {
            if (message == null) throw new ArgumentNullException(nameof(message));

            address = null;

            if (message.State != MessageState.Closed & message.Properties != null)
            {
                return ExtractRemoteEndpointAddress(message.Properties, out address);
            }

            return false;
        }

        public static bool ExtractRemoteEndpointAddress(MessageProperties properties, out string address)
        {
            if (properties == null) throw new ArgumentNullException(nameof(properties));

            address = null;

            if (properties.ContainsKey(RemoteEndpointMessageProperty.Name))
            {
                var remoteEndpoint = properties[RemoteEndpointMessageProperty.Name] as RemoteEndpointMessageProperty;
                if (!String.IsNullOrWhiteSpace(remoteEndpoint?.Address))
                {
                    address = remoteEndpoint.Address;
                    return true;
                }
            }

            return false;
        }

        public static string GetFaultCode(string messageContent)
        {
            var faultCode = "UnknownFault";

            try
            {
                var document = XDocument.Parse(messageContent);
                var ns = document.Root?.Name.Namespace;
                var faultCodeElement = document.Root?.Element(ns + "Body")?.Element(ns + "Fault")?.Element(ns + "Code");
                if (faultCodeElement != null)
                {
                    faultCode = GetFaultCode(faultCodeElement);
                }
            }
            catch
            {
                if (Debugger.IsAttached)
                {
                    throw;
                }
            }

            return faultCode;
        }

        private static string GetFaultCode(XElement faultCodeElement)
        {
            var ns = faultCodeElement.Name.Namespace;
            var code = faultCodeElement.Element(ns + "Value")?.Value;

            if (code != null)
            {
                code = code.Substring(code.IndexOf(":", StringComparison.Ordinal) + 1);
                code = Regex.Replace(code, @"\s+", "_");
            }

            var subFaultCodeElement = faultCodeElement.Element(ns + "Subcode");
            if (subFaultCodeElement != null)
            {
                code += ":" + GetFaultCode(subFaultCodeElement);
            }

            return code;
        }
    }
}
