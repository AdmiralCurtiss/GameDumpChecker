using GameDumpCheckerLib.Gamecube;
using System;
using System.Collections.Generic;
using System.IO;

namespace GameDumpCheckerLib.Readers {
    public class GamecubeGameReader : IGameReader {
        private string Filename;

        public GamecubeGameReader( string filename ) {
            Filename = filename;
        }

        public List<DataSection> GenerateSystemSpecificDataSections() {
            List<DataSection> sections = new List<DataSection>();


            using ( var stream = new FileStream( Filename, FileMode.Open ) ) {
                var iso = new IsoReader( stream );

                {
                    var gameData = new List<(string Key, string Value)>();
                    gameData.Add( ("Game Code", iso.DiskHeader.GameCode) );
                    gameData.Add( ("Maker Code", iso.DiskHeader.MakerCode) );
                    gameData.Add( ("Disc ID", iso.DiskHeader.DiskID.ToString()) );
                    gameData.Add( ("Version", String.Format( "1.{0:D2}", iso.DiskHeader.Version )) );
                    gameData.Add( ("Game Name", iso.DiskHeader.GameName.TrimEnd( '\0' )) );
                    sections.Add( new DataSection( "Game Data", gameData ) );
                }

                ParseBanners( sections, iso.Fst.Root.GetChildren(), stream, iso );
            }

            return sections;
        }

        private static void ParseBanners( List<DataSection> sections, List<FstEntry> entries, Stream stream, IsoReader iso, string path = "" ) {
            foreach ( FstEntry entry in entries ) {
                bool possiblyBanner = entry is FstFileEntry && entry.Filename.EndsWith( ".bnr" );
                if ( possiblyBanner ) {
                    try {
                        var bannerEntry = entry as FstFileEntry;
                        var banner = new Banner( new PartialStream( stream, bannerEntry.FileOffset, bannerEntry.FileLength ), iso.DiskHeader.GameCode.EndsWith( "J" ) );

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
                        sections.Add( new DataSection( "Banner at " + path + "/" + entry.Filename, gameData ) );
                    } catch ( Exception ) {
                        // probably not a banner then
                    }
                }

                if ( !( entry is FstFileEntry ) ) {
                    ParseBanners( sections, entry.GetChildren(), stream, iso, path + "/" + entry.Filename );
                }
            }
        }
    }
}
