# RegistreMedecins MVC Scaffold

- Namespace attendu : `Solution`
- Contexte EDMX attendu : `HopitalMedecinsDbEntities`
- Contrôleurs générés : CRUD LINQ/EDMX avec ViewModels.
- Écritures réservées à Admin et SuperUtilisateur.
- Consultation ouverte à Admin, SuperUtilisateur et Utilisateur.

## Vérifications après génération de l'EDMX
1. Nom exact du contexte EDMX.
2. Nom des EntitySet.
3. Rôles ASP.NET Identity : Admin, SuperUtilisateur, Utilisateur.
