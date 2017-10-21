using System;
using System.Collections.Generic;
using System.IO;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace GameDumpCheckerLib.PlayStation {
    // SFO format info from http://www.psdevwiki.com/ps3/PARAM.SFO
    public class Sfo {
        string Path;

        uint Magic;
        uint Version;
        uint KeyTableStart;
        uint DataTableStart;
        uint TablesEntries;

        List<ISfoEntry> SfoEntries;

        public Sfo( Stream s, string path = null ) {
            Path = path;

            s.Position = 0;

            uint expectedMagic = 0x46535000;
            Magic = s.ReadUInt32();
            if ( Magic != expectedMagic ) {
                throw new InvalidSfoException( String.Format( "Invalid SFO header: Magic bytes are 0x{0:X8} instead of 0x{1:X8}.", Magic, expectedMagic ) );
            }

            Version = s.ReadUInt32();
            KeyTableStart = s.ReadUInt32();
            DataTableStart = s.ReadUInt32();
            TablesEntries = s.ReadUInt32();

            SfoEntries = new List<ISfoEntry>();
            for ( uint i = 0; i < TablesEntries; ++i ) {
                SfoEntries.Add( ParseSfoEntry( s, KeyTableStart, DataTableStart ) );
            }
        }

        public static ISfoEntry ParseSfoEntry( Stream s, uint keyTableStart, uint dataTableStart ) {
            ushort keyOffset = s.ReadUInt16();
            ushort dataFmt = s.ReadUInt16();
            uint dataLen = s.ReadUInt32();
            uint dataMaxLen = s.ReadUInt32();
            uint dataOffset = s.ReadUInt32();

            string key = s.ReadUTF8NulltermFromLocationAndReset( keyTableStart + keyOffset );
            ISfoEntry entry;
            switch ( dataFmt ) {
                case 0x0204:
                    entry = new SfoEntryUtf8Nullterm( s, key, dataTableStart, dataLen, dataMaxLen, dataOffset );
                    break;
                case 0x0404:
                    entry = new SfoEntryInt32( s, key, dataTableStart, dataLen, dataMaxLen, dataOffset );
                    break;
                default:
                    throw new InvalidSfoException( String.Format( "Unknown SFO data format '{0:x4}'", dataFmt ) );
            }

            return entry;
        }

        public string GetVersionAsString() {
            byte[] v = BitConverter.GetBytes( Version );
            string s = String.Join( ".", v );
            while ( s.EndsWith( ".0" ) ) {
                s = s.Substring( 0, s.Length - 2 );
            }
            return s;
        }

        public DataSection GetAsDataSection() {
            var data = new List<(string Key, string Value)>();

            data.Add( ("SFO Version", GetVersionAsString()) );
            foreach ( ISfoEntry e in SfoEntries ) {
                data.Add( (e.GetKey(), e.GetValueAsString()) );
            }

            return new DataSection( Path != null ? Path : "Unknown Path", data );
        }
    }

    public interface ISfoEntry {
        string GetKey();
        string GetValueAsString();
    }

    public class SfoEntryUtf8Nullterm : ISfoEntry {
        string Key;
        string Value;

        public SfoEntryUtf8Nullterm( Stream s, string key, uint dataTableStart, uint dataLen, uint dataMaxLen, uint dataOffset ) {
            Key = key;
            Value = s.ReadUTF8NulltermFromLocationAndReset( dataTableStart + dataOffset );
        }

        public string GetKey() {
            return Key;
        }

        public string GetValueAsString() {
            return Value;
        }

        public override string ToString() {
            return GetKey() + ": " + GetValueAsString();
        }
    }

    public class SfoEntryInt32 : ISfoEntry {
        string Key;
        uint Value;

        public SfoEntryInt32( Stream s, string key, uint dataTableStart, uint dataLen, uint dataMaxLen, uint dataOffset ) {
            if ( dataLen != 4 || dataMaxLen != 4 ) {
                throw new InvalidSfoException( "Invalid SFO data length for Int32." );
            }

            Key = key;
            long pos = s.Position;
            s.Position = dataTableStart + dataOffset;
            Value = s.ReadUInt32();
            s.Position = pos;
        }

        public string GetKey() {
            return Key;
        }

        public string GetValueAsString() {
            return Value.ToString();
        }

        public override string ToString() {
            return GetKey() + ": " + GetValueAsString();
        }
    }

    public class InvalidSfoException : Exception {
        public InvalidSfoException() : base() { }
        public InvalidSfoException( string message ) : base( message ) { }
        public InvalidSfoException( string message, Exception inner ) : base( message, inner ) { }
    }
}
