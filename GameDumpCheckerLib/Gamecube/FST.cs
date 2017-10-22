using System;
using System.Collections.Generic;
using System.IO;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace GameDumpCheckerLib.Gamecube {
    // http://hitmen.c02.at/files/yagcd/yagcd/chap13.html#sec13.4

    public class Fst {
        public FstRootDirectoryEntry Root { get; private set; }

        public Fst( Stream stream ) {
            stream.Position = 0;
            Root = new FstRootDirectoryEntry( stream );
        }

        internal static (uint entriesParsed, FstEntry entry) ReadNextFstEntry( Stream stream, uint offset, uint filenameTableOffset ) {
            int type = stream.PeekUInt8();
            switch ( type ) {
                case 0:
                    return (1, new FstFileEntry( stream, filenameTableOffset ));
                case 1:
                    return FstDirectoryEntry.ReadNextFstDirectoryEntry( stream, offset, filenameTableOffset );
                default:
                    throw new Exception( "Invalid FST type." );
            }
        }
    }

    public interface FstEntry {
        String Filename { get; }

        FstEntry GetChild( String filename );
        List<FstEntry> GetChildren();
    }

    public class FstRootDirectoryEntry : FstEntry {
        public byte Flags { get; private set; }
        public uint FilenameOffset { get; private set; }
        public uint ParentOffset { get; private set; }
        public uint NumEntries { get; private set; }
        public String Filename { get => ""; }

        private List<FstEntry> Children;

        public FstRootDirectoryEntry( Stream stream ) {
            Flags = (byte)stream.ReadByte();
            FilenameOffset = stream.ReadUInt24().SwapEndian24();
            ParentOffset = stream.ReadUInt32().SwapEndian();
            NumEntries = stream.ReadUInt32().SwapEndian();

            uint filenameTableOffset = NumEntries * 0xC;

            Children = new List<FstEntry>();
            uint offset = 1;
            while ( offset < NumEntries ) {
                var (entriesParsed, entry) = Fst.ReadNextFstEntry( stream, offset, filenameTableOffset );
                offset += entriesParsed;
                Children.Add( entry );
            }
        }

        public FstEntry GetChild( string filename ) {
            foreach ( FstEntry entry in Children ) {
                if ( entry.Filename == filename ) {
                    return entry;
                }
            }
            return null;
        }

        public List<FstEntry> GetChildren() {
            return Children;
        }

        public override string ToString() {
            return "Root; " + NumEntries + " entries";
        }
    }

    public class FstDirectoryEntry : FstEntry {
        public byte Flags { get; private set; }
        public uint FilenameOffset { get; private set; }
        public uint ParentOffset { get; private set; }
        public uint NextOffset { get; private set; }
        public String Filename { get; private set; }

        private List<FstEntry> Children;

        public FstDirectoryEntry( byte flags, uint filenameOffset, uint parentOffset, uint nextOffset, string filename, List<FstEntry> children ) {
            Flags = flags;
            FilenameOffset = filenameOffset;
            ParentOffset = parentOffset;
            NextOffset = nextOffset;
            Filename = filename;
            Children = children;
        }

        public static (uint entriesParsed, FstDirectoryEntry entry) ReadNextFstDirectoryEntry( Stream stream, uint offset, uint filenameTableOffset ) {
            byte flags = (byte)stream.ReadByte();
            uint filenameOffset = stream.ReadUInt24().SwapEndian24();
            uint parentOffset = stream.ReadUInt32().SwapEndian();
            uint nextOffset = stream.ReadUInt32().SwapEndian();
            String filename = stream.ReadAsciiNulltermFromLocationAndReset( filenameTableOffset + filenameOffset );

            List<FstEntry> entries = new List<FstEntry>();
            uint offs = offset + 1;
            uint totalParsed = 1;
            while ( offs < nextOffset ) {
                var (numParsed, entry) = Fst.ReadNextFstEntry( stream, offs, filenameTableOffset );
                offs += numParsed;
                totalParsed += numParsed;
                entries.Add( entry );
            }

            return (totalParsed, new FstDirectoryEntry( flags, filenameOffset, parentOffset, nextOffset, filename, entries ));
        }

        public FstEntry GetChild( string filename ) {
            foreach ( FstEntry entry in Children ) {
                if ( entry.Filename == filename ) {
                    return entry;
                }
            }
            return null;
        }

        public List<FstEntry> GetChildren() {
            return Children;
        }

        public override string ToString() {
            return Filename;
        }
    }

    public class FstFileEntry : FstEntry {
        public byte Flags { get; private set; }
        public uint FilenameOffset { get; private set; }
        public uint FileOffset { get; private set; }
        public uint FileLength { get; private set; }
        public String Filename { get; private set; }

        public FstFileEntry( Stream stream, uint filenameTableOffset ) {
            Flags = (byte)stream.ReadByte();
            FilenameOffset = stream.ReadUInt24().SwapEndian24();
            FileOffset = stream.ReadUInt32().SwapEndian();
            FileLength = stream.ReadUInt32().SwapEndian();
            Filename = stream.ReadAsciiNulltermFromLocationAndReset( filenameTableOffset + FilenameOffset );
        }

        public FstEntry GetChild( string filename ) {
            return null;
        }

        public List<FstEntry> GetChildren() {
            return new List<FstEntry>();
        }

        public override string ToString() {
            return Filename;
        }
    }
}
