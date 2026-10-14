# Correctifs CMDPSF

1. `/Medecins/Index` redirige maintenant vers `/Cmdpsf/Index` avec la profession MEDECIN et les filtres de recherche/actifs. Liens explicites vers `Medecins/Index` remplacés dans les vues et contrôleurs concernés. Les liens vers `Medecins/Edit?id=...` restent fonctionnels pour les médecins existants.
2. Dans `Views/Cmdpsf/Fiche.cshtml`, les erreurs de validation ne sont affichées que lorsque `ModelState` est invalide : pas de bande orange/rouge vide lors de la création des dentistes, pharmaciens ou sages-femmes.
3. Nouveau médecin : `Cmdpsf/Creer?profession=MEDECIN` -> `Medecins/Create`, qui affiche directement le formulaire médical complet existant plutôt que de rediriger vers `Medecins/Edit/0`. Les anciennes URL Edit/0 redirigent vers Create. Après enregistrement le formulaire médical retourne à `Cmdpsf/Details?id=...`.

Aucune modification SQL. Vérifier la compilation .NET, les routes Razor, et les scénarios de création/édition en environnement de test.
