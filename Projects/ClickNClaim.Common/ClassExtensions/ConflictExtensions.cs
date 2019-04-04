using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace ClickNClaim.Common
{
    public partial class Conflict
    {
        public int MessageCount { get; set; }

        public Conflict(Conflict c)
        {
            this.Id = c.Id;
            
        }


    }
}
