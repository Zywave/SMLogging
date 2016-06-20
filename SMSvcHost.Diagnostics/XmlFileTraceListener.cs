namespace SMSvcHost.Diagnostics
{
    using System;
    using System.Collections;
    using System.Diagnostics;
    using System.Globalization;
    using System.IO;
    using System.Runtime.InteropServices;
    using System.Text;
    using System.Threading;
    using System.Xml;
    using System.Xml.XPath;

    /// <summary>
    /// Represents a trace listener that emits trace statements to files in the form of E2E XML.
    /// </summary>
    public class XmlFileTraceListener : FileTraceListener
    {
        #region Public Constructors

        /// <summary>
        /// Initializes a new instance of the <see cref="SMSvcHost.Diagnostics.XmlFileTraceListener"/> class.
        /// </summary>
        /// <param name="path">The file path.</param>
        public XmlFileTraceListener(string path)
            : base(path)
        { }

        /// <summary>
        /// Initializes a new instance of the <see cref="SMSvcHost.Diagnostics.XmlFileTraceListener"/> class.
        /// </summary>
        /// <param name="path">The file path.</param>
        /// <param name="name">The trace listener name.</param>
        public XmlFileTraceListener(string path, string name)
            : base(path, name)
        { }

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
                XmlTraceBuilder traceBuilder = new XmlTraceBuilder();

                traceBuilder.AppendHeader(source, eventType, id, eventCache, Guid.Empty, TimeZone);
                traceBuilder.AppendData(data);
                traceBuilder.AppendFooter(eventCache, TraceOutputOptions);

                WriteTrace(eventCache, source, eventType, id, traceBuilder.ToString());
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
                XmlTraceBuilder traceBuilder = new XmlTraceBuilder();

                traceBuilder.AppendHeader(source, eventType, id, eventCache, Guid.Empty, TimeZone);
                traceBuilder.AppendData(data);
                traceBuilder.AppendFooter(eventCache, TraceOutputOptions);

                WriteTrace(eventCache, source, eventType, id, traceBuilder.ToString());
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
                XmlTraceBuilder traceBuilder = new XmlTraceBuilder();

                traceBuilder.AppendHeader(source, eventType, id, eventCache, Guid.Empty, TimeZone);
                traceBuilder.AppendEscaped(message);
                traceBuilder.AppendFooter(eventCache, TraceOutputOptions);

                WriteTrace(eventCache, source, eventType, id, traceBuilder.ToString());
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

                XmlTraceBuilder traceBuilder = new XmlTraceBuilder();

                traceBuilder.AppendHeader(source, eventType, id, eventCache, Guid.Empty, TimeZone);
                traceBuilder.AppendEscaped(message);
                traceBuilder.AppendFooter(eventCache, TraceOutputOptions);

                WriteTrace(eventCache, source, eventType, id, traceBuilder.ToString());
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
            EnsureInitialized();

            XmlTraceBuilder traceBuilder = new XmlTraceBuilder();

            traceBuilder.AppendHeader(source, TraceEventType.Transfer, id, eventCache, relatedActivityId, TimeZone);
            traceBuilder.AppendEscaped(message);
            traceBuilder.AppendFooter(eventCache, TraceOutputOptions);

            WriteTrace(eventCache, source, TraceEventType.Transfer, id, traceBuilder.ToString());
        }

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
            if (AcquireLock())
            {
                try
                {
                    Write(message);
                }
                finally
                {
                    ReleaseLock();
                }
            }
        }

        /// <summary>
        /// Writes the specified message to the listener.
        /// </summary>
        /// <param name="message">The message to write.</param>
        public override void Write(string message)
        {
            if (EnsureWriter())
            {
                Writer.Write(message);
            }
        }

        /// <summary>
        /// Writes the specified message to the listener.
        /// </summary>
        /// <param name="message">The message to write.</param>
        public override void WriteLine(string message)
        {
        }

        #endregion

        #region Private Classes

        private class XmlTraceBuilder
        {
            public XmlTraceBuilder AppendData(object data)
            {
                return AppendData(new object[] { data });
            }

            public XmlTraceBuilder AppendData(params object[] data)
            {
                _builder.Append("<TraceData>");

                XmlTextWriter dataXmlTextWriter = null;
                StringWriter dataStringWriter = null;
                try
                {
                    StringBuilder dataStringBuilder = new StringBuilder();
                    dataStringWriter = new StringWriter(dataStringBuilder, CultureInfo.CurrentCulture);
                    dataXmlTextWriter = new XmlTextWriter(dataStringWriter);

                    foreach (object dataItem in data)
                    {
                        _builder.Append("<DataItem>");
                        XPathNavigator navigator = dataItem as XPathNavigator;
                        if (navigator != null)
                        {
                            dataStringBuilder.Clear();

                            navigator.MoveToRoot();
                            dataXmlTextWriter.WriteNode(navigator, false);

                            _builder.Append(dataStringBuilder.ToString());
                        }
                        else
                        {
                            AppendEscaped(dataItem.ToString());
                        }
                        _builder.Append("</DataItem>");
                    }
                }
                finally
                {
                    if (dataXmlTextWriter != null)
                    {
                        dataXmlTextWriter.Close();
                    }
                    else if (dataStringWriter != null)
                    {
                        dataStringWriter.Close();
                    }
                }

                _builder.Append("</TraceData>");

                return this;
            }

            public XmlTraceBuilder AppendHeader(string source, TraceEventType eventType, int id, TraceEventCache eventCache, Guid relatedActivityId, TimeZoneInfo timeZone)
            {
                _builder.Append("<E2ETraceEvent xmlns=\"http://schemas.microsoft.com/2004/06/E2ETraceEvent\"><System xmlns=\"http://schemas.microsoft.com/2004/06/windows/eventlog/system\">");
                _builder.Append("<EventID>");
                _builder.Append(((uint)id).ToString(CultureInfo.InvariantCulture));
                _builder.Append("</EventID>");
                _builder.Append("<Type>3</Type>");
                _builder.Append("<SubType Name=\"");
                _builder.Append(eventType.ToString());
                _builder.Append("\">0</SubType>");
                _builder.Append("<Level>");
                int eventTypeValue = (int)eventType;
                if (eventTypeValue > 0xff)
                {
                    eventTypeValue = 0xff;
                }
                if (eventTypeValue < 0)
                {
                    eventTypeValue = 0;
                }
                _builder.Append(eventTypeValue.ToString(CultureInfo.InvariantCulture));
                _builder.Append("</Level>");
                _builder.Append("<TimeCreated SystemTime=\"");
                DateTime dateTime = DateTime.UtcNow;
                if (eventCache != null)
                {
                    dateTime = eventCache.DateTime;
                }
                if (timeZone != null)
                {
                    dateTime = TimeZoneInfo.ConvertTime(dateTime, timeZone);
                }
                _builder.Append(dateTime.ToString("o", CultureInfo.InvariantCulture));
                _builder.Append("\" />");
                _builder.Append("<Source Name=\"");
                AppendEscaped(source);
                _builder.Append("\" />");
                _builder.Append("<Correlation ActivityID=\"");
                if (eventCache != null)
                {
                    _builder.Append(Trace.CorrelationManager.ActivityId.ToString("B"));
                }
                else
                {
                    _builder.Append(Guid.Empty.ToString("B"));
                }
                if (relatedActivityId != Guid.Empty)
                {
                    _builder.Append("\" RelatedActivityID=\"");
                    _builder.Append(relatedActivityId.ToString("B"));
                }
                _builder.Append("\" />");
                _builder.Append("<Execution ProcessName=\"");
                _builder.Append(_processName);
                _builder.Append("\" ProcessID=\"");
                _builder.Append(((uint)_processId).ToString(CultureInfo.InvariantCulture));
                _builder.Append("\" ThreadID=\"");
                if (eventCache != null)
                {
                    AppendEscaped(eventCache.ThreadId.ToString(CultureInfo.InvariantCulture));
                }
                else
                {
                    AppendEscaped(Thread.CurrentThread.ManagedThreadId.ToString(CultureInfo.InvariantCulture));
                }
                _builder.Append("\" />");
                _builder.Append("<Channel/>");
                _builder.Append("<Computer>");
                _builder.Append(_machineName);
                _builder.Append("</Computer>");
                _builder.Append("</System>");
                _builder.Append("<ApplicationData>");

                return this;
            }

            public XmlTraceBuilder AppendFooter(TraceEventCache eventCache, TraceOptions traceOutputOptions)
            {
                bool includeLogicalOperationStack = (TraceOptions.LogicalOperationStack & traceOutputOptions) != TraceOptions.None;
                bool includeCallstack = (TraceOptions.Callstack & traceOutputOptions) != TraceOptions.None;
                if ((eventCache != null) && (includeLogicalOperationStack || includeCallstack))
                {
                    _builder.Append("<System.Diagnostics xmlns=\"http://schemas.microsoft.com/2004/08/System.Diagnostics\">");
                    if (includeLogicalOperationStack)
                    {
                        _builder.Append("<LogicalOperationStack>");
                        Stack logicalOperationStack = eventCache.LogicalOperationStack;
                        if (logicalOperationStack != null)
                        {
                            foreach (object operation in logicalOperationStack)
                            {
                                _builder.Append("<LogicalOperation>");
                                AppendEscaped(operation.ToString());
                                _builder.Append("</LogicalOperation>");
                            }
                        }
                        _builder.Append("</LogicalOperationStack>");
                    }
                    _builder.Append("<Timestamp>");
                    _builder.Append(eventCache.Timestamp.ToString(CultureInfo.InvariantCulture));
                    _builder.Append("</Timestamp>");
                    if (includeCallstack)
                    {
                        _builder.Append("<Callstack>");
                        AppendEscaped(eventCache.Callstack);
                        _builder.Append("</Callstack>");
                    }
                    _builder.Append("</System.Diagnostics>");
                }
                _builder.Append("</ApplicationData></E2ETraceEvent>");

                return this;
            }

            public XmlTraceBuilder AppendEscaped(string value)
            {
                if (value != null)
                {
                    int startIndex = 0;
                    for (int i = 0; i < value.Length; i++)
                    {
                        switch (value[i])
                        {
                            case '\n':
                                _builder.Append(value.Substring(startIndex, i - startIndex));
                                _builder.Append("&#xA;");
                                startIndex = i + 1;
                                break;
                            case '\r':
                                _builder.Append(value.Substring(startIndex, i - startIndex));
                                _builder.Append("&#xD;");
                                startIndex = i + 1;
                                break;
                            case '&':
                                _builder.Append(value.Substring(startIndex, i - startIndex));
                                _builder.Append("&amp;");
                                startIndex = i + 1;
                                break;
                            case '\'':
                                _builder.Append(value.Substring(startIndex, i - startIndex));
                                _builder.Append("&apos;");
                                startIndex = i + 1;
                                break;
                            case '"':
                                _builder.Append(value.Substring(startIndex, i - startIndex));
                                _builder.Append("&quot;");
                                startIndex = i + 1;
                                break;
                            case '<':
                                _builder.Append(value.Substring(startIndex, i - startIndex));
                                _builder.Append("&lt;");
                                startIndex = i + 1;
                                break;
                            case '>':
                                _builder.Append(value.Substring(startIndex, i - startIndex));
                                _builder.Append("&gt;");
                                startIndex = i + 1;
                                break;
                        }
                    }

                    _builder.Append(value.Substring(startIndex, value.Length - startIndex));
                }

                return this;
            }

            public override string ToString()
            {
                return _builder.ToString();
            }

            private readonly StringBuilder _builder = new StringBuilder();

            static XmlTraceBuilder()
            {
                using (Process process = Process.GetCurrentProcess())
                {
                    _processId = process.Id;
                    _processName = process.ProcessName;
                }

                _machineName = Environment.MachineName;
            }

            private static readonly int _processId;
            private static readonly string _processName;
            private static readonly string _machineName;
        }

        #endregion
    }
}
