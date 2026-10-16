namespace Solution.Models
{
    public static class RoleAffectationOrganisationnelle
    {
        public const string ChefDepartement = "CHEF_DEPARTEMENT";
        public const string SecretaireDepartement = "SECRETAIRE_DEPARTEMENT";
        public const string AdjointSite = "ADJOINT_SITE";
        public const string ChefService = "CHEF_SERVICE";

        public static readonly string[] Tous =
        {
            ChefDepartement,
            SecretaireDepartement,
            AdjointSite,
            ChefService
        };

        public static string Libelle(string roleCode)
        {
            return roleCode switch
            {
                ChefDepartement => "Chef de département",
                SecretaireDepartement => "Secrétaire de département",
                AdjointSite => "Adjoint de site",
                ChefService => "Chef de service",
                _ => roleCode ?? ""
            };
        }
    }
}
