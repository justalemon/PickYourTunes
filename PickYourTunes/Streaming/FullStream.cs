using System;
using System.IO;

namespace PickYourTunes.Streaming
{
    /// <summary>
    /// Class that handles a complete audio stream.
    /// </summary>
    public class FullStream : Stream
    {
        /// <summary>
        /// The original stream.
        /// </summary>
        private readonly Stream SourceStream;
        /// <summary>
        /// TODO: Check what this does.
        /// </summary>
        private long _Position;
        /// <summary>
        /// TODO: Check what this does.
        /// </summary>
        private readonly byte[] ReadAheadBuffer;
        /// <summary>
        /// TODO: Check what this does.
        /// </summary>
        private int ReadAheadLength;
        /// <summary>
        /// TODO: Check what this does.
        /// </summary>
        private int ReadAheadOffset;

        /// <summary>
        /// If this stream can be readed.
        /// </summary>
        public override bool CanRead
        {
            get { return true; }
        }
        /// <summary>
        /// If this stream can seek.
        /// </summary>
        public override bool CanSeek
        {
            get { return false; }
        }
        /// <summary>
        /// If this stream can write.
        /// </summary>
        public override bool CanWrite
        {
            get { return false; }
        }
        /// <summary>
        /// The length of the stream.
        /// </summary>
        public override long Length
        {
            get { return _Position; }
        }
        /// <summary>
        /// The current position of the stream.
        /// </summary>
        public override long Position
        {
            get
            {
                return _Position;
            }
            set
            {
                throw new InvalidOperationException();
            }
        }

        public FullStream(Stream Source)
        {
            // Store the original stream
            SourceStream = Source;
            // Store this variable (Remember to check what it does later)
            ReadAheadBuffer = new byte[4096];
        }

        public override int Read(byte[] Buffer, int Offset, int Count)
        {
            // TODO: Add comments about what is going on over here
            int BytesRead = 0;
            while (BytesRead < Count)
            {
                int ReadAheadAvailableBytes = ReadAheadLength - ReadAheadOffset;
                int BytesRequired = Count - BytesRead;
                if (ReadAheadAvailableBytes > 0)
                {
                    int ToCopy = Math.Min(ReadAheadAvailableBytes, BytesRequired);
                    Array.Copy(ReadAheadBuffer, ReadAheadOffset, Buffer, Offset + BytesRead, ToCopy);
                    BytesRead += ToCopy;
                    ReadAheadOffset += ToCopy;
                }
                else
                {
                    ReadAheadOffset = 0;
                    // This line sometimes raises
                    // System.IO.IOException: Unable to read data from the transport connection: A blocking operation was interrupted by a call to WSACancelBlockingCall.
                    ReadAheadLength = SourceStream.Read(ReadAheadBuffer, 0, ReadAheadBuffer.Length);
                    if (ReadAheadLength == 0)
                    {
                        break;
                    }
                }
            }
            _Position += BytesRead;
            return BytesRead;
        }

        public override long Seek(long Offset, SeekOrigin Origin)
        {
            throw new InvalidOperationException();
        }

        public override void SetLength(long Value)
        {
            throw new InvalidOperationException();
        }

        public override void Write(byte[] Buffer, int Offset, int Count)
        {
            throw new InvalidOperationException();
        }

        public override void Flush()
        {
            throw new InvalidOperationException();
        }
    }
}
