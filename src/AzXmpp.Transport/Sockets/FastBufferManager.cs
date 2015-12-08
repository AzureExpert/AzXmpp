using System;
using System.Threading;

namespace AzXmpp.Transport.Sockets
{
    /// <summary>
    /// Represents a buffer manager.
    /// </summary>
    sealed class FastBufferManager
    {
        private const int Decongest = 16;

        /// <summary>
        /// Represents a portion of the buffers, designed to reduce congestion.
        /// </summary>
        private struct CpuBuffer
        {
            private readonly int _maxBuffers;
            private readonly int _bufferSize;
            private readonly byte[][] _buffers;
            private volatile int _count;

            /// <summary>
            /// Initializes a new instance of the <see cref="CpuBuffer"/> struct.
            /// </summary>
            /// <param name="maxBuffers">The maximum number of buffers.</param>
            /// <param name="bufferSize">The size of each buffer.</param>
            /// <param name="allocate">if set to <c>true</c> the buffers will be immediately allocated.</param>
            public CpuBuffer(int maxBuffers, int bufferSize, bool allocate)
            {
                _maxBuffers = maxBuffers;
                _bufferSize = bufferSize;
                _buffers = new byte[_maxBuffers][];

                if (allocate)
                {
                    _count = maxBuffers;
                    for (var i = 0; i < maxBuffers; i++)
                        _buffers[i] = new byte[_bufferSize];
                }
                else
                {
                    _count = 0;
                }
            }

            /// <summary>
            /// Returns the buffer to the manager.
            /// </summary>
            /// <param name="buffer">The buffer to return.</param>
            /// <param name="forceLock">if set to <c>true</c> a lock will be forced.</param>
            /// <returns>A value indicating whether the buffer was returned.</returns>
            public bool ReturnBuffer(byte[] buffer, bool forceLock)
            {
                if (_count == _maxBuffers)
                    return false;
                else if (forceLock)
                    Monitor.Enter(_buffers);
                else if (!Monitor.TryEnter(_buffers, 0))
                    return false;

                try
                {
                    if (_count < _maxBuffers)
                    {
                        _buffers[_count++] = buffer;
                        return true;
                    }
                    return false;
                }
                finally
                {
                    Monitor.Exit(_buffers);
                }
            }

            /// <summary>
            /// Takes a buffer from the buffer manager.
            /// </summary>
            /// <param name="forceLock">if set to <c>true</c> a lock will be forced.</param>
            /// <returns>
            /// The buffer.
            /// </returns>
            public byte[] TakeBuffer(bool forceLock)
            {
                if (_count == 0)
                    return null;
                else if (forceLock)
                    Monitor.Enter(_buffers);
                else if (!Monitor.TryEnter(_buffers, 0))
                    return null;

                try
                {
                    if (_count > 0)
                    {
                        return _buffers[--_count];
                    }
                    return null;
                }
                finally
                {
                    Monitor.Exit(_buffers);
                }
            }

            /// <summary>
            /// Takes multiple buffers from the buffer manager.
            /// </summary>
            /// <param name="target">The array into which buffers will be set.</param>
            /// <param name="offset">The offset into the array where buffers should be set to.</param>
            /// <param name="count">The number of buffers to set.</param>
            /// <param name="forceLock">if set to <c>true</c> a lock will be forced.</param>
            /// <returns>The number of buffers taken.</returns>
            internal int TakeBuffers(byte[][] target, int offset, int count, bool forceLock)
            {
                if (_count == 0)
                    return 0;
                else if (forceLock)
                    Monitor.Enter(_buffers);
                else if (!Monitor.TryEnter(_buffers, 0))
                    return 0;

                try
                {
                    var toPop = Math.Min(_count, count);
                    if (toPop > 0)
                    {
                        Array.Copy(_buffers, _count - toPop, target, offset, toPop);
                        _count -= toPop;
                    }
                    return toPop;
                }
                finally
                {
                    Monitor.Exit(_buffers);
                }
            }

            /// <summary>
            /// Clears the buffer manager.
            /// </summary>
            public void Clear()
            {
                lock (_buffers)
                {
                    _count = 0;
                }
            }
        }

        private readonly int _maxBuffers;
        private readonly int _bufferSize;
        private CpuBuffer[] _buffers;

