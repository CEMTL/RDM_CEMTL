# Adaptation responsive CMDPSF — 16 octobre 2026

## Objectif
Rendre toutes les pages du registre lisibles sur téléphone et petit écran sans débordement horizontal du document. Les tableaux larges conservent toutes leurs colonnes : le défilement se fait à l'intérieur du tableau.

## Fichiers modifiés
- `Views/Shared/_Layout.cshtml` : chargement du CSS/JavaScript communs, volet mobile refermable et accessible.
- `wwwroot/Content/registre-responsive.css` : grilles, tableaux, champs, pagination, fenêtres modales et menus adaptés selon la largeur.
- `wwwroot/Scripts/registre-responsive.js` : enveloppement automatique des tableaux non protégés, y compris les contenus injectés via Ajax, et affichage d'une indication de défilement sur téléphone.
- `Views/Account/Login.cshtml` : marges et titres ajustés sur téléphone.
- `Views/CotisationsAnnuelles/Recu.cshtml` : reçu et en-tête adaptés aux écrans étroits ; impression inchangée.

## Vérifications locales
- Navigateur Chromium avec largeur de viewport 320, 375, 390, 600, 768, 900, 1024 et 1366 px sur une page de validation simulant le gabarit.
- Aucune sortie de contenu en dehors de la largeur du document sur ces cas de test.
- Vérification de l'accès aux colonnes via les conteneurs de défilement horizontaux.
- Vérification de l'ajout dynamique de tableaux via JavaScript.
- Syntaxe JavaScript vérifiée avec `node --check`.

## Déploiement
1. Ouvrir `Solution.slnx` (ou `Solution.csproj`) dans Visual Studio.
2. Restaurer les paquets NuGet et recompiler.
3. Tester sur votre serveur et avec vos données : tableau de bord, registre CMDPSF, édition des médecins, gestion des privilèges, organisation, modales, exports et reçus.
4. Vider le cache du navigateur après la mise à jour si d'anciens styles restent affichés.

Les sources SQL, modèles et règles métier ne sont pas modifiés. Cette archive contient les sources du projet ; les dossiers de compilation temporaires `.vs`, `bin` et `obj` ont été exclus pour permettre une recompilation propre.
