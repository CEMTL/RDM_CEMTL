namespace Solution.Models
{
    using System.Collections.Generic;

    public partial class TypeCourriel
    {
        public TypeCourriel()
        {
            MedecinCourriels = new HashSet<MedecinCourriel>();
        }

        public int TypeCourrielId { get; set; }
        public string Libelle { get; set; }
        public bool EstActif { get; set; }

        public virtual ICollection<MedecinCourriel> MedecinCourriels { get; set; }
    }
}
