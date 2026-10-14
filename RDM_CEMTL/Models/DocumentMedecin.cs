using System;

namespace Solution.Models
{
    /// <summary>
    /// Document administratif rattaché à un médecin et, au besoin,
    /// à une entité métier (privilège, nomination, dossier, Fit Test, etc.).
    /// Le contenu est conservé dans SQL Server pour assurer une sauvegarde
    /// transactionnelle et une sécurité uniforme avec la base RDM.
    /// </summary>
    public class DocumentMedecin
    {
        public int DocumentMedecinId { get; set; }
        public int MedecinId { get; set; }
        public string TypeDocument { get; set; }
        public string NomFichier { get; set; }
        public string ContentType { get; set; }
        public long TailleOctets { get; set; }
        public string HashSHA256 { get; set; }
        public DateTime? DateDocument { get; set; }
        public DateTime? DateExpiration { get; set; }
        public string Description { get; set; }
        public string EntiteLiee { get; set; }
        public int? EntiteId { get; set; }
        public bool Confidentiel { get; set; }
        public byte[] Contenu { get; set; }
        public DateTime DateDepot { get; set; }
        public string DeposePar { get; set; }
        public bool EstActif { get; set; }

        public virtual Medecin Medecin { get; set; }
    }
}
