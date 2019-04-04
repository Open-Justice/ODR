using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace ClickNClaim.BodaccReader
{
    public class personne
    {
        public personneMorale personneMorale { get; set; }
        public personnePhysique personnePhysique { get; set; }
        public capital capital { get; set; }
        public adresse adresse { get; set; }
        public adresse siegeSocial { get; set; }

        public numeroImmatriculation numeroImmatriculation { get; set; }
    }
}
