using ClickNClaim.Common;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Web;

namespace ClickNClaim.WebPortal.Models
{
    public class CentreModel
    {
        public List<Conflict> ConflictsToAssigned { get; set; }
        public List<Conflict> ConflictsAssigned { get; set; }
        public List<Conflict> ArbitrationStarted { get; set; }
    }

    public class ConflictListModel
    {
        public List<Conflict> Conflicts { get; set; }
        public bool ShouldShowArbiter { get; set; }

        public bool IsForArbiter { get; set; }

        public bool ShouldShowState { get; set; }
    }

    public class ProcedureClosureModel
    {
        public Conflict Conflict { get; set; }
        public Debate Debat { get; set; }

        public string DocumentUrl { get; set; }

    }

   
}