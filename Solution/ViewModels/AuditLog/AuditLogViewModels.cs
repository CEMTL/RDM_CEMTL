using System;
using System.Collections.Generic;
using System.Web.Mvc;

namespace Solution.ViewModels.AuditLogs
{
    public class AuditLogIndexViewModel
    {
        public AuditLogIndexViewModel()
        {
            ActionsDisponibles = new List<SelectListItem>();
            Items = new List<AuditLogListItemViewModel>();
        }

        public string TableName { get; set; }
        public string ActionType { get; set; }
        public string Utilisateur { get; set; }
        public string Search { get; set; }
        public DateTime? DateDebut { get; set; }
        public DateTime? DateFin { get; set; }
        public IList<SelectListItem> ActionsDisponibles { get; set; }
        public IList<AuditLogListItemViewModel> Items { get; set; }
    }

    public class AuditLogListItemViewModel
    {
        public int AuditLogId { get; set; }
        public string TableName { get; set; }
        public int RecordId { get; set; }
        public string Action { get; set; }
        public string NomUtilisateur { get; set; }
        public DateTime DateAction { get; set; }
        public string AdresseIP { get; set; }
        public string ChangedFields { get; set; }
    }

    public class AuditLogDetailsViewModel
    {
        public int AuditLogId { get; set; }
        public string TableName { get; set; }
        public int RecordId { get; set; }
        public string Action { get; set; }
        public string OldValues { get; set; }
        public string NewValues { get; set; }
        public string ChangedFields { get; set; }
        public int? UtilisateurId { get; set; }
        public string NomUtilisateur { get; set; }
        public DateTime DateAction { get; set; }
        public string AdresseIP { get; set; }
        public string UserAgent { get; set; }
    }
}
