using GameDumpCheckerLib.Gamecube;
using GameDumpCheckerLib.N3DS;
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
			using ( var stream = new FileStream( Filename, FileMode.Open ) ) {
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
				var gameData = new List<(string Key, string Value)>();
				gameData.Add( ("Media Size", ( CCI.MediaSize * CCI.MediaunitSize ).ToString( "D" ) + " bytes") );
				gameData.Add( ("Media ID", CCI.MediaId.ToString( "X16" )) );
				sections.Add( new DataSection( "Game Data (NCSD Header)", gameData ) );
			}

			for ( int i = 0; i < CCI.Partitions.Length; ++i ) {
				NcchReader ncch = CCI.Partitions[i];
				if ( ncch != null ) {
					var gameData = new List<(string Key, string Value)>();
					gameData.Add( ("Content Size", ( ncch.ContentSize * CCI.MediaunitSize ).ToString( "D" ) + " bytes") );
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
					sections.Add( new DataSection( "Game Data (NCCH Header / Partition " + i + ")", gameData ) );
				}
			}

			return sections;
		}
	}
}
