// Thema-Umschalter fuer Song-Text (hell/dunkel)
(function () {
    var STORAGE_KEY = 'sb-song-theme';
    var html = document.documentElement;

    // Gespeichertes Thema beim Laden wiederherstellen
    var saved = localStorage.getItem(STORAGE_KEY);
    if (saved === 'light') {
        html.setAttribute('data-song-theme', 'light');
    }

    document.addEventListener('DOMContentLoaded', function () {
        var btn = document.getElementById('theme-toggle');
        if (!btn) return;

        btn.addEventListener('click', function () {
            var current = html.getAttribute('data-song-theme');
            var next = current === 'light' ? 'dark' : 'light';
            html.setAttribute('data-song-theme', next);
            localStorage.setItem(STORAGE_KEY, next);
        });
    });
})();

// Подсветка аккордов: клик по span → группа в панели
(function () {
    document.addEventListener('DOMContentLoaded', function () {
        var songBody = document.querySelector('.song-body');
        var panel = document.getElementById('chord-panel');
        if (!songBody || !panel) return;

        var active = null;

        function deactivate() {
            if (!active) return;
            Array.from(songBody.querySelectorAll('span.chord.is-active'))
                 .forEach(function (el) { el.classList.remove('is-active'); });
            Array.from(panel.querySelectorAll('.chord-group.is-active'))
                 .forEach(function (el) { el.classList.remove('is-active'); });
            active = null;
        }

        function activate(name) {
            if (active === name) { deactivate(); return; }
            deactivate();
            active = name;

            Array.from(songBody.querySelectorAll('span.chord'))
                 .filter(function (el) { return el.dataset.chord === name; })
                 .forEach(function (el) { el.classList.add('is-active'); });

            var group = document.getElementById('cg-' + encodeURIComponent(name));
            if (group) {
                group.classList.add('is-active');
                group.scrollIntoView({ behavior: 'smooth', block: 'nearest' });
            }
        }

        songBody.addEventListener('click', function (e) {
            var span = e.target.closest('span.chord');
            if (span) activate(span.dataset.chord);
        });

        document.addEventListener('click', function (e) {
            if (!songBody.contains(e.target) && !panel.contains(e.target))
                deactivate();
        });
    });
})();
