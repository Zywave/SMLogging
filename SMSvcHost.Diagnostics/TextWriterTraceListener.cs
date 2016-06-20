namespace SMSvcHost.Diagnostics
{
    using System;
    using System.IO;

    /// <summary>
    /// Represents a trace listener that emits trace statements to a <see cref="System.IO.TextWriter"/>.
    /// </summary>
    public class TextWriterTraceListener : TraceListenerBase
    {
        #region Public Constructors

        /// <summary>
        /// Initializes a new instance of the <see cref="SMSvcHost.Diagnostics.TextWriterTraceListener"/> class.
        /// </summary>
        public TextWriterTraceListener()
        { }

        /// <summary>
        /// Initializes a new instance of the <see cref="SMSvcHost.Diagnostics.TextWriterTraceListener"/> class.
        /// </summary>
        /// <param name="name">The name of the <see cref="System.Diagnostics.TraceListener"/>.</param>
        public TextWriterTraceListener(string name)
            : base(name)
        { }

        /// <summary>
        /// Initializes a new instance of the <see cref="SMSvcHost.Diagnostics.TextWriterTraceListener"/> class.
        /// </summary>
        /// <param name="stream">The stream to emit traces to.</param>
        /// <exception cref="System.ArgumentNullException">Throw if <paramref name="stream"/> is <see langword="null"/>.</exception>
        public TextWriterTraceListener(Stream stream)
            : this(stream, string.Empty)
        { }

        /// <summary>
        /// Initializes a new instance of the <see cref="SMSvcHost.Diagnostics.TextWriterTraceListener"/> class.
        /// </summary>
        /// <param name="writer">The text writer to emit traces to.</param>
        /// <exception cref="System.ArgumentNullException">Throw if <paramref name="writer"/> is <see langword="null"/>.</exception>
        public TextWriterTraceListener(TextWriter writer)
            : this(writer, string.Empty)
        { }

        /// <summary>
        /// Initializes a new instance of the <see cref="SMSvcHost.Diagnostics.TextWriterTraceListener"/> class.
        /// </summary>
        /// <param name="stream">The stream to emit traces to.</param>
        /// <param name="name">The name of the <see cref="System.Diagnostics.TraceListener"/>.</param>
        /// <exception cref="System.ArgumentNullException">Throw if <paramref name="stream"/> is <see langword="null"/>.</exception>
        public TextWriterTraceListener(Stream stream, string name)
            : base(name)
        {
            if (stream == null)
                throw new ArgumentNullException("stream");

            Writer = new StreamWriter(stream);
        }

        /// <summary>
        /// Initializes a new instance of the <see cref="SMSvcHost.Diagnostics.TextWriterTraceListener"/> class.
        /// </summary>
        /// <param name="writer">The text writer to emit traces to.</param>
        /// <param name="name">The name of the <see cref="System.Diagnostics.TraceListener"/>.</param>
        /// <exception cref="System.ArgumentNullException">Throw if <paramref name="writer"/> is <see langword="null"/>.</exception>
        public TextWriterTraceListener(TextWriter writer, string name)
            : base(name)
        {
            if (writer == null)
                throw new ArgumentNullException("writer");

            Writer = writer;
        }

        #endregion

        #region Protected Properties

        /// <summary>
        /// Gets or sets the <see cref="System.IO.TextWriter"/> to emit traces to.
        /// </summary>
        protected TextWriter Writer { get; set; }

        #endregion

        #region Public Methods

        /// <summary>
        /// Writes the specified message to the listener you create in the derived class.
        /// </summary>
        /// <param name="message">A message to write.</param>
        public override void Write(string message)
        {
            if (EnsureWriter())
            {
                if (NeedIndent)
                {
                    WriteIndent();
                }
                try
                {
                    Writer.Write(message);
                }
                catch (ObjectDisposedException) 
                { }
            }
        }

        /// <summary>
        /// Writes a message to the listener you create in the derived class, followed by a line terminator.
        /// </summary>
        /// <param name="message">A message to write.</param>
        public override void WriteLine(string message)
        {
            if (EnsureWriter())
            {
                if (NeedIndent)
                {
                    WriteIndent();
                }
                try
                {
                    Writer.WriteLine(message);
                    NeedIndent = true;
                }
                catch (ObjectDisposedException)
                { }
            }
        }

        /// <summary>
        /// Flushes the output buffer.
        /// </summary>
        public override void Flush()
        {
            if (EnsureWriter())
            {
                try
                {
                    Writer.Flush();
                }
                catch (ObjectDisposedException) 
                { }
            }
        }

        /// <summary>
        /// Closes the trace listener.
        /// </summary>
        public override void Close()
        {
            if (EnsureWriter())
            {
                try
                {
                    Writer.Close();
                }
                catch (ObjectDisposedException) 
                { }
            }
            Writer = null;
        }

        #endregion

        #region Protected Methods

        /// <summary>
        /// Ensures the <see cref="SMSvcHost.Diagnostics.TextWriterTraceListener.Writer"/>.
        /// </summary>
        /// <returns>true is the <see cref="SMSvcHost.Diagnostics.TextWriterTraceListener.Writer"/> is usable; otherwise false.</returns>
        protected virtual bool EnsureWriter()
        {
            return Writer != null;
        }

        /// <summary>
        /// Releases the unmanaged resources used by the <see cref="System.Diagnostics.TraceListener"/> and optionally releases the managed resources.
        /// </summary>
        /// <param name="disposing">true to release both managed and unmanaged resources; false to release only unmanaged resources.</param>
        protected override void Dispose(bool disposing)
        {
            try
            {
                if (disposing)
                {
                    Close();
                }
                else
                {
                    if (Writer != null)
                    {
                        try
                        {
                            Writer.Close();
                        }
                        catch (ObjectDisposedException) 
                        { }
                    }
                    Writer = null;
                }
            }
            finally
            {
                base.Dispose(disposing);
            }
        }

        #endregion
    }
}
