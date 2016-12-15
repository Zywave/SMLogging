using System;
using System.Collections.Generic;
using System.ComponentModel;
using System.Configuration;
using System.Diagnostics;
using System.Globalization;
using System.IO;
using System.Text;
using SMLogging.Properties;

namespace SMLogging
{
    /// <summary>
    /// Represents a trace listener that emits trace statements to files.
    /// </summary>
    public class FileTraceListener : TraceListenerBase
    {
        #region Public Constructors
        
        /// <summary>
        /// Initializes a new instance of the <see cref="FileTraceListener"/> class.
        /// </summary>
        /// <param name="path">The file path.</param>
        public FileTraceListener(string path)
        {
            _sourceFilePath = path;

            RollingMode = FileRollingMode.None;
            RollingInterval = FileRollingInterval.None;
            MaximumFileSize = 10485760;
            MaximumFileIndex = 1;
            LockingMode = FileLockingMode.Mutex;
            AppendToFile = true;
            Encoding = Encoding.Default;
        }

        /// <summary>
        /// Initializes a new instance of the <see cref="FileTraceListener"/> class.
        /// </summary>
        /// <param name="path">The file path.</param>
        /// <param name="name">The trace listener name.</param>
        public FileTraceListener(string path, string name)
            : base(name)
        {
            _sourceFilePath = path;

            RollingMode = FileRollingMode.None;
            RollingInterval = FileRollingInterval.None;
            MaximumFileSize = 10485760;
            MaximumFileIndex = 1;
            LockingMode = FileLockingMode.Mutex;
            AppendToFile = true;
            Encoding = Encoding.Default;
        }

        #endregion

        #region Public Properties

        /// <summary>
        /// Gets or sets the rolling mode.
        /// </summary>
        [ConfigurationProperty("rollingMode")]
        public FileRollingMode RollingMode { get; set; }

        /// <summary>
        /// Gets or sets the rolling interval.
        /// </summary>
        [ConfigurationProperty("rollingInterval")]
        public FileRollingInterval RollingInterval { get; set; }

        /// <summary>
        /// Gets or sets the maximum file size.
        /// </summary>
        [ConfigurationProperty("maximumFileSize")]
        public int MaximumFileSize { get; set; }

        /// <summary>
        /// Gets or sets the maximum file index.
        /// </summary>
        [ConfigurationProperty("maximumFileIndex")]
        public int MaximumFileIndex { get; set; }

        /// <summary>
        /// Gets or sets the file locking mode.
        /// </summary>
        [ConfigurationProperty("lockingMode")]
        public FileLockingMode LockingMode { get; set; }

        /// <summary>
        /// Gets or sets a value indicating whether to append to or replace the files.
        /// </summary>
        [ConfigurationProperty("appendToFile")]
        public bool AppendToFile { get; set; }

        /// <summary>
        /// Gets or sets the encoding.
        /// </summary>
        [ConfigurationProperty("encoding")]
        [TypeConverter(typeof(EncodingConverter))]
        public Encoding Encoding { get; set; }

        #endregion

        #region Protected Properties

        /// <summary>
        /// Gets the text writer.
        /// </summary>
        protected TextWriter Writer
        {
            get { return _writer; }
        }

        /// <summary>
        /// Gets the current file path.
        /// </summary>
        protected string FilePath
        {
            get { return _filePath; }
        }

        /// <summary>
        /// Gets the source file path.
        /// </summary>
        protected string SourceFilePath
        {
            get { return _sourceFilePath; }
        }

        #endregion

        #region Public Methods

        /// <summary>
        /// Writes trace information to the listener specific output after aquiring a file lock.
        /// </summary>
        /// <param name="eventCache">A <see cref="System.Diagnostics.TraceEventCache"/> object that contains the current process ID, thread ID, and stack trace information.</param>
        /// <param name="source">A name used to identify the output, typically the name of the application that generated the trace event.</param>
        /// <param name="eventType">One of the <see cref="System.Diagnostics.TraceEventType"/> values specifying the type of event that has caused the trace.</param>
        /// <param name="id">A numeric identifier for the event.</param>
        /// <param name="message">The message to write.</param>
        public override void WriteTrace(TraceEventCache eventCache, string source, TraceEventType eventType, int id, string message)
        {
            EnsureInitialized();

            RollFileIfNecessary();

            if (AcquireLock())
            { 
                try
                {
                    base.WriteTrace(eventCache, source, eventType, id, message);
                }
                finally
                {
                    ReleaseLock();
                }
            }
        }

