using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace GameDumpCheckerLib {
    public static class EndianUtil {
        // baffling thing seen in Gust textures, 0xXY <-> 0xYX
        public static byte SwapEndian4Bits( this byte x ) {
            return (byte)( ( ( x & 0x0F ) << 4 ) | ( ( x & 0xF0 ) >> 4 ) );
        }
        public static Int16 SwapEndian( this Int16 x ) {
            return (Int16)SwapEndian( (UInt16)x );
        }
        public static UInt16 SwapEndian( this UInt16 x ) {
            return x = (UInt16)
                       ( ( x << 8 ) |
                        ( x >> 8 ) );
        }

        public static Int32 SwapEndian24( this Int32 x ) {
            return (Int32)SwapEndian24( (UInt32)x );
        }
        public static UInt32 SwapEndian24( this UInt32 x ) {
            return ( ( x & 0x000000FF ) << 16 ) |
                   ( ( x & 0x0000FF00 ) ) |
                   ( ( x & 0x00FF0000 ) >> 16 );
        }

        public static Int32 SwapEndian( this Int32 x ) {
            return (Int32)SwapEndian( (UInt32)x );
        }
        public static UInt32 SwapEndian( this UInt32 x ) {
            return x = ( x << 24 ) |
                      ( ( x << 8 ) & 0x00FF0000 ) |
                      ( ( x >> 8 ) & 0x0000FF00 ) |
                       ( x >> 24 );
        }

        public static Int64 SwapEndian( this Int64 x ) {
            return (Int64)SwapEndian( (UInt64)x );
        }
        public static UInt64 SwapEndian( this UInt64 x ) {
            return x = ( x << 56 ) |
                        ( ( x << 40 ) & 0x00FF000000000000 ) |
                        ( ( x << 24 ) & 0x0000FF0000000000 ) |
                        ( ( x << 8 ) & 0x000000FF00000000 ) |
                        ( ( x >> 8 ) & 0x00000000FF000000 ) |
                        ( ( x >> 24 ) & 0x0000000000FF0000 ) |
                        ( ( x >> 40 ) & 0x000000000000FF00 ) |
                         ( x >> 56 );
        }

        public enum Endianness { LittleEndian, BigEndian }
        // man why the hell can you not constraint generic parameters to integers?
        public static Int16 ToEndian( this Int16 x, Endianness endian ) {
            return (Int16)ToEndian( (UInt16)x, endian );
        }
        public static UInt16 ToEndian( this UInt16 x, Endianness endian ) {
            switch ( endian ) {
                case Endianness.LittleEndian:
                    if ( BitConverter.IsLittleEndian ) { return x; } else { return x.SwapEndian(); }
                case Endianness.BigEndian:
                    if ( BitConverter.IsLittleEndian ) { return x.SwapEndian(); } else { return x; }
                default:
                    throw new Exception( "Invalid Endianness" );
            }
        }
        public static Int32 ToEndian( this Int32 x, Endianness endian ) {
            return (Int32)ToEndian( (UInt32)x, endian );
        }
        public static UInt32 ToEndian( this UInt32 x, Endianness endian ) {
            switch ( endian ) {
                case Endianness.LittleEndian:
                    if ( BitConverter.IsLittleEndian ) { return x; } else { return x.SwapEndian(); }
                case Endianness.BigEndian:
                    if ( BitConverter.IsLittleEndian ) { return x.SwapEndian(); } else { return x; }
                default:
                    throw new Exception( "Invalid Endianness" );
            }
        }
        public static Int64 ToEndian( this Int64 x, Endianness endian ) {
            return (Int64)ToEndian( (UInt64)x, endian );
        }
        public static UInt64 ToEndian( this UInt64 x, Endianness endian ) {
            switch ( endian ) {
                case Endianness.LittleEndian:
                    if ( BitConverter.IsLittleEndian ) { return x; } else { return x.SwapEndian(); }
                case Endianness.BigEndian:
                    if ( BitConverter.IsLittleEndian ) { return x.SwapEndian(); } else { return x; }
                default:
                    throw new Exception( "Invalid Endianness" );
            }
        }

        // honestly I'm not sure if it makes sense to have different To and From functions
        // since all cases I can think of result in the same thing, but better be safe than sorry,
        // and it also gives some information if we're reading in or writing out data
        public static Int16 FromEndian( this Int16 x, Endianness endian ) {
            return ToEndian( x, endian );
        }
        public static UInt16 FromEndian( this UInt16 x, Endianness endian ) {
            return ToEndian( x, endian );
        }
        public static Int32 FromEndian( this Int32 x, Endianness endian ) {
            return ToEndian( x, endian );
        }
        public static UInt32 FromEndian( this UInt32 x, Endianness endian ) {
            return ToEndian( x, endian );
        }
        public static Int64 FromEndian( this Int64 x, Endianness endian ) {
            return ToEndian( x, endian );
        }
        public static UInt64 FromEndian( this UInt64 x, Endianness endian ) {
            return ToEndian( x, endian );
        }
    }
}
