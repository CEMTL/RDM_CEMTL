using System;
using System.Collections.Generic;

namespace Solution.ViewModels.Shared
{
    public class AlerteMedecinItemViewModel
    {
        public int MedecinId { get; set; }
        public string Medecin { get; set; }
        public string Categorie { get; set; }
        public string Niveau { get; set; } // danger, warning, info
        public string Message { get; set; }
        public DateTime? DateEcheance { get; set; }
        public string Controller { get; set; }
        public string Action { get; set; }
        public int? Id { get; set; }
    }

    public class TableauBordAlertesViewModel
    {
        // Effectifs sans exclusion selon EstActif ou StatutMedecinId.
        public int TotalMedecins { get; set; }
        public int TotalProfessionnels { get; set; }
        public int MedecinsOmnipraticiens { get; set; }
        public int MedecinsSpecialistes { get; set; }
        public int MedecinsNonClasses { get; set; }
        public int Dentistes { get; set; }
        public int Pharmaciens { get; set; }
        public int SagesFemmes { get; set; }
        public int Privileges30Jours { get; set; }
        public int Privileges90Jours { get; set; }
        public int CotisationsNonReglees { get; set; }
        public int FitTestsAEcheance { get; set; }
        public int EntentesFdc90Jours { get; set; }
        public int AffectationsAValider { get; set; }
        public int Documents90Jours { get; set; }
        public DateTime DateGeneration { get; set; }

        public List<AlerteMedecinItemViewModel> Alertes { get; set; } = new();
    }
}
