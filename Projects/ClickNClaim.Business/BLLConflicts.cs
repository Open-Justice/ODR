using ClickNClaim.Common;
using ClickNClaim.Data.Repositories;
using System;
using System.Linq;
using Infrastructure.Data.EntityFramework;
using System.Collections.Generic;
using ClickNClaim.Common.Enums;

namespace ClickNClaim.Business
{
    public class BLLConflicts
    {


        public static List<ConflictDTO> GetConflictsWithInvitations()
        {
            using (var repo = new CRUDRepository<Conflict>())
            {
                return repo.GetQuery<Conflict>()
                    .Select(c => new ConflictDTO()
                    {
                        Id = c.Id,
                        Invitations = repo.GetQuery<Invitation>(d => d.IdConflict == c.Id).Where(d => !d.IsUsed).Select(i => new InvitationDTO()
                        {
                            Id = i.Id,
                            Email = i.Email,
                        }).ToList()
                  })
                  .ToList();
            }
        }

        #region CONFLICTS

        public static List<Conflict> SearchConflicts(int? id, string text, string situation, int nbPage, int nbElement, out int totalPages)
        {
            using (var repo = new CRUDRepository<Conflict>())
            {
                var conflicts = repo.GetQuery<Conflict>()
                    .Include("UsersInConflicts.User")
                    .Include("UsersInConflicts.UserCompany.Company")
                    .Include(c => c.Invitations)
                    .Include(c => c.Arbiter)
                    .Include(c => c.ConflictType);

                if(id != null && id.HasValue && id.Value > 0)
                {
                    totalPages = 1;
                    return conflicts.Where(c => c.Id == id).ToList();
                }
                if (!String.IsNullOrWhiteSpace(text))
                {
                    conflicts = conflicts.Where(c => c.UsersInConflicts.Any(d =>
                                                                            d.CompanyName.ToLower().Contains(text) ||
                                                                            d.User.FirstName.ToLower().Contains(text) ||
                                                                            d.User.LastName.ToLower().Contains(text)));
                    conflicts.Concat(conflicts.Where(c => c.Events.Any(d => d.Name.ToLower().Contains(text) ||
                                                                            d.Description.ToLower().Contains(text))));

                }
                if (!String.IsNullOrWhiteSpace(situation))
                {
                    ConflictState enumValue;
                    if (Enum.TryParse<ConflictState>(situation, out enumValue))
                        conflicts = conflicts.Where(c => c.State == (int)enumValue);
                }
                totalPages = conflicts.Count() / nbElement;
                return conflicts.OrderByDescending(c => c.Id).Skip(nbElement * nbPage).Take(nbElement).ToList();
            }
        }

        public static void MarkUserDeclarationHasCompleted(int idConflict, string idUser)
        {
            using (var repo = new CRUDRepository<UsersInConflict>())
            {
                var uic = repo.GetQuery<UsersInConflict>(c => c.IdConflict == idConflict && c.IdUser == idUser).FirstOrDefault();
                if (uic != null)
                {
                    uic.Completed = true;
                    repo.Update(uic);
                }
            }
        }

        public static void DisableUserInConflict(int idConflict, string idUser)
        {
            using (var repo= new CRUDRepository<UsersInConflict>())
            {
                var uic = repo.GetQuery<UsersInConflict>(c => c.IdConflict == idConflict && c.IdUser == idUser).FirstOrDefault();
                if (uic != null)
                {
                    uic.Disable = true;
                    repo.Update(uic);
                }
            }
        }

        public static bool IsUserInConflict(int conflictId, string idUser)
        {
            using (var repo = new CRUDRepository<UsersInConflict>())
            {
                return repo.GetQuery<UsersInConflict>(c => c.IdConflict == conflictId).Any(c => c.IdUser == idUser && !c.Disable);
            }
        }

        public static ConflictState GetConflictState(int conflictId)
        {
            using (var repo=  new CRUDRepository<Conflict>())
            {
                var conflict = repo.GetQuery<Conflict>(c => c.Id == conflictId).FirstOrDefault();
                if (conflict == null)
                    throw new Exception("Conflict not found");
                return (ConflictState)conflict.State;
            }
        }

        public static string GetConflictInstigator(int conflictId)
        {
            using (var repo = new CRUDRepository<Conflict>())
            {
                return repo.GetQuery<Conflict>(c => c.Id == conflictId).First().IdCreationUser;
            }
        }

