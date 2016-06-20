namespace SMSvcHost.Diagnostics
{
    /// <summary>
    /// Represents <see cref="FileTraceListener"/> file rolling modes.
    /// </summary>
    public enum FileRollingMode
    {
        /// <summary>
        /// No file rolling.
        /// </summary>
        None,

        /// <summary>
        /// Roll file based on size.
        /// </summary>
        Size,

        /// <summary>
        /// Role file based on date and time.
        /// </summary>
        DateTime
    }
}
