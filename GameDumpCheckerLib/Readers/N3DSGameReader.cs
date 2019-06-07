using GameDumpCheckerLib.Gamecube;
using GameDumpCheckerLib.N3DS;
using System;
using System.Collections.Generic;
using System.IO;

namespace GameDumpCheckerLib.Readers {
	public class N3DSGameReader : IGameReader {
		private string Filename;
		private NcsdReader CCI;

		public N3DSGameReader( string filename ) {
			Filename = filename;
			using ( var stream = new FileStream( Filename, FileMode.Open ) ) {
				CCI = new NcsdReader( stream );
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

			{
				var ncch = CCI.Partitions[0];
				var gameData = new List<(string Key, string Value)>();
				gameData.Add( ("Content Size", ( ncch.ContentSize * CCI.MediaunitSize ).ToString( "D" ) + " bytes") );
				gameData.Add( ("Title ID", ncch.TitleId.ToString( "X16" )) );
				gameData.Add( ("Maker Code", ncch.MakerCode) );
				gameData.Add( ("Version", ncch.Version.ToString( "D" )) );
				gameData.Add( ("Product Code", ncch.ProductCode.TrimEnd( '\0' )) );
				sections.Add( new DataSection( "Game Data (NCCH Header)", gameData ) );
			}

			return sections;
		}
	}
}
