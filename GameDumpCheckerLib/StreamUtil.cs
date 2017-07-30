using System;
using System.Collections.Generic;
using System.IO;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace GameDumpCheckerLib {
    public static class StreamUtil {
        public static ulong ReadUInt64( this Stream s ) {
            ulong b1 = (ulong)s.ReadByte();
            ulong b2 = (ulong)s.ReadByte();
            ulong b3 = (ulong)s.ReadByte();
            ulong b4 = (ulong)s.ReadByte();
            ulong b5 = (ulong)s.ReadByte();
            ulong b6 = (ulong)s.ReadByte();
            ulong b7 = (ulong)s.ReadByte();
            ulong b8 = (ulong)s.ReadByte();

            return (ulong)( b8 << 56 | b7 << 48 | b6 << 40 | b5 << 32 | b4 << 24 | b3 << 16 | b2 << 8 | b1 );
        }
        public static ulong PeekUInt64( this Stream s ) {
            long pos = s.Position;
            ulong retval = s.ReadUInt64();
            s.Position = pos;
            return retval;
        }
        public static void WriteUInt64( this Stream s, ulong num ) {
            s.Write( BitConverter.GetBytes( num ), 0, 8 );
        }

        public static ulong ReadUInt56( this Stream s ) {
            ulong b1 = (ulong)s.ReadByte();
            ulong b2 = (ulong)s.ReadByte();
            ulong b3 = (ulong)s.ReadByte();
            ulong b4 = (ulong)s.ReadByte();
            ulong b5 = (ulong)s.ReadByte();
            ulong b6 = (ulong)s.ReadByte();
            ulong b7 = (ulong)s.ReadByte();

            return (ulong)( b7 << 48 | b6 << 40 | b5 << 32 | b4 << 24 | b3 << 16 | b2 << 8 | b1 );
        }
        public static ulong PeekUInt56( this Stream s ) {
            long pos = s.Position;
            ulong retval = s.ReadUInt56();
            s.Position = pos;
            return retval;
        }

        public static ulong ReadUInt48( this Stream s ) {
            ulong b1 = (ulong)s.ReadByte();
            ulong b2 = (ulong)s.ReadByte();
            ulong b3 = (ulong)s.ReadByte();
            ulong b4 = (ulong)s.ReadByte();
            ulong b5 = (ulong)s.ReadByte();
            ulong b6 = (ulong)s.ReadByte();

            return (ulong)( b6 << 40 | b5 << 32 | b4 << 24 | b3 << 16 | b2 << 8 | b1 );
        }
        public static ulong PeekUInt48( this Stream s ) {
            long pos = s.Position;
            ulong retval = s.ReadUInt48();
            s.Position = pos;
            return retval;
        }

        public static ulong ReadUInt40( this Stream s ) {
            ulong b1 = (ulong)s.ReadByte();
            ulong b2 = (ulong)s.ReadByte();
            ulong b3 = (ulong)s.ReadByte();
            ulong b4 = (ulong)s.ReadByte();
            ulong b5 = (ulong)s.ReadByte();

            return (ulong)( b5 << 32 | b4 << 24 | b3 << 16 | b2 << 8 | b1 );
        }
        public static ulong PeekUInt40( this Stream s ) {
            long pos = s.Position;
            ulong retval = s.ReadUInt40();
            s.Position = pos;
            return retval;
        }

        public static uint ReadUInt32( this Stream s ) {
            int b1 = s.ReadByte();
            int b2 = s.ReadByte();
            int b3 = s.ReadByte();
            int b4 = s.ReadByte();

            return (uint)( b4 << 24 | b3 << 16 | b2 << 8 | b1 );
        }
        public static uint PeekUInt32( this Stream s ) {
            long pos = s.Position;
            uint retval = s.ReadUInt32();
            s.Position = pos;
            return retval;
        }
        public static void WriteUInt32( this Stream s, uint num ) {
            s.Write( BitConverter.GetBytes( num ), 0, 4 );
        }

        public static uint ReadUInt24( this Stream s ) {
            int b1 = s.ReadByte();
            int b2 = s.ReadByte();
            int b3 = s.ReadByte();

            return (uint)( b3 << 16 | b2 << 8 | b1 );
        }
        public static uint PeekUInt24( this Stream s ) {
            long pos = s.Position;
            uint retval = s.ReadUInt24();
            s.Position = pos;
            return retval;
        }

        public static ushort ReadUInt16( this Stream s ) {
            int b1 = s.ReadByte();
            int b2 = s.ReadByte();

            return (ushort)( b2 << 8 | b1 );
        }
        public static ushort PeekUInt16( this Stream s ) {
            long pos = s.Position;
            ushort retval = s.ReadUInt16();
            s.Position = pos;
            return retval;
        }
        public static void WriteUInt16( this Stream s, ushort num ) {
            s.Write( BitConverter.GetBytes( num ), 0, 2 );
        }

        public static byte ReadUInt8( this Stream s ) {
            return Convert.ToByte( s.ReadByte() );
        }
        public static byte PeekUInt8( this Stream s ) {
            long pos = s.Position;
            byte retval = s.ReadUInt8();
            s.Position = pos;
            return retval;
        }

        public static void DiscardBytes( this Stream s, uint count ) {
            s.Position = s.Position + count;
        }

        public static void ReadAlign( this Stream s, long alignment ) {
            while ( s.Position % alignment != 0 ) {
                s.DiscardBytes( 1 );
            }
        }
        public static void WriteAlign( this Stream s, long alignment, byte paddingByte = 0 ) {
            while ( s.Position % alignment != 0 ) {
                s.WriteByte( paddingByte );
            }
        }

        public static void WriteString( this Stream s, Encoding encoding, string str, int count = 0, bool trim = false ) {
            byte[] chars = encoding.GetBytes( str );
            if ( !trim && count > 0 && count < chars.Length ) {
                throw new Exception( "String won't fit in provided space!" );
            }

            int i;
            for ( i = 0; i < chars.Length; ++i ) {
                s.WriteByte( chars[i] );
            }
            for ( ; i < count; ++i ) {
                s.WriteByte( 0 );
            }
        }

        public static string ReadAsciiNulltermFromLocationAndReset( this Stream s, long location ) {
            long pos = s.Position;
            s.Position = location;
            string str = s.ReadAsciiNullterm();
            s.Position = pos;
            return str;
        }
        public static string ReadAsciiNullterm( this Stream s ) {
            StringBuilder sb = new StringBuilder();
            int b = s.ReadByte();
            while ( b != 0 && b != -1 ) {
                sb.Append( (char)( b ) );
                b = s.ReadByte();
            }
            return sb.ToString();
        }
        public static string ReadAscii( this Stream s, int count ) {
            StringBuilder sb = new StringBuilder( count );
            int b;
            for ( int i = 0; i < count; ++i ) {
                b = s.ReadByte();
                sb.Append( (char)( b ) );
            }
            return sb.ToString();
        }
        public static void WriteAscii( this Stream s, string str, int count = 0, bool trim = false ) {
            WriteString( s, Encoding.ASCII, str, count, trim );
        }

        public static string ReadUTF8NulltermFromLocationAndReset( this Stream s, long location ) {
            long pos = s.Position;
            s.Position = location;
            string str = s.ReadUTF8Nullterm();
            s.Position = pos;
            return str;
        }
        public static string ReadUTF8Nullterm( this Stream s ) {
            List<byte> data = new List<byte>();
            int b = s.ReadByte();
            while ( b != 0 && b != -1 ) {
                data.Add( (byte)( b ) );
                b = s.ReadByte();
            }
            return Encoding.UTF8.GetString( data.ToArray() );
        }
        public static void WriteUTF8( this Stream s, string str, int count = 0, bool trim = false ) {
            WriteString( s, Encoding.UTF8, str, count, trim );
        }
        public static void WriteUTF8Nullterm( this Stream s, string str ) {
            WriteUTF8( s, str, 0, false );
            s.WriteByte( 0 );
        }

        public static string ReadUTF16Nullterm( this Stream s ) {
            StringBuilder sb = new StringBuilder();
            byte[] b = new byte[2];
            int b0 = s.ReadByte();
            int b1 = s.ReadByte();
            while ( !( b0 == 0 && b1 == 0 ) && b1 != -1 ) {
                b[0] = (byte)b0;
                b[1] = (byte)b1;
                sb.Append( Encoding.Unicode.GetString( b, 0, 2 ) );
                b0 = s.ReadByte();
                b1 = s.ReadByte();
            }
            return sb.ToString();
        }

        public static void Write( this Stream s, byte[] data ) {
            s.Write( data, 0, data.Length );
        }
    }
}
