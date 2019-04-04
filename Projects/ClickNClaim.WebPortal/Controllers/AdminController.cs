using ClickNClaim.Business;
using ClickNClaim.Common;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Web;
using System.Web.Mvc;
using System.Web.Security;
using Microsoft.AspNet.Identity.Owin;

using static ClickNClaim.WebPortal.Models.AdminModels;
using System.Threading.Tasks;

namespace ClickNClaim.WebPortal.Controllers
{
   /// [RequireHttps]
    [Authorize(Roles= "Admin,SuperAdmin")]
    public class AdminController : Controller
    {
        // GET: Admin
        public ActionResult Index()
        {
            return View();
        }

        public ActionResult CaseList(int? id, string text,string situation, int nbPage = 0)
        {
            int totalPages = 0;
            var conflicts = BLLConflicts.SearchConflicts(id, text, situation, nbPage, 10, out totalPages);
            return View(new CaseListModel() { Conflicts = conflicts, Page = nbPage, TotalPages = totalPages, id = id, text = text, situation = situation });
        }

        public ActionResult UserList(string text, string role, int nbPage = 0)
        {
            int totalPages = 0;
            var users = BLLUsers.SearchUsers(text, role, 10, nbPage, out totalPages);
            var roles = BLLUsers.ListRoles();
            roles.RemoveAll(c => c.Name == "SuperAdmin");
            return View(new UserListModel() { Users = users, Page = nbPage, TotalPages = totalPages,text = text, role = role, Roles = roles});

        }

        public ActionResult UpdateRoles(string username, string[] roles, string text, string role, int nbPage)
        {
          
            var existing = BLLUsers.ListUserRoles(username);
            var toAdd = roles.Where(c => !existing.Any(d => d.Id == c));
            var toDelete = existing.Where(c => !roles.Contains(c.Id)).ToArray();

            BLLUsers.AddRolesToUser(username, toAdd.ToArray());
            BLLUsers.RemoveRolesToUser(username, toDelete.Select(c => c.Id).ToArray());

            return RedirectToAction("UserList", new { text= text, role =role, nbPage = nbPage});
        }

        public ActionResult UpdateSituation(int conflictId, int situation)
        {
            int totalPages = 0;
            BLLConflicts.UpdateConflictState(conflictId,  situation);
            var conflicts = BLLConflicts.SearchConflicts(null, null, Enum.GetName(typeof(ConflictState),(ConflictState)situation), 0, 10, out totalPages);
            return RedirectToAction("CaseList", new { id = conflictId });

        }

        public async Task<ActionResult> Impersonate(string email)
        {
            var signInManager = HttpContext.GetOwinContext().Get<ApplicationSignInManager>();
            var user = await signInManager.UserManager.FindByEmailAsync(email);
            await signInManager.SignInAsync(user, isPersistent: false, rememberBrowser: false);
            return RedirectToAction("Profil", "Account");
        }


    }
}