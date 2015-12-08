using System;
using System.Collections;
using System.Collections.Generic;
using System.Linq;

namespace AzXmpp.Transport.Sockets
{
    /// <summary>
    /// Represents a list of buffers.
    /// </summary>
    internal sealed class BufferList : IList<ArraySegment<byte>>
    {
        public const int BufferLength = 4096;
        private const int BufferLimit = 1024 * 1024 * 64;
        private static readonly FastBufferManager _bufferManager = new FastBufferManager(BufferLimit, BufferLength, true);


        private byte[][] _buffers;
        private int _capacity;
        private int _count;
        private int _finalBufferSize;

        /// <summary>
        /// Gets or sets the <see cref="ArraySegment{Byte}"/> at the specified index.
        /// </summary>
        /// <value>
        /// The <see cref="ArraySegment{Byte}"/>.
        /// </value>
        /// <param name="index">The index.</param>
        /// <returns></returns>
        /// <exception cref="System.InvalidOperationException">An attempt was made to set a value.</exception>
        public ArraySegment<byte> this[int index]
        {
            get
            {
                if (index >= _count) throw new ArgumentOutOfRangeException("index");

                var buffer = _buffers[index];
                if (index == _count - 1)
                    return new ArraySegment<byte>(buffer, 0, _finalBufferSize);
                return new ArraySegment<byte>(buffer, 0, BufferLength);
            }
            set { throw new InvalidOperationException(); }
        }

        /// <summary>
        /// Gets the number of elements contained in the <see cref="T:System.Collections.Generic.ICollection`1" />.
        /// </summary>
        public int Count
        {
            get { return _count; }
        }

        /// <summary>
        /// Gets a value indicating whether the <see cref="T:System.Collections.Generic.ICollection`1" /> is read-only.
        /// </summary>
        /// <remarks>
        /// Always returns <c>true</c>.
        /// </remarks>
        public bool IsReadOnly
        {
            get { return true; }
        }

        /// <summary>
        /// Initializes a new instance of the <see cref="BufferList"/> class.
        /// </summary>
        public BufferList()
        {
            _buffers = new byte[4][];
        }

        /// <summary>
        /// Sets the length of the buffer list.
        /// </summary>
        /// <param name="length">The length of the list in bytes.</param>
        /// <returns>A value indicating whether any changes were made to the list.</returns>
        public bool SetLength(int length)
        {
            var finalBufferSize = length % BufferLength;
            if (finalBufferSize == 0) finalBufferSize = BufferLength;

            var changed = _finalBufferSize != finalBufferSize;

            _finalBufferSize = finalBufferSize;

            var buffers = _count = (length + BufferLength - 1) / BufferLength;

            if (_buffers.Length < buffers)
            {
                changed = true;
                var size = buffers < 4 ? 4 : ((buffers * 3) / 2);
                var newValue = new byte[size][];
                Array.Copy(_buffers, 0, newValue, 0, _capacity);
                _buffers = newValue;
            }

            if (_capacity < _count)
            {
                _bufferManager.TakeBuffers(BufferLength, _buffers, _capacity, _count - _capacity);
                _capacity = _count;
            }

            return changed;
        }

        /// <summary>
        /// Copies all the bytes in this buffer list to specified buffer.
        /// </summary>
        /// <param name="buffer">The buffer.</param>
        /// <param name="length">The number of bytes to copy.</param>
        public void CopyTo(ArraySegment<byte> buffer, int length)
        {
            if (length == 0) return;

            var arr = buffer.Array;
            var ofs = buffer.Offset;
            var buffers = (length + BufferLength - 1) / BufferLength;
            var end = buffers - 1;

            byte[] src;
            for (var i = 0; i < end; i++)
            {
                src = _buffers[i];
                Buffer.BlockCopy(src, 0, arr, ofs + i * BufferLength, BufferLength);
            }

            src = _buffers[end];
            Buffer.BlockCopy(src, 0, arr, ofs + end * BufferLength, _finalBufferSize);
        }

        /// <summary>
        /// Copies all the bytes from the specified buffer into this buffer list.
        /// </summary>
        /// <param name="buffer">The buffer.</param>
        /// <returns>A value indicating whether any changes were made to the list.</returns>
        public bool CopyFrom(ArraySegment<byte> buffer)
        {
            var changed = SetLength(buffer.Count);
            if (buffer.Count == 0) return changed;

            var arr = buffer.Array;
            var ofs = buffer.Offset;
            var length = buffer.Count;
            var buffers = (length + BufferLength - 1) / BufferLength;
            var end = buffers - 1;

            byte[] dst;
            for (var i = 0; i < end; i++)
            {
                dst = _buffers[i];
                Buffer.BlockCopy(arr, ofs + i * BufferLength, dst, 0, BufferLength);
            }

            dst = _buffers[end];
            Buffer.BlockCopy(arr, ofs + end * BufferLength, dst, 0, _finalBufferSize);

            return changed;
        }

