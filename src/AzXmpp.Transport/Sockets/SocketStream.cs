using System;
using System.IO;
using System.Net.Sockets;
using System.Threading;
using System.Threading.Tasks;

namespace AzXmpp.Transport.Sockets
{
    /// <summary>
    /// Represents a socket stream.
    /// </summary>
    internal sealed class SocketStream : Stream
    {
        public static readonly Task Completed = Task.FromResult<object>(null);

        /// <summary>
        /// Gets a value indicating whether the current stream supports reading.
        /// </summary>
        /// <returns>true if the stream supports reading; otherwise, false.</returns>
        public override bool CanRead
        {
            get { return true; }
        }

        /// <summary>
        /// Gets a value indicating whether the current stream supports seeking.
        /// </summary>
        /// <returns>true if the stream supports seeking; otherwise, false.</returns>
        public override bool CanSeek
        {
            get { return false; }
        }

        /// <summary>
        /// Gets a value indicating whether the current stream supports writing.
        /// </summary>
        /// <returns>true if the stream supports writing; otherwise, false.</returns>
        public override bool CanWrite
        {
            get { return true; }
        }

        /// <summary>
        /// Gets the length in bytes of the stream.
        /// </summary>
        /// <returns>A long value representing the length of the stream in bytes.</returns>
        /// <exception cref="System.NotSupportedException">Seeking is not supported on <see cref="SocketStream"/>.</exception>
        public override long Length
        {
            get { throw new NotSupportedException(Properties.Resources.InvalidOperation_SeekingNotSupported); }
        }

        /// <summary>
        /// When overridden in a derived class, gets or sets the position within the current stream.
        /// </summary>
        /// <returns>The current position within the stream.</returns>
        /// <exception cref="System.NotSupportedException">Seeking is not supported on <see cref="SocketStream"/>.</exception>
        public override long Position
        {
            get { throw new NotSupportedException(Properties.Resources.InvalidOperation_SeekingNotSupported); }
            set { throw new NotSupportedException(Properties.Resources.InvalidOperation_SeekingNotSupported); }
        }

        /// <summary>
        /// Gets a value that determines whether the current stream can time out.
        /// </summary>
        /// <returns>A value that determines whether the current stream can time out.</returns>
        public override bool CanTimeout
        {
            get { return true; }
        }

        /// <summary>
        /// Gets or sets a value, in miliseconds, that determines how long the stream will attempt to read before timing out.
        /// </summary>
        /// <returns>A value, in miliseconds, that determines how long the stream will attempt to read before timing out.</returns>
        public override int ReadTimeout
        {
            get { return _socket.ReceiveTimeout; }
            set { _socket.ReceiveTimeout = value; }
        }

        /// <summary>
        /// Gets or sets a value, in miliseconds, that determines how long the stream will attempt to write before timing out.
        /// </summary>
        /// <returns>A value, in miliseconds, that determines how long the stream will attempt to write before timing out.</returns>
        public override int WriteTimeout
        {
            get { return _socket.SendTimeout; }
            set { _socket.SendTimeout = value; }
        }

        /// <summary>
        /// Gets a value indicating whether this instance is disposed.
        /// </summary>
        /// <value>
        /// <c>true</c> if this instance is disposed; otherwise, <c>false</c>.
        /// </value>
        public bool IsDisposed
        {
            get { return _state != 0; }
        }

        /// <summary>
        /// Gets the socket.
        /// </summary>
        /// <value>
        /// The socket.
        /// </value>
        public ISocket Socket
        {
            get { return _socket; }
        }

        private readonly ISocket _socket;
        private volatile int _state;

        /// <summary>
        /// Initializes a new instance of the <see cref="SocketStream"/> class.
        /// </summary>
        public SocketStream(ISocket socket)
        {
            if (socket == null) throw new ArgumentNullException("socket");
            _socket = socket;
        }

        /// <summary>
        /// Asynchronously reads a sequence of bytes from the current stream, advances the position within the stream by the number of bytes read, and monitors cancellation requests.
        /// </summary>
        /// <param name="buffer">The buffer to write the data into.</param>
        /// <param name="offset">The byte offset in <paramref name="buffer" /> at which to begin writing data from the stream.</param>
        /// <param name="count">The maximum number of bytes to read.</param>
        /// <param name="cancellationToken">The token to monitor for cancellation requests. The default value is <see cref="P:System.Threading.CancellationToken.None" />.</param>
        /// <returns>
        /// A task that represents the asynchronous read operation. The value of the <see name="Task{int}.Result" /> parameter contains the total number of bytes read into the buffer. The result value can be less than the number of bytes requested if the number of bytes currently available is less than the requested number, or it can be 0 (zero) if the end of the stream has been reached.
        /// </returns>
        public override Task<int> ReadAsync(byte[] buffer, int offset, int count, CancellationToken cancellationToken)
        {
            return DataSocketAsyncEventArgs.CheckOut().ReadAsync(_socket, buffer, offset, count, cancellationToken);
        }

