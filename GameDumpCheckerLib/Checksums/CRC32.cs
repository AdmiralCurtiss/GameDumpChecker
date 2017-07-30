using System;
using System.Collections.Generic;
using System.IO;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace GameDumpCheckerLib.Checksums {
    public static class CRC32 {
        public static byte[] CalculateCRC32( Stream s ) {
            long pos = s.Position;
            s.Position = 0;
            byte[] crc32 = CalculateCRC32( s, s.Length );
            s.Position = pos;
            return crc32;
        }

        private static byte[] CalculateCRC32( Stream s, long count ) {
            uint crc32 = crc_init();
            crc32 = crc_update( crc32, s, (ulong)count );
            crc32 = crc_finalize( crc32 );
            return BitConverter.GetBytes( crc32 ).Reverse().ToArray();
        }

        /**
         * Reflect all bits of a \a data word of \a data_len bytes.
         *
         * \param data         The data word to be reflected.
         * \param data_len     The width of \a data expressed in number of bits.
         * \return             The reflected data.
         *****************************************************************************/
        private static uint crc_reflect( uint data, ulong data_len ) {
            uint i;
            uint ret;

            ret = data & 0x01;
            for ( i = 1; i < data_len; i++ ) {
                data >>= 1;
                ret = ( ret << 1 ) | ( data & 0x01 );
            }
            return ret;
        }

        /**
         * Calculate the initial crc value.
         *
         * \return     The initial crc value.
         *****************************************************************************/
        private static uint crc_init() {
            return 0xffffffff;
        }

        /**
         * Update the crc value with new data.
         *
         * \param crc      The current crc value.
         * \param data     Pointer to a buffer of \a data_len bytes.
         * \param data_len Number of bytes in the \a data buffer.
         * \return         The updated crc value.
         *****************************************************************************/
        private static uint crc_update( uint crc, Stream data, ulong data_len ) {
            Stream d = data;
            uint i;
            bool bit;
            byte c;

            while ( ( data_len-- ) != 0 ) {
                c = (byte)d.ReadByte();
                for ( i = 0x01; ( i & 0xff ) != 0; i <<= 1 ) {
                    bit = ( crc & 0x80000000 ) != 0;
                    if ( ( c & i ) != 0 ) {
                        bit = !bit;
                    }
                    crc <<= 1;
                    if ( bit ) {
                        crc ^= 0x04c11db7;
                    }
                }
                crc &= 0xffffffff;
            }
            return crc & 0xffffffff;
        }

        /**
         * Calculate the final crc value.
         *
         * \param crc  The current crc value.
         * \return     The final crc value.
         *****************************************************************************/
        private static uint crc_finalize( uint crc ) {
            return crc_reflect( crc, 32 ) ^ 0xffffffff;
        }

    }
}
