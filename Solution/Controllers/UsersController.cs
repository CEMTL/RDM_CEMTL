using Microsoft.AspNet.Identity;
using Microsoft.AspNet.Identity.Owin;
using Solution.Models;
using Solution.ViewModels;
using System;
using System.Linq;
using System.Threading.Tasks;
using System.Web;
using System.Web.Mvc;

namespace Solution.Controllers
{
    //[Authorize(Roles = "Admin")]
    public class UsersController : Controller
    {
        // ── Accès aux managers OWIN (AspNetUsers / AspNetRoles / AspNetUserRoles) ──
        private ApplicationUserManager UserManager =>
            HttpContext.GetOwinContext().GetUserManager<ApplicationUserManager>();

        private ApplicationRoleManager RoleManager =>
            HttpContext.GetOwinContext().Get<ApplicationRoleManager>();

        private ApplicationDbContext db = new ApplicationDbContext();

        // ────────────────────────────────────────────────────────────────────
        // GET /Users  →  liste AspNetUsers + rôles (AspNetUserRoles)
        // ────────────────────────────────────────────────────────────────────
        public async Task<ActionResult> Index()
        {
            var users = UserManager.Users.OrderBy(u => u.Nom).ToList();
            var vm = new System.Collections.Generic.List<UserIndexViewModel>();

            foreach (var u in users)
            {
                var roles = await UserManager.GetRolesAsync(u.Id); // lit AspNetUserRoles
                vm.Add(new UserIndexViewModel
                {
                    Id = u.Id,
                    Nom = u.Nom,
                    Prenom = u.Prenom,
                    UserName = u.UserName,
                    Email = u.Email,
                    Roles = roles.Any() ? string.Join(", ", roles) : "—",
                    EstActif = u.LockoutEnabled
                });
            }

            return View(vm);
        }

        // ────────────────────────────────────────────────────────────────────
        // GET /Users/Create
        // ────────────────────────────────────────────────────────────────────
        public ActionResult Create()
        {
            return View(new UserCreateViewModel
            {
                RolesDisponibles = RolesEnSelectList()
            });
        }

        // POST /Users/Create  →  INSERT AspNetUsers + AspNetUserRoles
        [HttpPost, ValidateAntiForgeryToken]
        public async Task<ActionResult> Create(UserCreateViewModel vm)
        {
            if (!ModelState.IsValid)
            {
                vm.RolesDisponibles = RolesEnSelectList();
                return View(vm);
            }

            var user = new ApplicationUser
            {
                Nom = vm.Nom,
                Prenom = vm.Prenom,
                Email = vm.Email,
                UserName = vm.UserName,
                PhoneNumber = vm.PhoneNumber
            };

            // INSERT dans AspNetUsers
            var result = await UserManager.CreateAsync(user, vm.Password);
            if (!result.Succeeded)
            {
                AddErrors(result);
                vm.RolesDisponibles = RolesEnSelectList();
                return View(vm);
            }

            // INSERT dans AspNetUserRoles
            if (vm.RolesSelectionnes?.Any() == true)
                await UserManager.AddToRolesAsync(user.Id, vm.RolesSelectionnes.ToArray());

            TempData["Success"] = $"Utilisateur « {user.FullName} » créé avec succès.";
            return RedirectToAction("Index");
        }


        public async Task<ActionResult> Edit(string id)
        {
            if (id == null)
                return new HttpStatusCodeResult(System.Net.HttpStatusCode.BadRequest);

            var user = await UserManager.FindByIdAsync(id);
            if (user == null)
                return HttpNotFound();

            var roles = await UserManager.GetRolesAsync(id);

            return View(new UserEditViewModel
            {
                Id = user.Id,
                Nom = user.Nom,
                Prenom = user.Prenom,
                Email = user.Email,
                UserName = user.UserName,
                PhoneNumber = user.PhoneNumber,
                RolesSelectionnes = roles.ToList(),
                RolesDisponibles = RolesEnSelectList(roles.ToList())
            });
        }

