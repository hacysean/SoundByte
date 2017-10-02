﻿using System;
using System.IO;

namespace SoundByte.YouTubeParser.Models.MediaStreams
{
    /// <summary>
    /// Media stream
    /// </summary>
    public class MediaStream : Stream
    {
        private readonly Stream _innerStream;

        /// <summary>
        /// Metadata associated with this media stream
        /// </summary>
        public MediaStreamInfo Info { get; }

        /// <inheritdoc />
        public override bool CanRead => _innerStream.CanRead;

        /// <inheritdoc />
        public override bool CanSeek => _innerStream.CanSeek;

        /// <inheritdoc />
        public override bool CanWrite => false;

        /// <inheritdoc />
        public override long Length => Info.ContentLength;

        /// <inheritdoc />
        public override long Position
        {
            get => _innerStream.Position;
            set => _innerStream.Position = value;
        }

        /// <inheritdoc />
        public MediaStream(MediaStreamInfo mediaStreamInfo, Stream innerStream)
        {
            Info = mediaStreamInfo ?? throw new ArgumentNullException(nameof(mediaStreamInfo));
            _innerStream = innerStream ?? throw new ArgumentNullException(nameof(innerStream));
        }

        /// <inheritdoc />
        ~MediaStream()
        {
            Dispose(false);
        }

        /// <inheritdoc />
        public override void Flush() => _innerStream.Flush();

        /// <inheritdoc />
        public override int Read(byte[] buffer, int offset, int count) => _innerStream.Read(buffer, offset, count);

        /// <inheritdoc />
        public override long Seek(long offset, SeekOrigin origin) => _innerStream.Seek(offset, origin);

        /// <inheritdoc />
        public override void SetLength(long value) => _innerStream.SetLength(value);

        /// <inheritdoc />
        public override void Write(byte[] buffer, int offset, int count) => _innerStream.Write(buffer, offset, count);

        /// <inheritdoc />
        protected override void Dispose(bool disposing)
        {
            base.Dispose(disposing);
            if (disposing)
                _innerStream.Dispose();
        }
    }
}