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
    internal sealed class AcceptSocketAsyncEventArgs : SocketAsyncEventArgs
    {
        #region Pool
        private const int MaxPooled = 64;
        private static readonly ConcurrentStack<AcceptSocketAsyncEventArgs> _pool = new ConcurrentStack<AcceptSocketAsyncEventArgs>();

        /// <summary>
        /// Checks out an instance from the manager.
        /// </summary>
        /// <returns>A <see cref="AcceptSocketAsyncEventArgs" />.</returns>
        public static AcceptSocketAsyncEventArgs CheckOut()
        {
            AcceptSocketAsyncEventArgs result;
            if (!_pool.TryPop(out result))
                result = new AcceptSocketAsyncEventArgs();
            else
                result._checkedIn = 0;
            return result;
        }
        #endregion

        private volatile int _checkedIn = 0;
        private volatile bool _cancelled;

        private volatile TaskCompletionSource<ISocket> _completionSource;
        private CancellationTokenRegistration _registration;


        /// <summary>
        /// Prevents a default instance of the <see cref="AcceptSocketAsyncEventArgs"/> class from being created.
        /// </summary>
        private AcceptSocketAsyncEventArgs()
        {

        }

        /// <summary>Raises the <see cref="E:Completed" /> event.</summary>
        /// <param name="e">The <see cref="SocketAsyncEventArgs"/> instance containing the event data.</param>
        [System.Diagnostics.CodeAnalysis.SuppressMessage("Microsoft.Reliability", "CA2000:Dispose objects before losing scope", Justification = "Return value")]
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
                    AcceptSocket.Dispose();
                else // Accept
                    completionSource.TrySetResult(new NativeSocket(AcceptSocket));
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
                AcceptSocket = null;
                if (_pool.Count < MaxPooled)
                {
                    _pool.Push(this);
                    return;
                }
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
        /// <param name="cancellationToken">A token to monitor for cancellation requests. The default value is <see cref="System.Threading.CancellationToken.None" />.</param>
        /// <returns>
        /// A <see cref="Task{ISocket}" /> that represents the asynchronous accept operation.
        /// </returns>
        /// <exception cref="System.InvalidOperationException">EventArgs already in use.</exception>
        public Task<ISocket> AcceptAsync(ISocket socket, CancellationToken cancellationToken = default(CancellationToken))
        {
            var completionSource = new TaskCompletionSource<ISocket>();
            if (Interlocked.CompareExchange(ref _completionSource, completionSource, null) != null)
                throw new InvalidOperationException(Properties.Resources.InvalidOperation_ObjectInUse);

            _cancelled = false;
            _registration = cancellationToken.Register(Cancel);

            if (!socket.AcceptAsync(this))
                OnCompleted(this);

            return completionSource.Task;
        }
    }
}
