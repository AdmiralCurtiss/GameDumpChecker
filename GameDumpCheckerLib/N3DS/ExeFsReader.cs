using System;
using System.IO;
using System.Text;
using HyoutaPluginBase;

namespace GameDumpCheckerLib.N3DS {
	public class ExeFsReader {
		public class ExeFsSection {
			public string Name;
			public uint Offset;
			public uint Size;
			public byte[] Hash;
		}

		public DuplicatableStream Stream;

		public ExeFsSection[] Sections = new ExeFsSection[8];
		public SmdhReader Icon;

		public ExeFsReader( DuplicatableStream stream, NcsdReader ncsd, NcchReader ncch, KeyProvider keys, NcchReader.EncryptionType encryption, byte[] counter ) {
			Stream = stream.Duplicate();

			var data = new byte[0x200];
			stream.Read( data, 0, data.Length );

			if ( encryption != NcchReader.EncryptionType.None ) {
				data = Encryption.Decrypt( data, ncch.Key0, counter );
			}

			for ( int i = 0; i < 8; ++i ) {
				Sections[i] = new ExeFsSection() {
					Name = Encoding.ASCII.GetString( data, i * 0x10 + 0, 8 ).TrimEnd( '\0' ),
					Offset = BitConverter.ToUInt32( data, i * 0x10 + 8 ),
					Size = BitConverter.ToUInt32( data, i * 0x10 + 12 )
				};
				Sections[i].Hash = new byte[0x20];
				Array.Copy( data, 0x100 + ( 7 - i ) * 0x20, Sections[i].Hash, 0, 0x20 );

				if ( Sections[i].Name == "icon" ) {
					var bytes = ExtractFile( stream, Sections[i], ncch, encryption, counter );
					Icon = new SmdhReader( new MemoryStream( bytes ) );
				}
			}
		}

		public byte[] ExtractFile( Stream stream, ExeFsSection section, NcchReader ncch, NcchReader.EncryptionType encryption, byte[] initialCounter ) {
			uint offsetInExefs = section.Offset + 0x200;
			byte[] data = new byte[section.Size];
			stream.Position = offsetInExefs;
			stream.Read( data, 0, data.Length );

			if ( encryption != NcchReader.EncryptionType.None ) {
				byte[] counter = new byte[initialCounter.Length];
				initialCounter.CopyTo( counter, 0 );
				Encryption.AddToCounter( counter, offsetInExefs / 0x10 );
				var key = section.Name == "icon" || section.Name == "banner" ? ncch.Key0 : ncch.Key1;
				data = Encryption.Decrypt( data, key, counter );
			}

			return data;
		}
	}
}
