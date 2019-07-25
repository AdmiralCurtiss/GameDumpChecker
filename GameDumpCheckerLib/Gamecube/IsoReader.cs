using System;
using System.Collections.Generic;
using System.IO;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using HyoutaPluginBase;
using HyoutaUtils.Streams;

namespace GameDumpCheckerLib.Gamecube {
    public class IsoReader {
        public DiskHeader DiskHeader { get; private set; }
        public DiskHeaderInformation DiskHeaderInformation { get; private set; }
        public Fst Fst { get; private set; }

        public IsoReader( DuplicatableStream stream ) {
            DiskHeader = new DiskHeader( new PartialStream( stream, 0, 0x0440 ) );
            DiskHeaderInformation = new DiskHeaderInformation( new PartialStream( stream, 0x0440, 0x2000 ) );
            Fst = new Fst( new PartialStream( stream, DiskHeader.OffsetOfFst, DiskHeader.SizeOfFst ) );
        }
    }
}
