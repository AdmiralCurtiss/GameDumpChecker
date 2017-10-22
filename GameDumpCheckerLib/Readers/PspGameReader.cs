using DiscUtils.Iso9660;
using GameDumpCheckerLib.PlayStation;
using System;
using System.Collections.Generic;
using System.IO;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace GameDumpCheckerLib.Readers {
    public class PspGameReader : IGameReader {
        private string Filename;

        public PspGameReader( string filename ) {
            this.Filename = filename;
        }

        public List<DataSection> GenerateManuallyFilledInfoSections() {
            return new List<DataSection>();
        }

        public List<DataSection> GenerateSystemSpecificDataSections() {
            List<DataSection> sections = new List<DataSection>();

            using ( Stream s = new FileStream( Filename, FileMode.Open ) )
            using ( CDReader iso = new CDReader( s, true ) ) {
                foreach ( string filename in iso.GetFiles( "", "PARAM.SFO", SearchOption.AllDirectories ) ) {
                    using ( Stream file = iso.OpenFile( filename, FileMode.Open ) ) {
                        try {
                            sections.Add( new Sfo( file, filename ).GetAsDataSection() );
                        } catch ( InvalidSfoException ex ) {
                            var list = new List<(string Key, string Value)>();
                            list.Add( (Key: "Invalid SFO", Value: ex.ToString() ) );
                            sections.Add( new DataSection( filename, list ) );
                        }
                    }
                }
            }

            return sections;
        }
    }
}
