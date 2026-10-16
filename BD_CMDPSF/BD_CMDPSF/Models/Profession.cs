using System.Collections.Generic;
namespace Solution.Models;
public class Profession
{
    public int ProfessionId { get; set; }
    public string CodeProfession { get; set; }
    public string Libelle { get; set; }
    public bool EstActif { get; set; }
    public virtual ICollection<Medecin> Medecins { get; set; } = new List<Medecin>();
}
