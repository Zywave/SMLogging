using System.IO;
using System.Text;

namespace SMRequestLogging
{
    /// <summary>
    /// Represents a file lock handler implementation that allows only a single instance to lock the file, by keeping the file stream open for the life of the stream.
    /// </summary>
    public class ExclusiveFileLockHandler : FileLockHandlerBase
    {
        /// <summary>
        /// Opens the file.
        /// </summary>
        /// <param name="path">The path.</param>
        /// <param name="append">If set to <c>true</c>, append to the file.</param>
        /// <param name="encoding">The encoding.</param>
        public override void OpenFile(string path, bool append, Encoding encoding)
        {
            CreateStream(path, append, FileShare.Read);
        }

        /// <summary>
        /// Closes the file.
        /// </summary>
        public override void CloseFile()
        {
            CloseStream();
        }

        /// <summary>
        /// Acquires the lock on the file.
        /// </summary>
        /// <returns>The locked <see cref="Stream" /> object.</returns>
        public override Stream AcquireLock()
        {
            return Stream;
        }

        /// <summary>
        /// Releases the lock.
        /// </summary>
        public override void ReleaseLock()
        {
        }
    }
}
