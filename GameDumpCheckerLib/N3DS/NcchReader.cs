using System;
using System.Collections.Generic;
using HyoutaPluginBase;
using HyoutaUtils;
using HyoutaUtils.Streams;

namespace GameDumpCheckerLib.N3DS {
	public class NcchReader {
		public enum EncryptionType {
			None,
			Fixed,
			Secure,
		}

		public DuplicatableStream EncryptedStream;
		public DuplicatableStream DecryptedStream;

		public byte[] Signature;
		public uint ContentSize;
		public ulong TitleId;
		public string MakerCode;
		public ushort Version;
		public string ProductCode;
		public uint ExtendedHeaderSize;
		public byte[] Flags;
		public uint PlainRegionOffset;
		public uint PlainRegionSize;
		public uint LogoOffset;
		public uint LogoSize;
		public uint ExeFsOffset;
		public uint ExeFsSize;
		public uint RomFsOffset;
		public uint RomFsSize;
		public byte[] Key0;
		public byte[] Key1;

		public ExHeaderReader ExHeader;
		public ExeFsReader ExeFs;
		public RomFsReader RomFs;

		public NcchReader( DuplicatableStream ncchstream, NcsdReader ncsd, KeyProvider keys ) {
			EncryptedStream = ncchstream.Duplicate();
			List<(long offset, long size, DuplicatableStream substream)> l = new List<(long offset, long size, DuplicatableStream substream)>();
			DuplicatableStream stream = new PartialStream( ncchstream, 0, 0x200 );

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
			Flags = stream.ReadBytes( 8 );
			PlainRegionOffset = stream.ReadUInt32();
			PlainRegionSize = stream.ReadUInt32();
			LogoOffset = stream.ReadUInt32();
			LogoSize = stream.ReadUInt32();
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

			long exheaderOffset = 0x200;
			long plainRegionOffset = PlainRegionOffset * ncsd.MediaunitSize;
			long logoOffset = LogoOffset * ncsd.MediaunitSize;
			long exeFsOffset = ExeFsOffset * ncsd.MediaunitSize;
			long romFsOffset = RomFsOffset * ncsd.MediaunitSize;
			EncryptionType encryption;

			bool isEncrypted = ( Flags[7] & 4 ) == 0;
			if ( isEncrypted ) {
				bool fixedKeyCrypto = ( Flags[7] & 1 ) > 0;
				if ( fixedKeyCrypto ) {
					Console.WriteLine( "Fixed key crypto not implemented." );
					encryption = EncryptionType.Fixed;
				} else {
					byte[] key0x = keys.Slot0x2CKeyX;
					byte[] key0y = new byte[0x10];
					Array.Copy( Signature, key0y, 0x10 );

					byte[] key1x = null;
					switch ( Flags[3] ) {
						case 0: key1x = keys.Slot0x2CKeyX; break;
						case 1: key1x = keys.Slot0x25KeyX; break;
						case 10: key1x = keys.Slot0x18KeyX; break;
						case 11: key1x = keys.Slot0x1BKeyX; break;
						default: Console.WriteLine( "Unknown key1x." ); return;
					}

					byte[] key1y = null;
					bool usesSeed = ( Flags[7] & 0x20 ) > 0;
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

			if ( encryption != EncryptionType.None ) {
				byte[] newFlags = new byte[8];
				Flags.CopyTo( newFlags, 0 );
				newFlags[3] = 0; // remove key index
				newFlags[7] |= 4; // set decrypted bit
				l.Add( (0, 0x188, new PartialStream( stream, 0, 0x188 )) );
				l.Add( (0x188, 0x8, new DuplicatableByteArrayStream( newFlags )) );
				l.Add( (0x190, 0x70, new PartialStream( stream, 0x190, 0x70 )) );
			} else {
				l.Add( (0, 0x200, stream) );
			}

			if ( ExtendedHeaderSize > 0 ) {
				// The hardcoded 0x800 here seems strange here. I wonder if there's a better logic for that?
				using ( DuplicatableStream exHeaderStream = new PartialStream( ncchstream, exheaderOffset, 0x800 ) ) {
					byte[] exheaderCounter = GetCounter( 1, ncsd.MediaunitSize );
					ExHeader = new ExHeaderReader( exHeaderStream, ncsd, this, keys, encryption, exheaderCounter );
					l.Add( (exheaderOffset, ExHeader.DecryptedStream.Length, ExHeader.DecryptedStream) );
				}
			}

			if ( ExeFsSize > 0 ) {
				using ( DuplicatableStream exeFsStream = new PartialStream( ncchstream, exeFsOffset, ExeFsSize * ncsd.MediaunitSize ) ) {
					byte[] exefsCounter = GetCounter( 2, ncsd.MediaunitSize );
					ExeFs = new ExeFsReader( exeFsStream, ncsd, this, keys, encryption, exefsCounter );
					l.Add( (exeFsOffset, ExeFs.DecryptedStream.Length, ExeFs.DecryptedStream) );
				}
			}

			if ( RomFsSize > 0 ) {
				using ( DuplicatableStream romFsStream = new PartialStream( ncchstream, romFsOffset, RomFsSize * ncsd.MediaunitSize ) ) {
					byte[] romfsCounter = GetCounter( 3, ncsd.MediaunitSize );
					RomFs = new RomFsReader( romFsStream, ncsd, this, keys, encryption, romfsCounter );
					l.Add( (romFsOffset, RomFs.DecryptedStream.Length, RomFs.DecryptedStream) );
				}
			}

			DecryptedStream = EncryptedStreamConcat.MergePartiallyEncryptedStreams( ncchstream, l );

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
