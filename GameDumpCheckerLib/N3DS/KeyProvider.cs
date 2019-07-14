using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace GameDumpCheckerLib.N3DS {
	public class KeyProvider {
		// see https://github.com/citra-emu/citra/wiki/AES-Keys
		private byte[] _slot0x2CKeyX;
		private byte[] _slot0x25KeyX;
		private byte[] _slot0x18KeyX;
		private byte[] _slot0x1BKeyX;
		private byte[] _common0;
		private byte[] _common1;
		private byte[] _common2;
		private byte[] _common3;
		private byte[] _common4;
		private byte[] _common5;

		public byte[] Slot0x2CKeyX { get { if ( _slot0x2CKeyX == null ) { InitKeys(); if ( _slot0x2CKeyX == null ) { throw new Exception( "Unknown key." ); } } return _slot0x2CKeyX; } }
		public byte[] Slot0x25KeyX { get { if ( _slot0x25KeyX == null ) { InitKeys(); if ( _slot0x25KeyX == null ) { throw new Exception( "Unknown key." ); } } return _slot0x25KeyX; } }
		public byte[] Slot0x18KeyX { get { if ( _slot0x18KeyX == null ) { InitKeys(); if ( _slot0x18KeyX == null ) { throw new Exception( "Unknown key." ); } } return _slot0x18KeyX; } }
		public byte[] Slot0x1BKeyX { get { if ( _slot0x1BKeyX == null ) { InitKeys(); if ( _slot0x1BKeyX == null ) { throw new Exception( "Unknown key." ); } } return _slot0x1BKeyX; } }
		public byte[] Common0 { get { if ( _common0 == null ) { InitKeys(); if ( _common0 == null ) { throw new Exception( "Unknown key." ); } } return _common0; } }
		public byte[] Common1 { get { if ( _common1 == null ) { InitKeys(); if ( _common1 == null ) { throw new Exception( "Unknown key." ); } } return _common1; } }
		public byte[] Common2 { get { if ( _common2 == null ) { InitKeys(); if ( _common2 == null ) { throw new Exception( "Unknown key." ); } } return _common2; } }
		public byte[] Common3 { get { if ( _common3 == null ) { InitKeys(); if ( _common3 == null ) { throw new Exception( "Unknown key." ); } } return _common3; } }
		public byte[] Common4 { get { if ( _common4 == null ) { InitKeys(); if ( _common4 == null ) { throw new Exception( "Unknown key." ); } } return _common4; } }
		public byte[] Common5 { get { if ( _common5 == null ) { InitKeys(); if ( _common5 == null ) { throw new Exception( "Unknown key." ); } } return _common5; } }

		public void InitKeys( string path = "aes_keys.txt" ) {
			try {
				foreach ( string line in System.IO.File.ReadAllLines( path ) ) {
					string[] split = line.Split( '=' );
					if ( split.Length == 2 ) {
						switch ( split[0].Trim() ) {
							case "slot0x2CKeyX": _slot0x2CKeyX = Util.HumanReadableHexStringToByteArray( split[1].Trim() ); break;
							case "slot0x25KeyX": _slot0x25KeyX = Util.HumanReadableHexStringToByteArray( split[1].Trim() ); break;
							case "slot0x18KeyX": _slot0x18KeyX = Util.HumanReadableHexStringToByteArray( split[1].Trim() ); break;
							case "slot0x1BKeyX": _slot0x1BKeyX = Util.HumanReadableHexStringToByteArray( split[1].Trim() ); break;
							case "common0": _common0 = Util.HumanReadableHexStringToByteArray( split[1].Trim() ); break;
							case "common1": _common1 = Util.HumanReadableHexStringToByteArray( split[1].Trim() ); break;
							case "common2": _common2 = Util.HumanReadableHexStringToByteArray( split[1].Trim() ); break;
							case "common3": _common3 = Util.HumanReadableHexStringToByteArray( split[1].Trim() ); break;
							case "common4": _common4 = Util.HumanReadableHexStringToByteArray( split[1].Trim() ); break;
							case "common5": _common5 = Util.HumanReadableHexStringToByteArray( split[1].Trim() ); break;
							default: Console.WriteLine( "Don't know what key " + split[0] + " is." ); break;
						}
					}
				}
			} catch ( Exception ex ) {
				Console.WriteLine( "Failed to 3DS load keys from " + path + ": " + ex.ToString() );
			}
		}

		// https://github.com/profi200/Project_CTR/blob/master/ctrtool/aes_keygen.c
		private static long wrap_index( long i ) {
			return i < 0 ? ( ( i % 16 ) + 16 ) % 16 : ( i > 15 ? i % 16 : i );
		}

		private static byte[] n128_rrot( byte[] input, uint rot ) {
			rot = rot % 128;
			int byte_shift = (int)( rot / 8 );
			int bit_shift = (int)( rot % 8 );

			byte[] output = new byte[16];
			for ( int i = 0; i < 16; i++ ) {
				output[i] = (byte)( ( input[wrap_index( i - byte_shift )] >> bit_shift ) | ( input[wrap_index( i - byte_shift - 1 )] << ( 8 - bit_shift ) ) );
			}
			return output;
		}

		private static byte[] n128_lrot( byte[] input, uint rot ) {
			rot = rot % 128;
			int byte_shift = (int)( rot / 8 );
			int bit_shift = (int)( rot % 8 );

			byte[] output = new byte[16];
			for ( int i = 0; i < 16; i++ ) {
				output[i] = (byte)( ( input[wrap_index( i + byte_shift )] << bit_shift ) | ( input[wrap_index( i + byte_shift + 1 )] >> ( 8 - bit_shift ) ) );
			}
			return output;
		}


		private static byte[] n128_xor( byte[] a, byte[] b ) {
			byte[] output = new byte[16];
			for ( int i = 0; i < 16; i++ ) {
				output[i] = (byte)( a[i] ^ b[i] );
			}
			return output;
		}

		private static byte[] n128_add( byte[] a, byte[] b ) {
			byte carry = 0;
			uint sum = 0;

			byte[] output = new byte[16];
			for ( int i = 15; i >= 0; i-- ) {
				sum = (uint)( a[i] + b[i] + carry );
				carry = (byte)( sum >> 8 );
				output[i] = (byte)( sum & 0xff );
			}
			return output;
		}

		public static byte[] GenerateCombinedKey( byte[] keyX, byte[] keyY ) {
			byte[] KEYGEN_CONST = new byte[] { 0x1F, 0xF9, 0xE9, 0xAA, 0xC5, 0xFE, 0x04, 0x08, 0x02, 0x45, 0x91, 0xDC, 0x5D, 0x52, 0x76, 0x8A };

			// key = (((x <<< 2) ^ y) + KEYGEN_CONST) >>> 41
			return n128_rrot( n128_add( n128_xor( n128_lrot( keyX, 2 ), keyY ), KEYGEN_CONST ), 41 );
		}
	}
}
