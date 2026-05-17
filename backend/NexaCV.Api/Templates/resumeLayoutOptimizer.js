/**
 * resumeLayoutOptimizer.js — DIAGNOSTIC + COMPRESSION
 *
 * After fonts load:
 *   1. Measure the raw template state (no changes)          → log "before"
 *   2. If pages > 1: apply COMPRESS steps to fit on one page
 *   3. If max compression still leaves 2 pages:
 *        a. Inject page-breaks so no section is split across pages
 *           (first element on page 2 is offset by body's paddingTop so top
 *            margin is visually consistent with page 1)
 *        b. Expand spacing to fill both pages to ≥ 85%
 *        c. Re-run page-break detection after expansion
 *        d. Inject <style> with @media print @page margins so every PDF page
 *           gets the same top/right/bottom/left margin as the template's body padding
 *   4. Measure final state                                   → log "after"
 *   5. Post both logs to parent (triggers JSON download) + report page count
 *
 * Console logs "before" and (when optimization ran) "after" in the same
 * grouped/table format as before.
 *
 * CSS custom properties written to :root when compressing/expanding:
 *   --lopt-section-spacing   section { margin-bottom }
 *   --lopt-item-spacing      exp/edu/cert/achievement item spacing
 *   --lopt-line-height       body text line-height
 */
