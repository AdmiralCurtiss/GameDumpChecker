using GameDumpCheckerLib.Gamecube;
using GameDumpCheckerLib.N3DS;
using HyoutaUtils.Streams;
using System;
using System.Collections.Generic;
using System.IO;

namespace GameDumpCheckerLib.Readers {
	public class N3DSGameReader : IGameReader {
		private static KeyProvider Keys = new KeyProvider();
		private string Filename;
		public NcsdReader CCI;

		public N3DSGameReader( string filename ) {
			Filename = filename;
			using ( var stream = new DuplicatableFileStream( Filename ) ) {
				CCI = new NcsdReader( stream, Keys );
			}
		}

		public List<DataSection> GenerateManuallyFilledInfoSections() {
			var list = new List<DataSection>();
			return list;
		}

		public List<DataSection> GenerateSystemSpecificDataSections() {
			List<DataSection> sections = new List<DataSection>();

			{
				var decrypt = new List<(string Key, string Value)>();
				FileInfoProvider.CalculateHashes( decrypt, CCI.DecryptedStream, " (decrypted)" );
				sections.Add( new DataSection( "Decrypted Hashes", decrypt ) );
			}

			{
				var gameData = new List<(string Key, string Value)>();
				gameData.Add( ("Media Size", ( CCI.MediaSize * CCI.MediaunitSize ).ToString( "D" ) + " bytes") );
				gameData.Add( ("Media ID", CCI.MediaId.ToString( "X16" )) );
				sections.Add( new DataSection( "Game Data (NCSD Header)", gameData ) );
			}

			for ( int i = 0; i < CCI.Partitions.Length; ++i ) {
				NcchReader ncch = CCI.Partitions[i];
				if ( ncch != null ) {
					var gameData = new List<(string Key, string Value)>();
					gameData.Add( ("Starts at offset", "0x" + CCI.GetPartitionOffset( i ).ToString( "X" )) );
					gameData.Add( ("Ends at offset", "0x" + ( CCI.GetPartitionOffset( i ) + CCI.GetPartitionLength( i ) ).ToString( "X" )) );
					gameData.Add( ("Partition Size", CCI.GetPartitionLength( i ).ToString( "D" ) + " bytes") );
					gameData.Add( ("Title ID", ncch.TitleId.ToString( "X16" )) );
					gameData.Add( ("Product Code", ncch.ProductCode.TrimEnd( '\0' )) );
					if ( ncch.ExHeader != null ) {
						gameData.Add( ("Name (ExHeader)", ncch.ExHeader.Title) );
					}
					if ( ncch.ExeFs != null ) {
						for ( int j = 0; j < 16; ++j ) {
							var ts = ncch.ExeFs.Icon.TitleStructs[j];
							if ( ts.ShortDescription != "" || ts.LongDescription != "" || ts.Publisher != "" ) {
								gameData.Add( ("Short Name (#" + j + ")", ts.ShortDescription) );
								gameData.Add( ("Long Name (#" + j + ")", ts.LongDescription) );
								gameData.Add( ("Publisher (#" + j + ")", ts.Publisher) );
							}
						}
					}
					FileInfoProvider.CalculateHashes( gameData, ncch.EncryptedStream, " (encrypted)" );
					FileInfoProvider.CalculateHashes( gameData, ncch.DecryptedStream, " (decrypted)" );
					sections.Add( new DataSection( "Game Data (NCCH / Partition " + i + ")", gameData ) );
				}
			}

			return sections;
		}
	}
}
