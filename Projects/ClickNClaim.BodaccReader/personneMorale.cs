using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace ClickNClaim.BodaccReader
{
    public class personneMorale
    {
        public numeroImmatriculation numeroImmatriculation { get; set; }
        public string nonInscrit { get; set; }
        public string denomination { get; set; }
        public string formeJuridique { get; set; }
        public string nomCommercial { get; set; }
        public string sigle { get; set; }
        public string administration { get; set; }
        public capital capital { get; set; }
    }
}
