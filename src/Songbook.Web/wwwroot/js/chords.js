// Akkord-Diagramme: eigener SVG-Renderer + chords-db als Datenquelle.
(function () {
    var SUFFIX_MAP = [
        ['maj7b5', 'maj7b5'], ['maj7#5', 'maj7#5'], ['maj7', 'maj7'],
        ['m7b5', 'm7b5'], ['aug7', 'aug7'], ['dim7', 'dim7'],
        ['7sus4', '7sus4'], ['sus2', 'sus2'], ['sus4', 'sus4'],
        ['7b5', '7b5'], ['7b9', '7b9'], ['7#9', '7#9'],
        ['aug9', 'aug9'], ['9b5', '9b5'], ['9#11', '9#11'],
        ['m7', 'm7'], ['m9', 'm9'], ['m11', 'm11'],
        ['m13', 'm13'], ['m6', 'm6'], ['m69', 'm69'],
        ['dim', 'dim'], ['aug', 'aug'], ['sus', 'sus'],
        ['m', 'minor'], ['7', '7'], ['9', '9'],
        ['11', '11'], ['13', '13'], ['6', '6'],
        ['69', '69'], ['5', '5'], ['', 'major'],
    ];

    var KEY_MAP = {
        'C#': 'Csharp', 'Db': 'Csharp',
        'D#': 'Eb', 'Eb': 'Eb',
        'F#': 'Fsharp', 'Gb': 'Fsharp',
        'G#': 'Ab', 'Ab': 'Ab',
        'A#': 'Bb', 'Bb': 'Bb',
        'C': 'C', 'D': 'D', 'E': 'E', 'F': 'F', 'G': 'G', 'A': 'A', 'B': 'B'
    };

    var SVG_NS = 'http://www.w3.org/2000/svg';
    // Anzeige von oben nach unten: hohe E -> tiefe E
    var STRING_LABELS = ['e', 'B', 'G', 'D', 'A', 'E'];

    function parseChordName(name) {
        var keys = ['C#', 'Db', 'D#', 'Eb', 'F#', 'Gb', 'G#', 'Ab', 'A#', 'Bb',
                    'C', 'D', 'E', 'F', 'G', 'A', 'B'];
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

    function getPositions(db, name) {
        var parsed = parseChordName(name);
        if (!parsed) return null;
        var keyChords = db.chords[parsed.key];
        if (!keyChords) return null;
        for (var i = 0; i < keyChords.length; i++) {
            if (keyChords[i].suffix === parsed.suffix) return keyChords[i].positions || [];
        }
        return null;
    }

    // Anzeige: frets[0]=tiefe E ... frets[5]=hohe E -> Notation "x32010"
    function fretNotation(frets) {
        var out = '';
        for (var i = 0; i < frets.length; i++) {
            var f = frets[i];
            if (f === -1) out += 'x';
            else if (f >= 10) out += '(' + f + ')';
            else out += String(f);
        }
        return out;
    }

    function el(tag, attrs) {
        var node = document.createElementNS(SVG_NS, tag);
        if (attrs) {
            for (var k in attrs) {
                if (Object.prototype.hasOwnProperty.call(attrs, k)) {
                    node.setAttribute(k, attrs[k]);
                }
            }
        }
        return node;
    }

    function renderDiagram(position) {
        // Layout in viewBox-Einheiten (skaliert per CSS)
        var W = 220, H = 140;
        var padLeft = 28, padRight = 18, padTop = 14, padBottom = 22;
        var gridW = W - padLeft - padRight;
        var gridH = H - padTop - padBottom;
        var numFrets = 5;
        var fretW = gridW / numFrets;
        var strSpace = gridH / 5;

        var baseFret = position.baseFret || 1;
        var nut = baseFret === 1;
        var frets = position.frets;
        var barres = position.barres || [];

        var accent = '#e8a838';
        var gridColor = '#9a9a9a';
        var textColor = '#cfcfcf';
        var mutedColor = '#888';

        var svg = el('svg', {
            viewBox: '0 0 ' + W + ' ' + H,
            xmlns: SVG_NS
        });

        // Sattel oder baseFret-Beschriftung
        if (nut) {
            svg.appendChild(el('rect', {
                x: padLeft - 2, y: padTop,
                width: 3, height: gridH,
                fill: gridColor
            }));
        } else {
            var bf = el('text', {
                x: padLeft - 4, y: padTop + strSpace * 0.5 + 3.5,
                'font-size': 9, 'text-anchor': 'end',
                fill: textColor, 'font-family': 'inherit'
            });
            bf.textContent = baseFret + 'fr';
            svg.appendChild(bf);
        }

        // Bundlinien (vertikal)
        for (var f = 0; f <= numFrets; f++) {
            svg.appendChild(el('line', {
                x1: padLeft + f * fretW, y1: padTop,
                x2: padLeft + f * fretW, y2: padTop + gridH,
                stroke: gridColor, 'stroke-width': 1
            }));
        }
        // Saitenlinien (horizontal)
        for (var s = 0; s < 6; s++) {
            svg.appendChild(el('line', {
                x1: padLeft, y1: padTop + s * strSpace,
                x2: padLeft + gridW, y2: padTop + s * strSpace,
                stroke: gridColor, 'stroke-width': 1
            }));
        }

        // Saiten-Beschriftung links (oben hohe e, unten tiefe E)
        for (var i = 0; i < 6; i++) {
            var lbl = el('text', {
                x: padLeft - 8, y: padTop + i * strSpace + 3,
                'font-size': 9, 'text-anchor': 'end',
                fill: textColor, 'font-family': 'inherit'
            });
            lbl.textContent = STRING_LABELS[i];
            svg.appendChild(lbl);
        }

        // Stumme Saiten: "x" rechts vom Raster
        for (var s = 0; s < 6; s++) {
            if (frets[s] === -1) {
                var displayRow = 5 - s;
                var xm = el('text', {
                    x: padLeft + gridW + 6,
                    y: padTop + displayRow * strSpace + 3.5,
                    'font-size': 10, 'text-anchor': 'start',
                    fill: mutedColor, 'font-family': 'inherit',
                    'font-weight': '700'
                });
                xm.textContent = '×';
                svg.appendChild(xm);
            }
        }

        // Barr: Rechteck ueber min..max Saiten am Barr-Bund
        var barred = {};
        for (var bi = 0; bi < barres.length; bi++) {
            var barreFret = barres[bi];
            var minS = -1, maxS = -1;
            for (var ss = 0; ss < 6; ss++) {
                if (frets[ss] === barreFret) {
                    if (minS === -1) minS = ss;
                    maxS = ss;
                }
            }
            if (minS === -1) continue;
            var rowA = 5 - maxS; // obere Reihe
            var rowB = 5 - minS; // untere Reihe
            var yTop = padTop + rowA * strSpace;
            var yBot = padTop + rowB * strSpace;
            var cx = padLeft + (barreFret - 0.5) * fretW;
            var barW = Math.min(fretW * 0.55, 13);
            svg.appendChild(el('rect', {
                x: cx - barW / 2, y: yTop - 5,
                width: barW, height: (yBot - yTop) + 10,
                rx: barW / 2, ry: barW / 2,
                fill: accent
            }));
            for (var sb = minS; sb <= maxS; sb++) {
                if (frets[sb] === barreFret) barred[sb] = true;
            }
        }

        // Fingerpunkte
        for (var s2 = 0; s2 < 6; s2++) {
            var fr = frets[s2];
            if (fr <= 0) continue;
            if (barred[s2]) continue;
            var dr = 5 - s2;
            svg.appendChild(el('circle', {
                cx: padLeft + (fr - 0.5) * fretW,
                cy: padTop + dr * strSpace,
                r: 6, fill: accent
            }));
        }

        // Bundnummern unter dem Raster
        for (var fn = 0; fn < numFrets; fn++) {
            var num = el('text', {
                x: padLeft + (fn + 0.5) * fretW,
                y: padTop + gridH + 12,
                'font-size': 8.5, 'text-anchor': 'middle',
                fill: textColor, 'font-family': 'inherit'
            });
            num.textContent = String(baseFret + fn);
            svg.appendChild(num);
        }

        return svg;
    }

    function renderGroup(container, db, name) {
        var positions = getPositions(db, name);
        if (!positions || !positions.length) {
            console.warn('[chords] nicht gefunden:', name);
            return null;
        }

        var state = { positions: positions, index: 0, name: name };

        var diag = document.createElement('div');
        diag.className = 'chord-diagram-svg';
        container.appendChild(diag);

        var notation = document.createElement('div');
        notation.className = 'chord-notation';
        container.appendChild(notation);

        var nav, counter;
        if (positions.length > 1) {
            nav = document.createElement('div');
            nav.className = 'chord-nav';
            var prev = document.createElement('button');
            prev.type = 'button';
            prev.className = 'chord-nav-btn';
            prev.setAttribute('aria-label', 'previous variant');
            prev.textContent = '‹';
            counter = document.createElement('span');
            counter.className = 'chord-nav-counter';
            var next = document.createElement('button');
            next.type = 'button';
            next.className = 'chord-nav-btn';
            next.setAttribute('aria-label', 'next variant');
            next.textContent = '›';
            nav.appendChild(prev);
            nav.appendChild(counter);
            nav.appendChild(next);
            container.appendChild(nav);

            prev.addEventListener('click', function (e) {
                e.stopPropagation();
                state.index = (state.index - 1 + state.positions.length) % state.positions.length;
                update();
            });
            next.addEventListener('click', function (e) {
                e.stopPropagation();
                state.index = (state.index + 1) % state.positions.length;
                update();
            });
        }

        function update() {
            var pos = state.positions[state.index];
            diag.innerHTML = '';
            diag.appendChild(renderDiagram(pos));
            notation.textContent = fretNotation(pos.frets);
            if (counter) {
                counter.textContent = (state.index + 1) + ' / ' + state.positions.length;
            }
        }
        update();

        return state;
    }

    function setupTooltip(stateMap) {
        var songBody = document.querySelector('.song-body');
        if (!songBody) return;

        var tooltip = document.createElement('div');
        tooltip.className = 'chord-tooltip';
        document.body.appendChild(tooltip);

        var hoverTarget = null;

        function show(span) {
            var name = span.dataset.chord;
            var state = stateMap[name];
            if (!state) return;
            var pos = state.positions[state.index];

            tooltip.innerHTML = '';
            var title = document.createElement('div');
            title.className = 'chord-tooltip-title';
            title.textContent = name;
            tooltip.appendChild(title);
            tooltip.appendChild(renderDiagram(pos));
            var notation = document.createElement('div');
            notation.className = 'chord-notation';
            notation.textContent = fretNotation(pos.frets);
            tooltip.appendChild(notation);
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
            if (span) show(span);
            else tooltip.style.display = 'none';
        });

        songBody.addEventListener('mouseleave', function () {
            hoverTarget = null;
            tooltip.style.display = 'none';
        });
    }

    function renderAll() {
        var containers = document.querySelectorAll('.chord-diagram[data-chord-name]');
        if (!containers.length) return;

        fetch('/data/guitar.json')
            .then(function (r) {
                if (!r.ok) throw new Error('HTTP ' + r.status);
                return r.json();
            })
            .then(function (db) {
                var stateMap = {};
                containers.forEach(function (c) {
                    var name = c.getAttribute('data-chord-name');
                    var state = renderGroup(c, db, name);
                    if (state) stateMap[name] = state;
                });
                setupTooltip(stateMap);
            })
            .catch(function (e) { console.error('[chords] fetch Fehler:', e); });
    }

    if (document.readyState === 'loading') {
        document.addEventListener('DOMContentLoaded', renderAll);
    } else {
        renderAll();
    }
})();
