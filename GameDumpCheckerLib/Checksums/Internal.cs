using System;
using System.Collections.Generic;
using System.IO;
using System.Linq;
using System.Security.Cryptography;
using System.Text;
using System.Threading.Tasks;

namespace GameDumpCheckerLib.Checksums {
    public static class Internal {
        public static byte[] CalculateChecksum( Stream s, HashAlgorithm algorithm ) {
            long pos = s.Position;
            s.Position = 0;
            byte[] crc32 = algorithm.ComputeHash( s );
            s.Position = pos;
            return crc32;
        }

        public static byte[] CalculateMD5( Stream s ) {
            using ( var algorithm = System.Security.Cryptography.MD5.Create() ) {
                return CalculateChecksum( s, algorithm );
            }
        }

        public static byte[] CalculateSHA1( Stream s ) {
            using ( var algorithm = System.Security.Cryptography.SHA1.Create() ) {
                return CalculateChecksum( s, algorithm );
            }
        }

        public static byte[] CalculateSHA256( Stream s ) {
            using ( var algorithm = System.Security.Cryptography.SHA256.Create() ) {
                return CalculateChecksum( s, algorithm );
            }
        }

        public static byte[] CalculateSHA512( Stream s ) {
            using ( var algorithm = System.Security.Cryptography.SHA512.Create() ) {
                return CalculateChecksum( s, algorithm );
            }
        }
    }
}
