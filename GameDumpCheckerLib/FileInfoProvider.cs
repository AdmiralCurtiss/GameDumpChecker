using System;
using System.Collections.Generic;
using System.IO;
using HyoutaUtils.Streams;

namespace GameDumpCheckerLib {
	public static class FileInfoProvider {
		public static DataSection GetDataForFile( string filename ) {
			using ( Stream s = new DuplicatableFileStream( filename ) ) {
				var data = new List<(string Key, string Value)>();
				data.Add( ("Filename", Path.GetFileName( filename )) );
				data.Add( ("Filesize", Util.FormatByteCountForOutput( s.Length )) );
				Console.WriteLine( "Calculating Hashes of " + filename );
				CalculateHashes( data, s, "" );
				return new DataSection( "File Info", data );
			}
		}

		public static void CalculateHashes( List<(string Key, string Value)> data, Stream s, string postfix ) {
			Console.WriteLine( "Calculating CRC32..." );
			data.Add( ("CRC32" + postfix, Util.ByteArrayToHumanReadableHexString( Checksums.CRC32.CalculateCRC32( s ) )) );
			Console.WriteLine( "Calculating MD5..." );
			data.Add( ("MD5" + postfix, Util.ByteArrayToHumanReadableHexString( Checksums.Internal.CalculateMD5( s ) )) );
			Console.WriteLine( "Calculating SHA1..." );
			data.Add( ("SHA1" + postfix, Util.ByteArrayToHumanReadableHexString( Checksums.Internal.CalculateSHA1( s ) )) );
			Console.WriteLine( "Calculating SHA256..." );
			data.Add( ("SHA256" + postfix, Util.ByteArrayToHumanReadableHexString( Checksums.Internal.CalculateSHA256( s ) )) );
			Console.WriteLine( "Calculating SHA512..." );
			data.Add( ("SHA512" + postfix, Util.ByteArrayToHumanReadableHexString( Checksums.Internal.CalculateSHA512( s ) )) );
		}
	}
}
