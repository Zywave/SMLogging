using System;
using System.Collections.Generic;
using System.ComponentModel;
using System.Configuration;
using System.Diagnostics;
using System.Globalization;
using System.Reflection;
using System.Runtime.InteropServices;
using System.Text;
using SMSvcHost.Diagnostics.Properties;

namespace SMSvcHost.Diagnostics
{
    /// <summary>
    /// Represents a base class for trace listeners.
    /// </summary>
    public abstract class TraceListenerBase : TraceListener
    {       
        #region Protected Constructors

        /// <summary>
        /// Initializes a new instance of the <see cref="TraceListenerBase"/> class.
        /// </summary>
        protected TraceListenerBase()
            : base()
        {
            TraceFormat = _defaultTraceFormat;
            TraceDataDelimiter = _defaultTraceDataDelimiter;
            TraceOutputOptionsLevels = SourceLevels.All;
        }

        /// <summary>
        /// Initializes a new instance of the <see cref="TraceListenerBase"/> class.
        /// </summary>
        /// <param name="name">The trace listener name.</param>
        protected TraceListenerBase(string name)
            : base(name)
        {
            TraceFormat = _defaultTraceFormat;
            TraceDataDelimiter = _defaultTraceDataDelimiter;
            TraceOutputOptionsLevels = SourceLevels.All;
        }

        #endregion
        
        #region Public Properties

        /// <summary>
        /// Gets or sets the trace format.
        /// </summary>
        [ConfigurationProperty("traceFormat")]
        public string TraceFormat { get; set; }

        /// <summary>
        /// Gets or sets the time zone to use for date time.
        /// </summary>
        [ConfigurationProperty("timeZone")]
        [TypeConverter(typeof(TimeZoneInfoConverter))]
        public TimeZoneInfo TimeZone { get; set; }

        /// <summary>
        /// Gets or sets the trace data delimiter.
        /// </summary>
        [ConfigurationProperty("traceDataDelimiter")]
        public string TraceDataDelimiter { get; set; }

        /// <summary>
        /// Gets or sets the <see cref="System.Diagnostics.SourceLevels"/> for filtering the trace output options.
        /// </summary>
        [ConfigurationProperty("traceOutputOptionsLevels")]
        public SourceLevels TraceOutputOptionsLevels { get; set; }

