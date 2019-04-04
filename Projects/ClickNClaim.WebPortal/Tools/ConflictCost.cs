using ClickNClaim.Common;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Web;

namespace ClickNClaim.WebPortal.Tools
{
    public class ConflictCost
    {
        public static int GetConflictCost(ConflictType type)
        {
            switch (type.Id)
            {
                case 3:
                case 4:
                case 6:
                    return 1500;
                case 1:
                case 2:
                case 7:
                    return 2500;
                case 10:
                case 12:
                    return 3500;
                case 8:
                case 9:
                case 11:
                case 13:
                    return 4500;
                default:
                    return 4500;
            }
        }
    }
}