        public static bool IsRepresented(int conflictId, string idUser)
        {
            using (var repo = new CRUDRepository<UsersInConflict>())
            {
                var uic = repo.GetQuery<UsersInConflict>(c => c.IdConflict == conflictId && c.IdUser == idUser).FirstOrDefault();
                return uic != null && uic.IsRepresented;
            }
        }
        public static Conflict GetConflict(int id)
        {
            using (var repo = new CRUDRepository<Conflict>())
            {
                return repo.GetQuery<Conflict>(c => c.Id == id)
                           .Include("UsersInConflicts.User")
                           .Include(c => c.ConflictType)
                           .FirstOrDefault();

            }
        }

        public static Conflict GetConflictWithUsers(int id)
        {
            using (var repo = new CRUDRepository<Conflict>())
            {
                return repo.GetQuery<Conflict>(c => c.Id == id)
                    .Include("UsersInConflicts.User")
                    .Include(c => c.CreatedBy)

                    .Include(c => c.Arbiter)
                    .FirstOrDefault();
            }
        }

        public static Conflict GetConflictForUser(int idConflict, string idUser)
        {
            using (var repo = new CRUDRepository<Conflict>())
            {
                var conflict = repo.GetQuery<Conflict>(c => c.Id == idConflict)
                    .Include("UsersInConflicts.User")
                    .Include(c => c.Invitations)
                    .Include(c => c.Events)
                    // .Include(c => c.Resolutions)
                    .FirstOrDefault();
                //if (conflict == null)
                //    return null;
                //conflict.UsersInConflicts = repo.GetQuery<UsersInConflict>().Where(
                //    c => c.IdUser == idUser && c.IdConflict == idConflict).ToList();
                //conflict.Resolutions = repo.GetQuery<Resolution>().Where(
                //   c => c.IdUser == idUser && c.IdConflict == idConflict).ToList();
                return conflict;
            }
        }

        public static Conflict GetConflictForResolutionContestation(int idConflict)
        {
            using (var repo = new CRUDRepository<Conflict>())
            {
                var conflict = repo.GetQuery<Conflict>(c => c.Id == idConflict)
                    .Include("UsersInConflicts.User")
                    .Include(c => c.Invitations)
                    .Include("Events.Disagreements")
                    .FirstOrDefault();

                return conflict;
            }
        }

        public static Conflict GetConflictForArbitration(int id)
        {
            using (var repo = new CRUDRepository<Conflict>())
            {

                var res = repo.GetQuery<Conflict>(c => c.Id == id)
                .Include(c => c.Invitations)
                .Include("UsersInConflicts.User.MeetingPropositionAnswers")
                .Include("UsersInConflicts.Lawyer")
                .Include("Events.ProofFiles")
                .Include("Resolutions.ResolutionType")
                .Include("Events.Debates.MailDebates")
                .Include("Events.Disagreements")
                .Include("MeetingDebates.MeetingPropositions")
                .Include("Events.Comments")
                .Include(c => c.ConflictType)
                .Include("UsersInConflicts.UserCompany.Company")
                .Include(c => c.ConflictStateHistorics)
                .Include(c => c.Arbiter)
                .FirstOrDefault();

                res.MessageCount = repo.GetQuery<Message>(c => c.IdConflict == id).Count();

              //  res.UsersInConflicts = res.UsersInConflicts.ToList();

                res.ProofFiles = null;
                foreach (var item in res.Events)
                {
                    if (item.Type == (int)EventTypeEnum.Resolution)
                    {
                        item.CreatedBy.Conflicts = null;
                        item.CreatedBy.ArbiterSkills = null;
                        item.CreatedBy.AspNetUserClaims = null;
                        item.CreatedBy.AspNetUserLogins = null;
                        item.CreatedBy.Comments = null;
                        item.CreatedBy.ConflictOfLawyer = null;
                        item.CreatedBy.ConflictsAssigned = null;
                        item.CreatedBy.Disagreements = null;
                        item.CreatedBy.Events = null;
                        item.CreatedBy.Invitations = null;
                        item.CreatedBy.MailDebates = null;
                        item.CreatedBy.Resolutions = null;
                        item.CreatedBy.Roles = null;
                        item.CreatedBy.UsersInConflicts = null;
                    }
                    else
                    {
                        item.CreatedBy = null;
                    }
                    var debate = item.Debates.FirstOrDefault();
                    if (debate != null)
                        debate.Event = null;
                    item.Conflict = null;
                    var disagreement = item.Disagreements.FirstOrDefault();
                    if (disagreement != null)
                    {
                        disagreement.Event = null;
                        var firstName = disagreement.CreatedBy.FirstName;
                        var lastName = disagreement.CreatedBy.LastName;
                        disagreement.CreatedBy = new AspNetUser() { FirstName = firstName, LastName = lastName };

                    }
                    if (item.Comments != null && item.Comments.Count > 0)
                    {
                        foreach (var com in item.Comments)
                        {
                            com.CreatedBy = null;
                        }
                    }
                }

                if(res.Arbiter != null)
                {
                    res.Arbiter = new AspNetUser() { FirstName = res.Arbiter.FirstName, LastName = res.Arbiter.LastName, PhotoPath = res.Arbiter.PhotoPath };
                }

                foreach (var item in res.MeetingDebates)
                {
                    item.Event = null;
                    foreach (var meetingProp in item.MeetingPropositions)
                    {
                        foreach (var answer in meetingProp.MeetingPropositionAnswers)
                        {
                            answer.AspNetUser = null;
                        }
                    }
                }

                foreach (var item in res.Resolutions)
                {
                    item.User = null;
                }

                var firstname = res.CreatedBy.FirstName;
                var lastname = res.CreatedBy.LastName;
                res.CreatedBy = new AspNetUser();
                res.CreatedBy.FirstName = firstname;
                res.CreatedBy.LastName = lastname;

                res.Events = res.Events.OrderBy(c => c.DateBegin).ToList();

                return res;
            }
        }

