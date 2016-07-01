namespace SMLogging
{
    /// <summary>
    /// Represents <see cref="FileTraceListener"/> file locking modes.
    /// </summary>
    public enum FileLockingMode
    {
        /// <summary>
        /// Exclusive lock.
        /// </summary>
        Exclusive,

        /// <summary>
        /// Minimal lock.
        /// </summary>
        Minimal,

        /// <summary>
        /// Lock using mutex.
        /// </summary>
        Mutex
    }
}
