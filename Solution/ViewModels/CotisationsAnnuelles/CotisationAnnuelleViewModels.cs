using System;
using System.Collections.Generic;
using System.ComponentModel.DataAnnotations;
using System.Web.Mvc;

namespace Solution.ViewModels.CotisationsAnnuelles
{
    public class CotisationAnnuelleListViewModel
    {
        [Display(Name = "CotisationAnnuelleId")]
        public int CotisationAnnuelleId { get; set; }
        [Display(Name = "Médecin")]
        public int MedecinId { get; set; }
        [Display(Name = "Année")]
        public short AnneeExercice { get; set; }
        [Display(Name = "Code cotisation")]
        public int? CodeCotisationId { get; set; }
        [Display(Name = "Description source")]
        public string DescriptionSource { get; set; }
        [Display(Name = "Montant initial")]
        public decimal? MontantInitial { get; set; }
        [Display(Name = "Réduction")]
        public bool? ReductionAppliquee { get; set; }
        [Display(Name = "Motif réduction")]
        public string MotifReduction { get; set; }
        [Display(Name = "Montant réduction")]
        public decimal? MontantReduction { get; set; }
        [Display(Name = "Pénalité")]
        public bool? PenaliteAppliquee { get; set; }
        [Display(Name = "Texte pénalité")]
        public string PenaliteTexte { get; set; }
        [Display(Name = "Montant pénalité")]
        public decimal? MontantPenalite { get; set; }
        [Display(Name = "Montant final")]
        public decimal? MontantFinal { get; set; }
        [Display(Name = "État")]
        public string EtatCotisation { get; set; }
        [Display(Name = "Montant payé")]
        public decimal? MontantPaye { get; set; }
        [Display(Name = "Date paiement")]
        public DateTime? DatePaiement { get; set; }
        [Display(Name = "Type paiement (texte)")]
        public string TypePaiementTexte { get; set; }
        [Display(Name = "Type paiement")]
        public int? TypePaiementId { get; set; }
        [Display(Name = "Date dépôt")]
        public DateTime? DateDepot { get; set; }
        [Display(Name = "No dépôt")]
        public string NoDepot { get; set; }
        [Display(Name = "Reçu imposition")]
        public string RecuImposition { get; set; }
        [Display(Name = "Date reçu imposition")]
        public DateTime? DateRecuImposition { get; set; }
        public string MedecinNomComplet { get; set; }
        public string CodeCotisationLibelle { get; set; }
        public string TypePaiementLibelle { get; set; }
    }

    public class CotisationAnnuelleFormViewModel
    {
        public int CotisationAnnuelleId { get; set; }
        [Display(Name = "Médecin")]
        public int MedecinId { get; set; }
        [Display(Name = "Année")]
        public short AnneeExercice { get; set; }
        [Display(Name = "Code cotisation")]
        public int? CodeCotisationId { get; set; }
        [Display(Name = "Description source")]
        public string DescriptionSource { get; set; }
        [Display(Name = "Montant initial")]
        [DisplayFormat(DataFormatString = "{0:N2}", ApplyFormatInEditMode = true)]
        public decimal? MontantInitial { get; set; }
        [Display(Name = "Réduction")]
        public bool? ReductionAppliquee { get; set; }
        [Display(Name = "Motif réduction")]
        public string MotifReduction { get; set; }
        [Display(Name = "Montant réduction")]
        [DisplayFormat(DataFormatString = "{0:N2}", ApplyFormatInEditMode = true)]
        public decimal? MontantReduction { get; set; }
        [Display(Name = "Pénalité")]
        public bool? PenaliteAppliquee { get; set; }
        [Display(Name = "Texte pénalité")]
        public string PenaliteTexte { get; set; }
        [Display(Name = "Montant pénalité")]
        [DisplayFormat(DataFormatString = "{0:N2}", ApplyFormatInEditMode = true)]
        public decimal? MontantPenalite { get; set; }
        [Display(Name = "Montant final")]
        [DisplayFormat(DataFormatString = "{0:N2}", ApplyFormatInEditMode = true)]
        public decimal? MontantFinal { get; set; }
        [Display(Name = "État")]
        public string EtatCotisation { get; set; }
        [Display(Name = "Montant payé")]
        [DisplayFormat(DataFormatString = "{0:N2}", ApplyFormatInEditMode = true)]
        public decimal? MontantPaye { get; set; }
        [Display(Name = "Date paiement")]
        [DataType(DataType.Date)]
        public DateTime? DatePaiement { get; set; }
        [Display(Name = "Type paiement (texte)")]
        public string TypePaiementTexte { get; set; }
        [Display(Name = "Type paiement")]
        public int? TypePaiementId { get; set; }
        [Display(Name = "Date dépôt")]
        [DataType(DataType.Date)]
        public DateTime? DateDepot { get; set; }
        [Display(Name = "No dépôt")]
        public string NoDepot { get; set; }
        [Display(Name = "Reçu imposition")]
        public string RecuImposition { get; set; }
        [Display(Name = "Date reçu imposition")]
        [DataType(DataType.Date)]
        public DateTime? DateRecuImposition { get; set; }
        public IEnumerable<SelectListItem> MedecinIdOptions { get; set; }
        public IEnumerable<SelectListItem> CodeCotisationIdOptions { get; set; }
        public IEnumerable<SelectListItem> TypePaiementIdOptions { get; set; }
    }

    public class CotisationAnnuelleDetailsViewModel : CotisationAnnuelleListViewModel
    {
        public string MedecinLibelle { get; set; }
        public string CodeCotisationLibelle1 { get; set; }
        public string TypePaiementLibelle1 { get; set; }
    }
}