using System;
using System.IO;
using System.Text;

namespace SMLogging
{
    /// <summary>
    /// Represents a base class for file lock handlers.
    /// </summary>
    public abstract class FileLockHandlerBase
    {
        /// <summary>
        /// When overridden in a derived class, opens the file.
        /// </summary>
        /// <param name="path">The path.</param>
        /// <param name="append">If set to <c>true</c>, append to the file.</param>
        /// <param name="encoding">The encoding.</param>
        public abstract void OpenFile(string path, bool append, Encoding encoding);

        /// <summary>
        /// When overridden in a derived class, closes the file.
        /// </summary>
        public abstract void CloseFile();

        /// <summary>
        /// When overridden in a derived class, acquires the lock on the file.
        /// </summary>
        /// <returns>The locked <see cref="Stream"/> object.</returns>
        public abstract Stream AcquireLock();

        /// <summary>
        /// When overridden in a derived class, releases the lock.
        /// </summary>
        public abstract void ReleaseLock();

        /// <summary>
        /// Gets the stream.
        /// </summary>
        protected Stream Stream { get; private set; }

        /// <summary>
        /// Creates the stream.
        /// </summary>
        /// <param name="path">The path.</param>
        /// <param name="append">If set to <c>true</c> append to the file.</param>
        /// <param name="fileShare">The file share.</param>
        protected void CreateStream(string path, bool append, FileShare fileShare)
        {
            var fileCreated = !File.Exists(path) || !append;

            Stream = new FileStream(path, append ? FileMode.Append : FileMode.Create, FileAccess.Write, fileShare);

            if (fileCreated)
            {
                File.SetCreationTimeUtc(path, DateTime.UtcNow);
            }
        }

        /// <summary>
        /// Closes the stream.
        /// </summary>
        protected void CloseStream()
        {
            if (Stream != null)
            {
                Stream.Close();
                Stream = null;
            }
        }
    }
}
