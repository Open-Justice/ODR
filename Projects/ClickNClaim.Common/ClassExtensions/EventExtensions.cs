using ClickNClaim.Common.Enums;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace ClickNClaim.Common
{
    public partial class Event
    {
        public int Percent { get; set; }
        public bool IsDownloading { get; set; }
    }

    public partial class DefaultEvent
    {
        public int Percent { get; set; }
        public bool IsDownloading { get; set; }

        public int IdEvent { get; set; }

        public List<ProofFile> Files { get; set; }

        public DateTime Date { get; set; }

        public string Description { get; set; }

        public DefaultEvent()
        {
            Files = new List<ProofFile>();
                
        }

        public string SubName { get; set; }

    }


}
