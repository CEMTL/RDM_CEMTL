using System;
using System.Linq;
using System.Net;
using System.Web.Mvc;
using System.Data.Entity;
using Solution.Models;
using Solution.ViewModels.Genres;
using Solution.Services;

namespace Solution.Controllers
{
    [Authorize]
    public class GenresController : Controller
    {
        private readonly HopitalMedecinsDbEntities db = new HopitalMedecinsDbEntities();
        private AuditService _audit => new AuditService(db, HttpContext);

        private object CreerSnapshotAudit(Genre entity)
        {
            return new
            {
                entity.GenreId,
                entity.CodeGenre,
                entity.Libelle
            };
        }

        public ActionResult Index(string search = null)
        {
            var query = db.Genre
                .AsNoTracking()
                .AsQueryable();

            if (!string.IsNullOrWhiteSpace(search))
            {
                search = search.Trim();
                query = query.Where(x =>
                    (x.CodeGenre != null && x.CodeGenre.Contains(search)) ||
                    (x.Libelle != null && x.Libelle.Contains(search)));
            }

            var items = query
                .OrderBy(x => x.Libelle)
                .Select(x => new GenreListViewModel
                {
                    GenreId = x.GenreId,
                    CodeGenre = x.CodeGenre,
                    Libelle = x.Libelle
                })
                .ToList();

            ViewBag.Search = search;

            return View(items);
        }

        public ActionResult Details(int? id)
        {
            if (id == null)
                return new HttpStatusCodeResult(HttpStatusCode.BadRequest);

            var entity = db.Genre
                .AsNoTracking()
                .FirstOrDefault(x => x.GenreId == id.Value);

            if (entity == null)
                return HttpNotFound();

            var vm = new GenreDetailsViewModel
            {
                GenreId = entity.GenreId,
                CodeGenre = entity.CodeGenre,
                Libelle = entity.Libelle
            };

            return View(vm);
        }

        [Authorize(Roles = "Admin,SuperUtilisateur")]
        public ActionResult Create()
        {
            var vm = BuildFormViewModel(new GenreFormViewModel());
            return View(vm);
        }

        [HttpPost]
        [ValidateAntiForgeryToken]
        [Authorize(Roles = "Admin,SuperUtilisateur")]
        public ActionResult Create(GenreFormViewModel model)
        {
            if (!ModelState.IsValid)
            {
                return View(BuildFormViewModel(model));
            }

            var entity = new Genre();
            MapToEntity(model, entity);

            db.Genre.Add(entity);
            db.SaveChanges();

            var nouvelleValeur = CreerSnapshotAudit(entity);

            _audit.Enregistrer(
                "Genre",
                entity.GenreId,
                "INSERT",
                null,
                nouvelleValeur);

            db.SaveChanges();

            TempData["Success"] = "Genre créé avec succès.";
            return RedirectToAction("Index");
        }

        [Authorize(Roles = "Admin,SuperUtilisateur")]
        public ActionResult Edit(int? id)
        {
            if (id == null)
                return new HttpStatusCodeResult(HttpStatusCode.BadRequest);

            var entity = db.Genre.FirstOrDefault(x => x.GenreId == id.Value);
            if (entity == null)
                return HttpNotFound();

            var vm = new GenreFormViewModel
            {
                GenreId = entity.GenreId,
                CodeGenre = entity.CodeGenre,
                Libelle = entity.Libelle
            };

            return View(BuildFormViewModel(vm));
        }

        [HttpPost]
        [ValidateAntiForgeryToken]
        [Authorize(Roles = "Admin,SuperUtilisateur")]
        public ActionResult Edit(GenreFormViewModel model)
        {
            if (!ModelState.IsValid)
            {
                return View(BuildFormViewModel(model));
            }

            var entity = db.Genre.FirstOrDefault(x => x.GenreId == model.GenreId);
            if (entity == null)
                return HttpNotFound();

            var ancienneValeur = CreerSnapshotAudit(entity);

            MapToEntity(model, entity);

            var nouvelleValeur = CreerSnapshotAudit(entity);

            _audit.Enregistrer(
                "Genre",
                entity.GenreId,
                "UPDATE",
                ancienneValeur,
                nouvelleValeur);

            db.SaveChanges();

            TempData["Success"] = "Genre modifié avec succès.";
            return RedirectToAction("Index");
        }

        [Authorize(Roles = "Admin,SuperUtilisateur")]
        public ActionResult Delete(int? id)
        {
            if (id == null)
                return new HttpStatusCodeResult(HttpStatusCode.BadRequest);

            var entity = db.Genre
                .AsNoTracking()
                .FirstOrDefault(x => x.GenreId == id.Value);

            if (entity == null)
                return HttpNotFound();

            var vm = new GenreDetailsViewModel
            {
                GenreId = entity.GenreId,
                CodeGenre = entity.CodeGenre,
                Libelle = entity.Libelle
            };

            return View(vm);
        }

        [HttpPost]
        [ActionName("Delete")]
        [ValidateAntiForgeryToken]
        [Authorize(Roles = "Admin,SuperUtilisateur")]
        public ActionResult DeleteConfirmed(int GenreId)
        {
            var entity = db.Genre.FirstOrDefault(x => x.GenreId == GenreId);
            if (entity == null)
                return HttpNotFound();

            var ancienneValeur = CreerSnapshotAudit(entity);

            db.Genre.Remove(entity);

            _audit.Enregistrer(
                "Genre",
                GenreId,
                "DELETE",
                ancienneValeur,
                null);

            db.SaveChanges();

            TempData["Success"] = "Genre supprimé avec succès.";
            return RedirectToAction("Index");
        }

        private GenreFormViewModel BuildFormViewModel(GenreFormViewModel model)
        {
            return model;
        }

        private static void MapToEntity(GenreFormViewModel model, Genre entity)
        {
            entity.CodeGenre = model.CodeGenre;
            entity.Libelle = model.Libelle;
        }

        protected override void Dispose(bool disposing)
        {
            if (disposing) db.Dispose();
            base.Dispose(disposing);
        }
    }
}