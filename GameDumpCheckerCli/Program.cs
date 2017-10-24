using GameDumpCheckerLib;
using GameDumpCheckerLib.Readers;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace GameDumpCheckerCli {
    class Config {
        public FileType FileType;
    }

    class Program {
        static int Main( string[] args ) {
            List<string> freeArgs = new List<string>();
            Config config = new Config();
            {
                Queue<string> remainingArgs = new Queue<string>( args );
                try {
                    while ( remainingArgs.Count > 0 ) {
                        string arg = remainingArgs.Dequeue();
                        switch ( arg ) {
                            case "--":
                                freeArgs.AddRange( remainingArgs );
                                remainingArgs.Clear();
                                break;
                            case "--type":
                                config.FileType = (FileType)Enum.Parse( typeof( FileType ), remainingArgs.Dequeue(), true );
                                break;
                            default:
                                freeArgs.Add( arg );
                                break;
                        }
                    }
                } catch ( Exception ) {
                    PrintUsage();
                    return -1;
                }
            }

            if ( freeArgs.Count == 0 ) {
                PrintUsage();
                return -1;
            }

            foreach ( string arg in freeArgs ) {
                System.IO.File.WriteAllText( arg + ".info.txt", GetInfoAsString( config, arg ) );
            }

            return 0;
        }

        static void PrintUsage() {
            Console.WriteLine( "GameDumpChecker [options] file [file] [...]" );
            Console.WriteLine();
            Console.WriteLine( "Options:" );
            Console.WriteLine( " --type psp/gcn" );
            Console.WriteLine();
        }

        static string GetInfoAsString( Config config, string filename ) {
            StringBuilder sb = new StringBuilder();

            List<DataSection> dataSections = new List<DataSection>();
            dataSections.Add( FileInfoProvider.GetDataForFile( filename ) );
            Console.WriteLine( "Parsing game data of " + filename + " with type " + config.FileType );
            IGameReader reader = GameReaderFactory.ConstructFromType( config.FileType, filename );
            dataSections.AddRange( reader.GenerateManuallyFilledInfoSections() );
            dataSections.AddRange( reader.GenerateSystemSpecificDataSections() );

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
