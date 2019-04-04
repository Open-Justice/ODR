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
    public class BLLMeetingDoodle
    {
        public static MeetingDebate AddMeetingDebatePropositions(int idMeetingDebate, List<MeetingProposition> propositions)
        {
            using (var repo = new CRUDRepository<MeetingProposition>())
            {
                foreach (var item in propositions)
                {
                    if (!repo.GetQuery<MeetingProposition>().Any(c => c.IdMeetingDebate == item.IdMeetingDebate && c.DateTimeProposition == item.DateTimeProposition))
                        repo.Add(item);
                }

                return repo.GetQuery<MeetingDebate>(c => c.Id == idMeetingDebate)
                    .Include(c => c.MeetingPropositions)
                    .FirstOrDefault();
            }
        }

        public static MeetingPropositionAnswer AddAnswer(MeetingPropositionAnswer answer)
        {
            using (var repo = new CRUDRepository<MeetingPropositionAnswer>())
            {
                var existing = repo.GetQuery<MeetingPropositionAnswer>(c => c.IdMeetingProposition == answer.IdMeetingProposition && c.IdUser == answer.IdUser).FirstOrDefault();
                if (existing != null)
                {
                    existing.Response = answer.Response;
                    repo.Update(existing);
                    return existing;
                }
                else {
                    return repo.Add(answer);
                }
                
            }
        }

        public static MeetingDebate GetMeetingDebate(int idMeetingDebate)
        {
            using (var repo = new CRUDRepository<MeetingDebate>())
            {
                return repo.GetQuery<MeetingDebate>(c => c.Id == idMeetingDebate).FirstOrDefault();
            }
        }

        public static MeetingProposition GetMeetingProposition(int idMeetingProposition)
        {
            using (var repo = new CRUDRepository<MeetingProposition>())
            {
                return repo.GetQuery<MeetingProposition>(c => c.Id == idMeetingProposition).FirstOrDefault();
            }
        }

        public static MeetingDebate UpdateMeetingDebate(MeetingDebate debate)
        {
            using (var repo = new CRUDRepository<MeetingDebate>())
            {
                var prev = repo.GetQuery<MeetingDebate>(c => c.Id == debate.Id).FirstOrDefault();
                if (prev != null)
                {
                    prev.Date = debate.Date;
                    prev.EstimateDuration = debate.EstimateDuration;
                }
                repo.Update(prev);
                return prev;
            }
        }
    }
}
