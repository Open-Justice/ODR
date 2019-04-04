using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace ClickNClaim.Common
{
    public class ConflictDTO : Conflict
    {
        public List<InvitationDTO> Invitations { get; set; }
    }

    public class InvitationDTO : Invitation
    {

    }

}