        [HttpPost]
        [ValidateAntiForgeryToken]
        [Authorize(Roles = "Admin")]
        public async Task<ActionResult> Edit(UserEditViewModel model, string[] RolesSelectionnes)
        {
            if (model == null || string.IsNullOrWhiteSpace(model.Id))
            {
                return HttpNotFound();
            }

            var user = await UserManager.FindByIdAsync(model.Id);
            if (user == null)
            {
                return HttpNotFound();
            }

            var currentRoles = (await UserManager.GetRolesAsync(user.Id)).ToList();

            var selectedRoles = (RolesSelectionnes ?? new string[] { })
                .Where(x => !string.IsNullOrWhiteSpace(x))
                .Distinct(StringComparer.OrdinalIgnoreCase)
                .ToList();

            var connectedUserId = User.Identity.GetUserId();
            var isSelf = string.Equals(user.Id, connectedUserId, StringComparison.OrdinalIgnoreCase);
            var estCompteAdmin = string.Equals((user.UserName ?? "").Trim(), "Admin", StringComparison.OrdinalIgnoreCase);

            bool etaitAdmin = currentRoles.Any(r => r.Equals("Admin", StringComparison.OrdinalIgnoreCase));
            bool adminSoumis = selectedRoles.Any(r => r.Equals("Admin", StringComparison.OrdinalIgnoreCase));

            // Champs modifiables
            user.Nom = model.Nom;
            user.Prenom = model.Prenom;
            user.Email = model.Email;
            user.PhoneNumber = model.PhoneNumber;

            // Le compte "Admin" garde toujours le même UserName
            if (estCompteAdmin)
            {
                user.UserName = "Admin";

                if (!string.Equals((model.UserName ?? "").Trim(), "Admin", StringComparison.OrdinalIgnoreCase))
                {
                    ModelState.AddModelError("UserName", "Le nom d'utilisateur du compte 'Admin' ne peut pas être modifié.");
                }
            }
            else
            {
                user.UserName = model.UserName;
            }

            // Le compte "Admin" garde tous ses rôles inchangés
            if (estCompteAdmin)
            {
                bool rolesOntChange =
                    currentRoles.Except(selectedRoles, StringComparer.OrdinalIgnoreCase).Any() ||
                    selectedRoles.Except(currentRoles, StringComparer.OrdinalIgnoreCase).Any();

                if (rolesOntChange)
                {
                    ModelState.AddModelError("", "Les rôles du compte utilisateur 'Admin' ne peuvent pas être modifiés.");
                }

                selectedRoles = currentRoles.ToList();
            }

            // Un admin ne peut pas se retirer lui-même le rôle Admin
            if (isSelf && etaitAdmin && !adminSoumis)
            {
                ModelState.AddModelError("", "Vous ne pouvez pas retirer votre propre rôle Admin.");

                if (!selectedRoles.Contains("Admin", StringComparer.OrdinalIgnoreCase))
                {
                    selectedRoles.Add("Admin");
                }
            }

            if (!ModelState.IsValid)
            {
                model.UserName = user.UserName;
                model.RolesSelectionnes = selectedRoles;
                model.RolesDisponibles = RolesEnSelectList(selectedRoles);
                return View(model);
            }

            var updateUserResult = await UserManager.UpdateAsync(user);
            if (!updateUserResult.Succeeded)
            {
                foreach (var err in updateUserResult.Errors)
                {
                    ModelState.AddModelError("", err);
                }

                model.UserName = user.UserName;
                model.RolesSelectionnes = selectedRoles;
                model.RolesDisponibles = RolesEnSelectList(selectedRoles);
                return View(model);
            }

            var rolesToAdd = selectedRoles
                .Except(currentRoles, StringComparer.OrdinalIgnoreCase)
                .ToArray();

            var rolesToRemove = currentRoles
                .Except(selectedRoles, StringComparer.OrdinalIgnoreCase)
                .ToArray();

            if (rolesToRemove.Any())
            {
                var removeResult = await UserManager.RemoveFromRolesAsync(user.Id, rolesToRemove);
                if (!removeResult.Succeeded)
                {
                    foreach (var err in removeResult.Errors)
                    {
                        ModelState.AddModelError("", err);
                    }
                }
            }

            if (rolesToAdd.Any())
            {
                var addResult = await UserManager.AddToRolesAsync(user.Id, rolesToAdd);
                if (!addResult.Succeeded)
                {
                    foreach (var err in addResult.Errors)
                    {
                        ModelState.AddModelError("", err);
                    }
                }
            }

            if (!ModelState.IsValid)
            {
                model.UserName = user.UserName;
                model.RolesSelectionnes = selectedRoles;
                model.RolesDisponibles = RolesEnSelectList(selectedRoles);
                return View(model);
            }

            TempData["Success"] = "L'utilisateur a été modifié avec succès.";
            return RedirectToAction("Index");
        }

