using System;
using System.IO;
using System.Text;

namespace Base64Streaming
{
    /// <summary>
    /// A stream that is derived from an underlying string or text reader, plus an Encoding. 
    /// </summary>
    public class TextEncodingStream : Stream
    {
        private readonly string _base64;
        private int _position;
        private int _charsRemaining;
        private readonly Encoding _encoding = Encoding.ASCII;

        public TextEncodingStream(string base64, Encoding encoding = null)
        {
            if (base64 == null)
            {
                throw new ArgumentNullException("base64");
            }            

            _base64 = base64;
            _encoding = encoding ?? Encoding.Default;
            _charsRemaining = _base64.Length;
        }

        public override int Read(byte[] buffer, int offset, int count)
        {
            if (buffer == null)
            {
                throw new ArgumentNullException("buffer");
            }
            if (offset < 0)
            {
                throw new ArgumentOutOfRangeException("offset", "offset is negative");
            }
            if (count < 0)
            {
                throw new ArgumentOutOfRangeException("count", "count is negative");
            }
            int numBytesToUse = (count > _charsRemaining) ? _charsRemaining : count;

            int numBytesEncoded = _encoding.GetBytes(_base64, _position, numBytesToUse, buffer, offset);
            _position += numBytesEncoded;
            _charsRemaining -= numBytesEncoded;

            return numBytesEncoded;
        }

        public override void Flush()
        {
            // no-op, since we're read-only. This will get called.
        }

        /// <summary>
        /// When overridden in a derived class, sets the position within the current stream.
        /// </summary>
        /// <returns>
        /// The new position within the current stream.
        /// </returns>
        /// <param name="offset">A byte offset relative to the <paramref name="origin"/> parameter. </param>
        /// <param name="origin">A value of type <see cref="T:System.IO.SeekOrigin"/> indicating the reference point used to obtain the new position. </param>
        /// <exception cref="T:System.IO.IOException">An I/O error occurs. </exception>
        /// <exception cref="T:System.NotSupportedException">The stream does not support seeking, such as if the stream is constructed from a pipe or console output. </exception>
        /// <exception cref="T:System.ObjectDisposedException">Methods were called after the stream was closed. </exception>
        /// <filterpriority>1</filterpriority>
        public override long Seek(long offset, SeekOrigin origin)
        {
            throw new NotSupportedException("The stream does not support seeking.");
        }

        public override void SetLength(long value)
        {
            throw new NotSupportedException();
        }

        public override void Write(byte[] buffer, int offset, int count)
        {
            throw new NotSupportedException();
        }

        public override bool CanRead
        {
            get { return true; }
        }

        public override bool CanSeek
        {
            get { return false; }
        }

        public override bool CanWrite
        {
            get { return false; }
        }

        /// <summary>
        /// When overridden in a derived class, gets the length in bytes of the stream.
        /// </summary>
        /// <returns>
        /// A long value representing the length of the stream in bytes.
        /// </returns>
        /// <exception cref="T:System.NotSupportedException">A class derived from Stream does not support seeking. </exception>
        /// <exception cref="T:System.ObjectDisposedException">Methods were called after the stream was closed. </exception>
        /// <filterpriority>1</filterpriority>
        public override long Length
        {
            get { throw new NotSupportedException("The stream does not support seeking"); }
        }

        /// <summary>
        /// When overridden in a derived class, gets or sets the position within the current stream.
        /// </summary>
        /// <returns>
        /// The current position within the stream.
        /// </returns>
        /// <exception cref="T:System.IO.IOException">An I/O error occurs. </exception>
        /// <exception cref="T:System.NotSupportedException">The stream does not support seeking. </exception>
        /// <exception cref="T:System.ObjectDisposedException">Methods were called after the stream was closed. </exception>
        /// <filterpriority>1</filterpriority>
        public override long Position
        {
            get { return _position; }
            set { throw new NotSupportedException("The stream does not support seking"); }
        }
    }
}
