using GameDumpCheckerLib;
using GameDumpCheckerLib.Readers;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace GameDumpCheckerCli {
    class Program {
        static void Main( string[] args ) {
            foreach ( string arg in args ) {
                System.IO.File.WriteAllText( arg + ".info.txt", GetInfoAsString( arg ) );
            }
        }

        static string GetInfoAsString( string filename ) {
            StringBuilder sb = new StringBuilder();

            List<DataSection> dataSections = new List<DataSection>();
            dataSections.Add( FileInfoProvider.GetDataForFile( filename ) );
            dataSections.AddRange( new PspGameReader( filename ).GenerateSystemSpecificDataSections() );

            int keymax = 0;
            int valuemax = 0;

            foreach ( DataSection ds in dataSections ) {
                foreach ( var e in ds.Data ) {
                    keymax = Math.Max( keymax, e.Key.Split( '\n' ).Max( x => x.Length ) );
                    valuemax = Math.Max( valuemax, e.Value.Split( '\n' ).Max( x => x.Length ) );
                }
            }

            foreach ( DataSection ds in dataSections ) {
                sb.Append( " === " );
                sb.Append( ds.Name );
                sb.Append( " ".PadRight( ( keymax + 2 + valuemax ) - 6 - ds.Name.Length, '=' ) );
                sb.AppendLine();

                foreach ( var e in ds.Data ) {
                    string[] keylines = e.Key.Split( '\n' );
                    string[] vallines = e.Value.Split( '\n' );
                    for ( int i = 0; i < Math.Max( keylines.Length, vallines.Length ); ++i ) {
                        string key = i < keylines.Length ? keylines[i] : "";
                        string value = i < vallines.Length ? vallines[i] : "";
                        sb.AppendFormat( "{0}{2} {1}", key.PadLeft( keymax ), value, i == 0 ? ":" : " " );
                        sb.AppendLine();
                    }
                }

                sb.AppendLine();
            }

            return sb.ToString();
        }
    }
}
