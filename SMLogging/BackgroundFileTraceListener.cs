using System.Collections.Concurrent;
using System.Collections.Generic;
using System.Configuration;
using System.Diagnostics;
using System.Threading;
using System.Threading.Tasks;
using SMLogging.Properties;

namespace SMLogging
{
    /// <summary>
    /// Represents a <see cref="FileTraceListener"/> that writes to files on a background thread.
    /// </summary>
    /// <seealso cref="FileTraceListener" />
    public class BackgroundFileTraceListener : FileTraceListener
    {
        #region Public Constructors

        /// <summary>
        /// Initializes a new instance of the <see cref="BackgroundFileTraceListener"/> class.
        /// </summary>
        /// <param name="path">The file path.</param>
        public BackgroundFileTraceListener(string path)
            : base(path)
        {
            FlushInterval = 5000;
            MaxFlushSize = 1000;
            MaxQueueSize = 10000;
        }

        /// <summary>
        /// Initializes a new instance of the <see cref="BackgroundFileTraceListener"/> class.
        /// </summary>
        /// <param name="path">The file path.</param>
        /// <param name="name">The trace listener name.</param>
        public BackgroundFileTraceListener(string path, string name)
            : base(path, name)
        {
            FlushInterval = 5000;
            MaxFlushSize = 1000;
            MaxQueueSize = 10000;
        }

        #endregion

        #region Public Properties

        /// <summary>
        /// Gets or sets the flush interval in milliseconds. 
        /// </summary>
        /// <remarks>
        /// This value specifies the interval between flushes performed by a background thread. If a negative value is specified, background 
        /// flushing will not be performed.  Target perform a flush every time an event is traced, enabled auto flushing on the trace source; 
        /// however this may result in slight performance degradation, as the flush will occur in the main thread. The default flush interval 
        /// is 5000 milliseconds.
        /// </remarks>
        [ConfigurationProperty("flushInterval")]
        public int FlushInterval { get; set; }

        /// <summary>
        /// Gets or sets the maximum size of a flush.
        /// </summary>
        /// <remarks>
        /// This value specifies the maximum number of events to flush to the file. The default value is 1000.
        /// </remarks>
        [ConfigurationProperty("maxFlushSize")]
        public int MaxFlushSize { get; set; }

        /// <summary>
        /// Gets or sets the maximum size of the queue.
        /// </summary>
        /// <remarks>
        /// This value specifies the maximum number of events to queue. The default value is 10000.
        /// </remarks>
        [ConfigurationProperty("maxQueueSize")]
        public int MaxQueueSize { get; set; }

        #endregion

        #region Protected Properties

        /// <summary>
        /// Gets a value indicating whether background flushing is started.
        /// </summary>
        protected bool IsBackgroundFlushing
        {
            get { return _workerCts != null; }
        }

        #endregion

        #region Public Methods

        /// <summary>
        /// Writes trace information to the listener specific output.
        /// </summary>
        /// <param name="eventCache">A <see cref="System.Diagnostics.TraceEventCache"/> object that contains the current process ID, thread ID, and stack trace information.</param>
        /// <param name="source">A name used to identify the output, typically the name of the application that generated the trace event.</param>
        /// <param name="eventType">One of the <see cref="System.Diagnostics.TraceEventType"/> values specifying the type of event that has caused the trace.</param>
        /// <param name="id">A numeric identifier for the event.</param>
        /// <param name="message">The message to write.</param>
        public override void WriteTrace(TraceEventCache eventCache, string source, TraceEventType eventType, int id, string message)
        {
            EnsureInitialized();

            EnqueueEvent(eventCache, source, eventType, id, message);
        }

        /// <summary>
        /// Flushes the queue.
        /// </summary>
        public override void Flush()
        {
            Event queuedEvent;
            var events = new List<Event>();
            while (events.Count < MaxFlushSize && _queue.TryDequeue(out queuedEvent))
            {
                events.Add(queuedEvent);
            }

            if (events.Count > 0)
            {
                RollFileIfNecessary();

                if (AcquireLock())
                {
                    try
                    {
                        foreach (var evt in events)
                        {
                            WriteTracePassthrough(evt.EventCache, evt.Source, evt.EventType, evt.Id, evt.Message);
                        }

                    }
                    finally
                    {
                        ReleaseLock();
                    }
                }
                else
                {
                    foreach (var evt in events)
                    {
                        EnqueueEvent(evt, false);
                    }
                }
            }

            if (_queue.Count == 0)
            {
                StopBackgroundFlushing();
            }

            base.Flush();
        }

        #endregion

        #region Protected Methods

        /// <summary>
        /// Releases the unmanaged resources used by the <see cref="System.Diagnostics.TraceListener" /> and optionally releases the managed resources.
        /// </summary>
        /// <param name="disposing">true to release both managed and unmanaged resources; false to release only unmanaged resources.</param>
        protected override void Dispose(bool disposing)
        {
            if (disposing)
            {
                StopBackgroundFlushing();

                Flush();
            }

            base.Dispose(disposing);
        }

        #endregion
        
        #region Private Methods

        private void EnqueueEvent(TraceEventCache eventCache, string source, TraceEventType eventType, int id, string message)
        {
            var evt = new Event
            {
                EventCache = eventCache,
                Source = source,
                EventType = eventType,
                Id = id,
                Message = message
            };

            EnqueueEvent(evt, true);

            if (FlushInterval >= 0 && !Trace.AutoFlush && !IsBackgroundFlushing)
            {
                StartBackgroundFlushing();
            }
        }

        private void EnqueueEvent(Event evt, bool force)
        {
            if (_queue.Count >= MaxQueueSize)
            {
                if (force)
                {
                    Event oldEvt;
                    _queue.Enqueue(evt);
                    _queue.TryDequeue(out oldEvt);
                }

                if (!_maxQueueSizeReached)
                {
                    _maxQueueSizeReached = true;
                    Fail(AssemblyResources.MaxQueueSizeReached);
                }
            }
            else
            {
                _queue.Enqueue(evt);
                _maxQueueSizeReached = false;
            }
        }

        private void StartBackgroundFlushing()
        {
            lock (_workerLock)
            {
                if (_workerCts != null)
                {
                    return;
                }

                _workerCts = new CancellationTokenSource();

                var ct = _workerCts.Token;

                Task.Factory.StartNew(() =>
                {
                    while (!ct.IsCancellationRequested)
                    {
                        Thread.Sleep(FlushInterval);

                        Flush();
                    }
                }, _workerCts.Token, TaskCreationOptions.LongRunning, TaskScheduler.Default);
            }
        }

        private void StopBackgroundFlushing()
        {
            lock (_workerLock)
            {
                if (_workerCts != null)
                {
                    _workerCts.Cancel();
                    _workerCts.Dispose();
                    _workerCts = null;
                }
            }
        }

        #endregion

        #region Private Fields

        private readonly ConcurrentQueue<Event> _queue = new ConcurrentQueue<Event>();
        private CancellationTokenSource _workerCts;
        private readonly object _workerLock = new object();
        private bool _maxQueueSizeReached = false;

        #endregion

        #region Private Classes

        private class Event
        {
            public TraceEventCache EventCache { get; set; }
            public string Source { get; set; }
            public TraceEventType EventType { get; set; }
            public int Id { get; set; }
            public string Message { get; set; }
        }

        #endregion
    }
}
