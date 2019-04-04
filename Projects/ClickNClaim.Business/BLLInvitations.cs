using ClickNClaim.Common;
using ClickNClaim.Data.Repositories;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace ClickNClaim.Business
{
    public class BLLInvitations
    {
        public static Invitation AddInvitation(Invitation invitation)
        {
            using (var repo = new CRUDRepository<Invitation>())
            {
                if (!repo.GetQuery<Invitation>().Any(c => c.IdConflict == invitation.IdConflict && c.Email == invitation.Email))
                    return repo.Add(invitation);
                else
                    return null;

            }
        }

        public static Invitation GetInvitation(Guid uid)
        {
            using (var repo = new CRUDRepository<Invitation>())
            {
                return repo.GetQuery<Invitation>(c => c.Id == uid).FirstOrDefault();
            }
        }

        public static Invitation UpdateInvitation(Invitation invitation)
        {
            using (var repo = new CRUDRepository<Invitation>())
            {
                var inv = repo.GetQuery<Invitation>(c => c.Id == invitation.Id).FirstOrDefault();
                if (inv != null)
                {
                    inv.IsUsed = invitation.IsUsed;
                    if (!String.IsNullOrWhiteSpace(invitation.FirstName))
                    {
                        inv.FirstName = invitation.FirstName;
                    }
                    if (!String.IsNullOrWhiteSpace(invitation.LastName))
                        inv.LastName = invitation.LastName;
                    if (!String.IsNullOrWhiteSpace(invitation.CompanyName))
                        inv.CompanyName = invitation.CompanyName;
                    if(!String.IsNullOrWhiteSpace(invitation.Email))
                    {
                        inv.Email = invitation.Email;
                    }
                    inv.UsedDate = invitation.UsedDate;
                    repo.Update(inv);
                    return inv;
                }
                return null;
            }
        }

        public static void DeleteInvitation(Guid id)
        {
            using (var repo = new CRUDRepository<Invitation>())
            {
                var invit = repo.GetQuery<Invitation>(c => c.Id == id).FirstOrDefault();
                if (invit != null)
                {
                    repo.Delete(invit);
                }
            }
        }

    }
}