        public static Conflict GetFullConflict(int id)
        {
            using (var repo = new CRUDRepository<Conflict>())
            {

                var conflict = repo.GetQuery<Conflict>(c => c.Id == id)
                    .Include("UsersInConflicts.User.MeetingPropositionAnswers")
                    .Include("Events.ProofFiles")
                    .Include("Events.Comments")
                    .Include(c => c.ConflictType)
                    .Include("Resolutions.ResolutionType")
                    .Include(c => c.Invitations)
                    .Include("Events.Debates.MailDebates")
                    .Include("Events.Disagreements")
                    .Include("MeetingDebates.MeetingPropositions")
                    .FirstOrDefault();

                //conflict.ConflictType.Conflicts = null;
                //conflict.CreatedBy.Conflicts = null;

                //foreach (var c in conflict.Events)
                //{
                //    c.Conflict = null;
                //    if (c.ProofFiles == null)
                //        c.ProofFiles = new List<ProofFile>();
                //}
                //conflict.Events = conflict.Events.OrderBy(c => c.DateBegin).ToList();

                //foreach (var item in conflict.Invitations)
                //{
                //    item.Conflict = null;
                //}
                //foreach (var item in conflict.MeetingDebates)
                //{
                //    item.Conflict = null;
                //    item.Event = null;
                //}
                //foreach (var item in conflict.UsersInConflicts)
                //{
                //    item.Conflict = null;
                //    item.User.Conflicts = null;
                //    item.User.Disagreements = null;
                //    item.User.Events = null;
                //    item.User.Invitations = null;
                //    item.User.MailDebates = null;
                //    item.User.MeetingPropositionAnswers = null;
                //    item.User.Resolutions = null;
                //    item.User.UsersInConflicts = null;
                //}
                //foreach (var item in conflict.Resolutions)
                //{
                //    item.Conflict = null;
                //    item.Event.Conflict = null;
                //    item.Event.ProofFiles = null;
                //}
                //foreach (var item in conflict.ProofFiles)
                //{
                //    item.Conflict = null;
                //    item.Events = null;
                //}


                return conflict;
            }
        }

        public static UsersInConflict GetUserInConflict(string user, int conflictId)
        {
            using (var repo = new CRUDRepository<UsersInConflict>())
            {
                return repo.GetQuery<UsersInConflict>(c => c.IdUser == user && c.IdConflict == conflictId).FirstOrDefault();
            }
        }

