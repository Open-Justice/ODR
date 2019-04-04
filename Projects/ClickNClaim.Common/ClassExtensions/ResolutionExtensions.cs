using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace ClickNClaim.Common
{
    public partial class Resolution
    {
        public List<ProofFile> ProofFiles { get; set; }
        public string Name { get; set; }
        //public bool IsDownloading { get; set; }
        //public int Percent { get; set; }
    }
}
