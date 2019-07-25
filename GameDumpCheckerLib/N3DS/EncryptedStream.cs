using System;
using System.IO;
using System.Security.Cryptography;
using HyoutaPluginBase;

namespace GameDumpCheckerLib.N3DS {
	public class EncryptedStream : DuplicatableStream {
		private DuplicatableStream BaseStream;
		private byte[] Key;
		private byte[] InitialCounter;
		private Aes AesEngine;
		private ICryptoTransform AesEncryptor;

		private byte[] Counter;
		private byte[] XorPad;
		private ulong XorPadBlockNumber;

		public EncryptedStream( DuplicatableStream baseStream, byte[] key, byte[] initialCounter ) {
			BaseStream = baseStream.Duplicate();
			Key = new byte[16];
			InitialCounter = new byte[16];
			key.CopyTo( Key, 0 );
			initialCounter.CopyTo( InitialCounter, 0 );
			AesEngine = new AesManaged() { KeySize = 128, BlockSize = 128, Mode = CipherMode.ECB, Padding = PaddingMode.None };
			AesEncryptor = AesEngine.CreateEncryptor( Key, null );
			Counter = new byte[16];
			XorPad = new byte[16];
			XorPadBlockNumber = ulong.MaxValue;
		}

		public override bool CanRead => true;
		public override bool CanSeek => true;
		public override bool CanWrite => false;
		public override long Length => BaseStream.Length;
		public override long Position { get => BaseStream.Position; set => BaseStream.Position = value; }

		public override DuplicatableStream Duplicate() {
			return new EncryptedStream( BaseStream, Key, InitialCounter );
		}

		public override void End() {
			BaseStream.End();
		}

		public override void Flush() {
		}

		public override int Read( byte[] buffer, int offset, int count ) {
			// first read encrypted data into buffer
			ulong initialPosition = (ulong)Position;
			int bytesRead = BaseStream.Read( buffer, offset, count );

			if ( bytesRead <= 0 ) {
				return bytesRead;
			}

			// then decrypt data in buffer
			// TODO: This can be optimized a ton...
			ulong blockNum = ( initialPosition / 16 );
			ulong bytesIntoCurrentBlock = ( initialPosition % 16 );
			if ( blockNum != XorPadBlockNumber ) {
				RecalculateXorPad( blockNum );
			}

			int bufferPosition = 0;
			if ( bytesIntoCurrentBlock != 0 ) {
				for ( ulong i = bytesIntoCurrentBlock; i < 16; ++i ) {
					if ( bufferPosition < bytesRead ) {
						buffer[bufferPosition] ^= XorPad[i];
						++bufferPosition;
					} else {
						return bytesRead;
					}
				}
				++blockNum;
				RecalculateXorPad( blockNum );
			}

			while ( true ) {
				for ( ulong i = 0; i < 16; ++i ) {
					if ( bufferPosition < bytesRead ) {
						buffer[bufferPosition] ^= XorPad[i];
						++bufferPosition;
					} else {
						return bytesRead;
					}
				}
				++blockNum;
				RecalculateXorPad( blockNum );
			}
		}

		private void RecalculateXorPad( ulong blockNum ) {
			if ( blockNum < (ulong)( Length / 16 ) ) {
				InitialCounter.CopyTo( Counter, 0 );
				Encryption.AddToCounterInPlace( Counter, (uint)blockNum );
			} else {
				for ( int i = 0; i < 16; ++i ) {
					Counter[i] = 0;
				}
			}
			AesEncryptor.TransformBlock( Counter, 0, 16, XorPad, 0 );
			XorPadBlockNumber = blockNum;
		}

		public override void ReStart() {
			BaseStream.ReStart();
		}

		public override long Seek( long offset, SeekOrigin origin ) {
			return BaseStream.Seek( offset, origin );
		}

		public override void SetLength( long value ) {
			throw new NotSupportedException();
		}

		public override void Write( byte[] buffer, int offset, int count ) {
			throw new NotSupportedException();
		}
	}
}