        public static UsersInConflict UpdateUserInConflict(UsersInConflict uic)
        {
            using (var repo = new CRUDRepository<UsersInConflict>())
            {
                var previous = repo.GetQuery<UsersInConflict>(c => c.IdConflict == uic.IdConflict && c.IdUser == uic.IdUser).FirstOrDefault();
                if (previous != null)
                {
                    previous.IdLawyer = uic.IdLawyer;
                    previous.IsLawyer = uic.IsLawyer;
                    previous.IsLegalRepresentative = uic.IsLegalRepresentative;
                    previous.IsPhysical = uic.IsPhysical;
                    previous.IsRepresented = uic.IsRepresented;
                    previous.ReadyForArbitration = uic.ReadyForArbitration;
                    previous.UserDescriptionOfTheConflict = uic.UserDescriptionOfTheConflict;
                    previous.CompanyName = uic.CompanyName;
                    previous.UserFunction = uic.UserFunction;
                    previous.HasAcceptedArbitration = uic.HasAcceptedArbitration;
                    repo.Update(previous);

                }
                return previous;
            }
        }


        public static Event GetEvent(int eventId)
        {
            using (var repo = new CRUDRepository<Event>())
            {
                return repo.GetQuery<Event>(c => c.Id == eventId)
                    .Include("MeetingDebates.MeetingPropositions.MeetingPropositionAnswers")
                    .Include("Conflict.UsersInConflicts")
                    .FirstOrDefault();
            }
        }

        public static Conflict GetConflictEvents(int id)
        {
            using (var repo = new CRUDRepository<Conflict>())
            {
                var conflict = repo.GetQuery<Conflict>(c => c.Id == id)
                    .Include("Events.ProofFiles")
                    .Include("Events.Comments")
                    .Include("Events.Disagreements.CreatedBy")
                    .FirstOrDefault();
                foreach (var c in conflict.Events)
                {
                    if (c.ProofFiles == null)
                        c.ProofFiles = new List<ProofFile>();
                }
                conflict.Events = conflict.Events.OrderBy(c => c.DateBegin).ToList();
                return conflict;
            }
        }

        public static Conflict UpdateConflict(Conflict conflict)
        {
            using (var repo = new CRUDRepository<Conflict>())
            {
                var cft = repo.GetQuery<Conflict>(c => c.Id == conflict.Id)
                    .FirstOrDefault();
                if (cft != null)
                {
                    cft.State = conflict.State;
                    cft.HasArbitralClause = conflict.HasArbitralClause;
                    cft.AskedForArbitration = conflict.AskedForArbitration;
                    repo.Update(cft);
                }
                return cft;
            }
        }

        public static Conflict UpdateConflictState(int id, int state)
        {
            using (var repo = new CRUDRepository<Conflict>())
            {
                var conflict = repo.GetQuery<Conflict>(c => c.Id == id).FirstOrDefault();
                if (conflict != null)
                {
                    conflict.State = state;
                    repo.Update(conflict);
                }
                return conflict;
            }
        }

        public static Conflict UpdateConflictType(int idConflict, int conflictTypeId)
        {
            using (var repo = new CRUDRepository<Conflict>())
            {
                var cft = repo.GetQuery<Conflict>(c => c.Id == idConflict)
                    .FirstOrDefault();
                if (cft != null)
                {
                    cft.IdConflictType = conflictTypeId;
                    repo.Update(cft);
                }
                return cft;
            }
        }

        public static Conflict AddConflict(Conflict c)
        {
            using (var repo = new CRUDRepository<Conflict>())
            {
                if (c.UsersInConflicts == null)
                    c.UsersInConflicts = new List<UsersInConflict>();

                c.UsersInConflicts.Add(new UsersInConflict() { IdUser = c.IdCreationUser });
                c.CreateDate = DateTime.Now;
                c.State = (int)ConflictState.Created;
                return repo.Add(c);
            }
        }

        public static List<Conflict> ListConflictsForUser(string userId)
        {
            using (var repo = new CRUDRepository<Conflict>())
            {

                var conflicts = repo.GetQuery<Conflict>(c => c.UsersInConflicts.Any(d => d.IdUser == userId))
                                               .Include("UsersInConflicts.User")
                                               .Include(c => c.Invitations)
                                              .ToList();
                foreach (var item in conflicts)
                {
                    item.CreatedBy.Conflicts = null;
                    item.CreatedBy.ConflictsAssigned = null;
                    item.CreatedBy.UsersInConflicts = null;
                }
                return conflicts;
            }
        }

