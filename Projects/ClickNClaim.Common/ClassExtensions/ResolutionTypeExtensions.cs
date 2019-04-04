using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace ClickNClaim.Common
{
    public partial class ResolutionType
    {
        public bool IsSelected { get; set; }
        public string ResolutionComment { get; set; }
        public bool ShouldBeDisplayed {
            get
            {
                return true;
            }
        }

        public List<ProofFile> ProofFiles { get; set; }
        public bool IsDownloading { get; set; }
        public int Percent { get; set; }
    }
}