        [HttpPost]
        [ValidateAntiForgeryToken]
        [Authorize]
        public async Task<ActionResult> ToggleActive(string id, bool isActive)
        {
            if (string.IsNullOrWhiteSpace(id))
            {
                TempData["Erreur"] = "Identifiant utilisateur invalide.";
                return RedirectToAction("Index");
            }

            var user = await UserManager.FindByIdAsync(id);
            if (user == null)
            {
                TempData["Erreur"] = "Utilisateur introuvable.";
                return RedirectToAction("Index");
            }

            // Le compte "Admin" ne peut jamais être désactivé
            if (!isActive && string.Equals((user.UserName ?? "").Trim(), "Admin", StringComparison.OrdinalIgnoreCase))
            {
                TempData["Erreur"] = "Le compte utilisateur 'Admin' ne peut pas être désactivé.";
                return RedirectToAction("Index");
            }

            user.LockoutEnabled = isActive;

            var result = await UserManager.UpdateAsync(user);
            if (!result.Succeeded)
            {
                TempData["Erreur"] = string.Join(" | ", result.Errors);
                return RedirectToAction("Index");
            }

            TempData["Success"] = isActive
                ? "L'utilisateur a été activé avec succès."
                : "L'utilisateur a été désactivé avec succès.";

            return RedirectToAction("Index");
        }

        [HttpPost, ValidateAntiForgeryToken]
        public async Task<ActionResult> Delete(string id)
        {
            if (id == User.Identity.GetUserId())
            {
                TempData["Erreur"] = "Vous ne pouvez pas supprimer votre propre compte.";
                return RedirectToAction("Index");
            }

            var user = await UserManager.FindByIdAsync(id);
            if (user == null)
                return HttpNotFound();

            // Le compte "Admin" ne peut jamais être supprimé
            if (string.Equals((user.UserName ?? "").Trim(), "Admin", StringComparison.OrdinalIgnoreCase))
            {
                TempData["Erreur"] = "Le compte utilisateur 'Admin' ne peut pas être supprimé.";
                return RedirectToAction("Index");
            }

            await UserManager.DeleteAsync(user);

            TempData["Success"] = $"Utilisateur « {user.FullName} » supprimé.";
            return RedirectToAction("Index");
        }
        // ── Helpers ─────────────────────────────────────────────────────────
        private System.Collections.Generic.List<SelectListItem> RolesEnSelectList(
            System.Collections.Generic.List<string> selectionnes = null)
        {
            // .ToList() avant le Select pour sortir de LINQ-to-Entities
            // et éviter "Cannot compare elements of type List<string>"
            return RoleManager.Roles
                .OrderBy(r => r.Name)
                .ToList()                           // ← matérialisation en mémoire
                .Select(r => new SelectListItem
                {
                    Value = r.Name,
                    Text = r.Name,
                    Selected = selectionnes != null && selectionnes.Contains(r.Name)
                }).ToList();
        }

        private void AddErrors(IdentityResult result)
        {
            foreach (var e in result.Errors)
                ModelState.AddModelError("", e);
        }
    }
}