        public static List<Conflict> SearchConflicts(ConflictState state, bool shouldLookUpper = false)
        {
            using (var repo = new CRUDRepository<Conflict>())
            {
                var res = repo.GetQuery<Conflict>();
                if (!shouldLookUpper)
                    return res.Where(c => c.State == (int)state)
                    .Include("UsersInConflicts.User")
                    .Include(c => c.Invitations)
                    .Include(c => c.ConflictType)
                    .Include(c => c.Arbiter)
                    .OrderByDescending(c => c.Id)
                    .ToList();
                else
                {
                    return res.Where(c => c.State >= (int)state)
                   .Include("UsersInConflicts.User")
                   .Include(c => c.Arbiter)
                   .Include(c => c.ConflictType)
                    .Include(c => c.Invitations)
                   .OrderByDescending(c => c.Id)
                   .ToList();
                }
            }
        }

        #endregion

        #region DECLARATION

        public static Conflict SaveDeclaration(Conflict c, UsersInConflict uic, string userId)
        {
            using (var repo = new CRUDRepository<Conflict>())
            {

                var previous = GetConflict(c.Id);
                previous.HasArbitralClause = c.HasArbitralClause;
                if (c.HasArbitralClause)
                {
                    previous.State = (int)ConflictState.Open;
                }
                var decla = previous.UsersInConflicts.Where(u => u.IdUser == userId).FirstOrDefault();
                if (decla != null)
                {
                    decla.UserDescriptionOfTheConflict = uic.UserDescriptionOfTheConflict;
                    repo.Update<UsersInConflict>(decla);
                    repo.UnitOfWork.SaveChanges();

                }
                else
                {
                    repo.Add<UsersInConflict>(new UsersInConflict() { IdConflict = previous.Id, IdUser = userId, UserDescriptionOfTheConflict = uic.UserDescriptionOfTheConflict });
                    repo.UnitOfWork.SaveChanges();
                }
                repo.Update(previous);
                return previous;


            }
        }

        #endregion

        #region EVENTS

        public static ConflictType GetConflictTypeById(int id)
        {
            using (var repo = new CRUDRepository<ConflictType>())
            {
               return repo.GetQuery<ConflictType>(c => c.Id == id).FirstOrDefault();
            }
        }

        public static int GetDefaultEventConflictType(int idEvent)
        {
            using (var repo = new CRUDRepository<DefaultEvent>())
            {
                var defaultEvent = repo.GetQuery<DefaultEvent>(c => c.Id == idEvent).FirstOrDefault();
                if (defaultEvent != null)
                {
                    return defaultEvent.IdConflictType;
                }
                else
                    return 0;
                
            }
        }
        public static bool DoesEventAlreadyExists(Event e)
        {
            using (var repo = new CRUDRepository<Event>())
            {
                return repo.GetQuery<Event>().Any(ev => ev.Name == e.Name && ev.Description == e.Description && ev.DateBegin == e.DateBegin && e.IdConflict == ev.IdConflict);
            }
        }

        public static Conflict SaveEvents(Conflict c, string userId)
        {
            using (var repo = new CRUDRepository<Conflict>())
            {
                foreach (var item in c.Events)
                {
                    if (item.Id == 0)
                    {
                        item.Type = (int)EventTypeEnum.Event;
                        item.IdUser = userId;
                        repo.Add<Event>(item);
                    }
                    else
                    {
                        var previous = repo.GetQuery<Event>(ev => ev.Id == item.Id).FirstOrDefault();
                        if (previous != null)
                        {
                            previous.DateBegin = item.DateBegin;
                            previous.Name = item.Name;
                            previous.Description = item.Description;
                            repo.Update(previous);
                        }
                    }
                }

                repo.UnitOfWork.SaveChanges();

                repo.Update(c);
                return c;
            }
        }

