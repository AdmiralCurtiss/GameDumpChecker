using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using HyoutaPluginBase;

namespace GameDumpCheckerLib.N3DS {
	public class RomFsReader {
		public DuplicatableStream DecryptedStream;
		public string Title;
		public ulong Flags;

		public RomFsReader( DuplicatableStream stream, NcsdReader ncsd, NcchReader ncch, KeyProvider keys, NcchReader.EncryptionType encryption, byte[] counter ) {
			DuplicatableStream s = stream;
			if ( encryption != NcchReader.EncryptionType.None ) {
				s = new EncryptedStream( s, ncch.Key1, counter );
			}
			DecryptedStream = s;
		}
	}
}