        /// <summary>
        /// Removes all items from the <see cref="T:System.Collections.Generic.ICollection`1" />.
        /// </summary>
        public void Clear()
        {
            Limit(0);
        }

        /// <summary>
        /// Limits the internal buffers to the specified amount.
        /// </summary>
        /// <param name="limit">The amount of buffers to limit to.</param>
        public void Limit(int limit)
        {
            _finalBufferSize = 0;
            _count = 0;
            while (_capacity > limit)
            {
                _capacity--;
                _bufferManager.ReturnBuffer(_buffers[_capacity]);
                _buffers[_capacity] = null;
            }
        }

        /// <summary>
        /// Copies the elements of the <see cref="IList{T}" /> to an <see cref="Array" />
        /// starting at a particular <see cref="Array" /> index.
        /// </summary>
        /// <param name="array">The one-dimensional <see cref="Array" /> that is the destination of the elements copied
        /// from <see cref="IList{T}" />. The <see cref="Array" /> must have zero-based indexing.</param>
        /// <param name="arrayIndex">The zero-based index in <paramref name="array" /> at which copying begins.</param>
        public void CopyTo(ArraySegment<byte>[] array, int arrayIndex)
        {
            if (array == null) throw new ArgumentNullException("array");
            if (_count == 0) return;

            var end = _count - 1;
            for (var i = 0; i < end; i++)
            {
                array[arrayIndex + i] = new ArraySegment<byte>(_buffers[i], 0, BufferLength);
            }

            var buffer = _buffers[end];
            array[arrayIndex + end] = new ArraySegment<byte>(buffer, 0, _finalBufferSize);
        }

        /// <summary>
        /// This method is not supported.
        /// </summary>
        /// <param name="item">The item.</param>
        /// <exception cref="System.InvalidOperationException">The method was called.</exception>
        public void Add(ArraySegment<byte> item)
        {
            throw new InvalidOperationException();
        }

        /// <summary>
        /// This method is not supported.
        /// </summary>
        /// <param name="item">The item.</param>
        /// <returns></returns>
        /// <exception cref="System.InvalidOperationException">The method was called.</exception>
        public bool Contains(ArraySegment<byte> item)
        {
            throw new InvalidOperationException();
        }

        /// <summary>
        /// This method is not supported.
        /// </summary>
        /// <param name="item">The item.</param>
        /// <returns></returns>
        /// <exception cref="System.InvalidOperationException">The method was called.</exception>
        public int IndexOf(ArraySegment<byte> item)
        {
            throw new InvalidOperationException();
        }

        /// <summary>
        /// This method is not supported.
        /// </summary>
        /// <param name="index">The index.</param>
        /// <param name="item">The item.</param>
        /// <exception cref="System.InvalidOperationException">The method was called.</exception>
        public void Insert(int index, ArraySegment<byte> item)
        {
            throw new InvalidOperationException();
        }

        /// <summary>
        /// This method is not supported.
        /// </summary>
        /// <param name="item">The item.</param>
        /// <returns></returns>
        /// <exception cref="System.InvalidOperationException">The method was called.</exception>
        public bool Remove(ArraySegment<byte> item)
        {
            throw new InvalidOperationException();
        }

        /// <summary>
        /// This method is not supported.
        /// </summary>
        /// <param name="index">The index.</param>
        /// <exception cref="System.InvalidOperationException">The method was called.</exception>
        public void RemoveAt(int index)
        {
            throw new InvalidOperationException();
        }

        /// <summary>
        /// Returns an enumerator that iterates through the collection.
        /// </summary>
        /// <returns>
        /// An enumerator that can be used to iterate through the collection.
        /// </returns>
        public IEnumerator<ArraySegment<byte>> GetEnumerator()
        {
            return Enumerable.Range(0, _count).Select(i => this[i]).GetEnumerator();
        }

        /// <summary>
        /// Returns an enumerator that iterates through a collection.
        /// </summary>
        /// <returns>
        /// An <see cref="T:System.Collections.IEnumerator" /> object that can be used to iterate through the collection.
        /// </returns>
        IEnumerator IEnumerable.GetEnumerator()
        {
            return GetEnumerator();
        }
    }
}