        public static Event AddEvent(Event e)
        {
            using (var repo = new CRUDRepository<Event>())
            {
                if (!BLLConflicts.DoesEventAlreadyExists(e))
                {


                    if (e.ProofFiles != null)
                    {
                        List<ProofFile> files = new List<ProofFile>();
                        for (int i = 0; i < e.ProofFiles.Count; i++)
                        {
                            if (e.ProofFiles.ElementAt(i).Id > 0)
                            {
                                var id = e.ProofFiles.ElementAt(i).Id;
                                files.Add(repo.GetQuery<ProofFile>(c => c.Id == id).First());
                            }
                        }
                        e.ProofFiles = files;
                    }

                    var evt = repo.Add(e);

                    if (evt.ProofFiles == null)
                    {
                        evt.ProofFiles = new List<ProofFile>();
                    }

                    return evt;
                }
                return null;
            }
        }

        public static void RemoveEvent(int id)
        {
            using (var repo = new CRUDRepository<Event>())
            {
                var elt = repo.GetQuery<Event>(c => c.Id == id).Include(c => c.ProofFiles).FirstOrDefault();

                if (elt != null)
                {
                    elt.ProofFiles = null;
                    repo.Update(elt);
                    repo.Delete(elt);
                }
            }
        }

        public static Event UpdateEvent(Event e)
        {
            using (var repo = new CRUDRepository<Event>())
            {
                var evt = repo.GetQuery<Event>(c => c.Id == e.Id)
                    .Include("Disagreements.CreatedBy")
                    .Include(c => c.ProofFiles)
                    .Include(c => c.CreatedBy)
                    .FirstOrDefault();
                if (evt != null)
                {
                    evt.DateBegin = e.DateBegin;
                    evt.DateEnd = e.DateEnd;
                    evt.Description = e.Description;
                    evt.Name = e.Name;
                    evt.SubName = e.SubName;
                    repo.Update(evt);
                }

                if (evt.ProofFiles == null)
                {
                    evt.ProofFiles = new List<ProofFile>();
                }
              

                return evt;
            }
        }

        public static Comment AddComment(Comment c)
        {
            using (var repo = new CRUDRepository<Comment>())
            {
                c.CreateDate = DateTime.Now;
                return repo.Add(c);
            }
        }

        #endregion

        #region FILES

        public static ProofFile AddFile(ProofFile file, int eventId)
        {
            using (var repo = new CRUDRepository<Event>())
            {
                var evt = repo.GetQuery<Event>(c => c.Id == eventId).FirstOrDefault();
                if (evt != null)
                {
                    file.UploadDate = DateTime.Now;
                    evt.ProofFiles.Add(file);
                }
                repo.Update(evt);
                return file;
            }
        }

        public static ProofFile GetFile(int fileId)
        {
            using (var repo = new CRUDRepository<ProofFile>())
            {
                return repo.GetQuery<ProofFile>(c => c.Id == fileId).FirstOrDefault();
            }
        }

        public static ProofFile AddFile(ProofFile file)
        {
            using (var repo = new CRUDRepository<ProofFile>())
            {
                return repo.Add(file);
            }
        }

        //public static void RemoveFile(int id)
        //{
        //    using (var repo = new CRUDRepository<ProofFile>())
        //    {
        //        var file = repo.GetQuery<ProofFile>(c => c.Id == id).Include(c => c.Events).FirstOrDefault();
        //        if (file != null)
        //        {
        //            repo.Delete(file);
        //        }
        //    }
        //}

        public static void RemoveFile(int id)
        {
            using (var repo = new CRUDRepository<ProofFile>())
            {
                var eventFiles = repo.GetQuery<Event>()
                    .Where(c => c.ProofFiles.Any(p => p.Id == id))
                    .Include(c => c.ProofFiles)
                    .ToList();

                foreach (var item in eventFiles)
                {
                    var fileInEvent = item.ProofFiles.First(c => c.Id == id);
                    item.ProofFiles.Remove(fileInEvent);
                    repo.Update<Event>(item);
                    repo.UnitOfWork.SaveChanges();

                }


                var file = repo.GetQuery<ProofFile>(c => c.Id == id).FirstOrDefault();
                if (file != null)
                {
                    repo.Delete(file);
                }

            }
        }

        #endregion

        #region USERS

        public static UsersInConflict AddUserInConflictFromInvitation(string userEmail, Guid invitation, string userId)
        {
            using (var repo = new CRUDRepository<UsersInConflict>())
            {
                var inv = BLLInvitations.GetInvitation(invitation);
                if (!inv.IsUsed)
                {
                    inv.UsedDate = DateTime.Now;
                    inv.IsUsed = true;
                    BLLInvitations.UpdateInvitation(inv);
                    UsersInConflict uic = new UsersInConflict();
                    uic.IdConflict = inv.IdConflict;
                    uic.IdUser = userId;
                    return repo.Add(uic);
                }
                return null;
            }
        }

