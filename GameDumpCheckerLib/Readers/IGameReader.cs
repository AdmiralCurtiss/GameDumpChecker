using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace GameDumpCheckerLib.Readers {
    public interface IGameReader {
        List<DataSection> GenerateManuallyFilledInfoSections();
        List<DataSection> GenerateSystemSpecificDataSections();
    }
}
