using System;
using System.Collections.Generic;
using System.Drawing;
using System.IO;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using HyoutaUtils;

namespace GameDumpCheckerLib.N3DS {
	public class SmdhReader {
		// https://www.3dbrew.org/wiki/SMDH
		public ushort Version;
		public SmdhTitleStruct[] TitleStructs;
		public byte[] Ratings;
		public uint RegionLockout;
		public byte[] MatchMakerIds;
		public uint Flags;
		public ushort EulaVersion;
		public uint OptimalAnimationDefaultFrame;
		public uint StreetPassId;
		public IconGraphics SmallIcon;
		public IconGraphics LargeIcon;

		public SmdhReader( Stream stream ) {
			if ( stream.ReadAscii( 4 ) != "SMDH" ) {
				throw new Exception( "Wrong magic." );
			}

			Version = stream.ReadUInt16();
			stream.DiscardBytes( 2 );
			TitleStructs = new SmdhTitleStruct[16];
			for ( int i = 0; i < 16; ++i ) {
				TitleStructs[i] = new SmdhTitleStruct( stream );
			}
			Ratings = stream.ReadBytes( 0x10 );
			RegionLockout = stream.ReadUInt32();
			MatchMakerIds = stream.ReadBytes( 0xC );
			Flags = stream.ReadUInt32();
			EulaVersion = stream.ReadUInt16();
			stream.DiscardBytes( 2 );
			OptimalAnimationDefaultFrame = stream.ReadUInt32();
			StreetPassId = stream.ReadUInt32();
			stream.DiscardBytes( 8 );
			SmallIcon = new IconGraphics( stream, 24, 24 );
			LargeIcon = new IconGraphics( stream, 48, 48 );
		}
	}

	public class SmdhTitleStruct {
		public string ShortDescription;
		public string LongDescription;
		public string Publisher;

		public SmdhTitleStruct( Stream stream ) {
			ShortDescription = new MemoryStream( stream.ReadBytes( 0x80 ) ).ReadUTF16Nullterm();
			LongDescription = new MemoryStream( stream.ReadBytes( 0x100 ) ).ReadUTF16Nullterm();
			Publisher = new MemoryStream( stream.ReadBytes( 0x80 ) ).ReadUTF16Nullterm();
		}
	}

	public class IconGraphics {
		public byte[] Data;
		public int Width;
		public int Height;

		public IconGraphics( Stream stream, int width, int height ) {
			Data = stream.ReadBytes( width * height * 2 );
			Width = width;
			Height = height;
		}

		public Bitmap ConvertToBitmap() {
			int x = 0;
			int y = 0;
			Color[] tile = new Color[16];
			Bitmap bmp = new Bitmap( Width, Height );
			for ( int i = 0; i < Data.Length; i += 32 * 4 ) {
				for ( int k = 0; k < 4; ++k ) {
					for ( int j = 0; j < 16; ++j ) {
						tile[j] = ColorFromRGB565( BitConverter.ToUInt16( Data, ( i + k * 32 ) + j * 2 ) );
					}

					int lx = k % 2;
					int ly = k / 2;
					CopyTile( bmp, x + lx * 4, y + ly * 4, tile );
				}
				x += 8;
				if ( x >= Width ) {
					x = 0;
					y += 8;
				}
			}
			return bmp;
		}

		private static void CopyTile( Bitmap bmp, int x, int y, Color[] tile ) {
			bmp.SetPixel( x + 0, y + 0, tile[0] );
			bmp.SetPixel( x + 1, y + 0, tile[1] );
			bmp.SetPixel( x + 0, y + 1, tile[2] );
			bmp.SetPixel( x + 1, y + 1, tile[3] );
			bmp.SetPixel( x + 2, y + 0, tile[4] );
			bmp.SetPixel( x + 3, y + 0, tile[5] );
			bmp.SetPixel( x + 2, y + 1, tile[6] );
			bmp.SetPixel( x + 3, y + 1, tile[7] );
			bmp.SetPixel( x + 0, y + 2, tile[8] );
			bmp.SetPixel( x + 1, y + 2, tile[9] );
			bmp.SetPixel( x + 0, y + 3, tile[10] );
			bmp.SetPixel( x + 1, y + 3, tile[11] );
			bmp.SetPixel( x + 2, y + 2, tile[12] );
			bmp.SetPixel( x + 3, y + 2, tile[13] );
			bmp.SetPixel( x + 2, y + 3, tile[14] );
			bmp.SetPixel( x + 3, y + 3, tile[15] );
		}

		private static Color ColorFromRGB565( ushort color ) {
			int b = (int)( ( ( color & 0x001F ) ) << 3 );
			int g = (int)( ( ( color & 0x07E0 ) >> 5 ) << 2 );
			int r = (int)( ( ( color & 0xF800 ) >> 11 ) << 3 );
			return Color.FromArgb( r, g, b );
		}
	}
}
