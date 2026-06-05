/**
 * resumeLayoutOptimizer.js
 *
 * Automatic A4 pagination for NexaCV: Hybrid Engine with Asymmetric Shrink.
 * 
 * - Margins/Spacing shrink aggressively.
 * - Fonts/Line-heights shrink at exactly HALF the rate to preserve readability.
 */

(function () {
    'use strict';

    // ─────────────────────────────────────────────────────────────────────────────
    // CONFIGURATION
    // ─────────────────────────────────────────────────────────────────────────────

    const SHRINK_LIMIT_PCT = 1.30;
    const MIN_SCALE = 0.80;
    const SCALE_STEP = 0.05;

    // Fonts & Line-heights (Will shrink at HALF the speed of spacing)
    const fontVars = [
        '--font-size-base', '--font-size-name', '--font-size-subtitle', '--font-size-section',
        '--font-size-body', '--font-size-small', '--font-size-tiny', '--font-size-cert',
        '--font-size-icon', '--font-size-bullet-marker', '--font-size-sep',
        '--line-height-name', '--line-height-body', '--line-height-summary', '--line-height-bullet',
        '--line-height-achievement', '--line-height-bullet-marker'
    ];

    // Spacing, Margins, and Paddings (Will shrink at FULL speed to save space)
    const spaceVars = [
        '--space-section', '--space-item', '--space-item-edu', '--space-item-cert', '--space-item-achievement',
        '--header-pb', '--header-mb', '--header-rule-margin', '--header-title-mt', '--header-diamond-size', '--header-diamond-margin',
        '--section-title-pb', '--section-title-mb',
        '--exp-header-gap', '--exp-header-mb', '--exp-company-mb', '--exp-bullet-pl', '--exp-bullet-mb',
        '--edu-gap', '--edu-institution-mt', '--cert-gap', '--skills-col-gap', '--skill-title-mb',
        '--achievement-gap', '--achievement-icon-mt', '--achievement-icon-width',
        '--contact-item-gap', '--contact-sep-margin', '--vol-description-mt', '--other-list-gap'
    ];

    let originalValues = {};
    const root = document.documentElement;

    // ─────────────────────────────────────────────────────────────────────────────
    // HELPERS
    // ─────────────────────────────────────────────────────────────────────────────

    function createPage(afterPage) {
        const page = document.createElement('div');
        page.className = 'nexa-page';
        const content = document.createElement('div');
        content.className = 'nexa-page-content';
        page.appendChild(content);
        afterPage.insertAdjacentElement('afterend', page);
        return page;
    }

    function scaleValue(valStr, scale) {
        return valStr.split(' ').map(part => {
            const match = part.match(/^(-?[\d.]+)(.*)$/);
            if (!match) return part;
            const num = parseFloat(match[1]);
            const unit = match[2];
            let scaledNum = num * scale;
            if (unit === '' && scaledNum < 1.0) scaledNum = 1.0;
            return Number(scaledNum.toFixed(3)) + unit;
        }).join(' ');
    }

    function applyScale(scale) {
        // Math magic: If space scale is 0.80 (20% shrink), font scale becomes 0.90 (10% shrink).
        const fontScale = 1 - ((1 - scale) / 2);

        // Apply half-shrink to fonts
        fontVars.forEach(v => {
            if (originalValues[v]) {
                root.style.setProperty(v, scaleValue(originalValues[v], fontScale));
            }
        });

        // Apply full-shrink to spaces/margins
        spaceVars.forEach(v => {
            if (originalValues[v]) {
                root.style.setProperty(v, scaleValue(originalValues[v], scale));
            }
        });
    }

    function splitSection(section, currentContent, availableHeight) {
        const originalChildren = Array.from(section.children);
        let currentPageEl = currentContent.closest('.nexa-page');
        let activeContent = currentContent;
        let activeSection = section;

        activeSection.innerHTML = '';
        let itemsOnCurrentPage = 0;

        for (let i = 0; i < originalChildren.length; i++) {
            const child = originalChildren[i];
            activeSection.appendChild(child);

            if (activeContent.scrollHeight > availableHeight + 1) {
                activeSection.removeChild(child);

                // Orphaned header protection
                if (itemsOnCurrentPage <= 1 && activeContent.children.length > 1) {
                    activeContent.removeChild(activeSection);
                    const newPage = createPage(currentPageEl);
                    currentPageEl = newPage;
                    activeContent = newPage.querySelector('.nexa-page-content');

                    activeContent.appendChild(activeSection);
                    activeSection.appendChild(child);
                    itemsOnCurrentPage++;
                    continue;
                }

                // Create next page for remaining items
                const newPage = createPage(currentPageEl);
                currentPageEl = newPage;
                activeContent = newPage.querySelector('.nexa-page-content');

                const continuationSection = activeSection.cloneNode(false);
                continuationSection.style.marginTop = '0';
                continuationSection.style.paddingTop = '0';

                activeContent.appendChild(continuationSection);
                activeSection = continuationSection;

                activeSection.appendChild(child);
                itemsOnCurrentPage = 1;
            } else {
                itemsOnCurrentPage++;
            }
        }
        return { newPage: currentPageEl, newContent: activeContent };
    }

    // ─────────────────────────────────────────────────────────────────────────────
    // CORE LOGIC
    // ─────────────────────────────────────────────────────────────────────────────

    async function optimizeLayout() {
        await document.fonts.ready;

        const firstPage = document.querySelector('.nexa-page');
        if (!firstPage) return;
        const firstContent = firstPage.querySelector('.nexa-page-content');
        if (!firstContent || firstContent.children.length === 0) return;

        const pageStyle = getComputedStyle(firstPage);
        const availableHeight = firstPage.clientHeight
            - parseFloat(pageStyle.paddingTop)
            - parseFloat(pageStyle.paddingBottom);

        const initialTotalHeight = firstContent.scrollHeight;

        // Save original CSS variable values for BOTH arrays
        const computedRootStyle = getComputedStyle(root);
        [...fontVars, ...spaceVars].forEach(v => {
            const val = computedRootStyle.getPropertyValue(v).trim();
            if (val) originalValues[v] = val;
        });

        let currentScale = 1.0;

        // =========================================================================
        // PASS 1: GLOBAL SMART SHRINK (If <= 130%)
        // =========================================================================

        if (initialTotalHeight > availableHeight && initialTotalHeight <= (availableHeight * SHRINK_LIMIT_PCT)) {
            let testScale = 1.0;
            let fits = false;

            while (testScale > MIN_SCALE) {
                testScale -= SCALE_STEP;
                testScale = Math.round(testScale * 100) / 100; // Fix JS float math
                applyScale(testScale);

                if (firstContent.scrollHeight <= availableHeight) {
                    fits = true;
                    currentScale = testScale;
                    break;
                }
            }

            if (fits) {
                window.parent.postMessage({ type: 'nexacv-layout', pages: 1 }, '*');
                return;
            } else {
                applyScale(1.0);
                currentScale = 1.0;
            }
        }

        // =========================================================================
        // PASS 2 & 3: PAGINATION WITH PREDICTIVE SHRINK & DEEP SPLIT
        // =========================================================================

        const sections = Array.from(firstContent.children);
        sections.forEach((s) => firstContent.removeChild(s));

        let currentPage = firstPage;
        let currentContent = firstContent;

        for (const section of sections) {
            currentContent.appendChild(section);
            let overflows = currentContent.scrollHeight > availableHeight + 1;

            if (!overflows) continue;

            // ── Pass 2: Option B (Predictive Shrink) ──
            let savedScale = currentScale;
            let fitWithShrink = false;
            let testScale = currentScale;

            while (testScale > MIN_SCALE) {
                testScale -= SCALE_STEP;
                testScale = Math.round(testScale * 100) / 100;
                applyScale(testScale);

                if (currentContent.scrollHeight <= availableHeight + 1) {
                    fitWithShrink = true;
                    currentScale = testScale;
                    break;
                }
            }

            if (fitWithShrink) {
                continue;
            }

            // ── Pass 3: Option A (Deep Pagination / Normal Push) ──
            applyScale(savedScale);
            currentScale = savedScale;

            if (currentContent.children.length <= 1 && !section.hasAttribute('data-splittable')) {
                continue;
            }

            if (section.getAttribute('data-splittable') === 'true') {
                const updatedPointers = splitSection(section, currentContent, availableHeight);
                currentPage = updatedPointers.newPage;
                currentContent = updatedPointers.newContent;
            } else {
                currentContent.removeChild(section);
                const newPage = createPage(currentPage);
                currentPage = newPage;
                currentContent = newPage.querySelector('.nexa-page-content');
                currentContent.appendChild(section);
            }
        }

        const pageCount = document.querySelectorAll('.nexa-page').length;
        window.parent.postMessage({ type: 'nexacv-layout', pages: pageCount }, '*');
    }

    if (document.readyState === 'loading') {
        document.addEventListener('DOMContentLoaded', optimizeLayout);
    } else {
        optimizeLayout();
    }

}());