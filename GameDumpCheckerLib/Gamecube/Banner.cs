using System;
using System.Collections.Generic;
using System.IO;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace GameDumpCheckerLib.Gamecube {
    public class Banner {
        // http://hitmen.c02.at/files/yagcd/yagcd/chap14.html#sec14.1
        public List<BannerMetadata> Metadata { get; private set; }

        public Banner( Stream stream, bool parseAsShiftJis ) {
            stream.Position = 0;
            String magic = stream.ReadAscii( 4 );

            if ( magic != "BNR1" && magic != "BNR2" ) {
                throw new Exception( "Invalid magic." );
            }

            // TODO: Parse banner image?

            stream.Position = 0x1820;
            Metadata = new List<BannerMetadata>();
            while ( stream.Position < stream.Length ) {
                Metadata.Add( new BannerMetadata( stream, parseAsShiftJis ) );
            }
        }
    }

    public class BannerMetadata {
        public String GameName { get; private set; }
        public String Publisher { get; private set; }
        public String GameNameLong { get; private set; }
        public String PublisherLong { get; private set; }
        public String Description { get; private set; }

        public BannerMetadata( Stream stream, bool parseAsShiftJis ) {
            if ( parseAsShiftJis ) {
                GameName = stream.ReadShiftJis( 0x20 );
                Publisher = stream.ReadShiftJis( 0x20 );
                GameNameLong = stream.ReadShiftJis( 0x40 );
                PublisherLong = stream.ReadShiftJis( 0x40 );
                Description = stream.ReadShiftJis( 0x80 );
            } else {
                GameName = stream.ReadAscii( 0x20 );
                Publisher = stream.ReadAscii( 0x20 );
                GameNameLong = stream.ReadAscii( 0x40 );
                PublisherLong = stream.ReadAscii( 0x40 );
                Description = stream.ReadAscii( 0x80 );
            }
        }
    }
}
