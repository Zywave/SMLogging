using System;
using System.Diagnostics;
using System.Net;
using System.Net.Sockets;
using System.ServiceModel;
using System.ServiceModel.Channels;
using System.ServiceModel.Dispatcher;
using System.Xml;

namespace SMLogging
{
    using System.Globalization;
    using System.IO;
    using System.Runtime.Serialization;
    using System.Text;
    using SMLogging.Properties;

    /// <summary>
    /// Represents an error handler for logging service errors.
    /// </summary>
    /// <seealso cref="IErrorHandler" />
    public class ErrorLoggingErrorHandler : IErrorHandler
    {
        /// <summary>
        /// Initializes a new instance of the <see cref="ErrorLoggingErrorHandler"/> class.
        /// </summary>
        public ErrorLoggingErrorHandler()
        {
            TraceSource = new TraceSource("System.ServiceModel.ErrorLogging");
        }

        /// <summary>
        /// Gets the trace source.
        /// </summary>
        public TraceSource TraceSource { get; }

        /// <summary>
        /// Enables error-related processing and returns a value that indicates whether the dispatcher aborts the session and the instance 
        /// context in certain cases.
        /// </summary>
        /// <param name="error">The exception thrown during processing.</param>
        /// <returns>
        /// true if Windows Communication Foundation (WCF) should not abort the session (if there is one) and instance context if the instance
        ///  context is not <see cref="System.ServiceModel.InstanceContextMode.Single" />; otherwise, false. The default is false.
        /// </returns>
        public bool HandleError(Exception error)
        {
            if (error != null)
            {
                ErrorTraceData data = null;
                if (error.Data.Contains(_dataKey))
                {
                    data = error.Data[_dataKey] as ErrorTraceData;
                }

                data = data ?? new ErrorTraceData();

                data.ApplicationName = _applicationName;
                data.MachineName = _machineName;
                data.MachineIpAddress = _machineIpAddress;

                TraceError(data, error);
            }

            return false;
        }

        /// <summary>
        /// Enables the creation of a custom <see cref="System.ServiceModel.FaultException{T}" /> that is returned from an exception in the course of a service method.
        /// </summary>
        /// <param name="error">The <see cref="System.Exception" /> object thrown in the course of the service operation.</param>
        /// <param name="version">The SOAP version of the message.</param>
        /// <param name="fault">The <see cref="System.ServiceModel.Channels.Message" /> object that is returned to the client, or service, in the duplex case.</param>
        public void ProvideFault(Exception error, MessageVersion version, ref Message fault)
        {
            if (error != null)
            {
                var data = new ErrorTraceData();

                var operationContext = OperationContext.Current;

                if (operationContext?.IncomingMessageHeaders != null)
                {
                    Guid activityId;
                    Guid correlationId;
                    if (MessageHelpers.ExtractActivityAndCorrelationId(operationContext.IncomingMessageHeaders, out activityId, out correlationId))
                    {
                        data.ActivityId = activityId;
                        data.CorrelationId = correlationId;
                    }

                    Guid messageId;
                    if (MessageHelpers.ExtractMessageId(operationContext.IncomingMessageHeaders, out messageId))
                    {
                        data.MessageId = messageId;
                    }

                    data.Target = operationContext.IncomingMessageHeaders.To;
                    data.Action = operationContext.IncomingMessageHeaders.Action;
                }
                
                if (operationContext?.IncomingMessageProperties != null)
                {
                    string remoteEndpointAddress;
                    if (MessageHelpers.ExtractRemoteEndpointAddress(operationContext.IncomingMessageProperties, out remoteEndpointAddress))
                    {
                        data.ClientIpAddress = remoteEndpointAddress;
                    }
                }

                error.Data[_dataKey] = data;
            }
        }

        private void TraceError(ErrorTraceData data, Exception error)
        {
            TraceSource.TraceData(TraceEventType.Error, 0,
                data.ActivityId,
                data.CorrelationId,
                data.MessageId,
                data.ClientIpAddress ?? "0.0.0.0",
                data.ApplicationName ?? "null",
                data.MachineName ?? "null",
                data.MachineIpAddress ?? "0.0.0.0",
                data.Target?.Scheme ?? "null",
                data.Target?.Host ?? "null",
                data.Target?.Port ?? 0,
                data.Target?.ToString() ?? "null",
                data.Action ?? "null",
                GetErrorMessage(error));
        }

        private static string GetErrorMessage(Exception error)
        {
            var messageBuilder = new StringBuilder(error.ToString());

            var faultException = error as FaultException;
            if (faultException != null)
            {
                try
                {
                    Type exceptionType1 = null;
                    for (var exceptionType2 = faultException.GetType(); exceptionType2 != typeof(FaultException) && exceptionType2 != null; exceptionType2 = exceptionType2.BaseType)
                    {
                        if (exceptionType2.IsGenericType && exceptionType2.GetGenericTypeDefinition() == typeof(FaultException<>))
                        {
                            exceptionType1 = exceptionType2;
                            break;
                        }
                    }

                    Type detailType = null;
                    if (exceptionType1 != null)
                    {
                        detailType = exceptionType1.GetGenericArguments()[0];
                    }
                    if (detailType != null)
                    {
                        var detail = typeof(FaultException<>).MakeGenericType(detailType).GetProperty("Detail").GetValue(faultException, null);
                        if (detail != null)
                        {
                            var serializer = new DataContractSerializer(detailType, null, int.MaxValue, false, false, null);
                            messageBuilder.AppendLine().AppendLine(AssemblyResources.FaultDetailHeader);
                            using (var stringWriter = new StringWriter(messageBuilder, CultureInfo.InvariantCulture))
                            using (var xmlWriter = XmlWriter.Create(stringWriter))
                            {
                                serializer.WriteObject(xmlWriter, detail);
                            }
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
            }

            return messageBuilder.ToString();
        }

        private static readonly object _dataKey = new object();

        private static readonly string _machineName;
        private static readonly string _machineIpAddress;
        private static readonly string _applicationName;

        static ErrorLoggingErrorHandler()
        {
            _machineName = Dns.GetHostName();

            var host = Dns.GetHostEntry(Dns.GetHostName());
            foreach (var ip in host.AddressList)
            {
                if (ip.AddressFamily == AddressFamily.InterNetwork)
                {
                    _machineIpAddress = ip.ToString();
                    break; ;
                }
            }

            _applicationName = AppDomain.CurrentDomain.FriendlyName;
        }
    }
}
