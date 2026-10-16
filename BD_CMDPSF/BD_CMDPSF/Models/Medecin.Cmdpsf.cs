using System;
namespace Solution.Models;
public partial class Medecin
{
    public int ProfessionId { get; set; }
    public DateTime? DateObtentionPermis { get; set; }
    public DateTime? DateExpirationPermis { get; set; }
    public string NumeroMembreOrdre { get; set; }
    public DateTime? DateDerniereVerificationOrdre { get; set; }
    public string ReferenceVerificationOrdre { get; set; }
    public virtual Profession Profession { get; set; }
}
