using System.Linq;
using System.Threading.Tasks;
using System.Web;
using System.Web.Mvc;
using Microsoft.AspNet.Identity;
using Microsoft.AspNet.Identity.EntityFramework;
using Microsoft.AspNet.Identity.Owin;
using Solution.ViewModels;

namespace Solution.Controllers
{
    [Authorize(Roles = "Admin")]
    public class RolesController : Controller
    {
        // ── Accès à AspNetRoles via OWIN ─────────────────────────────────────
        private ApplicationRoleManager RoleManager =>
            HttpContext.GetOwinContext().Get<ApplicationRoleManager>();

        // ────────────────────────────────────────────────────────────────────
        // GET /Roles  →  SELECT AspNetRoles + COUNT(AspNetUserRoles)
        // ────────────────────────────────────────────────────────────────────
        public ActionResult Index()
        {
            var vm = RoleManager.Roles
                .OrderBy(r => r.Name)
                .Select(r => new RoleIndexViewModel
                {
                    Id = r.Id,
                    Name = r.Name,
                    NbUtilisateurs = r.Users.Count  // AspNetUserRoles
                }).ToList();

            return View(vm);
        }

        // ────────────────────────────────────────────────────────────────────
        // GET /Roles/Create
        // ────────────────────────────────────────────────────────────────────
        public ActionResult Create() => View(new RoleFormViewModel());

        // POST /Roles/Create  →  INSERT AspNetRoles
        [HttpPost, ValidateAntiForgeryToken]
        public async Task<ActionResult> Create(RoleFormViewModel vm)
        {
            if (!ModelState.IsValid) return View(vm);

            // Vérifier doublon
            if (await RoleManager.RoleExistsAsync(vm.Name.Trim()))
            {
                ModelState.AddModelError("Name", "Ce rôle existe déjà.");
                return View(vm);
            }

            // INSERT INTO AspNetRoles
            var result = await RoleManager.CreateAsync(new IdentityRole(vm.Name.Trim()));
            if (!result.Succeeded) { AddErrors(result); return View(vm); }

            TempData["Success"] = $"Rôle « {vm.Name} » créé.";
            return RedirectToAction("Index");
        }

        // ────────────────────────────────────────────────────────────────────
        // GET /Roles/Edit/id
        // ────────────────────────────────────────────────────────────────────
        public async Task<ActionResult> Edit(string id)
        {
            if (id == null) return new HttpStatusCodeResult(System.Net.HttpStatusCode.BadRequest);

            // SELECT AspNetRoles WHERE Id = id
            var role = await RoleManager.FindByIdAsync(id);
            if (role == null) return HttpNotFound();

            return View(new RoleFormViewModel { Id = role.Id, Name = role.Name });
        }

        // POST /Roles/Edit  →  UPDATE AspNetRoles
        [HttpPost, ValidateAntiForgeryToken]
        public async Task<ActionResult> Edit(RoleFormViewModel vm)
        {
            if (!ModelState.IsValid) return View(vm);

            var role = await RoleManager.FindByIdAsync(vm.Id);
            if (role == null) return HttpNotFound();

            role.Name = vm.Name.Trim();
            var result = await RoleManager.UpdateAsync(role);
            if (!result.Succeeded) { AddErrors(result); return View(vm); }

            TempData["Success"] = $"Rôle « {vm.Name} » modifié.";
            return RedirectToAction("Index");
        }

        // ────────────────────────────────────────────────────────────────────
        // POST /Roles/Delete/id  →  DELETE AspNetRoles (cascade AspNetUserRoles)
        // ────────────────────────────────────────────────────────────────────
        [HttpPost, ValidateAntiForgeryToken]
        public async Task<ActionResult> Delete(string id)
        {
            var role = await RoleManager.FindByIdAsync(id);
            if (role == null) return HttpNotFound();

            if (role.Users.Any())
            {
                TempData["Erreur"] = $"Impossible de supprimer « {role.Name} » : " +
                                     $"{role.Users.Count} utilisateur(s) y sont assignés.";
                return RedirectToAction("Index");
            }

            await RoleManager.DeleteAsync(role);
            TempData["Success"] = $"Rôle « {role.Name} » supprimé.";
            return RedirectToAction("Index");
        }

        // ── Helpers ─────────────────────────────────────────────────────────
        private void AddErrors(IdentityResult result)
        {
            foreach (var e in result.Errors)
                ModelState.AddModelError("", e);
        }
    }
}
