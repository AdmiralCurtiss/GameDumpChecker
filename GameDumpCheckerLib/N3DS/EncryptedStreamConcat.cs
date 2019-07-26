using HyoutaPluginBase;
using HyoutaUtils.Streams;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace GameDumpCheckerLib.N3DS {
	public static class EncryptedStreamConcat {
		public static DuplicatableStream MergePartiallyEncryptedStreams( DuplicatableStream parent, List<(long offset, long size, DuplicatableStream substream)> substreams ) {
			List<DuplicatableStream> decryptedStreams = new List<DuplicatableStream>();
			long currentOffset = 0;
			foreach ( var s in substreams.OrderBy( x => x.offset ) ) {
				if ( s.offset < currentOffset ) {
					throw new Exception( "Overlapping stream parts." );
				}
				if ( s.offset > currentOffset ) {
					decryptedStreams.Add( new PartialStream( parent, currentOffset, s.offset - currentOffset ) );
					currentOffset = s.offset;
				}
				decryptedStreams.Add( s.substream );
				currentOffset += s.size;
			}
			long leftover = parent.Length - currentOffset;
			if ( leftover > 0 ) {
				decryptedStreams.Add( new PartialStream( parent, currentOffset, leftover ) );
			}

			return ConcatenatedStream.CreateConcatenatedStream( decryptedStreams );
		}
	}
}
