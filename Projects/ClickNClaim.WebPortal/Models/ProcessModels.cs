using ClickNClaim.Common;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Web;

namespace ClickNClaim.WebPortal.Models
{

    public class ConflictTypeModel
    {
        public Conflict Conflict { get; set; }
        public List<DefaultEvent> DefaultEvents { get; set; }
    }
    public class DeclarationModel
    {
        public Conflict Conflict { get; set; }
        public List<UsersInConflict> PreviousDeclarations { get; set; }
        public UsersInConflict UserInConflict { get; set; }
    }

   

    public class TimelineModel
    {
        public Conflict Conflict { get; set; }

        public Disagreement DisagreementModel { get; set; }
    }

    public class ResolutionModel
    {
        public Conflict Conflict { get; set; }
        public List<ResolutionType> ResolutionTypes { get; set; }

        public List<Resolution> MyResolutions { get; set; }

    }

    public class MerciModel
    {
        public Conflict Conflict { get; set; }
    }

    public class IdentificationModel
    {
        public Conflict Conflict { get; set; }
        public AspNetUser Lawyer { get; set; }

        public AspNetUser UserRepresented { get; set; }

    }

    public class IdentificationForOpponentModel
    {
        public Conflict Conflict { get; set; }
        public AspNetUser Lawyer { get; set; }
        public UsersInConflict Me { get; set; }
    }

    public class ClauseModel
    {
        public Conflict Conflict { get; set; }

        public LegalDocument Clause{ get; set; }
    }

}