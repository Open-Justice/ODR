using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace ClickNClaim.BodaccReader
{
    public class annonce
    {
        public string nojo { get; set; }
        public string numeroAnnonce { get; set; }
        public string numeroDepartement { get; set; }
        public string tribunal { get; set; }
        public string denomination { get; set; }
        public string sigle { get; set; }
        public string formeJuridique { get; set; }
        public typeAnnonce typeAnnonce { get; set; }
        public numeroImmatriculation numeroImmatriculation { get; set; }
        public adresse adresse { get; set; }
        public depot depot { get; set; }
        public personnes personnes { get; set; }
    }
}

