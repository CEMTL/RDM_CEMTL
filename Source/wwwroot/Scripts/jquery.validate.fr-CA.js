(function ($) {
    "use strict";

    if (!$.validator) {
        return;
    }

    $.extend($.validator.messages, {
        required: "Ce champ est obligatoire.",
        remote: "Veuillez corriger ce champ.",
        email: "Veuillez saisir une adresse courriel valide.",
        url: "Veuillez saisir une adresse URL valide.",
        date: "Veuillez saisir une date valide.",
        dateISO: "Veuillez saisir une date valide au format AAAA-MM-JJ.",
        number: "Veuillez saisir un nombre valide.",
        digits: "Veuillez saisir uniquement des chiffres.",
        equalTo: "Veuillez saisir de nouveau la même valeur.",
        extension: "Veuillez saisir une valeur avec une extension valide.",
        maxlength: $.validator.format("Veuillez ne pas saisir plus de {0} caractères."),
        minlength: $.validator.format("Veuillez saisir au moins {0} caractères."),
        rangelength: $.validator.format("Veuillez saisir entre {0} et {1} caractères."),
        range: $.validator.format("Veuillez saisir une valeur comprise entre {0} et {1}."),
        max: $.validator.format("Veuillez saisir une valeur inférieure ou égale à {0}."),
        min: $.validator.format("Veuillez saisir une valeur supérieure ou égale à {0}."),
        step: $.validator.format("Veuillez saisir un multiple de {0}.")
    });

    // La virgule est le séparateur décimal de fr-CA. Les espaces ordinaires,
    // insécables et fines insécables sont acceptées comme séparateurs de milliers.
    $.validator.methods.number = function (value, element) {
        return this.optional(element) ||
            /^-?(?:\d+|\d{1,3}(?:[ \u00a0\u202f]\d{3})+)(?:[,.]\d+)?$/.test(value);
    };
}(jQuery));