        /// <summary>
        /// Asynchronously writes a sequence of bytes to the current stream, advances the current position within this stream by the number of bytes written, and monitors cancellation requests.
        /// </summary>
        /// <param name="buffer">The buffer to write data from.</param>
        /// <param name="offset">The zero-based byte offset in <paramref name="buffer" /> from which to begin copying bytes to the stream.</param>
        /// <param name="count">The maximum number of bytes to write.</param>
        /// <param name="cancellationToken">The token to monitor for cancellation requests. The default value is <see cref="P:System.Threading.CancellationToken.None" />.</param>
        /// <returns>
        /// A task that represents the asynchronous write operation.
        /// </returns>
        public override Task WriteAsync(byte[] buffer, int offset, int count, CancellationToken cancellationToken)
        {
            return DataSocketAsyncEventArgs.CheckOut().WriteAsync(_socket, buffer, offset, count, cancellationToken);
        }

        /// <summary>
        /// Asynchronously clears all buffers for this stream, causes any buffered data to be written to the underlying device, and monitors cancellation requests.
        /// </summary>
        /// <param name="cancellationToken">The token to monitor for cancellation requests. The default value is <see cref="P:System.Threading.CancellationToken.None" />.</param>
        /// <returns>
        /// A task that represents the asynchronous flush operation.
        /// </returns>
        public override Task FlushAsync(CancellationToken cancellationToken)
        {
            return Completed;
        }

        #region Simple Wrappers
        /// <summary>
        /// Clears all buffers for this stream and causes any buffered data to be written to the underlying device.
        /// </summary>
        public override void Flush()
        {

        }

        /// <summary>
        /// Reads a sequence of bytes from the current stream and advances the position within the stream by the number of bytes read.
        /// </summary>
        /// <param name="buffer">An array of bytes. When this method returns, the buffer contains the specified byte array with the values between <paramref name="offset" /> and (<paramref name="offset" /> + <paramref name="count" /> - 1) replaced by the bytes read from the current source.</param>
        /// <param name="offset">The zero-based byte offset in <paramref name="buffer" /> at which to begin storing the data read from the current stream.</param>
        /// <param name="count">The maximum number of bytes to be read from the current stream.</param>
        /// <returns>
        /// The total number of bytes read into the buffer. This can be less than the number of bytes requested if that many bytes are not currently available, or zero (0) if the end of the stream has been reached.
        /// </returns>
        public override int Read(byte[] buffer, int offset, int count)
        {
            return ReadAsync(buffer, offset, count).Result;
        }

        /// <summary>
        /// Begins an asynchronous read operation. (Consider using <see cref="M:System.IO.Stream.ReadAsync(System.Byte[],System.Int32,System.Int32)" /> instead; see the Remarks section.)
        /// </summary>
        /// <param name="buffer">The buffer to read the data into.</param>
        /// <param name="offset">The byte offset in <paramref name="buffer" /> at which to begin writing data read from the stream.</param>
        /// <param name="count">The maximum number of bytes to read.</param>
        /// <param name="callback">An optional asynchronous callback, to be called when the read is complete.</param>
        /// <param name="state">A user-provided object that distinguishes this particular asynchronous read request from other requests.</param>
        /// <returns>
        /// An <see cref="T:System.IAsyncResult" /> that represents the asynchronous read, which could still be pending.
        /// </returns>
        public override IAsyncResult BeginRead(byte[] buffer, int offset, int count, AsyncCallback callback, object state)
        {
            return ReadAsync(buffer, offset, count).AsApm(callback, state);
        }

        /// <summary>
        /// Waits for the pending asynchronous read to complete. (Consider using <see cref="M:System.IO.Stream.ReadAsync(System.Byte[],System.Int32,System.Int32)" /> instead; see the Remarks section.)
        /// </summary>
        /// <param name="asyncResult">The reference to the pending asynchronous request to finish.</param>
        /// <returns>
        /// The number of bytes read from the stream, between zero (0) and the number of bytes you requested. Streams return zero (0) only at the end of the stream, otherwise, they should block until at least one byte is available.
        /// </returns>
        public override int EndRead(IAsyncResult asyncResult)
        {
            return ((Task<int>)asyncResult).Result;
        }

