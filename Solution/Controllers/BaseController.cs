// Controllers/BaseController.cs
using System;
using System.Web.Mvc;
using Solution.Models;
using Solution.ViewModels;


namespace Solution.ViewModels
{
    [Authorize]
    public abstract class BaseController : Controller
    {
        protected HopitalMedecinsDbEntities db = new HopitalMedecinsDbEntities();

        protected override void OnActionExecuting(ActionExecutingContext filterContext)
        {
            ViewBag.CurrentUser = User.Identity.Name;
            base.OnActionExecuting(filterContext);
        }

        protected override void Dispose(bool disposing)
        {
            if (disposing)
            {
                db.Dispose();
            }
            base.Dispose(disposing);
        }

        protected void SetSuccessMessage(string message)
        {
            TempData["SuccessMessage"] = message;
        }

        protected void SetErrorMessage(string message)
        {
            TempData["ErrorMessage"] = message;
        }
    }
}