(function () {
    'use strict';

    // ─── Constants ────────────────────────────────────────────────────────────────
    // Math.ceil avoids the "false second page" bug: body.offsetHeight = 1123 but
    // naive Math.ceil(1123 / 1122.52) = 2.  Integer divisor gives correct result.
    var A4_HEIGHT_PX = 297 * (96 / 25.4);      // ≈ 1122.52
    var PAGE_LIMIT = Math.ceil(A4_HEIGHT_PX); // 1123 px

    // Bottom 15% of a page ≈ 168 px — large enough to catch a section heading
    // (≈25 px) + its first item (≈50 px) before they become orphaned.
    var BOTTOM_ZONE = Math.round(PAGE_LIMIT * 0.15);

    // ─── Spacing tables ───────────────────────────────────────────────────────────
    // [ sectionSpacing, itemSpacing, lineHeight ]
    // Step 0 = design defaults (same as the CSS fallback values used in templates).
    // Applying step 0 locks in the values without changing the visual appearance.
    var COMPRESS = [
        ['5.5mm', '4.5mm', '1.75'],   // step 0 = design defaults
        ['5.0mm', '4.0mm', '1.65'],
        ['4.5mm', '3.5mm', '1.58'],
        ['4.0mm', '3.0mm', '1.52'],
        ['3.5mm', '2.5mm', '1.46'],
        ['3.0mm', '2.0mm', '1.40'],
        ['2.5mm', '1.5mm', '1.35'],
        ['2.0mm', '1.2mm', '1.30'],
        ['1.8mm', '1.0mm', '1.25'],
        ['1.5mm', '0.8mm', '1.20'],
    ];

    var EXPAND = [
        ['5.5mm', '4.5mm', '1.75'],   // step 0 = same as COMPRESS[0]
        ['6.0mm', '5.0mm', '1.82'],
        ['6.5mm', '5.5mm', '1.88'],
        ['7.0mm', '6.0mm', '1.94'],
        ['7.5mm', '6.5mm', '2.00'],
        ['8.0mm', '7.0mm', '2.06'],
        ['8.5mm', '7.5mm', '2.12'],
    ];

    // Selectors for elements that must not be split across a page boundary
    var PUSH_SELECTOR = [
        'section',
        '.exp-item',
        '.edu-item',
        '.cert-item',
        '.achievement-item',
    ].join(', ');

    // ─── Helpers ──────────────────────────────────────────────────────────────────

    function applyStep(step) {
        var root = document.documentElement;
        root.style.setProperty('--lopt-section-spacing', step[0]);
        root.style.setProperty('--lopt-item-spacing', step[1]);
        root.style.setProperty('--lopt-line-height', step[2]);
    }

    function getHeight() {
        void document.body.offsetHeight;
        return document.body.offsetHeight;
    }

    function pageCount(h) {
        return h <= PAGE_LIMIT ? 1 : Math.ceil(h / PAGE_LIMIT);
    }

    function lastPageFill(h, n) {
        return n === 1 ? h / PAGE_LIMIT : (h - (n - 1) * PAGE_LIMIT) / PAGE_LIMIT;
    }

    // Convert computed-style px value to rounded mm (one decimal place)
    function pxToMm(px) {
        return Math.round(px * 25.4 / 96 * 10) / 10;
    }

    // ─── Page-break injection ─────────────────────────────────────────────────────
    // Pushes any section/item whose top edge is in the bottom BOTTOM_ZONE of a page
    // to the next page.  extraTopPx = body's paddingTop so page-2 content starts at
    // the same visual distance from the top as page-1 content does.
    function applyPageBreaks(extraTopPx) {
        var extra = extraTopPx || 0;

        // Reset any previous injections
        document.querySelectorAll('[data-lopt-pb]').forEach(function (el) {
            el.style.marginTop = '';
            el.style.breakBefore = '';
            el.removeAttribute('data-lopt-pb');
        });
        void document.body.offsetHeight;

        // Scan in DOM order; reflow after each push so subsequent positions are accurate
        document.querySelectorAll(PUSH_SELECTOR).forEach(function (el) {
            var top = el.getBoundingClientRect().top + (window.scrollY || 0);
            var pageNum = Math.floor(top / PAGE_LIMIT); // 0-based: 0=page1, 1=page2, …
            var posInPage = top % PAGE_LIMIT;
            var remaining = PAGE_LIMIT - posInPage;

            if (posInPage > 0 && remaining < BOTTOM_ZONE) {
                // Element starts in the bottom zone of a page → push it to the next page.
                // Add extraTopPx so its visual top on the new page matches body's paddingTop.
                var current = parseFloat(el.style.marginTop) || 0;
                el.style.marginTop = (current + remaining + extra) + 'px';
                el.style.breakBefore = 'page';
                el.setAttribute('data-lopt-pb', '1');
                void document.body.offsetHeight; // reflow so next element reads correct position
            } else if (extra > 0 && pageNum > 0 && posInPage < extra) {
                // Element is on page 2+ but too close to the top edge (posInPage < bodyPaddingTop).
                // Add the deficit so it starts at the same visual distance from the top as page 1.
                var deficit = extra - posInPage;
                var current = parseFloat(el.style.marginTop) || 0;
                el.style.marginTop = (current + deficit) + 'px';
                el.setAttribute('data-lopt-pb', '1');
                void document.body.offsetHeight;
            }
        });
        void document.body.offsetHeight;
    }

    // ─── Print margin injection ───────────────────────────────────────────────────
    // Injects an @media print rule so every PDF page gets the same margins as
    // the template's body padding (consistent top/bottom on page 1, 2, 3…).
    var _printStyleEl = null;
    function injectPrintMargins(tMm, rMm, bMm, lMm) {
        if (!_printStyleEl) {
            _printStyleEl = document.createElement('style');
            _printStyleEl.id = 'lopt-print-margins';
            document.head.appendChild(_printStyleEl);
        }
        _printStyleEl.textContent =
            '@media print {\n' +
            '  @page { size: A4; margin: ' + tMm + 'mm ' + rMm + 'mm ' + bMm + 'mm ' + lMm + 'mm; }\n' +
            '  body  { padding: 0 !important; }\n' +
            '}';
    }

    // ─── Snapshot ─────────────────────────────────────────────────────────────────
    // Measures the current rendered state and returns a plain log object.
    // Does NOT post or log — call logToConsole / postMessage separately.
    function snapshot() {
        void document.body.offsetHeight;

        var bodyHeight = document.body.offsetHeight;
        var bs = window.getComputedStyle(document.body);

        var bodyMetrics = {
            marginTop: bs.getPropertyValue('margin-top'),
            marginBottom: bs.getPropertyValue('margin-bottom'),
            marginLeft: bs.getPropertyValue('margin-left'),
            marginRight: bs.getPropertyValue('margin-right'),
            paddingTop: bs.getPropertyValue('padding-top'),
            paddingBottom: bs.getPropertyValue('padding-bottom'),
            paddingLeft: bs.getPropertyValue('padding-left'),
            paddingRight: bs.getPropertyValue('padding-right'),
            lineHeight: bs.getPropertyValue('line-height'),
            fontSize: bs.getPropertyValue('font-size'),
        };

        var sectionData = [];
        var totalSectionHeight = 0;

        document.querySelectorAll('section').forEach(function (sec, i) {
            var rect = sec.getBoundingClientRect();
            var ss = window.getComputedStyle(sec);
            var h = Math.round(rect.height);
            totalSectionHeight += h;

            var heading = sec.querySelector('h2, h3, h4, [class*="title"], [class*="heading"]');
            var name = heading
                ? heading.textContent.trim().replace(/\s+/g, ' ')
                : ('section-' + (i + 1));

            sectionData.push({
                index: i + 1,
                name: name,
                heightPx: h,
                topPx: Math.round(rect.top + (window.scrollY || 0)),
                marginTop: ss.getPropertyValue('margin-top'),
                marginBottom: ss.getPropertyValue('margin-bottom'),
                paddingTop: ss.getPropertyValue('padding-top'),
                paddingBottom: ss.getPropertyValue('padding-bottom'),
                lineHeight: ss.getPropertyValue('line-height'),
            });
        });

        var pageRules = [];
        try {
            Array.prototype.forEach.call(document.styleSheets, function (sheet) {
                var rules;
                try { rules = sheet.cssRules; } catch (_) { return; }
                Array.prototype.forEach.call(rules, function (rule) {
                    if (rule.type === 4 || (rule.constructor && rule.constructor.name === 'CSSPageRule')) {
                        pageRules.push(rule.cssText);
                    }
                });
            });
        } catch (_) { }

        return {
            pages: pageCount(bodyHeight),
            bodyHeightPx: bodyHeight,
            a4PageLimitPx: PAGE_LIMIT,
            totalSectionHeightPx: totalSectionHeight,
            nonSectionHeightPx: bodyHeight - totalSectionHeight,
            bodyMetrics: bodyMetrics,
            pageMargins: {
                note: 'body padding is the visual margin in screen/preview mode; @page rules apply to PDF',
                top: bodyMetrics.paddingTop,
                bottom: bodyMetrics.paddingBottom,
                left: bodyMetrics.paddingLeft,
                right: bodyMetrics.paddingRight,
                atPageRules: pageRules,
            },
            sections: sectionData,
        };
    }

    // ─── Console log (same format as before, for one snapshot) ───────────────────
    function logToConsole(label, s) {
        console.group('[NexaCV ' + label + ']');
        console.log('Pages:', s.pages, '  (body: ' + s.bodyHeightPx + 'px, A4 limit: ' + PAGE_LIMIT + 'px)');
        console.log('Total sections:', s.totalSectionHeightPx + 'px  (' + s.sections.length + ' sections)');
        console.log('Non-section:', s.nonSectionHeightPx + 'px  (header, footer, gaps, etc.)');
        console.log('Body padding:', 'T:' + s.bodyMetrics.paddingTop + '  R:' + s.bodyMetrics.paddingRight
            + '  B:' + s.bodyMetrics.paddingBottom + '  L:' + s.bodyMetrics.paddingLeft);
        console.log('Body margin:', 'T:' + s.bodyMetrics.marginTop + '  R:' + s.bodyMetrics.marginRight
            + '  B:' + s.bodyMetrics.marginBottom + '  L:' + s.bodyMetrics.marginLeft);
        console.log('Line-height:', s.bodyMetrics.lineHeight + '  font-size: ' + s.bodyMetrics.fontSize);
        if (s.pageMargins.atPageRules.length) console.log('@page rules:', s.pageMargins.atPageRules);
        console.table(s.sections.map(function (sec) {
            return {
                '#': sec.index,
                name: sec.name,
                'height(px)': sec.heightPx,
                'top(px)': sec.topPx,
                'mt': sec.marginTop,
                'mb': sec.marginBottom,
                'pt': sec.paddingTop,
                'line-h': sec.lineHeight,
            };
        }));
        console.groupEnd();
    }

    // ─── Main ─────────────────────────────────────────────────────────────────────
    function run() {
        // ── Resume name & timestamp ───────────────────────────────────────────────
        var resumeName = (document.title || '').trim();
        if (!resumeName) {
            var h1 = document.querySelector('h1');
            if (h1) resumeName = h1.textContent.trim().replace(/\s+/g, ' ');
        }
        if (!resumeName) resumeName = 'resume';

        var now = new Date();
        var isoTs = now.toISOString();
        var fileTs = now.getFullYear()
            + '-' + String(now.getMonth() + 1).padStart(2, '0')
            + '-' + String(now.getDate()).padStart(2, '0')
            + 'T' + String(now.getHours()).padStart(2, '0')
            + '-' + String(now.getMinutes()).padStart(2, '0')
            + '-' + String(now.getSeconds()).padStart(2, '0');

        // ── Step 1: raw snapshot (pure template, nothing modified) ────────────────
        var beforeLog = snapshot();

        var compressionSteps = 0;
        var pageBreaksApplied = false;
        var printMarginsInjected = false;

        // ── Step 2: compress if content overflows one page ────────────────────────
        if (beforeLog.pages > 1) {
            // Apply step 0 (design defaults) to lock in the CSS vars without
            // changing appearance, then walk the table until we reach 1 page.
            applyStep(COMPRESS[0]);
            var h = getHeight();
            var n = pageCount(h);

            if (n > 1) {
                for (var c = 1; c < COMPRESS.length; c++) {
                    applyStep(COMPRESS[c]);
                    h = getHeight();
                    n = pageCount(h);
                    compressionSteps = c;
                    if (n === 1) break;
                }
            }

            // ── Step 3: still > 1 page after max compression ──────────────────────
            if (n > 1) {
                // Reset to design defaults before working on the 2-page layout.
                // Tight compression was only needed while trying to fit on 1 page;
                // keeping it here would make sections look over-compressed.
                applyStep(COMPRESS[0]);
                h = getHeight();
                // n is still > 1 (we verified the content genuinely needs 2 pages)

                // Body paddingTop in px — used to keep page-2 top margin consistent
                var bodyPtPx = parseFloat(window.getComputedStyle(document.body).paddingTop) || 0;

                // Push split sections/items to the next page
                applyPageBreaks(bodyPtPx);
                h = getHeight();
                n = pageCount(h);
                pageBreaksApplied = true;

                // Expand spacing to fill both pages to ≥ 85%.
                // EXPAND[0] = design defaults, so start from step 1 (slightly looser).
                var targetPages = n;
                var fill = lastPageFill(h, n);
                if (fill < 0.85) {
                    for (var e = 1; e < EXPAND.length; e++) {
                        applyStep(EXPAND[e]);
                        h = getHeight();
                        if (pageCount(h) > targetPages) {
                            applyStep(EXPAND[e - 1]); // revert last step
                            h = getHeight();
                            break;
                        }
                        fill = lastPageFill(h, n);
                        if (fill >= 0.85) break;
                    }
                }

                // Re-run page breaks — expansion may shift sections near page boundary
                applyPageBreaks(bodyPtPx);
                h = getHeight();
                n = pageCount(h);

                // Inject @media print @page margins so every PDF page has the same
                // top/right/bottom/left margin as the template's body padding.
                var bs = window.getComputedStyle(document.body);
                injectPrintMargins(
                    pxToMm(parseFloat(bs.paddingTop) || 0),
                    pxToMm(parseFloat(bs.paddingRight) || 0),
                    pxToMm(parseFloat(bs.paddingBottom) || 0),
                    pxToMm(parseFloat(bs.paddingLeft) || 0)
                );
                printMarginsInjected = true;
            }
        }

        // ── Step 4: final snapshot (post-optimization state) ─────────────────────
        var afterLog = snapshot();
        var finalPages = afterLog.pages;

        // ── Step 5: console log ───────────────────────────────────────────────────
        console.group('[NexaCV Diagnostics] ' + resumeName + ' @ ' + isoTs);
        logToConsole('BEFORE (raw template)', beforeLog);
        if (beforeLog.pages > 1) {
            console.log(
                '▶ Optimization applied — compressionSteps: ' + compressionSteps +
                ' | pageBreaks: ' + pageBreaksApplied +
                ' | printMargins: ' + printMarginsInjected
            );
            logToConsole('AFTER (optimized)', afterLog);
        }
        console.groupEnd();

        // ── Step 6: post to parent ────────────────────────────────────────────────
        var filename = resumeName.replace(/[^a-z0-9_\-]/gi, '_') + '_' + fileTs + '.json';
        var payload = {
            resumeName: resumeName,
            capturedAt: isoTs,
            optimizationApplied: beforeLog.pages > 1,
            compressionSteps: compressionSteps,
            pageBreaksApplied: pageBreaksApplied,
            printMarginsInjected: printMarginsInjected,
            before: beforeLog,
            after: afterLog,
        };

        try { window.parent.postMessage({ type: 'nexacv-layout', pages: finalPages }, '*'); } catch (_) { }
        try { window.parent.postMessage({ type: 'nexacv-diagnostics', log: payload, filename: filename }, '*'); } catch (_) { }
    }

    // Wait for web fonts before measuring so text reflow is complete
    if (document.fonts && typeof document.fonts.ready !== 'undefined') {
        document.fonts.ready.then(run);
    } else {
        if (document.readyState === 'complete') {
            run();
        } else {
            window.addEventListener('load', run);
        }
    }
})();
