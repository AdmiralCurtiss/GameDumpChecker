using System;
using System.Collections.Generic;
using System.IO;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace GameDumpCheckerLib {
    public static class FileInfoProvider {
        public static DataSection GetDataForFile( string filename ) {
            var data = new List<(string Key, string Value)>();

            using ( Stream s = new System.IO.FileStream( filename, FileMode.Open ) ) {
                data.Add( ("Filename", System.IO.Path.GetFileName( filename )) );
                data.Add( ("Filesize", s.Length + " bytes") );
                data.Add( ("CRC32", Util.ByteArrayToHumanReadableHexString( Checksums.CRC32.CalculateCRC32( s ) )) );
                data.Add( ("MD5", Util.ByteArrayToHumanReadableHexString( Checksums.Internal.CalculateMD5( s ) )) );
                data.Add( ("SHA1", Util.ByteArrayToHumanReadableHexString( Checksums.Internal.CalculateSHA1( s ) )) );
                data.Add( ("SHA256", Util.ByteArrayToHumanReadableHexString( Checksums.Internal.CalculateSHA256( s ) )) );
                data.Add( ("SHA512", Util.ByteArrayToHumanReadableHexString( Checksums.Internal.CalculateSHA512( s ) )) );
            }

            return new DataSection( "File Info", data );
        }
    }
}
