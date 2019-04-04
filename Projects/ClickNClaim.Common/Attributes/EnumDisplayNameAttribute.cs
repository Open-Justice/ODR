using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace ClickNClaim.Common.Attributes
{
    public class EnumDisplayNameAttribute : Attribute
    {
        public string DisplayName { get; set; }        
        public EnumDisplayNameAttribute(string displayName)
        {
            DisplayName = displayName;
        }
    }

    public class EnumSituationAttribute : Attribute
    {
        public string Situation { get; set; }
        public EnumSituationAttribute(string situation)
        {
            Situation = situation;
        }
    }
}
