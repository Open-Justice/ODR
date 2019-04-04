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
    public class BLLDebates
    {
        public static Debate CreateDebate(Debate debate)
        {
            using (var repo = new CRUDRepository<Debate>())
            {
                return repo.Add(debate);
            }
        }

        public static Debate GetDebate(int id)
        {
            using (var repo = new CRUDRepository<Debate>())
            {
                return repo.GetQuery<Debate>(c => c.Id == id)
                    .Include("MailDebates.AspNetUser")
                    .Include(c => c.Event)
                    .FirstOrDefault();
            }
        }

        public static MailDebate AddMailDebate(MailDebate mailDebate)
        {
            using (var repo = new CRUDRepository<MailDebate>())
            {
                return repo.Add(mailDebate);
            }
        }

        public static MeetingDebate AddMeetingDate(MeetingDebate meetingDebate)
        {
            using (var repo = new CRUDRepository<MeetingDebate>())
            {
                return repo.Add(meetingDebate);
            }
        }

        public static List<string> GetUsersForDebate(int debateId)
        {
            using (var repo = new CRUDRepository<Debate>())
            {
                var res = repo.GetQuery<Debate>(c => c.Id == debateId)
                      .Include("Event.Conflict")
                      .Select(c => c.Event.Conflict)
                      .FirstOrDefault();
                var user = repo.GetQuery<AspNetUser>()
                      .Where(c => c.UsersInConflicts.Any(d => d.IdConflict == res.Id))
                      .ToList();
                return user.Select(c => c.Email).ToList();
            }
        }

        public static Debate CloseDebate(int id)
        {
            using (var repo = new CRUDRepository<Debate>())
            {
                var debate = repo.GetQuery<Debate>(c => c.Id == id).FirstOrDefault();
                if (debate != null)
                {
                    debate.Closed = true;
                    repo.Update(debate);
                }
                return debate;
            }
        }

        public static Debate ReOpenDebate(int id, int countDown)
        {
            using (var repo = new CRUDRepository<Debate>())
            {
                var debate = repo.GetQuery<Debate>(c => c.Id == id).FirstOrDefault();
                if (debate != null)
                {
                    debate.Closed = false;
                    debate.CreateDate = DateTime.Now;
                    debate.CountDown = countDown;
                    repo.Update(debate);
                }
                return debate;
            }
        }
    }
}
