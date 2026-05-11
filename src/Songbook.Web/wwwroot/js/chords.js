// Akkord-Diagramme mit svguitar + chords-db rendern
(function () {
    // Akkordname → { key, suffix } fuer chords-db
    var SUFFIX_MAP = [
        // Laengste Suffixe zuerst
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

    // Akkordname → JSON-Schluessel in chords-db (C# → Csharp, F# → Fsharp)
    // Enharmonische Equivalente auf vorhandene JSON-Schluessel mappen
    var KEY_MAP = {
        'C#': 'Csharp', 'Db': 'Csharp',
        'D#': 'Eb',     'Eb': 'Eb',
        'F#': 'Fsharp', 'Gb': 'Fsharp',
        'G#': 'Ab',     'Ab': 'Ab',
        'A#': 'Bb',     'Bb': 'Bb',
        'C': 'C', 'D': 'D', 'E': 'E', 'F': 'F', 'G': 'G', 'A': 'A', 'B': 'B',
    };

    // Akkordname aufteilen: "Am7" → { key: "A", suffix: "m7" }
    function parseChordName(name) {
        var keys = ['C#', 'Db', 'D#', 'Eb', 'F#', 'Gb', 'G#', 'Ab', 'A#', 'Bb', 'C', 'D', 'E', 'F', 'G', 'A', 'B'];
        var key = null;
        var rest = name;
        for (var i = 0; i < keys.length; i++) {
            if (name.indexOf(keys[i]) === 0) {
                key = KEY_MAP[keys[i]];
                rest = name.slice(keys[i].length);
                break;
            }
        }
        if (!key) return null;

        for (var j = 0; j < SUFFIX_MAP.length; j++) {
            if (rest === SUFFIX_MAP[j][0]) {
                return { key: key, suffix: SUFFIX_MAP[j][1] };
            }
        }
        return null;
    }

    // chords-db Position → svguitar chord-Objekt
    function toSvguitarChord(position) {
        var fingers = [];
        var frets = position.frets;
        var fingerNums = position.fingers;
        var base = position.baseFret || 1;

        for (var s = 0; s < frets.length; s++) {
            var fret = frets[s];
            var str = s + 1; // svguitar: string 1 = dickste Saite (links)
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

    document.addEventListener('DOMContentLoaded', function () {
        var containers = document.querySelectorAll('.chord-diagram[data-chord-name]');
        if (!containers.length) return;

        fetch('/data/guitar.json')
            .then(function (r) { return r.json(); })
            .then(function (db) {
                containers.forEach(function (el) {
                    var name = el.getAttribute('data-chord-name');
                    var parsed = parseChordName(name);
                    if (!parsed) { console.warn('[chords] nicht geparst:', name); return; }

                    var keyChords = db.chords[parsed.key];
                    if (!keyChords) { console.warn('[chords] key nicht gefunden:', parsed.key, 'fuer', name); return; }

                    var entry = null;
                    for (var i = 0; i < keyChords.length; i++) {
                        if (keyChords[i].suffix === parsed.suffix) {
                            entry = keyChords[i];
                            break;
                        }
                    }
                    if (!entry || !entry.positions.length) { console.warn('[chords] suffix nicht gefunden:', parsed.suffix, 'fuer', name); return; }

                    var chord = toSvguitarChord(entry.positions[0]);
                    console.log('[chords] render', name, '->', parsed, chord);

                    try {
                        var chart = new svguitar.SVGuitarChord(el);
                        chart.configure({
                            strings: 6,
                            frets: 4,
                            showTuning: false,
                            title: '',
                            color: '#e8a838',
                            stringColor: '#aaa',
                            fretColor: '#aaa',
                            fingerColor: '#e8a838',
                            fingerTextColor: '#111',
                            fontFamily: 'inherit',
                            width: 160,
                            height: 180,
                        }).chord(chord).draw();
                    } catch (e) {
                        console.error('[chords] svguitar Fehler fuer', name, e);
                    }
                });
            })
            .catch(function (e) { console.error('[chords] fetch Fehler:', e); });
    });
})();
