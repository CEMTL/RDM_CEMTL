using System.Linq;
using System.Web.Mvc;
using Solution.Models;

namespace Solution.Controllers
{
    [Authorize]
    public class HomeController : Controller
    {
        private readonly HopitalMedecinsDbEntities db = new HopitalMedecinsDbEntities();

        public ActionResult Index()
        {
            ViewBag.TotalMedecins = db.Medecin.Count();
            ViewBag.MedecinsActifs = db.Medecin.Count(x => x.EstActif);
            ViewBag.TotalProfils = db.MedecinProfilCEMTL.Count();
            ViewBag.TotalCotisations = db.CotisationAnnuelle.Count();
            ViewBag.CotisationsNonReglees = db.CotisationAnnuelle.Count(x => x.EtatCotisation != null && x.EtatCotisation != "Payée" && x.EtatCotisation != "Payee");
            ViewBag.TotalPrivileges = db.DossierPrivilege.Count();
            ViewBag.PrivilegesActifs = db.DossierPrivilege.Count(x => x.EstDossierActif);
            ViewBag.TotalInstallations = db.Installation.Count();
            return View();
        }

        protected override void Dispose(bool disposing)
        {
            if (disposing) db.Dispose();
            base.Dispose(disposing);
        }
    }
}
