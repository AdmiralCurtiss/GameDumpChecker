using System;
using System.Collections.Generic;
using System.IO;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace GameDumpCheckerLib {
    public class PartialStream : Stream {
        private Stream _FullStream;
        private long _StartPosition;
        private long _Length;
        private long _LocalPosition;

        public PartialStream( Stream stream, long startPosition, long length ) {
            _FullStream = stream;
            _StartPosition = startPosition;
            _Length = length;
            _LocalPosition = 0;
        }

        public override bool CanRead => _FullStream.CanRead;
        public override bool CanSeek => _FullStream.CanSeek;
        public override bool CanWrite => _FullStream.CanWrite;
        public override long Length => _Length;

        public override long Position {
            get {
                return _LocalPosition;
            }
            set {
                _FullStream.Position = value + _StartPosition;
                _LocalPosition = value;
            }
        }

        public override void Flush() {
            throw new NotImplementedException();
        }

        public override int Read( byte[] buffer, int offset, int count ) {
            long left = Length - Position;
            int bytesToRead;
            if ( left < count ) {
                bytesToRead = (int)left;
            } else {
                bytesToRead = count;
            }
            int bytesRead = _FullStream.Read( buffer, offset, bytesToRead );
            Position = Position + bytesRead;
            return bytesRead;
        }

        public override long Seek( long offset, SeekOrigin origin ) {
            switch ( origin ) {
                case SeekOrigin.Begin:
                    Position = offset;
                    break;
                case SeekOrigin.Current:
                    Position = Position + offset;
                    break;
                case SeekOrigin.End:
                    Position = Length - offset;
                    break;
            }
            return Position;
        }

        public override void SetLength( long value ) {
            throw new NotImplementedException();
        }

        public override void Write( byte[] buffer, int offset, int count ) {
            throw new NotImplementedException();
        }
    }
}