        /// <summary>
        /// Flushes the trace listener.
        /// </summary>
        public override void Flush()
        {
            if (AcquireLock())
            {
                try
                {
                    Writer.Flush();
                }
                finally
                {
                    ReleaseLock();
                }
            }
        }

        /// <summary>
        /// Writes the specified message to the listener.
        /// </summary>
        /// <param name="message">The message to write.</param>
        public override void Write(string message)
        {
            if (EnsureWriter())
            {
                if (NeedIndent)
                {
                    WriteIndent();
                }
                Writer.Write(message);
            }
        }

        /// <summary>
        /// Writes the specified message to the listener.
        /// </summary>
        /// <param name="message">The message to write.</param>
        public override void WriteLine(string message)
        {
            if (EnsureWriter())
            {
                if (NeedIndent)
                {
                    WriteIndent();
                }
                Writer.WriteLine(message);
                NeedIndent = true;
            }
        }

        #endregion

        #region Protected Methods

        /// <summary>
        /// Initializes the trace listener.
        /// </summary>
        protected override void Initialize()
        {
            base.Initialize();

            InitializeFile();
        }

        /// <summary>
        /// Initializes the file.
        /// </summary>
        protected virtual void InitializeFile()
        {
            ResolveInitialFilePath();
            OpenFile();
        }
       
        /// <summary>
        /// Ensures that the writer can be used.
        /// </summary>
        /// <returns>A value indicating that the writer can be used.</returns>
        protected virtual bool EnsureWriter()
        {
            return _writer != null && _lockStream != null && _lockStream.HasLock;
        }

        /// <summary>
        /// Attempts to get a lock on the file stream.
        /// </summary>
        /// <returns>A value indicating whether the stream was successfully locked.</returns>
        protected bool AcquireLock()
        {
            if (_lockStream != null)
            {
                try
                {
                    return _lockStream.AcquireLock(); 
                }
                catch (Exception ex)
                {
                    Fail(ex.Message, ex.ToString());
                    if (Debugger.IsAttached)
                    {
                        throw;
                    }
                }
            }
            return false;
        }

        /// <summary>
        /// Releases a the lock on the file stream.
        /// </summary>
        protected void ReleaseLock()
        {
            if (_lockStream != null)
            {
                try
                {
                    _lockStream.ReleaseLock();
                }
                catch (Exception ex)
                {
                    Fail(ex.Message, ex.ToString());
                    if (Debugger.IsAttached)
                    {
                        throw;
                    }
                }
            }
        }

        /// <summary>
        /// Open the file.
        /// </summary>
        protected void OpenFile()
        {
            if (_filePath != null)
            {
                try
                {
                    EnsureFileDirectory();

                    _lockHandler = CreateLockHandler();
                    _lockHandler.OpenFile(_filePath, AppendToFile, Encoding);
                    _lockStream = new FileLockStream(_lockHandler);
                }
                catch (Exception ex)
                {
                    Fail(ex.Message, ex.ToString());
                    if (Debugger.IsAttached)
                    {
                        throw;
                    }
                }

                if (AcquireLock())
                {
                    try
                    {
                        _writer = new StreamWriter(_lockStream, Encoding);
                    }
                    finally
                    {
                        ReleaseLock();
                    }
                }
            }
        }

        /// <summary>
        /// Close the file.
        /// </summary>
        protected void CloseFile()
        {
            if (_filePath != null)
            {
                if (AcquireLock())
                {
                    try
                    {
                        _writer.Close();
                    }
                    finally
                    {
                        ReleaseLock();
                    }
                }

                _writer = null;
                _lockHandler = null;
                _lockStream = null;
            }
        }

        /// <summary>
        /// Roll the file if necessary.
        /// </summary>
        protected void RollFileIfNecessary()
        {
            if (_filePath != null && CheckFileRollConditions())
            {
                CloseFile();

                ResolveNextFilePath();

                if (CheckFileRollConditions())
                {
                    DeleteFile();
                }

                OpenFile();
            }
        }

        /// <summary>
        /// Writes trace information to the listener specific output. Intended for sub-classes to use for passthrough to base class implementation.
        /// </summary>
        /// <param name="eventCache">A <see cref="System.Diagnostics.TraceEventCache"/> object that contains the current process ID, thread ID, and stack trace information.</param>
        /// <param name="source">A name used to identify the output, typically the name of the application that generated the trace event.</param>
        /// <param name="eventType">One of the <see cref="System.Diagnostics.TraceEventType"/> values specifying the type of event that has caused the trace.</param>
        /// <param name="id">A numeric identifier for the event.</param>
        /// <param name="message">The message to write.</param>
        protected void WriteTracePassthrough(TraceEventCache eventCache, string source, TraceEventType eventType, int id, string message)
        {
            base.WriteTrace(eventCache, source, eventType, id, message);
        }

