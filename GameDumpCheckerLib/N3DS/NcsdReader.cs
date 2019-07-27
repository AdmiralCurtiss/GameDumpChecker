using System;
using System.Collections.Generic;
using HyoutaPluginBase;
using HyoutaUtils;
using HyoutaUtils.Streams;

namespace GameDumpCheckerLib.N3DS {
	public class NcsdPartitionGeometry {
		public uint Offset;
		public uint Size;
	}

	public class NcsdReader {
		public DuplicatableStream DecryptedStream;

		public uint MediaSize;
		public ulong MediaId;
		public NcsdPartitionGeometry[] PartitionGeometry;
		public byte[] Flags;

		public NcchReader[] Partitions;

		public ushort TitleVersion;
		public ushort CardRevision;

		public long MediaunitSize {
			get {
				return 1L << ( 9 + Flags[6] );
			}
		}
		public long GetPartitionOffset( int i ) {
			return PartitionGeometry[i].Offset * MediaunitSize;
		}
		public long GetPartitionLength( int i ) {
			return PartitionGeometry[i].Size * MediaunitSize;
		}

		public NcsdReader( DuplicatableStream ncsdstream, KeyProvider keys ) {
			DuplicatableStream stream = ncsdstream.Duplicate();
			stream.Position = 0;
			byte[] signature = stream.ReadBytes( 0x100 );

			if ( stream.ReadAscii( 4 ) != "NCSD" ) {
				throw new Exception( "wrong magic for 3DS" );
			}

			MediaSize = stream.ReadUInt32();
			MediaId = stream.ReadUInt64();
			byte[] partitionFsType = stream.ReadBytes( 8 );
			byte[] partitionCryptType = stream.ReadBytes( 8 );
			PartitionGeometry = new NcsdPartitionGeometry[8];
			for ( int i = 0; i < 8; ++i ) {
				PartitionGeometry[i] = new NcsdPartitionGeometry();
				PartitionGeometry[i].Offset = stream.ReadUInt32();
				PartitionGeometry[i].Size = stream.ReadUInt32();
			}

			byte[] extendedHeaderHash = stream.ReadBytes( 0x20 );
			uint additionalHeaderSize = stream.ReadUInt32();
			uint sectorZeroOffset = stream.ReadUInt32();
			Flags = stream.ReadBytes( 8 );
			ulong[] titleIds = new ulong[8];
			for (int i = 0; i < 8; ++i ) {
				titleIds[i] = stream.ReadUInt64();
			}
			byte[] reserved = stream.ReadBytes( 0x30 );

			stream.Position = 0x310;
			TitleVersion = stream.ReadUInt16();
			CardRevision = stream.ReadUInt16();

			List<(long offset, long size, DuplicatableStream substream)> l = new List<(long offset, long size, DuplicatableStream substream)>();
			Partitions = new NcchReader[8];
			for ( int i = 0; i < 8; ++i ) {
				if ( PartitionGeometry[i].Size > 0 ) {
					using ( DuplicatableStream ncchStream = new PartialStream( ncsdstream, GetPartitionOffset( i ), GetPartitionLength( i ) ) ) {
						Partitions[i] = new NcchReader( ncchStream, this, keys );
						l.Add( (GetPartitionOffset( i ), Partitions[i].DecryptedStream.Length, Partitions[i].DecryptedStream) );
					}
				}
			}

			DecryptedStream = EncryptedStreamConcat.MergePartiallyEncryptedStreams( ncsdstream, l );

			return;
		}
	}
}
