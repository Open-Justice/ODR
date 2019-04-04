using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace ClickNClaim.Common
{
    public partial class AspNetUser
    {
        public string DisplayName
        {
            get
            {
                return String.Format("{0} {1}", this.FirstName, this.LastName);
            }
        }
    }
}
