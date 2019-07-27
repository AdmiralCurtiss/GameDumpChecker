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

		// TODO: Find better code for this, but this will do for power-of-two sizes for now...
		private static (string text, long powers) GetHumanReadableSize( long value ) {
			long v = value;
			long powers = 0;
			while ( v != 0 && ( v % 1024 ) == 0 ) {
				v /= 1024;
				++powers;
			}

			switch ( powers ) {
				case 0: return (v.ToString( "D" ) + " ", powers);
				case 1: return (v.ToString( "D" ) + " Ki", powers);
				case 2: return (v.ToString( "D" ) + " Mi", powers);
				case 3: return (v.ToString( "D" ) + " Gi", powers);
				case 4: return (v.ToString( "D" ) + " Ti", powers);
				case 5: return (v.ToString( "D" ) + " Pi", powers);
				case 6: return (v.ToString( "D" ) + " Ei", powers);
				case 7: return (v.ToString( "D" ) + " Zi", powers);
				case 8: return (v.ToString( "D" ) + " Yi", powers);
				default: return (value.ToString( "D" ) + " ", 0);
			}
		}

		public static string FormatByteCountForOutput( long value ) {
			var bytes = GetHumanReadableSize( value );
			StringBuilder sb = new StringBuilder();
			sb.Append( bytes.text ).Append( "Bytes" );
			if ( bytes.powers > 0 ) {
				sb.Append( " (" ).Append( value ).Append( " Bytes" ).Append( ")" );
			}
			return sb.ToString();
		}
	}
}
