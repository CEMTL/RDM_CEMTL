# Corrections CMDPSF — tableau de bord et exports

1. Le compteur **Médecins actifs** du tableau de bord pointe maintenant vers `Cmdpsf/Index?profession=MEDECIN&actifsSeulement=true`. Le bouton « Registre » dans les alertes pointe vers `Cmdpsf/Index?profession=MEDECIN`. Les actions internes `Medecins/Edit` restent inchangées.
2. Les actions d’export `Cmdpsf/Exporter?format=pdf|word|excel` reconstruisent la requête SQL en appliquant profession, recherche et actifsSeulement ; elles utilisent `ToListAsync()` sans `Skip`, `Take` ni limite 100. Le plafond applicatif antérieur de 50 000 enregistrements a été retiré.
3. Le `Take(100)` de `GetRegistre` **reste** volontairement actif pour la pagination DataTables à l’écran ; il ne concerne pas les exports.
4. Prudence : ces générateurs documentaires construisent les fichiers en mémoire. Pour un volume exceptionnel, surveiller la mémoire et la durée de génération, ou implémenter un export asynchrone/en flux.
5. Aucun schéma SQL modifié ; compilation et recette .NET/SQL Server à réaliser dans l’environnement utilisateur.