        /// <summary>
        /// Initializes a new instance of the <see cref="FastBufferManager"/> class.
        /// </summary>
        /// <param name="maxBufferPoolSize">The maximum size of the pooled buffers.</param>
        /// <param name="bufferSize">The size of the each buffer.</param>
        public FastBufferManager(int maxBufferPoolSize, int bufferSize)
            : this(maxBufferPoolSize, bufferSize, false)
        {

        }

        /// <summary>
        /// Initializes a new instance of the <see cref="FastBufferManager"/> class.
        /// </summary>
        /// <param name="maxBufferPoolSize">The maximum size of the pooled buffers.</param>
        /// <param name="bufferSize">The size of the each buffer.</param>
        /// <param name="allocate">If set to <c>true</c> all buffers will be allocated.</param>
        public FastBufferManager(int maxBufferPoolSize, int bufferSize, bool allocate)
        {
            if (maxBufferPoolSize < bufferSize) throw new ArgumentOutOfRangeException("maxBufferPoolSize");
            if (bufferSize <= 0) throw new ArgumentOutOfRangeException("bufferSize");

            _maxBuffers = ((maxBufferPoolSize - 1 + bufferSize) / bufferSize) / Decongest;
            _bufferSize = bufferSize;

            _buffers = new CpuBuffer[Decongest];
            for (var i = 0; i < Decongest; i++)
                _buffers[i] = new CpuBuffer(_maxBuffers, bufferSize, allocate);
        }

        /// <summary>
        /// Clears this instance.
        /// </summary>
        public void Clear()
        {
            for (var i = 0; i < Decongest; i++)
            {
                _buffers[i].Clear();
            }
        }

        /// <summary>
        /// Returns the buffer to the manager.
        /// </summary>
        /// <param name="buffer">The buffer to return.</param>
        public void ReturnBuffer(byte[] buffer)
        {
            if (buffer == null) throw new ArgumentNullException("buffer");
            if (buffer.Length != _bufferSize) throw new ArgumentOutOfRangeException("buffer");

            var id = Thread.CurrentThread.ManagedThreadId;
            for (var i = 0; i < Decongest; i++)
            {
                var j = (i + id) % Decongest;
                if (_buffers[j].ReturnBuffer(buffer, i == 0))
                    return;
            }
        }

        /// <summary>
        /// Takes a buffer from the buffer manager.
        /// </summary>
        /// <param name="bufferSize">The size of the buffer.</param>
        /// <returns>The buffer.</returns>
        public byte[] TakeBuffer(int bufferSize)
        {
            if (bufferSize != _bufferSize) throw new ArgumentOutOfRangeException("bufferSize");

            var id = Thread.CurrentThread.ManagedThreadId;
            for (var i = 0; i < Decongest; i++)
            {
                var j = (i + id) % Decongest;
                var result = _buffers[j].TakeBuffer(i == 0);
                if (result != null) return result;
            }
            return new byte[_bufferSize];
        }

        /// <summary>
        /// Takes a set of buffers from the buffer manager.
        /// </summary>
        /// <param name="bufferSize">The size of the buffers to pop.</param>
        /// <param name="target">The array into which buffers will be set.</param>
        /// <param name="offset">The offset into the array where buffers should be set to.</param>
        /// <param name="count">The number of buffers to set.</param>
        public void TakeBuffers(int bufferSize, byte[][] target, int offset, int count)
        {
            if (target == null) throw new ArgumentNullException("target");
            if (bufferSize != _bufferSize) throw new ArgumentOutOfRangeException("bufferSize");
            if (offset < 0) throw new ArgumentOutOfRangeException("offset");
            if (count < 0) throw new ArgumentOutOfRangeException("count");
            if (count == 0) return;

            var id = Thread.CurrentThread.ManagedThreadId;
            var remainingCount = count;
            for (var i = 0; i < Decongest && remainingCount != 0; i++)
            {
                var j = (i + id) % Decongest;
                var result = _buffers[j].TakeBuffers(target, offset, remainingCount, i == 0);
                offset += result;
                remainingCount -= result;
            }

            for (; remainingCount != 0; remainingCount--, offset++)
            {
                target[offset] = new byte[_bufferSize];
            }
        }
    }
}
