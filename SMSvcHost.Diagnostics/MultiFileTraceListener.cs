using System;
using System.Collections.Generic;
using System.Diagnostics;
using System.Globalization;
using System.Linq;

namespace SMSvcHost.Diagnostics
{
    /// <summary>
    /// Represents a <see cref="TraceListener"/> that writes to multiple files.
    /// </summary>
    public class MultiFileTraceListener : FileTraceListener
    {
        #region Public Constructors
        
        /// <summary>
        /// Initializes a new instance of the <see cref="MultiFileTraceListener"/> class.
        /// </summary>
        /// <param name="path">The file path.</param>
        public MultiFileTraceListener(string path)
            : base(path)
        {
        }

        /// <summary>
        /// Initializes a new instance of the <see cref="MultiFileTraceListener"/> class.
        /// </summary>
        /// <param name="path">The file path.</param>
        /// <param name="name">The trace listener name.</param>
        public MultiFileTraceListener(string path, string name)
            : base(path, name)
        {
        }
        
        #endregion

        #region Protected Properties
        
        /// <summary>
        /// Gets the child trace listeners.
        /// </summary>
        protected IDictionary<string, FileTraceListener> ChildTraceListeners
        {
            get { return _childTraceListeners; }
        }

        #endregion

        #region Public Methods

        /// <summary>
        /// Writes trace information to the listener specific output.
        /// </summary>
        /// <param name="eventCache">A <see cref="System.Diagnostics.TraceEventCache"/> object that contains the current process ID, thread ID, and stack trace information.</param>
        /// <param name="source">A name used to identify the output, typically the name of the application that generated the trace event.</param>
        /// <param name="eventType">One of the <see cref="System.Diagnostics.TraceEventType"/> values specifying the type of event that has caused the trace.</param>
        /// <param name="id">A numeric identifier for the event.</param>
        /// <param name="message">The message to write.</param>
        public override void WriteTrace(TraceEventCache eventCache, string source, TraceEventType eventType, int id, string message)
        {
            var path = ResolveFilePath(eventCache, source, eventType, id, message);

            var fileTraceListener = GetFileTraceListener(path);

            fileTraceListener.WriteTrace(eventCache, source, eventType, id, message);
        }

        /// <summary>
        /// Flushes the trace listener.
        /// </summary>
        public override void Flush()
        {
            foreach (var child in ChildTraceListeners.Values)
            {
                child.Flush();
            }
        }

        /// <summary>
        /// Writes the specified message to the listener.
        /// </summary>
        /// <param name="message">The message to write.</param>
        /// <remarks>
        /// Ingored. Write using the <see cref="M:WriteTrace"/> method.
        /// </remarks>
        public override void Write(string message)
        {
        }

        /// <summary>
        /// Writes the specified message to the listener.
        /// </summary>
        /// <param name="message">The message to write.</param>
        /// <remarks>
        /// Ingored. Write using the <see cref="M:WriteTrace"/> method.
        /// </remarks>
        public override void WriteLine(string message)
        {
        }

        /// <summary>
        /// Initializes the file.
        /// </summary>
        /// <remarks>
        /// Ignored.  File initialization handled by child listeners.
        /// </remarks>
        protected override void InitializeFile()
        {
        }

        #endregion

        #region Protected Methods
        
        /// <summary>
        /// Disposes of the trace listener.
        /// </summary>
        /// <param name="disposing">A value indicating whether to dispose managed and unmanaged resources.</param>
        protected override void Dispose(bool disposing)
        {
            if (disposing)
            {
                foreach (var child in ChildTraceListeners.Values)
                {
                    child.Dispose();
                }
            }

            base.Dispose(disposing);
        }

        /// <summary>
        /// Resolves the file path.
        /// </summary>
        /// <param name="eventCache">The event cache.</param>
        /// <param name="source">The source.</param>
        /// <param name="eventType">Type of the event.</param>
        /// <param name="id">The identifier.</param>
        /// <param name="message">The message.</param>
        /// <returns>A file path.</returns>
        protected virtual string ResolveFilePath(TraceEventCache eventCache, string source, TraceEventType eventType, int id, string message)
        {
            var namedArgs = new Dictionary<string, object>();
            
            namedArgs["Source"] = source;
            namedArgs["EventType"] = eventType.ToString();
            namedArgs["EventId"] = id;
            namedArgs["ThreadId"] = eventCache != null ? eventCache.ThreadId : String.Empty;
            namedArgs["ActivityId"] = Trace.CorrelationManager.ActivityId;
            namedArgs["LogicalOperationStack"] = String.Join("-", Trace.CorrelationManager.LogicalOperationStack.ToArray().Reverse());

            return StringHelpers.NamedFormat(CultureInfo.InvariantCulture, SourceFilePath, namedArgs, true);
        }

        /// <summary>
        /// Gets the file trace listener for the specified <paramref name="path"/>.
        /// </summary>
        /// <param name="path">The path.</param>
        /// <returns>A file trace listener.</returns>
        protected virtual FileTraceListener GetFileTraceListener(string path)
        {
            if (path == null) throw new ArgumentNullException("path");
            
            if (!ChildTraceListeners.ContainsKey(path))
            {
                ChildTraceListeners[path] = new FileTraceListener(path)
                {
                    AppendToFile = AppendToFile,
                    Encoding = Encoding,
                    FailTraceSource = FailTraceSource,
                    Filter = Filter,
                    IndentLevel = IndentLevel,
                    IndentSize = IndentSize,
                    LockingMode = LockingMode,
                    MaximumFileIndex = MaximumFileIndex,
                    MaximumFileSize = MaximumFileSize,
                    RollingInterval = RollingInterval,
                    RollingMode = RollingMode,
                    TimeZone = TimeZone,
                    TraceDataDelimiter = TraceDataDelimiter,
                    TraceFormat = TraceFormat,
                    TraceOutputOptions = TraceOutputOptions,
                    TraceOutputOptionsLevels = TraceOutputOptionsLevels
                };
            }

            return ChildTraceListeners[path];
        }

        #endregion

        #region Private Fields

        private readonly IDictionary<string, FileTraceListener> _childTraceListeners = new Dictionary<string, FileTraceListener>();

        #endregion
    }
}
