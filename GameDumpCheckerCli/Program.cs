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
            dataSections.Add( BasicInfoProvider.GetDataForFile( filename ) );
            dataSections.AddRange( new PspGameReader( filename ).GenerateSystemSpecificDataSections() );

            int keymax = 0;
            int valuemax = 0;

            foreach ( DataSection ds in dataSections ) {
                foreach ( var e in ds.Data ) {
                    keymax = Math.Max( keymax, e.Key.Length );
                    valuemax = Math.Max( valuemax, e.Value.Length );
                }
            }

            foreach ( DataSection ds in dataSections ) {
                sb.Append( " === " );
                sb.Append( ds.Name );
                sb.Append( " ".PadRight( ( keymax + 2 + valuemax ) - 6 - ds.Name.Length, '=' ) );
                sb.AppendLine();

                foreach ( var e in ds.Data ) {
                    sb.AppendFormat( "{0}: {1}", e.Key.PadLeft( keymax ), e.Value );
                    sb.AppendLine();
                }

                sb.AppendLine();
            }

            return sb.ToString();
        }
    }
}
