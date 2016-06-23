using System.IO;
using System.Text;

namespace SMRequestLogging
{

    /// <summary>
    /// Represents a file lock handler implementation that keeps a lock on a file for the minimal amount of time necessary, by opening and closing the file stream
    /// on <see cref="MinimalFileLockHandler.AcquireLock"/> and <see cref="MinimalFileLockHandler.ReleaseLock"/> respectively.
    /// </summary>
    public class MinimalFileLockHandler : FileLockHandlerBase
    {
        /// <summary>
        /// Opens the file.
        /// </summary>
        /// <param name="path">The path.</param>
        /// <param name="append">If set to <c>true</c>, append to the file.</param>
        /// <param name="encoding">The encoding.</param>
        public override void OpenFile(string path, bool append, Encoding encoding)
        {
            _path = path;
            _append = append;
        }

        /// <summary>
        /// Closes the file.
        /// </summary>
        public override void CloseFile()
        {
        }

        /// <summary>
        /// Acquires the lock on the file.
        /// </summary>
        /// <returns>The locked <see cref="Stream" /> object.</returns>
        public override Stream AcquireLock()
        {
            if (Stream == null)
            {
                CreateStream(_path, _append, FileShare.Read);
            }
            return Stream;
        }

        /// <summary>
        /// Releases the lock.
        /// </summary>
        public override void ReleaseLock()
        {
            CloseStream();
        }

        private string _path;
        private bool _append;
    }
}