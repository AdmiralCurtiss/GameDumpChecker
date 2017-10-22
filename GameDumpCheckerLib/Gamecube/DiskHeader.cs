using System;
using System.Collections.Generic;
using System.IO;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace GameDumpCheckerLib.Gamecube {
    public class DiskHeader {
        // http://hitmen.c02.at/files/yagcd/yagcd/chap13.html#sec13.1

        public String GameCode { get; private set; }
        public String MakerCode { get; private set; }
        public byte DiskID { get; private set; }
        public byte Version { get; private set; }
        public byte AudioStreaming { get; private set; }
        public byte StreamBufferSize { get; private set; }

        public String GameName { get; private set; }
        public uint OffsetOfDebugMonitor { get; private set; }
        public uint AddrToLoadDebugMonitor { get; private set; }
        public uint OffsetOfMainExecutableDol { get; private set; }
        public uint OffsetOfFst { get; private set; }
        public uint SizeOfFst { get; private set; }
        public uint MaxSizeOfFst { get; private set; }
        public uint UserPosition { get; private set; }
        public uint UserLength { get; private set; }
        public uint Unknown { get; private set; }

        public DiskHeader( Stream stream ) {
            stream.Position = 0;
            GameCode = stream.ReadAscii( 4 );
            MakerCode = stream.ReadAscii( 2 );
            DiskID = (byte)stream.ReadByte();
            Version = (byte)stream.ReadByte();
            AudioStreaming = (byte)stream.ReadByte();
            StreamBufferSize = (byte)stream.ReadByte();

            stream.Position = 0x1C;
            uint expectedMagic = 0xC2339F3D;
            uint magic = stream.ReadUInt32().SwapEndian();
            if ( expectedMagic != magic ) {
                throw new Exception( "Invalid magic." );
            }

            stream.Position = 0x20;
            GameName = stream.ReadAscii( 0x3e0 );

            stream.Position = 0x400;
            OffsetOfDebugMonitor = stream.ReadUInt32().SwapEndian();
            AddrToLoadDebugMonitor = stream.ReadUInt32().SwapEndian();

            stream.Position = 0x420;
            OffsetOfMainExecutableDol = stream.ReadUInt32().SwapEndian();
            OffsetOfFst = stream.ReadUInt32().SwapEndian();
            SizeOfFst = stream.ReadUInt32().SwapEndian();
            MaxSizeOfFst = stream.ReadUInt32().SwapEndian();
            UserPosition = stream.ReadUInt32().SwapEndian();
            UserLength = stream.ReadUInt32().SwapEndian();
            Unknown = stream.ReadUInt32().SwapEndian();
        }
    }
}
