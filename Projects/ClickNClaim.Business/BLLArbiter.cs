using ClickNClaim.Common;
using ClickNClaim.Data.Repositories;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace ClickNClaim.Business
{
    public class BLLArbiter
    {
        public static void AssignArbiterToConflict(int idConflict, string idArbiter)
        {
            using (var repo = new CRUDRepository<Conflict>())
            {
                var conflict = repo.GetQuery<Conflict>(c => c.Id == idConflict).FirstOrDefault();
                if (conflict != null)
                {
                    conflict.State = (int)ConflictState.ArbiterAssigned;
                    conflict.IdArbiterAssigned = idArbiter;
                    repo.Update(conflict);
                }
            }
        }

        public static void AcceptConflict(int idConflict, string idArbiter, bool noconflict, bool legitimate)
        {
            using (var repo = new CRUDRepository<Conflict>())
            {
                var conflict = repo.GetQuery<Conflict>(c => c.Id == idConflict && c.IdArbiterAssigned == idArbiter).FirstOrDefault();
                if (conflict != null)
                {
                    conflict.HasArbiterAccepted = true;
                    conflict.ArbiterRecognizeLegitimate = legitimate;
                    conflict.ArbiterRecognizeNoConflictOfInterest = noconflict;
                    conflict.State = (int)ConflictState.ArbitrationStarted;
                    repo.Update(conflict);
                }

            }
        }

        public static void AddSkills(string[] skills, string arbiterId)
        {
            using (var repo = new CRUDRepository<Skill>())
            {
                var existing = repo.GetQuery<ArbiterSkill>(c => c.IdArbiter == arbiterId).Select(c => c.Skill.Name).ToList();

                var toRemove = existing.Where(c => !skills.Contains(c)).ToList();
                for (int i = 0; i < toRemove.Count; i++)
                {
                    var elt = toRemove[i];
                    repo.Delete<ArbiterSkill>(c => c.Skill.Name == elt);

                }
                repo.UnitOfWork.SaveChanges();

                foreach (var item in skills)
                {
                    if (existing.Contains(item))
                        continue;

                    var skill = repo.GetQuery<Skill>(c => c.Name == item).FirstOrDefault();
                    if (skill == null)
                    {
                        var toAdd = new Skill() { Name = item };
                        repo.Add<Skill>(toAdd);
                        repo.UnitOfWork.SaveChanges();
                        repo.Add<ArbiterSkill>(new ArbiterSkill() { IdArbiter = arbiterId, IdSkill = toAdd.Id });
                        repo.UnitOfWork.SaveChanges();

                    }
                    if (skill != null)
                    {
                        repo.Add<ArbiterSkill>(new ArbiterSkill() { IdArbiter = arbiterId, IdSkill = skill.Id });
                        repo.UnitOfWork.SaveChanges();
                    }
                }
            }
        }


        public static void UpdatePresentation(ArbiterInformation arbiterInformation)
        {
            using (var repo = new CRUDRepository<ArbiterInformation>())
            {
                if (arbiterInformation != null)
                {
                    var previous = repo.GetQuery<ArbiterInformation>(c => c.Id == arbiterInformation.Id).FirstOrDefault();
                    if (previous != null)
                    {
                        previous.Presentation = arbiterInformation.Presentation;
                        repo.Update(previous);
                    }
                }
            }
        }
    }
}
