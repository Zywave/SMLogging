namespace SMSvcHost.Diagnostics
{
    using System;
    using System.Configuration;

    /// <summary>
    /// Represents a trace listener that emits trace statements to the console.
    /// </summary>
    public class ConsoleTraceListener : TextWriterTraceListener
    {
        #region Public Constructors

        /// <summary>
        /// Initializes a new instance of the <see cref="SMSvcHost.Diagnostics.ConsoleTraceListener"/> class.
        /// </summary>
        public ConsoleTraceListener()
        {
            UseErrorStream = false;
        }

        /// <summary>
        /// Initializes a new instance of the <see cref="SMSvcHost.Diagnostics.ConsoleTraceListener"/> class.
        /// </summary>
        /// <param name="name">The name of the <see cref="System.Diagnostics.TraceListener"/>.</param>
        public ConsoleTraceListener(string name)
            : base(name)
        {
            UseErrorStream = false;
        }

        #endregion

        #region Public Properties

        /// <summary>
        /// Gets or sets a value indicating whether to use the error stream of the console.
        /// </summary>
        [ConfigurationProperty("useErrorStream")]
        public bool UseErrorStream { get; set; }

        #endregion

        #region Public Methods

        /// <summary>
        /// Closes this instance.
        /// </summary>
        public override void Close()
        { }

        #endregion

        #region Protected Methods

        /// <summary>
        /// Initializes the trace listener.
        /// </summary>
        protected override void Initialize()
        {
            base.Initialize();

            Writer = UseErrorStream ? Console.Error : Console.Out;
        }

        #endregion
    }
}
