using System;
using System.IO;

namespace SMSvcHost.Diagnostics
{
    /// <summary>
    /// Represents a stream that can locked for writing to a file.
    /// </summary>
    public sealed class FileLockStream : Stream
    {
        /// <summary>
        /// Initializes a new instance of the <see cref="FileLockStream" /> class.
        /// </summary>
        /// <param name="lockHandler">The lock handler.</param>
        /// <exception cref="ArgumentNullException">Throw if <paramref name="lockHandler"/> is null.</exception>
        public FileLockStream(FileLockHandlerBase lockHandler)
        {
            if (lockHandler == null) throw new ArgumentNullException("lockHandler");

            _lockHandler = lockHandler;
        }

        /// <summary>
        /// Gets a value indicating whether this instance has a lock.
        /// </summary>
        public bool HasLock
        {
            get { return _innerStream != null; }
        }

        /// <summary>
        /// Gets a value indicating whether the current stream supports reading.
        /// </summary>
        public override bool CanRead
        {
            get { return false; }
        }

        /// <summary>
        /// Gets a value indicating whether the current stream supports seeking.
        /// </summary>
        public override bool CanSeek
        {
            get { return _innerStream.CanSeek; }
        }

        /// <summary>
        /// Gets a value indicating whether the current stream supports writing.
        /// </summary>
        public override bool CanWrite
        {
            get { return _innerStream.CanWrite; }
        }

        /// <summary>
        /// Gets the length in bytes of the stream.
        /// </summary>
        public override long Length
        {
            get { return _innerStream.Length; }
        }

        /// <summary>
        /// Gets or sets the position within the current stream.
        /// </summary>
        public override long Position
        {
            get { return _innerStream.Position; }
            set { _innerStream.Position = value; }
        }

        /// <summary>
        /// Begins an asynchronous read operation.
        /// </summary>
        /// <param name="buffer">The buffer to read the data into.</param>
        /// <param name="offset">The byte offset in <paramref name="buffer" /> at which to begin writing data read from the stream.</param>
        /// <param name="count">The maximum number of bytes to read.</param>
        /// <param name="callback">An optional asynchronous callback, to be called when the read is complete.</param>
        /// <param name="state">A user-provided object that distinguishes this particular asynchronous read request from other requests.</param>
        /// <returns>An <see cref="IAsyncResult" /> that represents the asynchronous read, which could still be pending.</returns>
        public override IAsyncResult BeginRead(byte[] buffer, int offset, int count, AsyncCallback callback, object state)
        {
            var result = _innerStream.BeginRead(buffer, offset, count, callback, state);
            _totalRead = EndRead(result);
            return result;
        }

        /// <summary>
        /// Waits for the pending asynchronous read to complete.
        /// </summary>
        /// <param name="asyncResult">The reference to the pending asynchronous request to finish.</param>
        /// <returns>
        /// The number of bytes read from the stream, between zero (0) and the number of bytes you requested. Streams return zero (0) only at
        ///  the end of the stream, otherwise, they should block until at least one byte is available.
        /// </returns>
        public override int EndRead(IAsyncResult asyncResult)
        {
            return _totalRead;
        }

        /// <summary>
        /// Begins an asynchronous write operation.
        /// </summary>
        /// <param name="buffer">The buffer to write data from.</param>
        /// <param name="offset">The byte offset in <paramref name="buffer" /> from which to begin writing.</param>
        /// <param name="count">The maximum number of bytes to write.</param>
        /// <param name="callback">An optional asynchronous callback, to be called when the write is complete.</param>
        /// <param name="state">A user-provided object that distinguishes this particular asynchronous write request from other requests.</param>
        /// <returns>An <see cref="IAsyncResult" /> that represents the asynchronous write, which could still be pending.</returns>
        public override IAsyncResult BeginWrite(byte[] buffer, int offset, int count, AsyncCallback callback, object state)
        {
            var result = _innerStream.BeginWrite(buffer, offset, count, callback, state);
            EndWrite(result);
            return result;
        }

        /// <summary>
        /// Ends an asynchronous write operation.
        /// </summary>
        /// <param name="asyncResult">A reference to the outstanding asynchronous I/O request.</param>
        public override void EndWrite(IAsyncResult asyncResult)
        {
        }

        /// <summary>
        /// Closes the current stream and releases any resources associated with the current stream.
        /// </summary>
        public override void Close()
        {
            _lockHandler.CloseFile();
        }

