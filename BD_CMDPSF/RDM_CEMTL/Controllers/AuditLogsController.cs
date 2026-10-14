using Microsoft.AspNetCore.Mvc.Rendering;
using Microsoft.AspNetCore.Authorization;
using System;
using System.Collections.Generic;
using Microsoft.EntityFrameworkCore;
using System.Linq;
using System.Net;
using Microsoft.AspNetCore.Mvc;
using Solution.Models;
using Solution.ViewModels.AuditLogs;
using Solution.Helpers;

namespace Solution.Controllers
{
    [Authorize(Roles = "Admin,SuperUtilisateur")]
    public class AuditLogsController : Controller
    {
        private readonly HopitalMedecinsDbEntities db;

        public AuditLogsController(HopitalMedecinsDbEntities db)
        {
            this.db = db;
        }

        [HttpGet]
        public ActionResult Index(
            string tableName,
            string actionType,
            string utilisateur,
            string search,
            DateTime? dateDebut,
            DateTime? dateFin,
            bool exportAll = false)
        {
            var nombreMaximum = exportAll && User.IsInRole("Admin")
                ? DataTableRequest.MaximumExportRows
                : 5000;
            var query = db.AuditLog.AsNoTracking().AsQueryable();

            if (!string.IsNullOrWhiteSpace(tableName))
            {
                tableName = tableName.Trim();
                query = query.Where(x => x.TableName.Contains(tableName));
            }

            if (!string.IsNullOrWhiteSpace(actionType))
            {
                actionType = actionType.Trim();
                query = query.Where(x => x.Action == actionType);
            }

            if (!string.IsNullOrWhiteSpace(utilisateur))
            {
                utilisateur = utilisateur.Trim();
                query = query.Where(x => x.NomUtilisateur.Contains(utilisateur));
            }

            if (!string.IsNullOrWhiteSpace(search))
            {
                search = search.Trim();
                query = query.Where(x =>
                    (x.TableName ?? "").Contains(search) ||
                    (x.Action ?? "").Contains(search) ||
                    (x.NomUtilisateur ?? "").Contains(search) ||
                    (x.AdresseIP ?? "").Contains(search) ||
                    (x.OldValues ?? "").Contains(search) ||
                    (x.NewValues ?? "").Contains(search) ||
                    (x.ChangedFields ?? "").Contains(search));
            }

            if (dateDebut.HasValue)
            {
                var debut = dateDebut.Value.Date;
                query = query.Where(x => x.DateAction >= debut);
            }

            if (dateFin.HasValue)
            {
                var finExclusif = dateFin.Value.Date.AddDays(1);
                query = query.Where(x => x.DateAction < finExclusif);
            }

            var model = new AuditLogIndexViewModel
            {
                TableName = tableName,
                ActionType = actionType,
                Utilisateur = utilisateur,
                Search = search,
                DateDebut = dateDebut,
                DateFin = dateFin,
                ActionsDisponibles = new List<SelectListItem>
                {
                    new SelectListItem { Value = "", Text = "-- Toutes --" },
                    new SelectListItem { Value = "INSERT", Text = "INSERT", Selected = actionType == "INSERT" },
                    new SelectListItem { Value = "UPDATE", Text = "UPDATE", Selected = actionType == "UPDATE" },
                    new SelectListItem { Value = "DELETE", Text = "DELETE", Selected = actionType == "DELETE" }
                },
                Items = query
                    .OrderByDescending(x => x.DateAction)
                    .ThenByDescending(x => x.AuditLogId)
                    .Take(nombreMaximum)
                    .ToList()
                    .Select(x => new AuditLogListItemViewModel
                    {
                        AuditLogId = x.AuditLogId,
                        TableName = x.TableName,
                        RecordId = x.RecordId,
                        Action = x.Action,
                        NomUtilisateur = x.NomUtilisateur,
                        DateAction = x.DateAction,
                        AdresseIP = x.AdresseIP,
                        ChangedFields = x.ChangedFields
                    })
                    .ToList()
            };

            return View(model);
        }

        [HttpGet]
        public ActionResult Details(int? id)
        {
            if (!id.HasValue)
                return BadRequest();

            var entity = db.AuditLog.AsNoTracking().FirstOrDefault(x => x.AuditLogId == id.Value);

            if (entity == null)
                return NotFound();

            var model = new AuditLogDetailsViewModel
            {
                AuditLogId = entity.AuditLogId,
                TableName = entity.TableName,
                RecordId = entity.RecordId,
                Action = entity.Action,
                OldValues = entity.OldValues,
                NewValues = entity.NewValues,
                ChangedFields = entity.ChangedFields,
                UtilisateurId = entity.UtilisateurId,
                NomUtilisateur = entity.NomUtilisateur,
                DateAction = entity.DateAction,
                AdresseIP = entity.AdresseIP,
                UserAgent = entity.UserAgent
            };

            return View(model);
        }

        [HttpGet]
        public ActionResult DetailsModal(int? id)
        {
            if (!id.HasValue)
                return BadRequest();

            var entity = db.AuditLog.AsNoTracking().FirstOrDefault(x => x.AuditLogId == id.Value);

            if (entity == null)
                return NotFound();

            var model = new AuditLogDetailsViewModel
            {
                AuditLogId = entity.AuditLogId,
                TableName = entity.TableName,
                RecordId = entity.RecordId,
                Action = entity.Action,
                OldValues = entity.OldValues,
                NewValues = entity.NewValues,
                ChangedFields = entity.ChangedFields,
                UtilisateurId = entity.UtilisateurId,
                NomUtilisateur = entity.NomUtilisateur,
                DateAction = entity.DateAction,
                AdresseIP = entity.AdresseIP,
                UserAgent = entity.UserAgent
            };

            return PartialView("_DetailsModal", model);
        }
}
}
