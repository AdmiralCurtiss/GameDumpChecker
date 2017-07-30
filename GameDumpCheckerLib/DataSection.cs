using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace GameDumpCheckerLib {
    public class DataSection {
        public string Name { get; private set; }
        public List<(string Key, string Value)> Data { get; private set; }

        public DataSection( string name, List<(string Key, string Value)> data ) {
            this.Name = name;
            this.Data = data;
        }
    }
}
