using System;
using System.Collections.Generic;
using System.IO;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace GameDumpCheckerLib.N3DS {
	public class NcchReader {
		public uint ContentSize;
		public ulong TitleId;
		public string MakerCode;
		public ushort Version;
		public string ProductCode;
		public uint ExeFsOffset;
		public uint ExeFsSize;
		public uint RomFsOffset;
		public uint RomFsSize;

		public NcchReader( Stream stream, long offset, NcsdReader ncsd ) {
			stream.Position = offset + 0x100;
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
			stream.DiscardBytes( 4 );
			stream.DiscardBytes( 4 );
			stream.DiscardBytes( 8 );
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

			long exeFsOffset = offset + ExeFsOffset * ncsd.MediaunitSize;

			// TODO: decrypt and parse icon

			return;
		}
	}
}
