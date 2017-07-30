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

        public List<DataSection> GenerateSystemSpecificDataSections() {
            List<DataSection> sections = new List<DataSection>();

            using ( Stream s = new FileStream( Filename, FileMode.Open ) )
            using ( CDReader iso = new CDReader( s, true ) ) {
                List<Sfo> sfos = new List<Sfo>();
                foreach ( string filename in iso.GetFiles( "", "PARAM.SFO", SearchOption.AllDirectories ) ) {
                    using ( Stream file = iso.OpenFile( filename, FileMode.Open ) ) {
                        sfos.Add( new Sfo( file, filename ) );
                    }
                }

                foreach ( Sfo sfo in sfos ) {
                    sections.Add( sfo.GetAsDataSection() );
                }
            }

            return sections;
        }
    }
}
