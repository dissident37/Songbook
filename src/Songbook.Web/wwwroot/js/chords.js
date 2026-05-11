// Akkord-Diagramme mit svguitar + chords-db rendern
(function () {
    var SUFFIX_MAP = [
        ['maj7b5',  'maj7b5'],
        ['maj7#5',  'maj7#5'],
        ['maj7',    'maj7'],
        ['m7b5',    'm7b5'],
        ['aug7',    'aug7'],
        ['dim7',    'dim7'],
        ['7sus4',   '7sus4'],
        ['sus2',    'sus2'],
        ['sus4',    'sus4'],
        ['7b5',     '7b5'],
        ['7b9',     '7b9'],
        ['7#9',     '7#9'],
        ['aug9',    'aug9'],
        ['9b5',     '9b5'],
        ['9#11',    '9#11'],
        ['m7',      'm7'],
        ['m9',      'm9'],
        ['m11',     'm11'],
        ['m13',     'm13'],
        ['m6',      'm6'],
        ['m69',     'm69'],
        ['dim',     'dim'],
        ['aug',     'aug'],
        ['sus',     'sus'],
        ['m',       'minor'],
        ['7',       '7'],
        ['9',       '9'],
        ['11',      '11'],
        ['13',      '13'],
        ['6',       '6'],
        ['69',      '69'],
        ['5',       '5'],
        ['',        'major'],
    ];

    var KEY_MAP = {
        'C#': 'Csharp', 'Db': 'Csharp',
        'D#': 'Eb',     'Eb': 'Eb',
        'F#': 'Fsharp', 'Gb': 'Fsharp',
        'G#': 'Ab',     'Ab': 'Ab',
        'A#': 'Bb',     'Bb': 'Bb',
        'C': 'C', 'D': 'D', 'E': 'E', 'F': 'F', 'G': 'G', 'A': 'A', 'B': 'B',
    };

    function parseChordName(name) {
        var keys = ['C#', 'Db', 'D#', 'Eb', 'F#', 'Gb', 'G#', 'Ab', 'A#', 'Bb', 'C', 'D', 'E', 'F', 'G', 'A', 'B'];
        for (var i = 0; i < keys.length; i++) {
            if (name.indexOf(keys[i]) === 0) {
                var key = KEY_MAP[keys[i]];
                var rest = name.slice(keys[i].length);
                for (var j = 0; j < SUFFIX_MAP.length; j++) {
                    if (rest === SUFFIX_MAP[j][0]) return { key: key, suffix: SUFFIX_MAP[j][1] };
                }
                return null;
            }
        }
        return null;
    }

    function toSvguitarChord(position) {
        var fingers = [];
        var frets = position.frets;
        var fingerNums = position.fingers;
        var base = position.baseFret || 1;
        for (var s = 0; s < frets.length; s++) {
            var fret = frets[s];
            var str = s + 1;
            if (fret === -1) {
                fingers.push([str, 'x']);
            } else if (fret === 0) {
                fingers.push([str, 0]);
            } else {
                fingers.push([str, fret, fingerNums[s] > 0 ? String(fingerNums[s]) : '']);
            }
        }
        var barres = [];
        if (position.barres && position.barres.length > 0) {
            position.barres.forEach(function (fret) {
                barres.push({ fret: fret, fromString: 1, toString: 6 });
            });
        }
        return { fingers: fingers, barres: barres, position: base > 1 ? base : undefined };
    }

    function renderSvg(el, db, name) {
        var parsed = parseChordName(name);
        if (!parsed) { console.warn('[chords] nicht geparst:', name); return null; }

        var keyChords = db.chords[parsed.key];
        if (!keyChords) { console.warn('[chords] key nicht gefunden:', parsed.key); return null; }

        var entry = null;
        for (var i = 0; i < keyChords.length; i++) {
            if (keyChords[i].suffix === parsed.suffix) { entry = keyChords[i]; break; }
        }
        if (!entry || !entry.positions.length) { console.warn('[chords] suffix nicht gefunden:', parsed.suffix, 'fuer', name); return null; }

        try {
            var chart = new svguitar.SVGuitarChord(el);
            chart.configure({
                strings: 6,
                frets: 4,
                orientation: 'horizontal',
                color: '#e8a838',
                fretColor: '#aaa',
                fingerColor: '#e8a838',
                fingerTextColor: '#111',
                fontFamily: 'inherit',
            }).chord(toSvguitarChord(entry.positions[0])).draw();

            var svg = el.querySelector('svg');
            if (svg) {
                svg.removeAttribute('width');
                svg.removeAttribute('height');
                svg.style.width = '100%';
                svg.style.height = '100%';
                svg.style.transform = 'scaleX(-1)';
            }
            return svg;
        } catch (e) {
            console.error('[chords] svguitar Fehler fuer', name, e);
            return null;
        }
    }

    function setupTooltip(svgCache) {
        var songBody = document.querySelector('.song-body');
        if (!songBody) return;

        var tooltip = document.createElement('div');
        tooltip.className = 'chord-tooltip';
        document.body.appendChild(tooltip);

        var hoverTarget = null;

        function showTooltip(span) {
            var name = span.dataset.chord;
            var cached = svgCache[name];
            if (!cached) return;

            tooltip.innerHTML = '';
            tooltip.appendChild(cached.cloneNode(true));
            tooltip.style.display = 'block';

            var rect = span.getBoundingClientRect();
            var tw = tooltip.offsetWidth;
            var th = tooltip.offsetHeight;
            var left = rect.left;
            var top = rect.bottom + 6;
            if (left + tw > window.innerWidth - 8) left = window.innerWidth - tw - 8;
            if (left < 8) left = 8;
            if (top + th > window.innerHeight - 8) top = rect.top - th - 6;
            tooltip.style.left = left + 'px';
            tooltip.style.top = top + 'px';
        }

        songBody.addEventListener('mouseover', function (e) {
            var span = e.target.closest('span.chord');
            if (span === hoverTarget) return;
            hoverTarget = span;
            if (span) {
                showTooltip(span);
            } else {
                tooltip.style.display = 'none';
            }
        });

        songBody.addEventListener('mouseleave', function () {
            hoverTarget = null;
            tooltip.style.display = 'none';
        });
    }

    function renderAll() {
        var containers = document.querySelectorAll('.chord-diagram[data-chord-name]');
        if (!containers.length) return;
        if (typeof svguitar === 'undefined' || !svguitar.SVGuitarChord) {
            console.error('[chords] svguitar nicht geladen');
            return;
        }

        fetch('/data/guitar.json')
            .then(function (r) {
                if (!r.ok) throw new Error('HTTP ' + r.status);
                return r.json();
            })
            .then(function (db) {
                var svgCache = {};
                containers.forEach(function (el) {
                    var name = el.getAttribute('data-chord-name');
                    var svg = renderSvg(el, db, name);
                    if (svg) svgCache[name] = svg.cloneNode(true);
                });
                setupTooltip(svgCache);
            })
            .catch(function (e) { console.error('[chords] fetch Fehler:', e); });
    }

    if (document.readyState === 'loading') {
        document.addEventListener('DOMContentLoaded', renderAll);
    } else {
        renderAll();
    }
})();
