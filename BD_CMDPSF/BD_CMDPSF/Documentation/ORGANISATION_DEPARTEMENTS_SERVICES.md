# Page Organisation médicale

La page de consultation est accessible à l’adresse `/Departements/Organisation` et depuis le menu **Référentiels > Organisation médicale**.

Elle affiche les départements actifs avec les affectations organisationnelles courantes :

- chef de département et date de début de la nomination administrative;
- secrétaire de département;
- adjoint de site dont le site source est « Santa Cabrini »;
- bouton ouvrant la liste des services actifs du département.

La modale des services présente le chef de service, son titre et son courriel. Plusieurs responsables peuvent être affichés pour un même département ou service. Les affectations inactives, expirées ou qui ne sont pas encore commencées sont exclues. Les éléments marqués intérimaires ou à valider sont signalés visuellement.

Tous les utilisateurs authentifiés peuvent consulter les détails. Les utilisateurs ayant le rôle `Admin` ou `SuperUtilisateur` disposent directement sur cette page des commandes permettant :

- d’ajouter, modifier ou supprimer un département;
- d’ajouter, modifier ou supprimer un service;
- d’ajouter, modifier ou supprimer un chef de département, un secrétaire, un adjoint de site ou un chef de service.

Les suppressions utilisent les écrans de confirmation existants et conservent leurs contrôles d’intégrité. Un service lié à une installation, à un privilège ou à une affectation ne peut pas être supprimé.

Les cellules de données sont strictement informatives. Toutes les commandes de consultation, modification et suppression sont regroupées dans la colonne **Actions**, y compris celles des affectations organisationnelles.

Un département ne peut avoir qu’un seul chef actif et un service ne peut avoir qu’un seul chef actif. Ce contrôle est appliqué côté serveur lors de la création, de la réactivation ou du déplacement d’une affectation. Les boutons d’ajout correspondants sont également masqués lorsqu’un chef actif existe déjà.

Dans la colonne **Actions**, les boutons secondaires **Responsable/Chef** et **Affectations** sont affichés sur une rangée sous les boutons Détails/Modifier/Supprimer.

Le bouton **Voir les services** est suivi du nombre de services dans un compteur blanc à texte bleu foncé afin d’assurer un contraste clair.

La page lit uniquement `Departement`, `Service` et `AffectationOrganisationnelle`. Elle n’utilise pas `MedecinProfilCEMTL`.