        /// <summary>
        /// Clears all buffers for this stream and causes any buffered data to be written to the underlying device.
        /// </summary>
        public override void Flush()
        {
            _innerStream.Flush();
        }

        /// <summary>
        /// Reads a sequence of bytes from the current stream and advances the position within the stream by the number of bytes read.
        /// </summary>
        /// <param name="buffer">
        /// An array of bytes. When this method returns, the buffer contains the specified byte array with the values between <paramref name="offset" /> 
        /// and (<paramref name="offset" /> + <paramref name="count" /> - 1) replaced by the bytes read from the current source.
        /// </param>
        /// <param name="offset">The zero-based byte offset in <paramref name="buffer" /> at which to begin storing the data read from the current stream.</param>
        /// <param name="count">The maximum number of bytes to be read from the current stream.</param>
        /// <returns>
        /// The total number of bytes read into the buffer. This can be less than the number of bytes requested if that many bytes are not currently 
        /// available, or zero (0) if the end of the stream has been reached.
        /// </returns>
        public override int Read(byte[] buffer, int offset, int count)
        {
            return _innerStream.Read(buffer, offset, count);
        }

        /// <summary>
        /// Reads a byte from the stream and advances the position within the stream by one byte, or returns -1 if at the end of the stream.
        /// </summary>
        /// <returns>The unsigned byte cast to an integer, or -1 if at the end of the stream.</returns>
        public override int ReadByte()
        {
            return _innerStream.ReadByte();
        }

        /// <summary>
        /// Sets the position within the current stream.
        /// </summary>
        /// <param name="offset">A byte offset relative to the <paramref name="origin" /> parameter.</param>
        /// <param name="origin">A value of type <see cref="SeekOrigin" /> indicating the reference point used to obtain the new position.</param>
        /// <returns>The new position within the current stream.</returns>
        public override long Seek(long offset, SeekOrigin origin)
        {
            return _innerStream.Seek(offset, origin);
        }

        /// <summary>
        /// Sets the length of the current stream.
        /// </summary>
        /// <param name="value">The desired length of the current stream in bytes.</param>
        public override void SetLength(long value)
        {
            _innerStream.SetLength(value);
        }

        /// <summary>
        /// Writes a byte to the current position in the stream and advances the position within the stream by one byte.
        /// </summary>
        /// <param name="value">The byte to write to the stream.</param>
        public override void WriteByte(byte value)
        {
            _innerStream.WriteByte(value);
        }

        /// <summary>
        /// Writes a sequence of bytes to the current stream and advances the current position within this stream by the number of bytes written.
        /// </summary>
        /// <param name="buffer">An array of bytes. This method copies <paramref name="count" /> bytes from <paramref name="buffer" /> to the current stream.</param>
        /// <param name="offset">The zero-based byte offset in <paramref name="buffer" /> at which to begin copying bytes to the current stream.</param>
        /// <param name="count">The number of bytes to be written to the current stream.</param>
        public override void Write(byte[] buffer, int offset, int count)
        {
            _innerStream.Write(buffer, offset, count);
        }

        /// <summary>
        /// Acquires the lock on the file.
        /// </summary>
        /// <returns><c>true</c> if a lock was acquired successfully.</returns>
        public bool AcquireLock()
        {
            var result = false;
            lock (_lock)
            {
                if (_lockLevel == 0)
                {
                    _innerStream = _lockHandler.AcquireLock();
                }
                if (_innerStream != null)
                {
                    _lockLevel++;
                    result = true;
                }
            }
            return result;
        }

        /// <summary>
        /// Releases the lock.
        /// </summary>
        public void ReleaseLock()
        {
            lock (_lock)
            {
                _lockLevel--;
                if (_lockLevel == 0)
                {
                    _lockHandler.ReleaseLock();
                    _innerStream = null;
                }
            }
        }

        /// <summary>
        /// Releases the unmanaged resources used by the <see cref="Stream" /> and optionally releases the managed resources.
        /// </summary>
        /// <param name="disposing">true to release both managed and unmanaged resources; false to release only unmanaged resources.</param>
        protected override void Dispose(bool disposing)
        {
            Close();

            base.Dispose(disposing);
        }

        private Stream _innerStream = null;
        private readonly FileLockHandlerBase _lockHandler = null;
        private int _totalRead = -1;
        private int _lockLevel = 0;
        private readonly object _lock = new object();
    }
}