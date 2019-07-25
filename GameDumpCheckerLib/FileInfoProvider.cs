using System;
using System.Collections.Generic;
using System.IO;
using HyoutaUtils.Streams;

namespace GameDumpCheckerLib {
    public static class FileInfoProvider {
        public static DataSection GetDataForFile( string filename ) {
            var data = new List<(string Key, string Value)>();

            using ( Stream s = new DuplicatableFileStream( filename ) ) {
                data.Add( ("Filename", System.IO.Path.GetFileName( filename )) );
                data.Add( ("Filesize", s.Length + " bytes") );
                Console.WriteLine( "Calculating CRC32 of " + filename );
                data.Add( ("CRC32", Util.ByteArrayToHumanReadableHexString( Checksums.CRC32.CalculateCRC32( s ) )) );
                Console.WriteLine( "Calculating MD5 of " + filename );
                data.Add( ("MD5", Util.ByteArrayToHumanReadableHexString( Checksums.Internal.CalculateMD5( s ) )) );
                Console.WriteLine( "Calculating SHA1 of " + filename );
                data.Add( ("SHA1", Util.ByteArrayToHumanReadableHexString( Checksums.Internal.CalculateSHA1( s ) )) );
                Console.WriteLine( "Calculating SHA256 of " + filename );
                data.Add( ("SHA256", Util.ByteArrayToHumanReadableHexString( Checksums.Internal.CalculateSHA256( s ) )) );
                Console.WriteLine( "Calculating SHA512 of " + filename );
                data.Add( ("SHA512", Util.ByteArrayToHumanReadableHexString( Checksums.Internal.CalculateSHA512( s ) )) );
            }

            return new DataSection( "File Info", data );
        }
    }
}
