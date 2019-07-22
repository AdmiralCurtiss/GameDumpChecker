using System;
using System.Collections.Generic;
using System.IO;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using HyoutaUtils;

namespace GameDumpCheckerLib.N3DS {
	public class ExHeaderReader {
		public string Title;
		public ulong Flags;

		public ExHeaderReader( Stream stream, long offset, NcsdReader ncsd, NcchReader ncch, KeyProvider keys, NcchReader.EncryptionType encryption, byte[] counter ) {
			stream.Position = offset;
			var data = new byte[0x800];
			stream.Read( data, 0, data.Length );

			if ( encryption != NcchReader.EncryptionType.None ) {
				data = Encryption.Decrypt( data, ncch.Key0, counter );
			}

			// there's a lot more in here but we don't really care about the rest
			Title = new MemoryStream( new MemoryStream( data ).ReadBytes( 8 ) ).ReadAsciiNullterm();
			Flags = BitConverter.ToUInt64( data, 0x8 );
		}
	}
}