        /// <summary>
        /// Disposes of the trace listener.
        /// </summary>
        /// <param name="disposing">A value indicating whether to dispose managed and unmanaged resources.</param>
        protected override void Dispose(bool disposing)
        {
            if (disposing)
            {
                if (_lockStream != null && _lockStream.AcquireLock())
                {
                    try
                    {
                        _writer.Close();
                    }
                    finally
                    {
                        _lockStream.ReleaseLock();
                    }

                    _lockStream.Close();
                }

                _writer = null;
                _lockHandler = null;
                _lockStream = null;
            }

            base.Dispose(disposing);
        }
        
        #endregion

        #region Private Methods

        private void EnsureFileDirectory()
        {
            Directory.CreateDirectory(Path.GetDirectoryName(_filePath));
        }

        private void ResolveInitialFilePath()
        {
            string path = FormatFilePath(_sourceFilePath);

            path = GetFullPath(path);

            if (ValidateFilePath(path))
            {
                if (RollingMode != FileRollingMode.None && MaximumFileIndex > 1)
                {
                    string filePath = Path.GetDirectoryName(path);
                    string fileName = Path.GetFileNameWithoutExtension(path);
                    string fileExtension = Path.GetExtension(path);
                    int currentFileIndex = -1;
                    do
                    {
                        if (currentFileIndex >= MaximumFileIndex)
                        {
                            break;
                        }
                        currentFileIndex++;
                        path = String.Format(CultureInfo.InvariantCulture, @"{0}\{1}.{2}{3}", filePath, fileName, currentFileIndex, fileExtension);
                    }
                    while (CheckFileRollConditions(path));
                }

                _filePath = path;
            }
        }

        private void ResolveNextFilePath()
        {
            string path = FormatFilePath(_sourceFilePath);

            path = GetFullPath(path);

            if (ValidateFilePath(path))
            {
                if (RollingMode != FileRollingMode.None && MaximumFileIndex > 1)
                {
                    int currentFileIndex = -1;
                    string filePath = Path.GetDirectoryName(path);
                    string fileName = Path.GetFileNameWithoutExtension(path);
                    string fileExtension = Path.GetExtension(path);
                    int subStringIndex = fileName.LastIndexOf('.') + 1;
                    if (subStringIndex > 0)
                    {
                        string indexString = fileName.Substring(subStringIndex);
                        if (!Int32.TryParse(indexString, out currentFileIndex))
                        {
                            currentFileIndex = -1;
                        }
                    }

                    int nextFileIndex = currentFileIndex + 1;
                    if (nextFileIndex >= MaximumFileIndex)
                    {
                        nextFileIndex = 0;
                    }

                    path = String.Format(CultureInfo.InvariantCulture, @"{0}\{1}.{2}{3}", filePath, fileName, nextFileIndex, fileExtension);
                }

                _filePath = path;
            }
        }

