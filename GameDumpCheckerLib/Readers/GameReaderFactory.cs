using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace GameDumpCheckerLib.Readers {
    public static class GameReaderFactory {
        public static IGameReader ConstructFromType( FileType type, string filename ) {
            switch ( type ) {
                case FileType.PSP:
                    return new PspGameReader( filename );
                case FileType.GCN:
                    return new GamecubeGameReader( filename );
                case FileType.N3DS:
                    return new N3DSGameReader( filename );
            }

            throw new Exception( "Invalid FileType: " + type );
        }
    }
}
