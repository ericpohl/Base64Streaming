using System;
using System.IO;

namespace Base64Streaming
{
    public class Base64DecodingStream : Stream
    {
        private long _position;
        private readonly TextReader _base64TextReader;

        public Base64DecodingStream(TextReader base64TextReader)
        {
            if (base64TextReader == null)
            {
                throw new ArgumentNullException(nameof(base64TextReader));
            }
            _base64TextReader = base64TextReader;
        }

        private const string Codes = "ABCDEFGHIJKLMNOPQRSTUVWXYZabcdefghijklmnopqrstuvwxyz0123456789+/=";

        /// <summary>When overridden in a derived class, reads a sequence of bytes from the current stream and advances the position within the stream by the number of bytes read.</summary>
        /// <returns>The total number of bytes read into the buffer. This can be less than the number of bytes requested if that many bytes are not currently available, or zero (0) if the end of the stream has been reached.</returns>
        /// <param name="buffer">An array of bytes. When this method returns, the buffer contains the specified byte array with the values between <paramref name="offset" /> and (<paramref name="offset" /> + <paramref name="count" /> - 1) replaced by the bytes read from the current source. </param>
        /// <param name="offset">The zero-based byte offset in <paramref name="buffer" /> at which to begin storing the data read from the current stream. </param>
        /// <param name="count">The maximum number of bytes to be read from the current stream. </param>
        /// <exception cref="T:System.ArgumentException">The sum of <paramref name="offset" /> and <paramref name="count" /> is larger than the buffer length. </exception>
        /// <exception cref="T:System.ArgumentNullException">
        /// <paramref name="buffer" /> is null. </exception>
        /// <exception cref="T:System.ArgumentOutOfRangeException">
        /// <paramref name="offset" /> or <paramref name="count" /> is negative. </exception>
        /// <exception cref="T:System.IO.IOException">An I/O error occurs. </exception>
        /// <exception cref="T:System.NotSupportedException">The stream does not support reading. </exception>
        /// <exception cref="T:System.ObjectDisposedException">Methods were called after the stream was closed. </exception>
        /// <filterpriority>1</filterpriority>
        public override int Read(byte[] buffer, int offset, int count)
        {
            if (buffer == null)
            {
                throw new ArgumentNullException(nameof(buffer));
            }
            if (offset < 0)
            {
                throw new ArgumentOutOfRangeException(nameof(offset), "offset is negative");
            }
            if (count < 0)
            {
                throw new ArgumentOutOfRangeException(nameof(count), "count is negative");
            }

            char[] textBuffer = new char[4];
            int[] b = new int[4];
            int pos = offset;

            // We can only read a multiple of 3 bytes
            int innerCount = (count/3)*3;
            while (pos < offset + innerCount)
            {
                int charsRead = _base64TextReader.Read(textBuffer, 0, 4);
                if (charsRead == 0)
                {
                    // End of the input reader
                    break;
                }

                if (charsRead == 1)
                {
                    throw new FormatException("Invalid length for Base-64 character stream");
                }

                if (charsRead < 4)
                {
                    // If we're at the end but missing characters, so assume these should be pad characters
                    for (int k = charsRead; k < 4; k++)
                    {
                        textBuffer[k] = '=';
                    }
                }

                for (int k = 0; k < 4; k++)
                {
                    // TODO: optimize this by doing ASCII math instead of IndexOf().
                    // See ValueFromChar() vs. ValueFromChar2() below
                    b[k] = Codes.IndexOf(textBuffer[k]);
                }

                buffer[pos++] = (byte)((b[0] << 2) | (b[1] >> 4));
                if (b[2] < 64)
                {
                    buffer[pos++] = (byte)((b[1] << 4) | (b[2] >> 2));
                    if (b[3] < 64)
                    {
                        buffer[pos++] = (byte)((b[2] << 6) | b[3]);
                    }
                }
            }

            var bytesReturnedCount = pos - offset;
            _position += bytesReturnedCount;

            return bytesReturnedCount;
        }

        public static int ValueFromChar(char c)
        {
            return Codes.IndexOf(c);
        }

        public static uint ValueFromChar2(uint currCode)
        {
            unchecked
            {
                const uint intA = (uint)'A';
                const uint inta = (uint)'a';
                const uint int0 = (uint)'0';
                const uint intEq = (uint)'=';
                const uint intPlus = (uint)'+';
                const uint intSlash = (uint)'/';
                const uint intSpace = (uint)' ';
                const uint intTab = (uint)'\t';
                const uint intNLn = (uint)'\n';
                const uint intCRt = (uint)'\r';
                const uint intAtoZ = (uint)('Z' - 'A');  // = ('z' - 'a')
                const uint int0to9 = (uint)('9' - '0');

                if (currCode - intA <= intAtoZ)
                    currCode -= intA;

                else if (currCode - inta <= intAtoZ)
                    currCode -= (inta - 26u);

                else if (currCode - int0 <= int0to9)
                    currCode -= (int0 - 52u);

                else
                {
                    // Use the slower switch for less common cases:
                    switch (currCode)
                    {
                        // Significant chars:
                        case intPlus:
                            currCode = 62u;
                            break;

                        case intSlash:
                            currCode = 63u;
                            break;

                        case intEq:
                            currCode = 64u;
                            break;

                        // Legal no-value chars (we ignore these):
                        case intCRt:
                        case intNLn:
                        case intSpace:
                        case intTab:
                            currCode = 65u;
                            break;

                        // The equality char is only legal at the end of the input.
                        // Jump after the loop to make it easier for the JIT register predictor to do a good job for the loop itself:

                        // Other chars are illegal:
                        default:
                            throw new FormatException("Format_BadBase64Char");
                    }
                }

                return currCode;
            }
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

        /// <summary>When overridden in a derived class, sets the length of the current stream.</summary>
        /// <param name="value">The desired length of the current stream in bytes. </param>
        /// <exception cref="T:System.IO.IOException">An I/O error occurs. </exception>
        /// <exception cref="T:System.NotSupportedException">The stream does not support both writing and seeking, such as if the stream is constructed from a pipe or console output. </exception>
        /// <exception cref="T:System.ObjectDisposedException">Methods were called after the stream was closed. </exception>
        /// <filterpriority>2</filterpriority>
        public override void SetLength(long value)
        {
            throw new NotSupportedException();
        }

        public override void Write(byte[] buffer, int offset, int count)
        {
            throw new NotSupportedException();
        }

        public override bool CanRead => true;

        public override bool CanSeek => false;

        public override bool CanWrite => false;

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
            set { throw new NotSupportedException("The stream does not support seeking"); }
        }
    }
}
