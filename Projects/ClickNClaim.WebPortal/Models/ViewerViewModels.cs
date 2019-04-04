using ClickNClaim.Common;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Web;

namespace ClickNClaim.WebPortal.Models
{
    public class AskArbitrationViewModel
    {
        public Conflict Conflict { get; set; }

        public Company Company { get; set; }
        public AspNetUser User { get; set; }
    }

    public class WritePreSentenceModel
    {
        public Conflict Conflict { get; set; }
        public string Url { get; set; }
    }
}