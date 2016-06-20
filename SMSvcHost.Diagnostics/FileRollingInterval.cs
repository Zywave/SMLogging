namespace SMSvcHost.Diagnostics
{
    /// <summary>
    /// Represents <see cref="FileTraceListener"/> file rolling intervals.
    /// </summary>
    public enum FileRollingInterval
    {
        /// <summary>
        /// No file rolling.
        /// </summary>
        None,

        /// <summary>
        /// Roll every minute.
        /// </summary>
        Minute,

        /// <summary>
        /// Roll every hour.
        /// </summary>
        Hour,

        /// <summary>
        /// Roll every day.
        /// </summary>
        Day,

        /// <summary>
        /// Roll every week.
        /// </summary>
        Week,

        /// <summary>
        /// Roll every month.
        /// </summary>
        Month
    }
}