        /// <summary>
        /// Writes a sequence of bytes to the current stream and advances the current position within this stream by the number of bytes written.
        /// </summary>
        /// <param name="buffer">An array of bytes. This method copies <paramref name="count" /> bytes from <paramref name="buffer" /> to the current stream.</param>
        /// <param name="offset">The zero-based byte offset in <paramref name="buffer" /> at which to begin copying bytes to the current stream.</param>
        /// <param name="count">The number of bytes to be written to the current stream.</param>
        public override void Write(byte[] buffer, int offset, int count)
        {
            WriteAsync(buffer, offset, count).Wait();
        }

        /// <summary>
        /// Begins an asynchronous write operation. (Consider using <see cref="M:System.IO.Stream.WriteAsync(System.Byte[],System.Int32,System.Int32)" /> instead; see the Remarks section.)
        /// </summary>
        /// <param name="buffer">The buffer to write data from.</param>
        /// <param name="offset">The byte offset in <paramref name="buffer" /> from which to begin writing.</param>
        /// <param name="count">The maximum number of bytes to write.</param>
        /// <param name="callback">An optional asynchronous callback, to be called when the write is complete.</param>
        /// <param name="state">A user-provided object that distinguishes this particular asynchronous write request from other requests.</param>
        /// <returns>
        /// An IAsyncResult that represents the asynchronous write, which could still be pending.
        /// </returns>
        public override IAsyncResult BeginWrite(byte[] buffer, int offset, int count, AsyncCallback callback, object state)
        {
            return WriteAsync(buffer, offset, count).AsApm(callback, state);
        }

        /// <summary>
        /// Ends an asynchronous write operation. (Consider using <see cref="M:System.IO.Stream.WriteAsync(System.Byte[],System.Int32,System.Int32)" /> instead; see the Remarks section.)
        /// </summary>
        /// <param name="asyncResult">A reference to the outstanding asynchronous I/O request.</param>
        /// <exception cref="System.ArgumentOutOfRangeException">asyncResult</exception>
        public override void EndWrite(IAsyncResult asyncResult)
        {
            ((Task)asyncResult).Wait();
        }

        /// <summary>
        /// When overridden in a derived class, sets the position within the current stream.
        /// </summary>
        /// <param name="offset">A byte offset relative to the <paramref name="origin" /> parameter.</param>
        /// <param name="origin">A value of type <see cref="T:System.IO.SeekOrigin" /> indicating the reference point used to obtain the new position.</param>
        /// <returns>
        /// The new position within the current stream.
        /// </returns>
        /// <exception cref="System.NotSupportedException">Seeking is not supported on <see cref="SocketStream"/>.</exception>
        public override long Seek(long offset, SeekOrigin origin)
        {
            throw new NotSupportedException(Properties.Resources.InvalidOperation_SeekingNotSupported);
        }

        /// <summary>
        /// When overridden in a derived class, sets the length of the current stream.
        /// </summary>
        /// <param name="value">The desired length of the current stream in bytes.</param>
        /// <exception cref="System.NotSupportedException">Seeking is not supported on <see cref="SocketStream"/>.</exception>
        public override void SetLength(long value)
        {
            throw new NotSupportedException(Properties.Resources.InvalidOperation_SeekingNotSupported);
        }

        /// <summary>
        /// Releases the unmanaged resources used by the <see cref="T:System.IO.Stream" /> and optionally releases the managed resources.
        /// </summary>
        /// <param name="disposing">true to release both managed and unmanaged resources; false to release only unmanaged resources.</param>
        [System.Diagnostics.CodeAnalysis.SuppressMessage("Microsoft.Design", "CA1031:DoNotCatchGeneralExceptionTypes", Justification = "Ingoring termination from the remote party.")]
        protected override void Dispose(bool disposing)
        {
            if (disposing)
            {
                if (Interlocked.Exchange(ref _state, 1) == 0)
                {
                    try
                    {
                        _socket.Shutdown(SocketShutdown.Both);
                    }
                    catch { }
                }
                if (Interlocked.Exchange(ref _state, 2) == 1)
                {
                    try
                    {
                        _socket.Close(100);
                    }
                    catch { }
                }
                if (Interlocked.Exchange(ref _state, 3) == 2)
                {
                    try
                    {
                        _socket.Dispose();
                    }
                    catch { }
                }
            }
            base.Dispose(disposing);
        }
        #endregion
    }
}
