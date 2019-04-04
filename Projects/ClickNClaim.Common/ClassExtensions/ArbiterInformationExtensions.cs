using System;
using System.Collections.Generic;
using System.ComponentModel.DataAnnotations;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using System.Web.Mvc;

namespace ClickNClaim.Common
{
    [MetadataType(typeof(ArbiterInformationMetadata))]
    public partial class ArbiterInformation
    {
        internal sealed class ArbiterInformationMetadata
        {
            [AllowHtml]
            public string Presentation { get; set; }
        }
    }
}
