using System;
using System.Globalization;
using System.IO;
using System.Text;
using System.Threading;

namespace SMLogging
{
    /// <summary>
    /// Represents a file lock handler that uses a mutex to allow multiple processes to write to a file.
    /// </summary>
    public class MutexFileLockHandler : FileLockHandlerBase
    {
        /// <summary>
        /// Initializes a new instance of the <see cref="MutexFileLockHandler" /> class.
        /// </summary>
        public MutexFileLockHandler()
        {
        }

        /// <summary>
        /// Initializes a new instance of the <see cref="MutexFileLockHandler" /> class.
        /// </summary>
        /// <param name="mutexNamePrefix">The mutex name prefix.</param>
        public MutexFileLockHandler(string mutexNamePrefix)
        {
            _mutexNamePrefix = mutexNamePrefix;
        }

        /// <summary>
        /// Gets the mutex.
        /// </summary>
        protected Mutex Mutex { get; private set; }

        /// <summary>
        /// Opens the file.
        /// </summary>
        /// <param name="path">The path.</param>
        /// <param name="append">If set to <c>true</c>, append to the file.</param>
        /// <param name="encoding">The encoding.</param>
        public override void OpenFile(string path, bool append, Encoding encoding)
        {
            if (path == null) throw new ArgumentNullException("path");

            CreateStream(path, append, FileShare.ReadWrite);

            if (Stream != null)
            {
                var mutexName = String.Format(CultureInfo.InvariantCulture, "{0}-{1}", _mutexNamePrefix, path.Replace("\\", "_").Replace(":", "_").Replace("/", "_"));
                Mutex = new Mutex(false, mutexName);
            }
        }

        /// <summary>
        /// Closes the file.
        /// </summary>
        public override void CloseFile()
        {
            CloseStream();

            if (Mutex != null)
            {
                Mutex.ReleaseMutex();
                Mutex.Close();
                Mutex = null;
            }
        }

        /// <summary>
        /// Acquires the lock on the file.
        /// </summary>
        /// <returns>The locked <see cref="Stream" /> object.</returns>
        public override Stream AcquireLock()
        {
            if (Stream != null && Mutex != null)
            {
                Mutex.WaitOne();

                if (Stream.CanSeek)
                {
                    Stream.Seek(0, SeekOrigin.End);
                }
            }

            return Stream;
        }

        /// <summary>
        /// Releases the lock.
        /// </summary>
        public override void ReleaseLock()
        {
            if (Mutex != null)
            {
                Mutex.ReleaseMutex();
            }
        }

        private readonly string _mutexNamePrefix = "F72B817542FA4317BD70282748C0A79C";
    }
}
