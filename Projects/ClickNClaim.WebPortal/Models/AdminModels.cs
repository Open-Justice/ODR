using ClickNClaim.Common;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Web;

namespace ClickNClaim.WebPortal.Models
{
    public class AdminModels
    {
        public class CaseListModel
        {
            public List<Conflict> Conflicts { get; set; }
            public int Page { get; set; }
            public int TotalPages { get; set; }
            public int? id { get; set; }
            public string text { get; set; }

            public string situation { get; set; }
        }

        public class UserListModel
        {
            public List<AspNetUser> Users { get; set; }
            public int Page { get; set; }
            public int TotalPages { get; set; }
            public string text { get; set; }
            public string role { get; set; }

            public List<AspNetRole> Roles { get; set; }
        }

    }
}