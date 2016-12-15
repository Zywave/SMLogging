using System;
using System.Diagnostics;
using System.Text;

namespace SMLogging
{
    /// <summary>
    /// Represents a trace listener that writes to the Windows event log.
    /// </summary>
    /// <seealso cref="SMLogging.TraceListenerBase" />
    public class WindowsEventLogTraceListener : TraceListenerBase
    {
        /// <summary>
        /// Initializes a new instance of the <see cref="WindowsEventLogTraceListener"/> class.
        /// </summary>
        /// <param name="eventLog">The event log.</param>
        public WindowsEventLogTraceListener(EventLog eventLog)
            : base((eventLog != null) ? eventLog.Source : string.Empty)
        {
            EventLog = eventLog;
            TraceFormat = _defaultTraceFormat;
        }

        /// <summary>
        /// Initializes a new instance of the <see cref="WindowsEventLogTraceListener"/> class.
        /// </summary>
        /// <param name="source">The source.</param>
        public WindowsEventLogTraceListener(string source)
            : base(source)
        {
            EventLog = new EventLog();
            EventLog.Source = source;
            TraceFormat = _defaultTraceFormat;
        }

        /// <summary>
        /// Gets the event log.
        /// </summary>
        public EventLog EventLog { get; private set; }

        /// <summary>
        /// Closes the trace listener.
        /// </summary>
        public override void Close()
        {
            EventLog?.Close();

            base.Close();
        }

        /// <summary>
        /// Releases the unmanaged resources used by the <see cref="T:System.Diagnostics.TraceListener" /> and optionally releases the managed resources.
        /// </summary>
        /// <param name="disposing">true to release both managed and unmanaged resources; false to release only unmanaged resources.</param>
        protected override void Dispose(bool disposing)
        {
            try
            {
                if (disposing)
                {
                    EventLog?.Dispose();
                    EventLog = null;
                }
            }
            finally
            {
                base.Dispose(disposing);
            }
        }

        /// <summary>
        /// Not supported. Overriding WriteTrace method.
        /// </summary>
        /// <param name="message"></param>
        public override void Write(string message)
        {
            throw new NotSupportedException();
        }

        /// <summary>
        /// Not supported. Overriding WriteTrace method.
        /// </summary>
        /// <param name="message"></param>
        public override void WriteLine(string message)
        {
            throw new NotSupportedException();
        }

        /// <summary>
        /// Writes trace information to the listener specific output.
        /// </summary>
        /// <param name="eventCache">A <see cref="System.Diagnostics.TraceEventCache" /> object that contains the current process ID, thread ID, and stack trace information.</param>
        /// <param name="source">A name used to identify the output, typically the name of the application that generated the trace event.</param>
        /// <param name="eventType">One of the <see cref="System.Diagnostics.TraceEventType" /> values specifying the type of event that has caused the trace.</param>
        /// <param name="id">A numeric identifier for the event.</param>
        /// <param name="message">The message to write.</param>
        public override void WriteTrace(TraceEventCache eventCache, string source, TraceEventType eventType, int id, string message)
        {
            if (EventLog == null)
            {
                return;
            }
            
            var builder = new StringBuilder(FormatTrace(eventCache, source, eventType, id, message));
            
            WriteTraceOutput(eventCache, eventType, s => builder.AppendLine(s));

            var entryType = GetEventLogEntryType(eventType);

            EventLog.WriteEntry(builder.ToString(), entryType, id);
        }

        private static EventLogEntryType GetEventLogEntryType(TraceEventType eventType)
        {
            switch (eventType)
            {
                case TraceEventType.Error:
                case TraceEventType.Critical:
                    return EventLogEntryType.Error;
                case TraceEventType.Warning:
                    return EventLogEntryType.Warning;
                default:
                    return EventLogEntryType.Information;
            }
        }

        private const string _defaultTraceFormat = "Process: {ProcessName} ({ProcessId}){NewLine}App: {AppName}{NewLine}{Message}";
    }
}
