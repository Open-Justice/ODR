using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace ClickNClaim.Common
{
    public class ConflictResume
    {
        public List<UsersInConflict> UsersInConflicts { get; set; }

        public bool UserCanUpdate(string idUser)
        {
            if (UsersInConflicts != null)
            {
                var uic = UsersInConflicts.FirstOrDefault(c => c.IdUser == idUser);
                if (uic != null)
                {
                    return !uic.Completed;
                }
                return false;
            }
            return false;
        }

        public List<Event> Events { get; set; }
        public List<Event> Resolutions { get; set; }
    }
}