        private static string GetFullPath(string path)
        {
            path = Environment.ExpandEnvironmentVariables(path);

            if (path.StartsWithAny("~/", @"~\"))
            {
                path = AppDomain.CurrentDomain.BaseDirectory + path.Substring(2);
            }

            return Path.GetFullPath(path);
        }

        private string FormatFilePath(string path)
        {
            var namedArgs = new Dictionary<string, object>();
            var dateTime = DateTime.UtcNow;
            if (TimeZone != null)
            {
                dateTime = TimeZoneInfo.ConvertTime(dateTime, TimeZone);
            }
            namedArgs["DateTime"] = dateTime;
            namedArgs["Timestamp"] = DateTime.UtcNow.Ticks;
            namedArgs["ProcessId"] = ProcessId;
            namedArgs["ProcessName"] = RemoveInvalidFileNameCharacters(ProcessName);
            namedArgs["AppName"] = RemoveInvalidFileNameCharacters(AppName);

            return StringHelpers.NamedFormat(CultureInfo.InvariantCulture, path, namedArgs);
        }

        private bool ValidateFilePath(string path)
        {
            bool isValid;

            if (path.IndexOfAny(Path.GetInvalidPathChars()) == -1 &&
                Path.GetFileName(path).IndexOfAny(Path.GetInvalidFileNameChars()) == -1 &&
                !String.IsNullOrEmpty(Path.GetFileNameWithoutExtension(path)) &&
                !String.IsNullOrEmpty(Path.GetExtension(path)))
            {
                isValid = true;
            }
            else
            {
                isValid = false;
                Fail(AssemblyResources.InvalidTraceListenerFilePath);
            }

            return isValid;
        }
        
        private void DeleteFile()
        {
            try
            {
                File.Delete(_filePath);
            }
            catch (Exception ex)
            {
                Fail(ex.Message, ex.ToString());
                if (Debugger.IsAttached)
                {
                    throw;
                }
            }
        }

        private bool CheckFileRollConditions()
        {
            return CheckFileRollConditions(_filePath);
        }

        private bool CheckFileRollConditions(string path)
        {
            bool conditionMet = false;

            if (RollingMode != FileRollingMode.None)
            {
                FileInfo fileInfo = new FileInfo(path);
                if (fileInfo.Exists)
                {
                    if (RollingMode == FileRollingMode.Size && MaximumFileSize > 0)
                    {
                        if (fileInfo.Length >= MaximumFileSize)
                        {
                            conditionMet = true;
                        }
                    }
                    else if (RollingMode == FileRollingMode.DateTime && RollingInterval != FileRollingInterval.None)
                    {
                        DateTime fileCreationTime = fileInfo.CreationTimeUtc;
                        if (TimeZone != null)
                        {
                            fileCreationTime = TimeZoneInfo.ConvertTime(fileCreationTime, TimeZone);
                        }
                        DateTime rollingIntervalDateTime = GetRollingIntervalDateTime();
                        if (fileCreationTime < rollingIntervalDateTime)
                        {
                            conditionMet = true;
                        }
                    }
                }
            }

            return conditionMet;
        }

        private DateTime GetRollingIntervalDateTime()
        {
            DateTime dateTime = DateTime.UtcNow;
            if (TimeZone != null)
            {
                dateTime = TimeZoneInfo.ConvertTime(dateTime, TimeZone);
            }
            switch (RollingInterval)
            {
                case FileRollingInterval.Minute:
                    dateTime = dateTime.AddMilliseconds(-dateTime.Millisecond);
                    dateTime = dateTime.AddSeconds(-dateTime.Second);
                    break;
                case FileRollingInterval.Hour:
                    dateTime = dateTime.AddMilliseconds(-dateTime.Millisecond);
                    dateTime = dateTime.AddSeconds(-dateTime.Second);
                    dateTime = dateTime.AddMinutes(-dateTime.Minute);
                    break;
                case FileRollingInterval.Day:
                    dateTime = dateTime.AddMilliseconds(-dateTime.Millisecond);
                    dateTime = dateTime.AddSeconds(-dateTime.Second);
                    dateTime = dateTime.AddMinutes(-dateTime.Minute);
                    dateTime = dateTime.AddHours(-dateTime.Hour);
                    break;
                case FileRollingInterval.Week:
                    dateTime = dateTime.AddMilliseconds(-dateTime.Millisecond);
                    dateTime = dateTime.AddSeconds(-dateTime.Second);
                    dateTime = dateTime.AddMinutes(-dateTime.Minute);
                    dateTime = dateTime.AddHours(-dateTime.Hour);
                    dateTime = dateTime.AddDays(-(int)dateTime.DayOfWeek);
                    break;
                case FileRollingInterval.Month:
                    dateTime = dateTime.AddMilliseconds(-dateTime.Millisecond);
                    dateTime = dateTime.AddSeconds(-dateTime.Second);
                    dateTime = dateTime.AddMinutes(-dateTime.Minute);
                    dateTime = dateTime.AddHours(-dateTime.Hour);
                    dateTime = dateTime.AddDays(1 - dateTime.Day);
                    break;
            }
            return dateTime;
        }

        private FileLockHandlerBase CreateLockHandler()
        {
            switch (LockingMode)
            {
                case FileLockingMode.Exclusive: return new ExclusiveFileLockHandler();
                case FileLockingMode.Minimal: return new MinimalFileLockHandler();
                case FileLockingMode.Mutex: return new MutexFileLockHandler("D49A1B991B5D4A1396026DB4F1A54EA5");
                default: return new ExclusiveFileLockHandler();
            }
        }

        private static string RemoveInvalidFileNameCharacters(string value)
        { 
            foreach (var invalidChar in Path.GetInvalidFileNameChars())
            {
                value = value.Replace(invalidChar.ToString(), string.Empty);
            }

            return value;
        }

        #endregion

        #region Private Fields

        private string _filePath;
        private readonly string _sourceFilePath;
        private FileLockHandlerBase _lockHandler;
        private FileLockStream _lockStream;
        private TextWriter _writer;
        
        #endregion
    }
}
