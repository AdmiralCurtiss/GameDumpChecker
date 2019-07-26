using System;
using System.Collections.Generic;
using System.Linq;
using HyoutaPluginBase;
using HyoutaUtils;
using HyoutaUtils.Streams;

namespace GameDumpCheckerLib.N3DS {
	public class ExeFsReader {
		public class ExeFsSection {
			public string Name;
			public uint Offset;
			public uint Size;
			public byte[] Hash;
		}

		public DuplicatableStream DecryptedStream;

		public ExeFsSection[] Sections = new ExeFsSection[8];
		public SmdhReader Icon;

		public ExeFsReader( DuplicatableStream stream, NcsdReader ncsd, NcchReader ncch, KeyProvider keys, NcchReader.EncryptionType encryption, byte[] counter ) {
			DuplicatableStream header = new PartialStream( stream, 0, 0x200 );
			if ( encryption != NcchReader.EncryptionType.None ) {
				header = new EncryptedStream( header, ncch.Key0, counter );
			}

			List<(long offset, long size, DuplicatableStream substream)> l = new List<(long offset, long size, DuplicatableStream substream)>();
			l.Add( (0, 0x200, header) );
			bool sameKeys = ncch.Key0.SequenceEqual( ncch.Key1 );
			for ( int i = 0; i < 8; ++i ) {
				string name = header.ReadAscii( 8 ).TrimNull();
				uint offset = header.ReadUInt32();
				uint size = header.ReadUInt32();
				if ( size > 0 ) {
					uint alignedSize = size.Align( 0x100 );
					uint offsetInExefs = offset + 0x200;
					DuplicatableStream substream = new PartialStream( stream, offsetInExefs, alignedSize );
					if ( encryption != NcchReader.EncryptionType.None ) {
						if ( !sameKeys && !( name == "icon" || name == "banner" ) ) {
							// file itself uses key1 but blank space between files uses key0, what a mess!
							var unencryptedSubstream = substream;
							substream = new EncryptedStream( substream, ncch.Key1, Encryption.AddToCounter( counter, offsetInExefs / 0x10 ) );
							if ( alignedSize != size ) {
								var k0s = new PartialStream( new EncryptedStream( unencryptedSubstream, ncch.Key0, Encryption.AddToCounter( counter, offsetInExefs / 0x10 ) ), size, alignedSize - size );
								var k1s = new PartialStream( substream, 0, size );
								substream = ConcatenatedStream.CreateConcatenatedStream( new List<DuplicatableStream>() { k1s, k0s } );
							}
						} else {
							substream = new EncryptedStream( substream, ncch.Key0, Encryption.AddToCounter( counter, offsetInExefs / 0x10 ) );
						}
					}
					l.Add( (offsetInExefs, alignedSize, substream) );

					if ( name == "icon" ) {
						Icon = new SmdhReader( substream.Duplicate() );
					}
				}
			}

			DecryptedStream = EncryptedStreamConcat.MergePartiallyEncryptedStreams( new EncryptedStream( stream, ncch.Key0, counter ), l );
		}
	}
}
