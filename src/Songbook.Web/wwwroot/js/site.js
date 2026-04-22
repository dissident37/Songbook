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
