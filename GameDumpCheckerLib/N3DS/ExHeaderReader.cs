using System;
using HyoutaPluginBase;
using HyoutaUtils;
using HyoutaUtils.Streams;

namespace GameDumpCheckerLib.N3DS {
	public class ExHeaderReader {
		public DuplicatableStream DecryptedStream;
		public string Title;
		public ulong Flags;

		public ExHeaderReader( DuplicatableStream stream, NcsdReader ncsd, NcchReader ncch, KeyProvider keys, NcchReader.EncryptionType encryption, byte[] counter ) {
			DuplicatableStream s = stream;
			if ( encryption != NcchReader.EncryptionType.None ) {
				s = new EncryptedStream( s, ncch.Key0, counter );
			}

			// there's a lot more in here but we don't really care about the rest
			Title = s.ReadAscii( 8 ).TrimNull();
			Flags = s.ReadUInt64();

			DecryptedStream = s;
		}
	}
}
