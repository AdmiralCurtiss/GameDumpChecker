using System;
using System.Collections.Generic;
using System.IO;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace GameDumpCheckerLib.N3DS {
	public class NcsdPartitionGeometry {
		public uint Offset;
		public uint Size;
	}

	public class NcsdReader {
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

		public NcsdReader( Stream stream, KeyProvider keys ) {
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
					Partitions[i] = new NcchReader( stream, PartitionGeometry[i].Offset * MediaunitSize, this, keys );
				}
			}

			return;
		}
	}
}
