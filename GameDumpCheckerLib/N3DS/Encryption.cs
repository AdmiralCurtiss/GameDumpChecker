using System;
using System.Collections.Generic;
using System.Linq;
using System.Security.Cryptography;
using System.Text;
using System.Threading.Tasks;

namespace GameDumpCheckerLib.N3DS {
	public class Encryption {
		public static byte[] Decrypt( byte[] data, byte[] key, byte[] initialCounter ) {
			byte[] counter = new byte[initialCounter.Length];
			initialCounter.CopyTo( counter, 0 );
			byte[] output = new byte[data.Length];
			byte[] tmp = new byte[16];
			using ( Aes aes = new AesManaged() { KeySize = 128, BlockSize = 128, Mode = CipherMode.ECB, Padding = PaddingMode.None } ) {
				using ( var enc = aes.CreateEncryptor( key, null ) ) {
					int remainingBytes = data.Length;
					int position = 0;
					while ( remainingBytes >= 16 ) {
						enc.TransformBlock( counter, 0, 16, tmp, 0 );

						for ( int i = 0; i < 16; ++i ) {
							output[position] = (byte)( tmp[i] ^ data[position] );
							++position;
							--remainingBytes;
						}

						AddToCounter( counter, 1 );
					}
					if ( remainingBytes > 0 ) {
						enc.TransformBlock( new byte[16], 0, 16, tmp, 0 );
						for ( int i = 0; i < remainingBytes; ++i ) {
							output[position] = (byte)( tmp[i] ^ data[position] );
							++position;
						}
					}
				}
			}

			return output;
		}

		public static void AddToCounter( byte[] counter, uint block_num ) {
			uint[] ctr = new uint[4];
			ctr[3] = BitConverter.ToUInt32( counter, 0 );
			ctr[2] = BitConverter.ToUInt32( counter, 4 );
			ctr[1] = BitConverter.ToUInt32( counter, 8 );
			ctr[0] = BitConverter.ToUInt32( counter, 12 );
			for ( uint i = 0; i < 4; ++i ) {
				ctr[i] = EndianUtil.SwapEndian( ctr[i] );
			}

			for ( uint i = 0; i < 4; i++ ) {
				ulong total = ctr[i] + block_num;
				// if there wasn't a wrap around, add the two together and exit
				if ( total <= 0xffffffff ) {
					ctr[i] += block_num;
					break;
				}

				// add the difference
				ctr[i] = (uint)( total - 0x100000000 );
				// carry to next word
				block_num = (uint)( total >> 32 );
			}

			for ( uint i = 0; i < 4; ++i ) {
				ctr[i] = EndianUtil.SwapEndian( ctr[i] );
			}
			Array.Copy( BitConverter.GetBytes( ctr[3] ), 0, counter, 0, 4 );
			Array.Copy( BitConverter.GetBytes( ctr[2] ), 0, counter, 4, 4 );
			Array.Copy( BitConverter.GetBytes( ctr[1] ), 0, counter, 8, 4 );
			Array.Copy( BitConverter.GetBytes( ctr[0] ), 0, counter, 12, 4 );
		}
	}
}
