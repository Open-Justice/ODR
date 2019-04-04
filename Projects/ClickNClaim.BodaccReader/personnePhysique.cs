using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace ClickNClaim.BodaccReader
{
    public class personnePhysique
    {
        public numeroImmatriculation numeroImmatriculation { get; set; }
        public string nonInscrit { get; set; }
        public string nom { get; set; }
        public string prenom { get; set; }
        public string nomUsage { get; set; }
        public string pseudonyme { get; set; }
        public string nomCommercial { get; set; }
        public string nationalite { get; set; }
    }
}
