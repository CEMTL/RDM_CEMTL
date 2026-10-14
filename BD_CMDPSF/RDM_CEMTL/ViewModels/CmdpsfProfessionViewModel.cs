using System;
using System.ComponentModel.DataAnnotations;
namespace Solution.ViewModels;
public class CmdpsfProfessionViewModel
{
    public int MedecinId { get; set; }
    [Required(ErrorMessage = "La profession est obligatoire.")]
    public string CodeProfession { get; set; }
    [Required, StringLength(20)] public string Permis { get; set; }
    [Required, StringLength(150)] public string Nom { get; set; }
    [Required, StringLength(150)] public string Prenom { get; set; }
    public bool EstActif { get; set; } = true;
    [DataType(DataType.Date)] public DateTime? DateEntreeFonction { get; set; }
    [DataType(DataType.Date)] public DateTime? DateDepart { get; set; }
    [DataType(DataType.Date)] public DateTime? DateObtentionPermis { get; set; }
    [DataType(DataType.Date)] public DateTime? DateExpirationPermis { get; set; }
    [StringLength(50)] public string NumeroMembreOrdre { get; set; }
    [DataType(DataType.Date)] public DateTime? DateDerniereVerificationOrdre { get; set; }
    [StringLength(255)] public string ReferenceVerificationOrdre { get; set; }
    [StringLength(4000)] public string Commentaires { get; set; }
}
