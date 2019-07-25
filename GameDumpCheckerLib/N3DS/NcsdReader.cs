using System;
using HyoutaPluginBase;
using HyoutaUtils;
using HyoutaUtils.Streams;

namespace GameDumpCheckerLib.N3DS {
	public class NcsdPartitionGeometry {
		public uint Offset;
		public uint Size;
	}

	public class NcsdReader {
		public DuplicatableStream Stream;

		public uint MediaSize;
		public ulong MediaId;
		public NcsdPartitionGeometry[] PartitionGeometry;
		public byte[] Flags;

		public NcchReader[] Partitions;

		public long MediaunitSize {
			get {
				return 1L << ( 9 + Flags[6] );
			}
		}

		public NcsdReader( DuplicatableStream stream, KeyProvider keys ) {
			Stream = stream.Duplicate();

			stream.Position = 0x100;
			if ( stream.ReadAscii( 4 ) != "NCSD" ) {
				throw new Exception( "wrong magic for 3DS" );
			}

			MediaSize = stream.ReadUInt32();
			MediaId = stream.ReadUInt64();
			stream.DiscardBytes( 16 );
			PartitionGeometry = new NcsdPartitionGeometry[8];
			for ( int i = 0; i < 8; ++i ) {
				PartitionGeometry[i] = new NcsdPartitionGeometry();
				PartitionGeometry[i].Offset = stream.ReadUInt32();
				PartitionGeometry[i].Size = stream.ReadUInt32();
			}

			stream.DiscardBytes( 0x20 );
			stream.DiscardBytes( 4 );
			stream.DiscardBytes( 4 );
			Flags = new byte[8];
			for ( int i = 0; i < 8; ++i ) {
				Flags[i] = stream.ReadUInt8();
			}

			Partitions = new NcchReader[8];
			for ( int i = 0; i < 8; ++i ) {
				if ( PartitionGeometry[i].Size > 0 ) {
					using ( DuplicatableStream ncchStream = new PartialStream( Stream, PartitionGeometry[i].Offset * MediaunitSize, PartitionGeometry[i].Size * MediaunitSize ) ) {
						Partitions[i] = new NcchReader( ncchStream, this, keys );
					}
				}
			}

			return;
		}
	}
}
