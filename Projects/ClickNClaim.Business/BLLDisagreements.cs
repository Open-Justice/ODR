using ClickNClaim.Common;
using ClickNClaim.Data.Repositories;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using Infrastructure.Data.EntityFramework;

namespace ClickNClaim.Business
{
    public class BLLDisagreements
    {
        public static Disagreement AddDisagreement(Disagreement action)
        {
            using (var repo = new CRUDRepository<Disagreement>())
            {
                if (action.Id > 0)
                {
                    var previous = repo.GetQuery<Disagreement>(c => c.Id == action.Id)
                                       .Include(c => c.CreatedBy)
                                       .FirstOrDefault();

                    previous.ConcurrentDate = action.ConcurrentDate;
                    previous.ConcurrentDescription = action.ConcurrentDescription;
                    previous.ConcurrentPieces = action.ConcurrentPieces;
                    previous.DisagreementOnDate = action.DisagreementOnDate;
                    previous.DisagreementOnDescription = action.DisagreementOnDescription;
                    previous.DisagreementOnPiece = action.DisagreementOnPiece;
                    previous.Comment = action.Comment;
                    repo.Update(previous);
                    return previous;
                }
                else
                {
                    action = repo.Add(action);
                    action.CreatedBy = BLLUsers.GetUserById(action.IdUser);
                    return action;
                }
            }
        }


    }
}
