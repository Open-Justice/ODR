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
    public class BLLUsers
    {

        public static List<AspNetUser> SearchUsers(string text, string role, int nbElements, int nbPage, out int totalPages)
        {
            using (var repo = new CRUDRepository<AspNetUser>())
            {
                var users = repo.GetQuery<AspNetUser>().Include(c => c.Roles);

                if (!String.IsNullOrWhiteSpace(text))
                {
                    users = users.Where(c => c.FirstName.ToLower().Contains(text) ||
                                        c.LastName.ToLower().Contains(text) ||
                                        c.Email.ToLower().Contains(text));
                }
                if (!String.IsNullOrWhiteSpace(role))
                {
                    users = users.Where(c => c.Roles.Any(d => d.Id == role));
                }
                totalPages = users.Count() /nbElements;
                return users.OrderBy(c => c.CreateDate).Skip(nbElements * nbPage).Take(nbElements).ToList();
            }
        }

        public static List<AspNetRole> ListRoles()
        {
            using (var repo = new CRUDRepository<AspNetRole>())
            {
                return repo.GetQuery<AspNetRole>().ToList();
            }
        }

        public static List<AspNetRole> ListUserRoles(string userId)
        {
            using (var repo = new CRUDRepository<AspNetRole>())
            {
                return repo.GetQuery<AspNetUser>(c => c.Id == userId).Include(c => c.Roles).SelectMany(c => c.Roles).ToList();
            }
        }


        public static void AddRolesToUser(string userId, string[] roleId)
        {
            using (var repo = new CRUDRepository<AspNetUser>())
            {
                var user = repo.GetQuery<AspNetUser>(c => c.Id == userId).Include(c => c.Roles).FirstOrDefault();
                foreach (var item in roleId)
                {
                    var role = repo.GetQuery<AspNetRole>(c => c.Id == item).FirstOrDefault();
                    if (role != null && !user.Roles.Any(c => c.Id == role.Id))
                        user.Roles.Add(role);
                }
                repo.Update(user);
            }
        }

        public static void RemoveRolesToUser(string userId, string[] rolesId)
        {
            using (var repo = new CRUDRepository<AspNetUser>())
            {
                var user = repo.GetQuery<AspNetUser>(c => c.Id == userId).Include(c => c.Roles).FirstOrDefault();
                foreach (var item in rolesId)
                {
                    var role = repo.GetQuery<AspNetRole>(c => c.Id == item).FirstOrDefault();
                    if (role != null && user.Roles.Any(c => c.Id == role.Id))
                        user.Roles.Remove(role);
                }
                repo.Update(user);
            }
        }

        public static void AssociateUserAccounts(string provider, string providerUserId, string userId)
        {
            using (var repo = new CRUDRepository<AspNetUserLogin>())
            {
                repo.Add(new AspNetUserLogin() { LoginProvider = provider, ProviderKey = providerUserId, UserId = userId });
            }
        }

        public static AspNetUser GetUserById(string id)
        {
            using (var repo = new CRUDRepository<AspNetUser>())
            {
                return repo.GetQuery<AspNetUser>(c => c.Id == id).FirstOrDefault();
            }
        }

        public static AspNetUser GetMyProfil(string id)
        {
            using (var repo = new CRUDRepository<AspNetUser>())
            {


                var user = repo.GetQuery<AspNetUser>(c => c.Id == id)
                           .Include("Conflicts.UsersInConflicts.User")
                           .Include("UsersInConflicts.Conflict.CreatedBy")
                           .Include("UsersInConflicts.Conflict.Invitations")
                           .Include("UserCompanies.Company")
                           .FirstOrDefault();


                if (user != null)
                {
                    user.Invitations = repo.GetQuery<Invitation>(c => c.Email == user.Email && !c.IsUsed).ToList();
                    var conflicts = user.Conflicts.ToList();
                    conflicts.AddRange(repo.GetQuery<Conflict>().Include(c => c.UsersInConflicts).Where(c => c.UsersInConflicts.Any(d => d.IdUser == id)).ToList());
                    user.Conflicts = conflicts;
                }
                return user;
            }
        }

        public static AspNetUser GetUserById(string id, int conflictId)
        {
            using (var repo = new CRUDRepository<AspNetUser>())
            {
                var user = repo.GetQuery<AspNetUser>(c => c.Id == id)
                    .FirstOrDefault();
                user.UsersInConflicts = repo.GetQuery<UsersInConflict>(c => c.IdUser == id && c.IdConflict == conflictId).ToList();
                return user;
            }
        }

        public static bool UserEmailExists(string email)
        {
            using (var repo = new CRUDRepository<AspNetUser>())
            {
                return repo.GetQuery<AspNetUser>(c => c.Email == email).Any();
            }
        }

        public static AspNetUser GetUserByEmail(string email)
        {
            using (var repo = new CRUDRepository<AspNetUser>())
            {
                return repo.GetQuery<AspNetUser>().Where(c => c.Email == email).FirstOrDefault();
            }
        }

        public static AspNetUser GetUserByName(string name)
        {
            using (var repo = new CRUDRepository<AspNetUser>())
            {

                return repo.GetQuery<AspNetUser>()
                    .Where(c => (c.FirstName + " " + c.LastName) == name)
                    .FirstOrDefault();
            }
        }

        public static UsersInConflict UpdateUserInConflict(UsersInConflict uic)
        {
            using (var repo = new CRUDRepository<UsersInConflict>())
            {
                var userinconflict = repo.GetQuery<UsersInConflict>(c => c.IdConflict == uic.IdConflict && c.IdUser == uic.IdUser).FirstOrDefault();
                userinconflict.CompanyName = uic.CompanyName;
                repo.Update(userinconflict);
                UpdateUser(uic.User);
                return uic;
            }
        }

        public static AspNetUser UpdateUser(AspNetUser u)
        {
            using (var repo = new CRUDRepository<AspNetUser>())
            {
                var user = repo.GetQuery<AspNetUser>(c => c.Id == u.Id).FirstOrDefault();
                if (user != null)
                {
                    user.FirstName = u.FirstName;
                    user.LastName = u.LastName;
                    user.Email = u.Email;
                    user.PhotoPath = u.PhotoPath;
                    repo.Update(user);
                    return user;
                }
                return null;
            }
        }

        public static void UpdateUserMainCompany(int idCompany, string iduser)
        {
            using (var repo = new CRUDRepository<UserCompany>())
            {
                var userCompany = repo.GetQuery<UserCompany>(u => u.IdUser == iduser && u.IdCompany == idCompany).FirstOrDefault();
                if (userCompany == null)
                {
                    UserCompany uc = new UserCompany();
                    uc.IdCompany = idCompany;
                    uc.IdUser = iduser;
                    repo.Add(uc);
                }
            }
        }

        public static AspNetUser UpdateUserPersonalInfo(AspNetUser u)
        {
            using (var repo = new CRUDRepository<AspNetUser>())
            {
                var user = repo.GetQuery<AspNetUser>(c => c.Id == u.Id).FirstOrDefault();
                var previousEmail = user.Email;
                if (user != null)
                {
                    user.FirstName = u.FirstName;
                    user.LastName = u.LastName;
                    user.Email = u.Email;
                    user.TelMobile = u.TelMobile;
                    user.DateOfBirth = u.DateOfBirth;
                    repo.Update(user);

                    if (u.Email != previousEmail)
                    {
                        var invitToUpdate = repo.GetQuery<Invitation>(c => c.Email == previousEmail).ToList();
                        foreach (var item in invitToUpdate)
                        {
                            item.Email = u.Email;
                            repo.Update<Invitation>(item);
                            repo.UnitOfWork.SaveChanges();
                        }

                    }

                    return user;
                }
                return null;
            }
        }

        public static List<AspNetUser> UpdateUsers(List<AspNetUser> users)
        {
            List<AspNetUser> toAdd = new List<AspNetUser>();
            foreach (var item in users)
            {
                toAdd.Add(UpdateUser(item));
            }
            return toAdd;
        }

        public static List<AspNetUser> ListArbiters()
        {
            using (var repo = new CRUDRepository<AspNetUser>())
            {
                return repo.GetQuery<AspNetUser>()
                    .Where(c => c.Roles.Any(r => r.Name == "Arbiter"))
                    .Include("ArbiterSkills.Skill")
                    .ToList();
            }
        }

        public static AspNetUser GetFullArbiter(string id)
        {
            using (var repo = new CRUDRepository<AspNetUser>())
            {
                var data = repo.GetQuery<AspNetUser>(c => c.Id == id &&
                                                 c.Roles.Any(r => r.Name == "Arbiter"))
                           .Include("ArbiterSkills.Skill")
                           //.Include("Conflicts.UsersInConflicts.User")
                           //.Include("Conflicts.ConflictType")
                           .Include("ConflictsAssigned.UsersInConflicts.User")
                           .Include("ConflictsAssigned.Arbiter")
                            .Include("ConflictsAssigned.ConflictType")
                           .Include(c => c.ArbiterInformation)
                           .FirstOrDefault();

                if (data != null)
                {
                    data.ConflictsAssigned = data.ConflictsAssigned.Where(c => c.State>= (int) ConflictState.ArbiterAssigned ).OrderByDescending(c => c.Id).ToList();
                }

                if (data.ArbiterInformation != null)
                {
                    data.ArbiterInformation.User = null;
                }
                if(data.ArbiterSkills != null && data.ArbiterSkills.Count> 0)
                {
                    foreach (var item in data.ArbiterSkills)
                    {
                        item.Arbiter = null;
                        item.Skill.ArbiterSkills = null;
                    }
                }



                return data;
            }
        }

        public static AspNetUser GetArbiter(string id)
        {
            using (var repo = new CRUDRepository<AspNetUser>())
            {
                var data = repo.GetQuery<AspNetUser>(c => c.Id == id &&
                                                 c.Roles.Any(r => r.Name == "Arbiter"))
                           .Include("ArbiterSkills.Skill")
                          .Include(c => c.ArbiterInformation)
                           .FirstOrDefault();

                if (data != null)
                {
                    data.ConflictsAssigned = data.ConflictsAssigned.OrderByDescending(c => c.Id).ToList();
                }
                return data;
            }
        }

        public static bool IsArbiterForConflict(int conflictId, string idUser)
        {
            using (var repo = new CRUDRepository<Conflict>())
            {
                var conflict = repo.GetQuery<Conflict>(c => c.Id == conflictId).FirstOrDefault();
                if (conflict != null)
                {
                    return conflict.IdArbiterAssigned == idUser;
                }
                else
                    return false;
            }
        }

        public static Company AddOrUpdateCompany(Company c)
        {
            using (var repo = new CRUDRepository<Company>())
            {
                if (c.Id > 0)
                {
                    var previous = repo.GetQuery<Company>(d => d.Id == c.Id).FirstOrDefault();
                    if (previous != null)
                    {
                        previous.Address1 = c.Address1;
                        previous.Address2 = c.Address2;
                        previous.Address3 = c.Address3;
                        previous.City = c.City;
                        previous.Fonction = c.Fonction;
                        previous.Name = c.Name;
                        previous.PostalCode = c.PostalCode;
                        previous.Siret = c.Siret;
                        previous.TelCompany = c.TelCompany;
                        repo.Update(previous);
                        return previous;
                    }
                    else
                    {
                        c.Id = 0;
                        return repo.Add(c);

                    }

                }
                else
                {
                    var previousBySiret = BLLCompanies.GetCompany(c.Siret);
                    if (previousBySiret != null)
                    {
                        return previousBySiret;
                    }
                    return repo.Add(c);

                }
            }
        }

        public static bool AutoConfirmUser(string idUser)
        {
            using (var repo = new CRUDRepository<AspNetUser>())
            {
                var user = repo.GetQuery<AspNetUser>(c => c.Id == idUser).FirstOrDefault();
                if (user != null)
                {
                    user.EmailConfirmed = true;
                    repo.Update(user);
                    return true;
                }
                return false;
            }
        }

    }
}
