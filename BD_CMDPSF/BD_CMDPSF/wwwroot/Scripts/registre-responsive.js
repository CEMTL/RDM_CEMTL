/* Gestion centrale des tableaux : évite le débordement de la page et
   garde accessibles toutes les colonnes sur téléphone, y compris celles
   des onglets et des fenêtres modales chargés par Ajax. */
(function () {
    'use strict';
    var framePending = false;

    function isAlreadyScrollable(table) {
        return !!table.closest('.table-responsive, .table-scroll-shell, .dataTables_scrollBody');
    }

    function prepareTable(table) {
        if (isAlreadyScrollable(table) || !table.parentNode) return;
        // Ne pas toucher aux tableaux destinés aux documents/impressions.
        if (table.closest('.receipt, .print-only')) return;
        var shell = document.createElement('div');
        shell.className = 'table-scroll-shell';
        table.parentNode.insertBefore(shell, table);
        shell.appendChild(table);
    }

    function refreshOverflow() {
        var containers = document.querySelectorAll('.main-content .table-responsive, .main-content .table-scroll-shell, .modal .table-responsive, .modal .table-scroll-shell');
        containers.forEach(function (container) {
            if (!container.hasAttribute('role')) container.setAttribute('role', 'region');
            if (!container.hasAttribute('aria-label')) container.setAttribute('aria-label', 'Tableau défilant horizontalement');
            if (!container.hasAttribute('tabindex')) container.setAttribute('tabindex', '0');

            var hint = container.previousElementSibling;
            if (!hint || !hint.classList.contains('table-scroll-hint')) {
                hint = document.createElement('div');
                hint.className = 'table-scroll-hint';
                hint.textContent = '↔ Faites glisser le tableau pour afficher toutes les colonnes.';
                container.parentNode.insertBefore(hint, container);
            }
            var hasOverflow = container.clientWidth > 0 && container.scrollWidth > container.clientWidth + 3;
            hint.classList.toggle('is-visible', hasOverflow);
        });
    }

    function scan() {
        document.querySelectorAll('.main-content table.table, .modal table.table').forEach(prepareTable);
        refreshOverflow();
    }

    function queueScan() {
        if (framePending) return;
        framePending = true;
        window.requestAnimationFrame(function () {
            framePending = false;
            scan();
        });
    }

    document.addEventListener('DOMContentLoaded', function () {
        scan();
        var root = document.querySelector('.app-shell');
        if (root && window.MutationObserver) {
            var observer = new MutationObserver(function (mutations) {
                // Ignore les simples mises à jour de texte : aucune nouvelle table.
                if (mutations.some(function (mutation) { return mutation.addedNodes.length || mutation.removedNodes.length; })) {
                    queueScan();
                }
            });
            observer.observe(root, { childList: true, subtree: true });
        }
        window.addEventListener('resize', queueScan);
        document.addEventListener('shown.bs.tab', queueScan);
        document.addEventListener('shown.bs.modal', queueScan);
        if (window.jQuery) {
            window.jQuery(document).on('draw.dt shown.bs.tab shown.bs.modal', queueScan);
        }
    });
})();
