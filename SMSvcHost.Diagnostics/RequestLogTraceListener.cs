using System;
using System.Diagnostics;
using System.Globalization;
using System.Runtime.InteropServices;

namespace SMSvcHost.Diagnostics
{
    /// <summary>
    /// Represents a <see cref="TraceListener"/> that writes writes IIS style request logs for net.tcp and net.pipe WCF service calls.
    /// </summary>
    public class RequestLogTraceListener : MultiFileTraceListener
    {
        #region Public Constructors

        /// <summary>
        /// Initializes a new instance of the <see cref="RequestLogTraceListener"/> class.
        /// </summary>
        /// <param name="path">The file path.</param>
        public RequestLogTraceListener(string path)
            : base(path)
        {
        }

        /// <summary>
        /// Initializes a new instance of the <see cref="RequestLogTraceListener"/> class.
        /// </summary>
        /// <param name="path">The file path.</param>
        /// <param name="name">The trace listener name.</param>
        public RequestLogTraceListener(string path, string name)
            : base(path, name)
        {
        }

        #endregion

        #region Public Methods

        /// <summary>
        /// Writes trace information, a data object and event information to the listener specific output.
        /// </summary>
        /// <param name="eventCache">A <see cref="System.Diagnostics.TraceEventCache"/> object that contains the current process ID, thread ID, and stack trace information.</param>
        /// <param name="source">A name used to identify the output, typically the name of the application that generated the trace event.</param>
        /// <param name="eventType">One of the <see cref="System.Diagnostics.TraceEventType"/> values specifying the type of event that has caused the trace.</param>
        /// <param name="id">A numeric identifier for the event.</param>
        /// <param name="data">The trace data to emit.</param>
        [ComVisible(false)]
        public override void TraceData(TraceEventCache eventCache, string source, TraceEventType eventType, int id, object data)
        {
            EnsureInitialized();

            if (ShouldTrace(eventCache, source, eventType, id, null, null, data, null))
            {
                // resolve message

                WriteTrace(eventCache, source, eventType, id, message);
            }
        }

        /// <summary>
        /// Writes trace information, an array of data objects and event information to the listener specific output.
        /// </summary>
        /// <param name="eventCache">A <see cref="System.Diagnostics.TraceEventCache"/> object that contains the current process ID, thread ID, and stack trace information.</param>
        /// <param name="source">A name used to identify the output, typically the name of the application that generated the trace event.</param>
        /// <param name="eventType">One of the <see cref="System.Diagnostics.TraceEventType"/> values specifying the type of event that has caused the trace.</param>
        /// <param name="id">A numeric identifier for the event.</param>
        /// <param name="data">The trace data to emit.</param>
        [ComVisible(false)]
        public override void TraceData(TraceEventCache eventCache, string source, TraceEventType eventType, int id, params object[] data)
        {
            EnsureInitialized();

            if (ShouldTrace(eventCache, source, eventType, id, null, null, null, data))
            {
                // resolve message

                WriteTrace(eventCache, source, eventType, id, message);
            }
        }

        /// <summary>
        /// Writes trace information, a message, and event information to the listener specific output.
        /// </summary>
        /// <param name="eventCache">A <see cref="System.Diagnostics.TraceEventCache"/> object that contains the current process ID, thread ID, and stack trace information.</param>
        /// <param name="source">A name used to identify the output, typically the name of the application that generated the trace event.</param>
        /// <param name="eventType">One of the <see cref="System.Diagnostics.TraceEventType"/> values specifying the type of event that has caused the trace.</param>
        /// <param name="id">A numeric identifier for the event.</param>
        /// <param name="message">A message to write.</param>
        [ComVisible(false)]
        public override void TraceEvent(TraceEventCache eventCache, string source, TraceEventType eventType, int id, string message)
        {
            EnsureInitialized();

            if (ShouldTrace(eventCache, source, eventType, id, message))
            {
                // resolve message

                WriteTrace(eventCache, source, eventType, id, message);
            }
        }

        /// <summary>
        /// Writes trace information, a formatted array of objects and event information to the listener specific output.
        /// </summary>
        /// <param name="eventCache">A <see cref="System.Diagnostics.TraceEventCache"/> object that contains the current process ID, thread ID, and stack trace information.</param>
        /// <param name="source">A name used to identify the output, typically the name of the application that generated the trace event.</param>
        /// <param name="eventType">One of the <see cref="System.Diagnostics.TraceEventType"/> values specifying the type of event that has caused the trace.</param>
        /// <param name="id">A numeric identifier for the event.</param>
        /// <param name="format">A format string that contains zero or more format items, which correspond to objects in the args array.</param>
        /// <param name="args">An object array containing zero or more objects to format.</param>
        [ComVisible(false)]
        public override void TraceEvent(TraceEventCache eventCache, string source, TraceEventType eventType, int id, string format, params object[] args)
        {
            EnsureInitialized();

            if (ShouldTrace(eventCache, source, eventType, id, format, args))
            {
                string message = String.Empty;
                if (args != null)
                {
                    message = String.Format(CultureInfo.InvariantCulture, format, args);
                }
                else
                {
                    message = format;
                }

                // resolve message

                WriteTrace(eventCache, source, eventType, id, message);
            }
        }

        /// <summary>
        /// Writes trace information, a message, a related activity identity and event information to the listener specific output.
        /// </summary>
        /// <param name="eventCache">A <see cref="System.Diagnostics.TraceEventCache"/> object that contains the current process ID, thread ID, and stack trace information.</param>
        /// <param name="source">A name used to identify the output, typically the name of the application that generated the trace event.</param>
        /// <param name="id">A numeric identifier for the event.</param>
        /// <param name="message">A message to write.</param>
        /// <param name="relatedActivityId">The related activity id.</param>
        [ComVisible(false)]
        public override void TraceTransfer(TraceEventCache eventCache, string source, int id, string message, Guid relatedActivityId)
        {
            //ignore?
        }

        #endregion
    }
}
