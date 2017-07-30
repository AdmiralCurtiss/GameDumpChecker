using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace GameDumpCheckerLib {
    public static class Util {
        public static string ByteArrayToHumanReadableHexString( byte[] data ) {
            StringBuilder sb = new StringBuilder( data.Length * 2 );
            foreach ( byte b in data ) {
                sb.AppendFormat( "{0:X2}", b );
            }
            return sb.ToString();
        }
    }
}
