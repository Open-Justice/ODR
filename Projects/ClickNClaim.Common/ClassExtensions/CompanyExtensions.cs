using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace ClickNClaim.Common
{
    public partial class Company
    {
        public bool isFilled()
        {
            if (String.IsNullOrWhiteSpace(this.Address1))
                return false;
            if (String.IsNullOrWhiteSpace(this.City))
                return false;
            if (String.IsNullOrWhiteSpace(this.Name))
                return false;
            if (String.IsNullOrWhiteSpace(this.PostalCode))
                return false;
            if (string.IsNullOrWhiteSpace(this.Siret))
                return false;
            return true;
        }
    }
}
