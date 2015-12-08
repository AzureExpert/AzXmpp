using System;
using System.Collections.Concurrent;
using System.Net.Sockets;
using System.Threading;
using System.Threading.Tasks;

namespace AzXmpp.Transport.Sockets
{
    /// <summary>
    /// Represents <see cref="SocketAsyncEventArgs"/> that is suitable for send and receive operations.
    /// </summary>
    internal sealed class DataSocketAsyncEventArgs : SocketAsyncEventArgs
    {
        #region Pool
        private const int MaxPooled = 256;
        private static readonly ConcurrentStack<DataSocketAsyncEventArgs> _pool = new ConcurrentStack<DataSocketAsyncEventArgs>();

        /// <summary>
        /// Checks out an instance from the manager.
        /// </summary>
        /// <returns>A <see cref="DataSocketAsyncEventArgs" />.</returns>
        public static DataSocketAsyncEventArgs CheckOut()
        {
            DataSocketAsyncEventArgs result;
            if (!_pool.TryPop(out result))
                result = new DataSocketAsyncEventArgs();
            else
                result._checkedIn = 0;
            return result;
        }
        #endregion

        /// <summary>
        /// Gets the buffer list.
        /// </summary>
        /// <value>
        /// The buffer list.
        /// </value>
        public new BufferList BufferList
        {
            get { return _bufferList; }
        }

        private volatile int _checkedIn = 0;
        private volatile bool _cancelled;

        private volatile TaskCompletionSource<int> _completionSource;
        private CancellationTokenRegistration _registration;

        private readonly BufferList _bufferList;

        /// <summary>
        /// Initializes a new instance of the <see cref="DataSocketAsyncEventArgs"/> class.
        /// </summary>
        private DataSocketAsyncEventArgs()
        {
            base.BufferList = _bufferList = new BufferList();
        }

        /// <summary>
        /// Raises the <see cref="E:Completed" /> event.
        /// </summary>
        /// <param name="e">The <see cref="SocketAsyncEventArgs"/> instance containing the event data.</param>
        protected override void OnCompleted(SocketAsyncEventArgs e)
        {
            var completionSource = Interlocked.Exchange(ref _completionSource, null);
            try
            {
                if (completionSource == null)
                    return;
                else if (SocketError != SocketError.Success)
                    completionSource.TrySetException(new SocketException((int)SocketError));
                else if (_cancelled)
                {
                    // Do nothing.
                }
                else if (LastOperation == SocketAsyncOperation.Send
                    || LastOperation == SocketAsyncOperation.SendPackets
                    || LastOperation == SocketAsyncOperation.SendTo)
                    completionSource.TrySetResult(BytesTransferred);
                else // Read
                {
                    //_bufferList.CopyTo(_readDestination, BytesTransferred);
                    completionSource.TrySetResult(BytesTransferred);
                }
            }
            finally
            {
                CheckIn();
                base.OnCompleted(e);
            }
        }

        /// <summary>
        /// Checks in the event args if nessecary.
        /// </summary>
        public void CheckIn()
        {
            _registration.Dispose();
            if (Interlocked.Exchange(ref _checkedIn, 1) == 0)
            {
                if (_pool.Count < MaxPooled)
                {
                    _bufferList.Limit(4);
                    _pool.Push(this);
                    return;
                }
                _bufferList.Clear();
            }
        }

        /// <summary>
        /// Cancels the pending operation.
        /// </summary>
        private void Cancel()
        {
            _cancelled = true;
            _registration.Dispose();
            _completionSource.TrySetCanceled();
        }

        /// <summary>
        /// Begins an asynchronous read operation that uses these event args.
        /// </summary>
        /// <param name="socket">The socket.</param>
        /// <param name="buffer">The buffer.</param>
        /// <param name="offset">The offset.</param>
        /// <param name="count">The count.</param>
        /// <param name="cancellationToken">A token to monitor for cancellation requests. The default value is <see cref="System.Threading.CancellationToken.None" />.</param>
        /// <returns>
        /// A <see cref="Task{System.Int32}"/> that represents the asynchronous read operation.
        /// </returns>
        /// <exception cref="System.InvalidOperationException">EventArgs already in use.</exception>
        internal Task<int> ReadAsync(ISocket socket, byte[] buffer, int offset, int count, CancellationToken cancellationToken = default(CancellationToken))
        {
            try
            {
                if (count == 0) return Task.FromResult<int>(0);

                var completionSource = new TaskCompletionSource<int>();
                if (Interlocked.CompareExchange(ref _completionSource, completionSource, null) != null)
                    throw new InvalidOperationException(Properties.Resources.InvalidOperation_ObjectInUse);

                _cancelled = false;
                _registration = cancellationToken.Register(Cancel);

                // .Net sometimes returns before the data has been read (with an incorrect length).
                // KBs etc. that resolve this issue are unknown, so it's completely unusable.
                // Set the buffer directly.
                base.BufferList = null;
                SetBuffer(buffer, offset, count);

                if (!socket.ReceiveAsync(this))
                    OnCompleted(this);
                return completionSource.Task;
            }
            catch
            {
                CheckIn();
                throw;
            }
        }

        /// <summary>
        /// Begins an asynchronous write operation that uses these event args.
        /// </summary>
        /// <param name="socket">The socket.</param>
        /// <param name="buffer">The buffer.</param>
        /// <param name="offset">The offset.</param>
        /// <param name="count">The count.</param>
        /// <param name="cancellationToken">A token to monitor for cancellation requests. The default value is <see cref="System.Threading.CancellationToken.None" />.</param>
        /// <returns>
        /// A <see cref="Task"/> that represents the asynchronous write operation.
        /// </returns>
        /// <exception cref="System.InvalidOperationException">EventArgs already in use.</exception>
        internal Task WriteAsync(ISocket socket, byte[] buffer, int offset, int count, CancellationToken cancellationToken = default(CancellationToken))
        {
            try
            {
                var completionSource = new TaskCompletionSource<int>();
                if (Interlocked.CompareExchange(ref _completionSource, completionSource, null) != null)
                    throw new InvalidOperationException(Properties.Resources.InvalidOperation_ObjectInUse);

                _cancelled = false;
                _registration = cancellationToken.Register(Cancel);

                SetBuffer(null, 0, 0);
                if (Buffer == null | BufferList.CopyFrom(new ArraySegment<byte>(buffer, offset, count)))
                    base.BufferList = _bufferList;

                if (!socket.SendAsync(this))
                    OnCompleted(this);
                return completionSource.Task;
            }
            catch
            {
                CheckIn();
                throw;
            }
        }
    }
}
