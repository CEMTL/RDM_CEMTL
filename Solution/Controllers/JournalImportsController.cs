using System;
using System.Linq;
using System.Net;
using System.Web.Mvc;
using System.Data.Entity;
using Solution.Models;
using Solution.ViewModels.JournalImports;

namespace Solution.Controllers
{
    [Authorize]
    public class JournalImportsController : Controller
    {
        private readonly HopitalMedecinsDbEntities db = new HopitalMedecinsDbEntities();

        public ActionResult Index(string search = null)
        {
            var query = db.JournalImport.AsNoTracking().AsQueryable();
            if (!string.IsNullOrWhiteSpace(search))
            {
                query = query.Where(x => (x.NomFichier != null && x.NomFichier.Contains(search)) || (x.Commentaire != null && x.Commentaire.Contains(search)));
            }
            var items = query
                .OrderBy(x => x.NomFichier)
                .Select(x => new JournalImportListViewModel
                {
                    JournalImportId = x.JournalImportId,
                    NomFichier = x.NomFichier,
                    DateImport = x.DateImport,
                    LignesImportees = x.LignesImportees,
                    LignesRejetees = x.LignesRejetees,
                    Commentaire = x.Commentaire,
                })
                .ToList();
            return View(items);
        }

        public ActionResult Details(int? id)
        {
            if (id == null) return new HttpStatusCodeResult(HttpStatusCode.BadRequest);

            var entity = db.JournalImport

                .AsNoTracking()
                .FirstOrDefault(x => x.JournalImportId == id.Value);

            if (entity == null) return HttpNotFound();

            var vm = new JournalImportDetailsViewModel
            {
            JournalImportId = entity.JournalImportId,
            NomFichier = entity.NomFichier,
            DateImport = entity.DateImport,
            LignesImportees = entity.LignesImportees,
            LignesRejetees = entity.LignesRejetees,
            Commentaire = entity.Commentaire,
            };

            return View(vm);
        }

        [Authorize(Roles = "Admin,SuperUtilisateur")]
        public ActionResult Create()
        {
            var vm = BuildFormViewModel(new JournalImportFormViewModel());
            return View(vm);
        }

        [HttpPost]
        [ValidateAntiForgeryToken]
        [Authorize(Roles = "Admin,SuperUtilisateur")]
        public ActionResult Create(JournalImportFormViewModel model)
        {
            if (!ModelState.IsValid)
            {
                return View(BuildFormViewModel(model));
            }

            var entity = new JournalImport();
            MapToEntity(model, entity);

            db.JournalImport.Add(entity);
            db.SaveChanges();

            TempData["Success"] = "JournalImport créé avec succès.";
            return RedirectToAction("Index");
        }

        [Authorize(Roles = "Admin,SuperUtilisateur")]
        public ActionResult Edit(int? id)
        {
            if (id == null) return new HttpStatusCodeResult(HttpStatusCode.BadRequest);

            var entity = db.JournalImport.FirstOrDefault(x => x.JournalImportId == id.Value);
            if (entity == null) return HttpNotFound();

            var vm = new JournalImportFormViewModel
            {
                JournalImportId = entity.JournalImportId,
                NomFichier = entity.NomFichier,
                DateImport = entity.DateImport,
                LignesImportees = entity.LignesImportees,
                LignesRejetees = entity.LignesRejetees,
                Commentaire = entity.Commentaire,
            };

            return View(BuildFormViewModel(vm));
        }

        [HttpPost]
        [ValidateAntiForgeryToken]
        [Authorize(Roles = "Admin,SuperUtilisateur")]
        public ActionResult Edit(JournalImportFormViewModel model)
        {
            if (!ModelState.IsValid)
            {
                return View(BuildFormViewModel(model));
            }

            var entity = db.JournalImport.FirstOrDefault(x => x.JournalImportId == model.JournalImportId);
            if (entity == null) return HttpNotFound();

            MapToEntity(model, entity);
            db.SaveChanges();

            TempData["Success"] = "JournalImport modifié avec succès.";
            return RedirectToAction("Index");
        }

        [Authorize(Roles = "Admin,SuperUtilisateur")]
        public ActionResult Delete(int? id)
        {
            if (id == null) return new HttpStatusCodeResult(HttpStatusCode.BadRequest);

            var entity = db.JournalImport

                .AsNoTracking()
                .FirstOrDefault(x => x.JournalImportId == id.Value);

            if (entity == null) return HttpNotFound();

            var vm = new JournalImportDetailsViewModel
            {
            JournalImportId = entity.JournalImportId,
            NomFichier = entity.NomFichier,
            DateImport = entity.DateImport,
            LignesImportees = entity.LignesImportees,
            LignesRejetees = entity.LignesRejetees,
            Commentaire = entity.Commentaire,
            };

            return View(vm);
        }

        [HttpPost, ActionName("Delete")]
        [ValidateAntiForgeryToken]
        [Authorize(Roles = "Admin,SuperUtilisateur")]
        public ActionResult DeleteConfirmed(int id)
        {
            var entity = db.JournalImport.FirstOrDefault(x => x.JournalImportId == id);
            if (entity == null) return HttpNotFound();

            db.JournalImport.Remove(entity);
            db.SaveChanges();

            TempData["Success"] = "JournalImport supprimé avec succès.";
            return RedirectToAction("Index");
        }

        private JournalImportFormViewModel BuildFormViewModel(JournalImportFormViewModel model)
        {
            // aucune liste déroulante
            return model;
        }

        private static void MapToEntity(JournalImportFormViewModel model, JournalImport entity)
        {
            entity.NomFichier = model.NomFichier;
            entity.DateImport = model.DateImport;
            entity.LignesImportees = model.LignesImportees;
            entity.LignesRejetees = model.LignesRejetees;
            entity.Commentaire = model.Commentaire;
        }

        protected override void Dispose(bool disposing)
        {
            if (disposing) db.Dispose();
            base.Dispose(disposing);
        }
    }
}
