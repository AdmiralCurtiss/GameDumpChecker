using System;
using HyoutaPluginBase;
using HyoutaUtils;
using HyoutaUtils.Streams;

namespace GameDumpCheckerLib.N3DS {
	public class ExHeaderReader {
		public DuplicatableStream Stream;
		public string Title;
		public ulong Flags;

		public ExHeaderReader( DuplicatableStream stream, NcsdReader ncsd, NcchReader ncch, KeyProvider keys, NcchReader.EncryptionType encryption, byte[] counter ) {
			Stream = stream.Duplicate();

			var data = new byte[0x800];
			stream.Read( data, 0, data.Length );

			if ( encryption != NcchReader.EncryptionType.None ) {
				data = Encryption.Decrypt( data, ncch.Key0, counter );
			}

			// there's a lot more in here but we don't really care about the rest
			Title = new DuplicatableByteArrayStream( data ).ReadAscii( 8 ).TrimNull();
			Flags = BitConverter.ToUInt64( data, 0x8 );
		}
	}
}