        public static UsersInConflict AddUserDirectly(string userId, int conflictId)
        {
            using (var repo = new CRUDRepository<UsersInConflict>())
            {
                UsersInConflict uic = new UsersInConflict();
                uic.IdConflict = conflictId;
                uic.IdUser = userId;
                return repo.Add(uic);
            }
        }

        #endregion

        #region RESOLUTIONS

        public static Resolution AddOrUpdateResolution(Resolution r, string userName, int conflictId, string idUser, int eventId)
        {
            using (var repo = new CRUDRepository<Resolution>())
            {
                var resolution = repo.GetQuery<Resolution>(c => c.IdConflict == conflictId && c.IdUser == idUser && c.IdResolutionType == r.Id).FirstOrDefault();
                if (resolution == null)
                {
                    Resolution toAdd = new Resolution();
                    toAdd.IdConflict = conflictId;
                    toAdd.IdEvent = eventId;
                    toAdd.IdResolutionType = r.Id;
                    toAdd.IdUser = idUser;
                    toAdd.ResolutionComment = r.ResolutionComment;
                    toAdd.ResolutionValue = r.ResolutionValue;

                    return repo.Add(toAdd);
                }
                else
                {
                    resolution.ResolutionComment = r.ResolutionComment;
                    resolution.ResolutionValue = r.ResolutionValue;
                    repo.Update(resolution);
                    return resolution;
                }
            }
        }

        public static void AcceptArbitration(int conflictId, string userId)
        {
            using (var repo = new CRUDRepository<UsersInConflict>())
            {
                var uic = repo.GetQuery<UsersInConflict>(c => c.IdUser == userId && c.IdConflict == conflictId).FirstOrDefault();
                if (uic != null)
                {
                    uic.ReadyForArbitration = true;
                    repo.Update(uic);
                }
            }
        }

        public static List<Event> AddOrUpdateResolutionEvent(int conflictId, string v, List<Resolution> myResolutions)
        {
            List<Event> events = new List<Event>();
            using (var repo = new CRUDRepository<Event>())
            {

                foreach (var item in myResolutions)
                {
                    var resoEventPrevious = repo.GetQuery<Event>(c => c.IdUser == v &&
                                                           c.IdConflict == conflictId &&
                                                           c.Type == (int)EventTypeEnum.Resolution &&
                                                           c.Name == item.Name )
                                                           .Include(c => c.ProofFiles).FirstOrDefault();
                    if(resoEventPrevious != null)
                    {
                        resoEventPrevious.Description = item.ResolutionComment;
                        if (item.ProofFiles != null)
                        {
                            if (resoEventPrevious.ProofFiles == null)
                            {
                                resoEventPrevious.ProofFiles = new List<ProofFile>();
                            }
                            foreach (var pf in item.ProofFiles)
                            {
                                resoEventPrevious.ProofFiles.Add(pf);
                            }
                        }
                        repo.Update(resoEventPrevious);
                        events.Add(resoEventPrevious);
                    }
                    else
                    {
                        Event evt = new Event();
                        evt.DateBegin = DateTime.Now;
                        evt.Description = item.ResolutionComment;
                        evt.IdConflict = conflictId;
                        evt.IdUser = v;
                        evt.Name = item.Name;
                        evt.Type = (int)EventTypeEnum.Resolution;
                        if (evt.ProofFiles == null)
                        {
                            evt.ProofFiles = new List<ProofFile>();
                        }
                        myResolutions.ForEach(c =>
                        {
                            if (c.ProofFiles != null)
                            {
                                foreach (var pf in c.ProofFiles)
                                {
                                    evt.ProofFiles.Add(pf);
                                }
                            }
                        });
                        repo.Add(evt);
                        events.Add(evt);
                    }
                }
            }
            return events;
        }

        public static void ClearResolutionEvent(int eventId)
        {
            using (var repo = new CRUDRepository<Resolution>())
            {
                repo.Delete<Resolution>(c => c.IdEvent == eventId);
                repo.UnitOfWork.SaveChanges();
            }
        }

