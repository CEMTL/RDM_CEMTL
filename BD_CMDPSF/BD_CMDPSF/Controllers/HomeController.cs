using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Diagnostics;
using Microsoft.Extensions.Logging;
using Microsoft.AspNetCore.Mvc;
using Solution.Services;

namespace Solution.Controllers
{
    [Authorize]
    public class HomeController : Controller
    {
        private readonly ILogger<HomeController> _logger;
        private readonly IMedecinAlerteService _alertes;

        public HomeController(ILogger<HomeController> logger, IMedecinAlerteService alertes)
        {
            _logger = logger;
            _alertes = alertes;
        }

        public ActionResult Index()
        {
            // Tableau de bord volontairement synthétique :
            // les indicateurs détaillés restent accessibles dans leurs modules,
            // l'accueil ne présente que ce qui nécessite une attention immédiate.
            return View(_alertes.ConstruireTableauBord(8));
        }
        [AllowAnonymous]
        public ActionResult Error()
        {
            var feature = HttpContext.Features.Get<IExceptionHandlerPathFeature>();
            var exception = feature?.Error;

            if (exception != null)
            {
                _logger.LogError(
                    exception,
                    "Erreur non gérée dans RDM. Chemin={Path}, Reference={Reference}",
                    feature?.Path,
                    HttpContext.TraceIdentifier);
            }

            ViewBag.ErrorMessage = exception != null
                ? UserFriendlyExceptionService.GetMessage(exception)
                : "Une erreur inattendue est survenue. Veuillez réessayer.";
            ViewBag.ErrorReference = HttpContext.TraceIdentifier;

            Response.StatusCode = StatusCodes.Status500InternalServerError;
            return View();
        }
    }
}
