(function (window, document, $) {
    "use strict";

    var maximumLignes = 50000;

    function textePropre(valeur) {
        if (valeur === null || valeur === undefined) return "";
        if (valeur === true || valeur === "true") return "Oui";
        if (valeur === false || valeur === "false") return "Non";

        var conteneur = document.createElement("div");
        if (valeur && valeur.nodeType) {
            conteneur.appendChild(valeur.cloneNode(true));
        } else {
            conteneur.innerHTML = String(valeur);
        }

        Array.prototype.forEach.call(
            conteneur.querySelectorAll("script, style, button, .btn, .no-export, [data-export-ignore='true']"),
            function (element) { element.remove(); }
        );

        return (conteneur.textContent || conteneur.innerText || "")
            .replace(/\u00a0/g, " ")
            .replace(/\s+/g, " ")
            .trim();
    }

    function estColonneActions(cellule) {
        if (!cellule) return false;
        if (cellule.getAttribute("data-export-ignore") === "true") return true;
        return /^(actions?|opérations?)$/i.test(textePropre(cellule.textContent));
    }

    function trouverTableauPrincipal() {
        var selecteurs = [
            ".modal.in .tab-pane.active table:visible",
            ".modal.in table:visible",
            "main .tab-pane.active table:visible",
            "main table.registre-table:visible",
            "main .dashboard-card table:visible",
            "main table:visible"
        ];

        for (var index = 0; index < selecteurs.length; index++) {
            var tableaux = $(selecteurs[index]).filter(function () {
                return !$(this).closest(".dataTables_scrollHead").length;
            });
            if (!tableaux.length) continue;

            var meilleur = tableaux.get(0);
            tableaux.each(function () {
                if ($(this).find("tbody tr").length > $(meilleur).find("tbody tr").length) {
                    meilleur = this;
                }
            });
            return meilleur;
        }

        return null;
    }

    function colonnesTableau(tableau, parametres) {
        var cellulesEntete = $(tableau).find("thead tr").last().children("th").toArray();
        var resultat = [];

        cellulesEntete.forEach(function (cellule, index) {
            var colonne = parametres && parametres.aoColumns ? parametres.aoColumns[index] : null;
            var titre = textePropre(cellule.textContent);
            var cellulesDonnees = $(tableau).find("tbody tr").map(function () {
                return $(this).children("th,td").get(index);
            }).get().filter(Boolean);
            var colonneBoutonsSansTitre = !titre && cellulesDonnees.length > 0 && cellulesDonnees.every(function (celluleDonnee) {
                return $(celluleDonnee).find("a.btn,button.btn").length > 0 && textePropre(celluleDonnee.innerHTML) === "";
            });

            if (estColonneActions(cellule)) return;
            if (colonneBoutonsSansTitre) return;
            if (colonne && colonne.bVisible === false) return;

            resultat.push({
                index: index,
                titre: titre || ("Colonne " + (index + 1)),
                colonneDataTable: colonne
            });
        });

        return resultat;
    }

    function valeurDataTable(colonne, ligne, indexLigne, parametres) {
        try {
            if (colonne.colonneDataTable && typeof colonne.colonneDataTable.fnGetData === "function") {
                return textePropre(colonne.colonneDataTable.fnGetData(ligne, "display", {
                    settings: parametres,
                    row: indexLigne,
                    col: colonne.index
                }));
            }

            if (Array.isArray(ligne)) return textePropre(ligne[colonne.index]);
            var propriete = colonne.colonneDataTable ? colonne.colonneDataTable.mData : null;
            if (typeof propriete === "string") return textePropre(ligne[propriete]);
            return "";
        } catch (erreur) {
            return "";
        }
    }

    function extraireDataTableClient(tableau, instance, parametres, colonnes) {
        var donnees = instance.rows({ search: "applied", order: "applied" }).data().toArray();
        if (donnees.length > maximumLignes) return Promise.reject(new Error("Le nombre de lignes dépasse la limite de " + maximumLignes + ". Export annulé pour éviter un fichier incomplet."));
        return Promise.resolve({
            Entetes: colonnes.map(function (x) { return x.titre; }),
            Lignes: donnees.map(function (ligne, indexLigne) {
                return colonnes.map(function (colonne) {
                    return valeurDataTable(colonne, ligne, indexLigne, parametres);
                });
            })
        });
    }

    // Les registres paginés sur SQL Server ne chargent que 100 lignes par requête.
    // Récupérer chaque lot en conservant les mêmes filtres et le même ordre.
    // Toute limite doit produire une erreur visible, jamais un export silencieusement tronqué.
    async function extraireDataTableServeur(tableau, instance, parametres, colonnes) {
        var configurationAjax = parametres.ajax;
        var url = typeof configurationAjax === "string" ? configurationAjax : configurationAjax.url;
        var methode = (typeof configurationAjax === "object" && (configurationAjax.type || configurationAjax.method)) || "GET";
        var parametresBase = $.extend(true, {}, instance.ajax.params() || {});
        var total = instance.page.info().recordsDisplay || 0;
        if (total > maximumLignes) {
            throw new Error("La sélection compte " + total + " lignes. Le service d'export accepte au maximum " + maximumLignes + " lignes par document. Réduisez les filtres ou utilisez un export serveur dédié.");
        }
        var toutes = [];
        var tailleLot = 100;
        for (var debut = 0; debut < total; debut += tailleLot) {
            var params = $.extend(true, {}, parametresBase);
            params.start = debut;
            params.length = Math.min(tailleLot, total - debut);
            // Ne jamais demander exportAll: les contrôleurs courants appliquent leurs propres plafonds.
            if (typeof configurationAjax === "object") {
                if (typeof configurationAjax.data === "function") {
                    var supplement = configurationAjax.data(params, parametres);
                    if (supplement) params = supplement;
                } else if (configurationAjax.data) {
                    $.extend(params, configurationAjax.data);
                }
            }
            var reponse = await $.ajax({url:url, type:methode, data:params, dataType:"json", timeout:120000});
            var lignes;
            if (typeof configurationAjax === "object" && typeof configurationAjax.dataSrc === "function") {
                lignes = configurationAjax.dataSrc(reponse);
            } else {
                var source = typeof configurationAjax === "object" && typeof configurationAjax.dataSrc === "string" ? configurationAjax.dataSrc : "data";
                lignes = source ? reponse[source] : reponse;
            }
            if (!Array.isArray(lignes) || lignes.length !== params.length) {
                throw new Error("Export interrompu : le serveur a retourné " + (Array.isArray(lignes) ? lignes.length : 0) + " lignes au lieu de " + params.length + ". Aucun fichier partiel n'a été généré.");
            }
            lignes.forEach(function(ligne, idx) {
                toutes.push(colonnes.map(function(colonne) {
                    return valeurDataTable(colonne, ligne, debut + idx, parametres);
                }));
            });
        }
        return {Entetes:colonnes.map(function(x){return x.titre;}), Lignes:toutes};
    }

    function extraireTableau(tableau) {
        if ($.fn.dataTable && $.fn.dataTable.isDataTable(tableau)) {
            var instance = $(tableau).DataTable();
            var parametres = instance.settings()[0];
            var colonnes = colonnesTableau(tableau, parametres);
            if (!colonnes.length) return Promise.reject(new Error("Aucune colonne exportable n'a été trouvée."));

            if (parametres.oFeatures && parametres.oFeatures.bServerSide) {
                return extraireDataTableServeur(tableau, instance, parametres, colonnes);
            }
            return extraireDataTableClient(tableau, instance, parametres, colonnes);
        }

        var colonnesDom = colonnesTableau(tableau, null);
        var lignes = [];
        $(tableau).find("tbody tr").each(function () {
            var cellules = $(this).children("th,td").toArray();
            if (!cellules.length || $(this).hasClass("child")) return;
            lignes.push(colonnesDom.map(function (colonne) {
                return textePropre(cellules[colonne.index] ? cellules[colonne.index].innerHTML : "");
            }));
        });

        if (lignes.length > maximumLignes) return Promise.reject(new Error("Trop de lignes pour un export complet."));
        return Promise.resolve({
            Entetes: colonnesDom.map(function (x) { return x.titre; }),
            Lignes: lignes
        });
    }

    function normaliserRecherche(valeur) {
        var texte = String(valeur || "").toLocaleLowerCase("fr-CA");
        return typeof texte.normalize === "function"
            ? texte.normalize("NFD").replace(/[\u0300-\u036f]/g, "")
            : texte;
    }

    function appliquerRechercheRapide(donnees, recherche) {
        var termes = normaliserRecherche(recherche).split(/\s+/).filter(Boolean);
        if (!termes.length) return donnees;

        donnees.Lignes = donnees.Lignes.filter(function (ligne) {
            var contenu = normaliserRecherche(ligne.join(" "));
            return termes.every(function (terme) { return contenu.indexOf(terme) >= 0; });
        });
        return donnees;
    }

    function extraireTableauPourExport(tableau) {
        var urlComplet = tableau.getAttribute("data-export-all-url");
        if (!urlComplet) return extraireTableau(tableau);

        var rechercheRapide = "";
        if ($.fn.dataTable && $.fn.dataTable.isDataTable(tableau)) {
            rechercheRapide = $(tableau).DataTable().search() || "";
        }

        return fetch(urlComplet, {
            method: "GET",
            credentials: "same-origin",
            headers: { "X-Requested-With": "XMLHttpRequest" }
        }).then(function (reponse) {
            if (!reponse.ok) throw new Error("Toutes les données n'ont pas pu être chargées pour l'exportation.");
            return reponse.text();
        }).then(function (html) {
            var documentDistant = new DOMParser().parseFromString(html, "text/html");
            var tableauDistant = tableau.id ? documentDistant.getElementById(tableau.id) : null;
            if (!tableauDistant) tableauDistant = documentDistant.querySelector("main table");
            if (!tableauDistant) throw new Error("Le tableau complet est introuvable dans la réponse du serveur.");

            return extraireTableau(tableauDistant).then(function (donnees) {
                return appliquerRechercheRapide(donnees, rechercheRapide);
            });
        });
    }

    function libelleChamp(champ) {
        var id = champ.id;
        var etiquette = id ? document.querySelector("label[for='" + CSS.escape(id) + "']") : null;
        if (!etiquette) etiquette = $(champ).closest("label").get(0);
        if (etiquette) return textePropre(etiquette.textContent);
        return champ.getAttribute("placeholder") || champ.getAttribute("name") || "Champ";
    }

    function valeurChamp(champ) {
        var type = (champ.getAttribute("type") || "").toLowerCase();
        if (type === "checkbox" || type === "radio") return champ.checked ? "Oui" : "Non";
        if (champ.tagName === "SELECT") {
            return Array.prototype.filter.call(champ.options, function (option) { return option.selected; })
                .map(function (option) { return textePropre(option.text); })
                .join(", ");
        }
        return textePropre(champ.value);
    }

    function extrairePageSansTableau() {
        var lignes = [];
        var dejaVus = {};

        $("main .dashboard-kpi:visible").each(function () {
            var titre = textePropre($(this).find(".title").text());
            var valeur = textePropre($(this).find(".value").text());
            var detail = textePropre($(this).find(".detail").text());
            if (titre || valeur) lignes.push([titre || "Indicateur", [valeur, detail].filter(Boolean).join(" — ")]);
        });

        $("main dl:visible").each(function () {
            $(this).children("dt").each(function () {
                var libelle = textePropre(this.textContent);
                var valeur = textePropre($(this).next("dd").text());
                if (libelle && valeur) lignes.push([libelle, valeur]);
            });
        });

        $("main input:visible, main select:visible, main textarea:visible").each(function () {
            var type = (this.getAttribute("type") || "").toLowerCase();
            if (["hidden", "password", "submit", "button", "reset", "file"].indexOf(type) >= 0) return;
            if (this.disabled || !this.name) return;
            var cle = this.name + "|" + (this.value || "");
            if (dejaVus[cle]) return;
            dejaVus[cle] = true;
            var libelle = libelleChamp(this);
            var valeur = valeurChamp(this);
            if (libelle && valeur) lignes.push([libelle, valeur]);
        });

        return Promise.resolve({ Entetes: ["Champ", "Valeur"], Lignes: lignes });
    }

    function filtresActifs() {
        var filtres = [];
        var dejaVus = {};

        $("main .filters-bar:visible input, main .filters-bar:visible select, main .filters-bar:visible textarea").each(function () {
            var type = (this.getAttribute("type") || "").toLowerCase();
            if (["hidden", "submit", "button", "reset"].indexOf(type) >= 0 || this.disabled) return;
            if ((type === "checkbox" || type === "radio") && !this.checked) return;
            var valeur = valeurChamp(this);
            if (!valeur) return;
            var libelle = libelleChamp(this).replace(/[:：]\s*$/, "");
            var texte = libelle + " : " + valeur;
            if (!dejaVus[texte]) {
                dejaVus[texte] = true;
                filtres.push(texte);
            }
        });

        $("main .dataTables_filter input:visible").each(function () {
            var valeur = textePropre(this.value);
            if (!valeur) return;
            var texte = "Recherche rapide : " + valeur;
            if (!dejaVus[texte]) {
                dejaVus[texte] = true;
                filtres.push(texte);
            }
        });

        return filtres.length ? filtres.join(" | ") : "Aucun filtre appliqué";
    }

    function nomFichier(reponse, format) {
        var disposition = reponse.headers.get("Content-Disposition") || "";
        var utf8 = disposition.match(/filename\*=UTF-8''([^;]+)/i);
        if (utf8) {
            try { return decodeURIComponent(utf8[1]); } catch (erreur) { /* Valeur de repli. */ }
        }
        var simple = disposition.match(/filename=\"?([^\";]+)\"?/i);
        if (simple) return simple[1];
        var extensions = { pdf: ".pdf", word: ".docx", excel: ".xlsx" };
        return "Exportation_RDM" + (extensions[format] || "");
    }

    function messageErreur(reponse) {
        return reponse.text().then(function (texte) {
            try {
                var objet = JSON.parse(texte);
                return objet.message || "L'exportation a échoué.";
            } catch (erreur) {
                return texte || "L'exportation a échoué.";
            }
        });
    }

    function telecharger(url, jeton, demande) {
        return fetch(url, {
            method: "POST",
            credentials: "same-origin",
            headers: {
                "Content-Type": "application/json; charset=utf-8",
                "RequestVerificationToken": jeton
            },
            body: JSON.stringify(demande)
        }).then(function (reponse) {
            if (!reponse.ok) {
                return messageErreur(reponse).then(function (message) { throw new Error(message); });
            }
            var fichier = nomFichier(reponse, demande.Format);
            return reponse.blob().then(function (blob) {
                var lien = document.createElement("a");
                var objetUrl = URL.createObjectURL(blob);
                lien.href = objetUrl;
                lien.download = fichier;
                document.body.appendChild(lien);
                lien.click();
                lien.remove();
                window.setTimeout(function () { URL.revokeObjectURL(objetUrl); }, 1000);
            });
        });
    }

    function initialiser() {
        var barre = document.getElementById("adminExportToolbar");
        if (!barre) return;

        var boutons = barre.querySelectorAll("[data-export-format]");
        var statut = barre.querySelector(".admin-export-status");
        var jeton = barre.querySelector("input[name='__RequestVerificationToken']").value;
        var url = barre.getAttribute("data-export-url");
        var titre = barre.getAttribute("data-export-title") || document.title || "Exportation RDM";

        function attente(active) {
            Array.prototype.forEach.call(boutons, function (bouton) { bouton.disabled = active; });
            barre.classList.toggle("is-exporting", active);
        }

        Array.prototype.forEach.call(boutons, function (bouton) {
            bouton.addEventListener("click", function () {
                var format = bouton.getAttribute("data-export-format");
                if (window.location.pathname.toLowerCase().replace(/\/$/, "") === "/cmdpsf" ||
                    window.location.pathname.toLowerCase().replace(/\/$/, "") === "/cmdpsf/index") {
                    var params = new URLSearchParams({
                        format: format,
                        profession: $("#profession").val() || "",
                        recherche: $("#recherche").val() || "",
                        actifsSeulement: $("#actifsSeulement").is(":checked") ? "true" : "false"
                    });
                    window.location.href = "/Cmdpsf/Exporter?" + params.toString();
                    return;
                }
                var tableau = trouverTableauPrincipal();
                attente(true);
                statut.className = "admin-export-status text-muted";
                statut.textContent = "Préparation des données filtrées…";

                var extraction = tableau ? extraireTableauPourExport(tableau) : extrairePageSansTableau();
                extraction.then(function (donnees) {
                    if (!donnees.Entetes.length || !donnees.Lignes.length) {
                        throw new Error("Aucune donnée ne correspond aux filtres actuellement appliqués.");
                    }

                    statut.textContent = "Création du document…";
                    return telecharger(url, jeton, {
                        Format: format,
                        Titre: titre,
                        Filtres: filtresActifs(),
                        PageSource: window.location.pathname + window.location.search,
                        Entetes: donnees.Entetes,
                        Lignes: donnees.Lignes
                    }).then(function () {
                        statut.className = "admin-export-status text-success";
                        statut.textContent = donnees.Lignes.length + " ligne(s) exportée(s).";
                    });
                }).catch(function (erreur) {
                    statut.className = "admin-export-status text-danger";
                    statut.textContent = erreur && erreur.message ? erreur.message : "L'exportation a échoué.";
                }).then(function () {
                    attente(false);
                });
            });
        });
    }

    $(initialiser);
})(window, document, window.jQuery);