        public static List<Resolution> GetUserResolutions(int conflictId, string idUser)
        {
            using (var repo = new CRUDRepository<Resolution>())
            {
                var resolutions = repo.GetQuery<Resolution>(c => c.IdConflict == conflictId && c.IdUser == idUser)
                    .Include(c => c.Event)
                    .ToList();

                foreach (var item in resolutions)
                {
                    if (item.Event != null)
                    {
                        item.Event.ProofFiles = repo.GetQuery<Event>(c => c.Id == item.Id).SelectMany(c => c.ProofFiles).ToList();
                    }
                }
                return resolutions;
            }
        }

        #endregion

        #region RESUME

        public static ConflictResume GetConflictResume(int conflictId)
        {
            ConflictResume cr = new ConflictResume();

            using (var repo = new CRUDRepository<Conflict>())
            {
                cr.UsersInConflicts = repo.GetQuery<UsersInConflict>(c => c.IdConflict == conflictId)
                    .Include(c => c.User)
                    .Where(c => c.IsLawyer == null || !c.IsLawyer.Value)
                    .ToList();
                cr.UsersInConflicts.AddRange(
                repo.GetQuery<Invitation>(c => c.IdConflict == conflictId)
                    .Where(c => c.UsedDate == null)
                    .ToList()
                    .Select(c => new UsersInConflict() { User = new AspNetUser() { FirstName = c.FirstName, LastName = c.LastName, Email = c.Email, }, IdUser = c.Id.ToString(), CompanyName = c.CompanyName })
                    .ToList());
            
                cr.Events = repo.GetQuery<Event>(c => c.IdConflict == conflictId)
                    .Include(c => c.CreatedBy)
                    .Where(c => c.Type != (int)EventTypeEnum.Resolution)
                    .ToList();

                cr.Resolutions = repo.GetQuery<Event>(c => c.IdConflict == conflictId)
                    .Include(c => c.CreatedBy)
                    .Where(c => c.Type == (int)EventTypeEnum.Resolution)
                    .ToList();
            }
            return cr;

        }

        #endregion

        #region PAYMENT

        public static PaymentStateEnum UpdatePaiement(int conflictId)
        {
            using (var repo = new CRUDRepository<Conflict>())
            {
                var conflict =repo.GetQuery<Conflict>(c => c.Id == conflictId).FirstOrDefault();
                if (conflict != null)
                {
                    if( conflict.PaymentState == null)
                    {
                        conflict.PaymentState = (int)PaymentStateEnum.Half;
                    }
                    else if (conflict.PaymentState == (int)PaymentStateEnum.Half)
                    {
                        conflict.PaymentState = (int)PaymentStateEnum.Complete;
                    }
                    repo.Update(conflict);
                    return (PaymentStateEnum)conflict.PaymentState;
                }
                throw new Exception("Une erreur est survenue lors de la confirmation du paiement du litige. Nous reviendrons vers vous dès que possible.");
            }
        }

        #endregion

        #region ConflictStateHistoric

        public static ConflictStateHistoric AddConflictStateHistoric(ConflictStateHistoric csh)
        {
            using (var repo = new CRUDRepository<ConflictStateHistoric>())
            {
                csh.CreateDate = DateTime.Now;
                return repo.Add(csh);
            }
        }

        public static ConflictStateHistoric UpdateConflictStateHistoric(ConflictStateHistoric csh)
        {
            using (var repo = new CRUDRepository<ConflictStateHistoric>())
            {
                var conflictStateHistoric = repo.GetQuery<ConflictStateHistoric>(c => c.Id == csh.Id).FirstOrDefault();
                if (conflictStateHistoric != null)
                {
                    conflictStateHistoric.ConflictStateId = csh.ConflictStateId;
                    conflictStateHistoric.ConflictStateName = csh.ConflictStateName;
                    conflictStateHistoric.CountDown = csh.CountDown;
                    repo.Update(conflictStateHistoric);
                }
                return conflictStateHistoric;
            }
        }

        #endregion

        public static Message AddMessage(Message m)
        {
            using (var repo = new CRUDRepository<Message>())
            {
                return repo.Add(m);
            }
        }

        public static List<Message> GetConflictMessages(int conflictId)
        {
            using (var repo = new CRUDRepository<Message>())
            {
                return repo.GetQuery<Message>(c => c.IdConflict == conflictId).Include(c => c.AspNetUser).ToList();
            }
        }

    }
}