        /// <summary>
        /// Gets or sets the <see cref="System.Diagnostics.TraceSource"/> for tracing trace listener failures.
        /// </summary>
        [ConfigurationProperty("failTraceSource")]
        [TypeConverter(typeof(TraceSourceConverter))]
        public TraceSource FailTraceSource { get; set; }

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
                string message = String.Empty;
                if (data != null)
                {
                    message = data.ToString();
                }
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
                StringBuilder builder = new StringBuilder();
                if (data != null)
                {
                    for (int i = 0; i < data.Length; i++)
                    {
                        if (i != 0)
                        {
                            builder.Append(TraceDataDelimiter);
                        }
                        if (data[i] != null)
                        {
                            builder.Append(data[i].ToString());
                        }
                    }
                }
                WriteTrace(eventCache, source, eventType, id, builder.ToString());
            }
        }

        /// <summary>
        /// Writes trace and event information to the listener specific output.
        /// </summary>
        /// <param name="eventCache">A <see cref="System.Diagnostics.TraceEventCache"/> object that contains the current process ID, thread ID, and stack trace information.</param>
        /// <param name="source">A name used to identify the output, typically the name of the application that generated the trace event.</param>
        /// <param name="eventType">One of the <see cref="System.Diagnostics.TraceEventType"/> values specifying the type of event that has caused the trace.</param>
        /// <param name="id">A numeric identifier for the event.</param>
        [ComVisible(false)]
        public override void TraceEvent(TraceEventCache eventCache, string source, TraceEventType eventType, int id)
        {
            TraceEvent(eventCache, source, eventType, id, String.Empty);
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
                WriteTrace(eventCache, source, eventType, id, message);
            }
        }

        /// <summary>
        /// Writes trace information to the listener specific output.
        /// </summary>
        /// <param name="eventCache">A <see cref="System.Diagnostics.TraceEventCache"/> object that contains the current process ID, thread ID, and stack trace information.</param>
        /// <param name="source">A name used to identify the output, typically the name of the application that generated the trace event.</param>
        /// <param name="eventType">One of the <see cref="System.Diagnostics.TraceEventType"/> values specifying the type of event that has caused the trace.</param>
        /// <param name="id">A numeric identifier for the event.</param>
        /// <param name="message">The message to write.</param>
        public virtual void WriteTrace(TraceEventCache eventCache, string source, TraceEventType eventType, int id, string message)
        {
            WriteLine(FormatTrace(eventCache, source, eventType, id, message));

            bool includeOptions = ((int)TraceOutputOptionsLevels & (int)eventType) != 0;
            if (includeOptions && eventCache != null)
            {
                IndentLevel++;
                if ((TraceOptions.ProcessId & TraceOutputOptions) != TraceOptions.None)
                {
                    WriteLine(String.Format(CultureInfo.InvariantCulture, AssemblyResources.ProcessIdTraceToken, eventCache.ProcessId));
                }
                if ((TraceOptions.LogicalOperationStack & TraceOutputOptions) != TraceOptions.None)
                {
                    string stack = StringHelpers.Join(", ", eventCache.LogicalOperationStack.ToArray());
                    WriteLine(String.Format(CultureInfo.InvariantCulture, AssemblyResources.LogicalOperationStackTraceToken, stack));
                }
                if ((TraceOptions.ThreadId & TraceOutputOptions) != TraceOptions.None)
                {
                    WriteLine(String.Format(CultureInfo.InvariantCulture, AssemblyResources.ThreadIdTraceToken, eventCache.ThreadId));
                }
                if ((TraceOptions.DateTime & TraceOutputOptions) != TraceOptions.None)
                {
                    WriteLine(String.Format(CultureInfo.InvariantCulture, AssemblyResources.DateTimeTraceToken, eventCache.DateTime));
                }
                if ((TraceOptions.Timestamp & TraceOutputOptions) != TraceOptions.None)
                {
                    WriteLine(String.Format(CultureInfo.InvariantCulture, AssemblyResources.TimestampTraceToken, eventCache.Timestamp));
                }
                if ((TraceOptions.Callstack & TraceOutputOptions) != TraceOptions.None)
                {
                    WriteLine(String.Format(CultureInfo.InvariantCulture, AssemblyResources.CallstackTraceToken, eventCache.Callstack));
                }
                IndentLevel--;
            }
        }

        /// <summary>
        /// Emits an error message and a detailed error message to the listener you create when you implement the <see cref="System.Diagnostics.TraceListener"/> class.
        /// </summary>
        /// <param name="message">A message to emit.</param>
        /// <param name="detailMessage">A detailed message to emit.</param>
        public override void Fail(string message, string detailMessage)
        {
            StringBuilder builder = new StringBuilder();
            builder.Append(AssemblyResources.TraceListenerFailed);
            builder.Append(" ");
            builder.Append(message);
            if (detailMessage != null)
            {
                builder.Append(" ");
                builder.Append(detailMessage);
            }

            if (FailTraceSource != null)
            {
                FailTraceSource.TraceEvent(TraceEventType.Error, 0, builder.ToString());
            }
            else
            {
                TraceEvent(null, Name, TraceEventType.Error, 0, builder.ToString());
            }
        }

        /// <summary>
        /// Closes the trace listener.
        /// </summary>
        public override void Close()
        {
            Dispose(true);
        }

        #endregion

        #region Protected Methods
        
        /// <summary>
        /// Formats a trace statement using the configured trace format.
        /// </summary>
        /// <param name="eventCache">A <see cref="System.Diagnostics.TraceEventCache"/> object that contains the current process ID, thread ID, and stack trace information.</param>
        /// <param name="source">A name used to identify the output, typically the name of the application that generated the trace event.</param>
        /// <param name="eventType">One of the <see cref="System.Diagnostics.TraceEventType"/> values specifying the type of event that has caused the trace.</param>
        /// <param name="id">A numeric identifier for the event.</param>
        /// <param name="message">The message to format.</param>
        /// <returns>A formatted trace statement.</returns>
        protected virtual string FormatTrace(TraceEventCache eventCache, string source, TraceEventType eventType, int id, string message)
        {
            Dictionary<string, object> namedArgs = new Dictionary<string, object>();
            DateTime dateTime = eventCache != null ? eventCache.DateTime : DateTime.UtcNow;
            if (TimeZone != null)
            {
                dateTime = TimeZoneInfo.ConvertTime(dateTime, TimeZone);
            }
            namedArgs["DateTime"] = dateTime;
            namedArgs["Timestamp"] = eventCache != null ? eventCache.Timestamp : DateTime.UtcNow.Ticks;
            namedArgs["Source"] = source;
            namedArgs["EventType"] = eventType.ToString();
            namedArgs["EventId"] = id;
            namedArgs["Message"] = message;
            namedArgs["ProcessId"] = eventCache != null ? eventCache.ProcessId : 0;
            namedArgs["ThreadId"] = eventCache != null ? eventCache.ThreadId : String.Empty;
            namedArgs["ActivityId"] = Trace.CorrelationManager.ActivityId;
            namedArgs["LogicalOperationStack"] = String.Join(", ", Trace.CorrelationManager.LogicalOperationStack.ToArray());
            namedArgs["NewLine"] = Environment.NewLine;

            string result = String.Empty;

            try
            {
                result = StringHelpers.NamedFormat(CultureInfo.InvariantCulture, TraceFormat, namedArgs);
            }
            catch (FormatException ex)
            {
                Fail(AssemblyResources.InvalidTraceListenerTraceFormat, ex.GetBaseException().Message);
            }

            return result;
        }

        /// <summary>
        /// Determines if the trace listener should emit a trace statement based on the trace information provided.
        /// </summary>
        /// <param name="eventCache">A <see cref="System.Diagnostics.TraceEventCache"/> object that contains the current process ID, thread ID, and stack trace information.</param>
        /// <param name="source">A name used to identify the output, typically the name of the application that generated the trace event.</param>
        /// <param name="eventType">One of the <see cref="System.Diagnostics.TraceEventType"/> values specifying the type of event that has caused the trace.</param>
        /// <param name="id">A numeric identifier for the event.</param>
        /// <param name="message">A message to emit.</param>
        /// <returns>A value indicating that a trace statement should be emitted.</returns>
        protected bool ShouldTrace(TraceEventCache eventCache, string source, TraceEventType eventType, int id, string message)
        {
            return ShouldTrace(eventCache, source, eventType, id, message, null, null, null);
        }

        /// <summary>
        /// Determines if the trace listener should emit a trace statement based on the trace information provided.
        /// </summary>
        /// <param name="eventCache">A <see cref="System.Diagnostics.TraceEventCache"/> object that contains the current process ID, thread ID, and stack trace information.</param>
        /// <param name="source">A name used to identify the output, typically the name of the application that generated the trace event.</param>
        /// <param name="eventType">One of the <see cref="System.Diagnostics.TraceEventType"/> values specifying the type of event that has caused the trace.</param>
        /// <param name="id">A numeric identifier for the event.</param>
        /// <param name="format">A format string that contains zero or more format items, which correspond to objects in the args array.</param>
        /// <param name="args">An object array containing zero or more objects to format.</param>
        /// <returns>A value indicating that a trace statement should be emitted.</returns>
        protected bool ShouldTrace(TraceEventCache eventCache, string source, TraceEventType eventType, int id, string format, object[] args)
        {
            return ShouldTrace(eventCache, source, eventType, id, format, args, null, null);
        }

        /// <summary>
        /// Determines if the trace listener should emit a trace statement based on the trace information provided.
        /// </summary>
        /// <param name="eventCache">A <see cref="System.Diagnostics.TraceEventCache"/> object that contains the current process ID, thread ID, and stack trace information.</param>
        /// <param name="source">A name used to identify the output, typically the name of the application that generated the trace event.</param>
        /// <param name="eventType">One of the <see cref="System.Diagnostics.TraceEventType"/> values specifying the type of event that has caused the trace.</param>
        /// <param name="id">A numeric identifier for the event.</param>
        /// <param name="formatOrMessage">The message or formatted message.</param>
        /// <param name="args">An object array containing zero or more objects to format.</param>
        /// <param name="data1">The trace data to emit.</param>
        /// <param name="data">The trace data array to emit.</param>
        /// <returns>A value indicating that a trace statement should be emitted.</returns>
        protected virtual bool ShouldTrace(TraceEventCache eventCache, string source, TraceEventType eventType, int id, string formatOrMessage, object[] args, object data1, object[] data)
        {
            return (Filter == null) || Filter.ShouldTrace(eventCache, source, eventType, id, formatOrMessage, args, data1, data);
        }

        /// <summary>
        /// Gets the custom attributes supported by the trace listener.
        /// </summary>
        /// <returns>
        /// A string array naming the custom attributes supported by the trace listener, or null if there are no custom attributes.
        /// </returns>
        protected sealed override string[] GetSupportedAttributes()
        {
            _hasConfiguration = true;

            List<string> supportedAttributes = new List<string>();

            PropertyDescriptorCollection properties = TypeDescriptor.GetProperties(this);
            foreach (PropertyDescriptor property in properties)
            {
                ConfigurationPropertyAttribute configurationPropertyAttribute = property.Attributes[typeof(ConfigurationPropertyAttribute)] as ConfigurationPropertyAttribute;
                if (configurationPropertyAttribute != null)
                {
                    supportedAttributes.Add(configurationPropertyAttribute.Name);
                }
            }

            return supportedAttributes.ToArray();
        }

        /// <summary>
        /// Initializes the trace listener.
        /// </summary>
        protected virtual void Initialize()
        {
            _isInitialized = true;

            if (_hasConfiguration)
            {
                ApplyConfiguration();
            }
        }

        /// <summary>
        /// Ensures the the trace listener has been initialized.
        /// </summary>
        protected void EnsureInitialized()
        {
            if (!_isInitialized)
            {
                Initialize();
            }
        }        

        /// <summary>
        /// Applies the custom configuration attributes to the configuration properties of the trace listener.
        /// </summary>
        protected void ApplyConfiguration()
        {
            FieldInfo fieldInfo = typeof(ConfigurationElement).GetField("s_nullPropertyValue", BindingFlags.Static | BindingFlags.NonPublic);
            object nullValue = fieldInfo.GetValue(null);

            PropertyDescriptorCollection properties = TypeDescriptor.GetProperties(this);
            foreach (PropertyDescriptor property in properties)
            {
                ConfigurationPropertyAttribute configurationPropertyAttribute = property.Attributes[typeof(ConfigurationPropertyAttribute)] as ConfigurationPropertyAttribute;

                if (configurationPropertyAttribute != null)
                {
                    string name = configurationPropertyAttribute.Name;
                    object defaultValue = configurationPropertyAttribute.DefaultValue;
                    bool isRequired = configurationPropertyAttribute.IsRequired;
                    object value = null;

                    if (!Attributes.ContainsKey(name))
                    {
                        if (isRequired)
                        {
                            throw new ConfigurationErrorsException();
                        }
                        else if (!defaultValue.Equals(nullValue))
                        {
                            value = defaultValue;
                        }
                    }
                    else
                    {
                        value = Attributes[name];
                    }

                    if (value != null)
                    {
                        TypeConverter converter = property.Converter;
                        if (converter != null)
                        {
                            value = converter.ConvertFrom(value);
                        }
                    }

                    if (value != null && !property.IsReadOnly)
                    {
                        property.SetValue(this, value);
                    }
                }
            }
        }

        #endregion

        #region Private Fields

        private bool _hasConfiguration = false;
        private bool _isInitialized = false;

        private const string _defaultTraceFormat = "{DateTime:yyyy-MM-dd HH:mm:ss.FFF} {Source} {EventType} - {Message}";
        private const string _defaultTraceDataDelimiter = ", ";

        #endregion
    }
}
