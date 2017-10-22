using GameDumpCheckerLib.Gamecube;
using System;
using System.Collections.Generic;
using System.IO;

namespace GameDumpCheckerLib.Readers {
    public class GamecubeGameReader : IGameReader {
        private string Filename;
        private IsoReader Iso;
        private List<(string path, Banner banner)> Banners;

        public GamecubeGameReader( string filename ) {
            Filename = filename;
            using ( var stream = new FileStream( Filename, FileMode.Open ) ) {
                Iso = new IsoReader( stream );
                Banners = new List<(string path, Banner banner)>();
                ParseBanners( Banners, Iso.Fst.Root.GetChildren(), stream, Iso );
            }
        }

        private static char LastChar( string s ) {
            if ( s.Length > 0 ) {
                return s[s.Length - 1];
            } else {
                return '\0';
            }
        }

        public List<DataSection> GenerateManuallyFilledInfoSections() {
            var list = new List<DataSection>();
            var info = new List<(string Key, string Value)>();
            var bannerMeta = FindMostLikelyBannerMetadata( Banners );
            info.Add( ("Game Title", bannerMeta != null ? bannerMeta.GameNameLong.TrimEnd( '\0' ) : "Unknown") );

            string region;
            string languages;
            string serial;
            switch ( LastChar( Iso.DiskHeader.GameCode ) ) {
                case 'J':
                    region = "Japan";
                    languages = "Japanese";
                    serial = "DL-DOL-" + Iso.DiskHeader.GameCode + "-JPN";
                    break;
                case 'E':
                    region = "USA";
                    languages = "English";
                    serial = "DL-DOL-" + Iso.DiskHeader.GameCode + "-USA";
                    break;
                case 'P':
                    region = "Europe";
                    languages = "EFIGS + Dutch";
                    serial = "DL-DOL-" + Iso.DiskHeader.GameCode + "-EUR";
                    break;
                default:
                    region = "Unknown";
                    languages = "English";
                    serial = "DL-DOL-" + Iso.DiskHeader.GameCode + "-XXX";
                    break;
            }

            info.Add( ("Region", region) );
            info.Add( ("Languages", languages) );
            info.Add( ("Language Select", "Uses System Language / In-Game Language Select / Unknown") );
            info.Add( ("Disc Serial", serial) );
            info.Add( ("Edition", "Original / Unknown") );
            info.Add( ("Case Barcode", "Unknown") );
            info.Add( ("Case Serial", "Unknown") );
            info.Add( ("Mastering Code (back)", "Unknown") );
            info.Add( ("Mastering SID Code (back)", "Unknown / IFPI LL38 / IFPI LQ12") );
            info.Add( ("Mould SID Code (back)", "Unknown / IFPI XXXX") );
            info.Add( ("Additional Mould Text (back)", "Unknown / None / MADE IN JAPAN") );
            info.Add( ("Mould SID Code (front)", "Unknown / IFPI XXXX") );
            info.Add( ("Additional Mould Text (front)", "Unknown / None / MADE IN JAPAN") );
            list.Add( new DataSection( "Manually Filled Info", info ) );
            return list;
        }

        private static BannerMetadata FindMostLikelyBannerMetadata( List<(string path, Banner banner)> banners ) {
            foreach ( var b in banners ) {
                if ( b.path == "/opening.bnr" && b.banner.Metadata.Count > 0 ) {
                    return b.banner.Metadata[0];
                }
            }
            foreach ( var b in banners ) {
                if ( b.banner.Metadata.Count > 0 ) {
                    return b.banner.Metadata[0];
                }
            }
            return null;
        }

        public List<DataSection> GenerateSystemSpecificDataSections() {
            List<DataSection> sections = new List<DataSection>();

            {
                var gameData = new List<(string Key, string Value)>();
                gameData.Add( ("Game Code", Iso.DiskHeader.GameCode) );
                gameData.Add( ("Maker Code", Iso.DiskHeader.MakerCode) );
                gameData.Add( ("Disc ID", Iso.DiskHeader.DiskID.ToString()) );
                gameData.Add( ("Version", string.Format( "1.{0:D2}", Iso.DiskHeader.Version )) );
                gameData.Add( ("Game Name", Iso.DiskHeader.GameName.TrimEnd( '\0' )) );
                sections.Add( new DataSection( "Game Data", gameData ) );
            }

            foreach ( var bannerData in Banners ) {
                var banner = bannerData.banner;
                var gameData = new List<(string Key, string Value)>();
                gameData.Add( ("Version", banner.Magic) );
                gameData.Add( ("Metadata Count", banner.Metadata.Count.ToString()) );
                for ( int i = 0; i < banner.Metadata.Count; ++i ) {
                    BannerMetadata meta = banner.Metadata[i];
                    String id = "(" + ( i + 1 ) + ")";
                    gameData.Add( ("Game Name " + id, meta.GameName.TrimEnd( '\0' )) );
                    gameData.Add( ("Company " + id, meta.Publisher.TrimEnd( '\0' )) );
                    gameData.Add( ("Game Name Long " + id, meta.GameNameLong.TrimEnd( '\0' )) );
                    gameData.Add( ("Company Long " + id, meta.PublisherLong.TrimEnd( '\0' )) );
                    gameData.Add( ("Description " + id, meta.Description.TrimEnd( '\0' )) );
                }
                sections.Add( new DataSection( "Banner at " + bannerData.path, gameData ) );
            }

            return sections;
        }

        private static void ParseBanners( List<(string path, Banner banner)> banners, List<FstEntry> entries, Stream stream, IsoReader iso, string path = "" ) {
            foreach ( FstEntry entry in entries ) {
                bool possiblyBanner = entry is FstFileEntry && entry.Filename.EndsWith( ".bnr" );
                if ( possiblyBanner ) {
                    try {
                        var bannerEntry = entry as FstFileEntry;
                        var banner = new Banner( new PartialStream( stream, bannerEntry.FileOffset, bannerEntry.FileLength ), iso.DiskHeader.GameCode.EndsWith( "J" ) );
                        banners.Add( (path + "/" + entry.Filename, banner) );
                    } catch ( Exception ) {
                        // probably not a banner then
                    }
                }

                if ( !( entry is FstFileEntry ) ) {
                    ParseBanners( banners, entry.GetChildren(), stream, iso, path + "/" + entry.Filename );
                }
            }
        }
    }
}
