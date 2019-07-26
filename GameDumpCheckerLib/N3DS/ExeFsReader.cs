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
			for ( int i = 0; i < 8; ++i ) {
				string name = header.ReadAscii( 8 ).TrimNull();
				uint offset = header.ReadUInt32();
				uint size = header.ReadUInt32();
				if ( size > 0 ) {
					uint alignedSize = size.Align( 0x100 );
					uint offsetInExefs = offset + 0x200;
					DuplicatableStream substream = new PartialStream( stream, offsetInExefs, alignedSize );
					if ( encryption != NcchReader.EncryptionType.None ) {
						var key = name == "icon" || name == "banner" ? ncch.Key0 : ncch.Key1;
						substream = new EncryptedStream( substream, key, Encryption.AddToCounter( counter, offsetInExefs / 0x10 ) );
					}
					l.Add( (offsetInExefs, alignedSize, substream) );

					if ( name == "icon" ) {
						Icon = new SmdhReader( substream.Duplicate() );
					}
				}
			}

			DecryptedStream = EncryptedStreamConcat.MergePartiallyEncryptedStreams( stream, l );
		}
	}
}
