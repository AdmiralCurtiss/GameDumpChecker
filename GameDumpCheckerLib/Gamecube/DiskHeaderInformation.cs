using System;
using System.Collections.Generic;
using System.IO;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace GameDumpCheckerLib.Gamecube {
    public class DiskHeaderInformation {
        // http://hitmen.c02.at/files/yagcd/yagcd/chap13.html#sec13.2

        uint DebugMonitorSize;
        uint SimulatedMemorySize;
        uint ArgumentOffset;
        uint DebugFlag;
        uint TrackLocation;
        uint TrackSize;
        uint CountryCode;
        uint Unknown;

        public DiskHeaderInformation( Stream stream ) {
            stream.Position = 0;
            DebugMonitorSize = stream.ReadUInt32().SwapEndian();
            SimulatedMemorySize = stream.ReadUInt32().SwapEndian();
            ArgumentOffset = stream.ReadUInt32().SwapEndian();
            DebugFlag = stream.ReadUInt32().SwapEndian();
            TrackLocation = stream.ReadUInt32().SwapEndian();
            TrackSize = stream.ReadUInt32().SwapEndian();
            CountryCode = stream.ReadUInt32().SwapEndian();
            Unknown = stream.ReadUInt32().SwapEndian();
        }
    }
}
