using System;
using System.Collections.Generic;
using System.IO;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using HyoutaUtils;

namespace GameDumpCheckerLib.N3DS {
	public class NcchReader {
		public enum EncryptionType {
			None,
			Fixed,
			Secure,
		}

		public byte[] Signature;
		public uint ContentSize;
		public ulong TitleId;
		public string MakerCode;
		public ushort Version;
		public string ProductCode;
		public uint ExtendedHeaderSize;
		public ulong Flags;
		public uint ExeFsOffset;
		public uint ExeFsSize;
		public uint RomFsOffset;
		public uint RomFsSize;
		public byte[] Key0;
		public byte[] Key1;

		public ExHeaderReader ExHeader;
		public ExeFsReader ExeFs;

		public NcchReader( Stream stream, long offset, NcsdReader ncsd, KeyProvider keys ) {
			stream.Position = offset;

			Signature = new byte[0x100];
			stream.Read( Signature, 0, Signature.Length );

			if ( stream.ReadAscii( 4 ) != "NCCH" ) {
				throw new Exception( "wrong magic for 3DS" );
			}

			ContentSize = stream.ReadUInt32();
			TitleId = stream.ReadUInt64();
			MakerCode = stream.ReadAscii( 2 );
			Version = stream.ReadUInt16();
			stream.DiscardBytes( 4 );
			stream.DiscardBytes( 8 );
			stream.DiscardBytes( 0x10 );
			stream.DiscardBytes( 0x20 );
			ProductCode = stream.ReadAscii( 0x10 );
			stream.DiscardBytes( 0x20 );
			ExtendedHeaderSize = stream.ReadUInt32();
			stream.DiscardBytes( 4 );
			Flags = stream.ReadUInt64();
			stream.DiscardBytes( 4 );
			stream.DiscardBytes( 4 );
			stream.DiscardBytes( 4 );
			stream.DiscardBytes( 4 );
			ExeFsOffset = stream.ReadUInt32();
			ExeFsSize = stream.ReadUInt32();
			stream.DiscardBytes( 4 );
			stream.DiscardBytes( 4 );
			RomFsOffset = stream.ReadUInt32();
			RomFsSize = stream.ReadUInt32();
			stream.DiscardBytes( 4 );
			stream.DiscardBytes( 4 );
			stream.DiscardBytes( 0x20 );
			stream.DiscardBytes( 0x20 );

			long exheaderOffset = offset + 0x200;
			long exeFsOffset = offset + ExeFsOffset * ncsd.MediaunitSize;
			EncryptionType encryption;

			bool isEncrypted = ( Flags & 4 ) == 0;
			if ( isEncrypted ) {
				bool fixedKeyCrypto = ( Flags & 1 ) > 0;
				if ( fixedKeyCrypto ) {
					Console.WriteLine( "Fixed key crypto not implemented." );
					encryption = EncryptionType.Fixed;
				} else {
					byte[] key0x = keys.Slot0x2CKeyX;
					byte[] key0y = new byte[0x10];
					Array.Copy( Signature, key0y, 0x10 );

					byte[] key1x = null;
					switch ( ( Flags & 0x000000FF00000000ul ) >> 32 ) {
						case 0: key1x = keys.Slot0x2CKeyX; break;
						case 1: key1x = keys.Slot0x25KeyX; break;
						case 10: key1x = keys.Slot0x18KeyX; break;
						case 11: key1x = keys.Slot0x1BKeyX; break;
						default: Console.WriteLine( "Unknown key1x." ); return;
					}

					byte[] key1y = null;
					bool usesSeed = ( Flags & 0x20 ) > 0;
					if ( usesSeed ) {
						Console.WriteLine( "Seed crypto not implemented." );
						return;
					} else {
						key1y = key0y;
					}

					Key0 = KeyProvider.GenerateCombinedKey( key0x, key0y );
					Key1 = KeyProvider.GenerateCombinedKey( key1x, key1y );

					encryption = EncryptionType.Secure;
				}
			} else {
				encryption = EncryptionType.None;
			}

			if ( ExtendedHeaderSize > 0 ) {
				byte[] exheaderCounter = GetCounter( 1, ncsd.MediaunitSize );
				ExHeader = new ExHeaderReader( stream, exheaderOffset, ncsd, this, keys, encryption, exheaderCounter );
			}

			if ( ExeFsSize > 0 ) {
				byte[] exefsCounter = GetCounter( 2, ncsd.MediaunitSize );
				ExeFs = new ExeFsReader( stream, exeFsOffset, ncsd, this, keys, encryption, exefsCounter );
			}

			return;
		}

		private byte[] GetCounter( byte type, long mediaUnitSize ) {
			byte[] counter = new byte[16];
			if ( Version == 2 || Version == 0 ) {
				for ( int i = 0; i < 8; ++i ) {
					counter[i] = (byte)( ( TitleId >> ( 8 * ( 7 - i ) ) ) & 0xFF );
				}
				counter[8] = type;
			} else if ( Version == 1 ) {
				uint x;
				switch ( type ) {
					case 1: x = 0x200; break;
					case 2: x = (uint)( ExeFsOffset * mediaUnitSize ); break;
					case 3: x = (uint)( RomFsOffset * mediaUnitSize ); break;
					default: throw new Exception( "Invalid type for counter: " + type );
				}
				for ( int i = 0; i < 8; ++i ) {
					counter[i] = (byte)( ( TitleId >> ( 8 * i ) ) & 0xFF );
				}
				for ( int i = 0; i < 4; ++i ) {
					counter[12 + i] = (byte)( x >> ( ( 3 - i ) * 8 ) );
				}
			}
			return counter;
		}
	}
}
