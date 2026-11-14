/**
 * 3D Interactive Bookshelf Engine
 * Cover-Angle Perspective Flow (Mặt bìa nghiêng 3D)
 * Pure JavaScript + CSS 3D Transforms (60fps requestAnimationFrame)
 */

(function () {
    'use strict';

    let shelfSection = null;
    let track = null;
    let books = [];
    let activeIndex = 0;
    let targetIndex = 0;

    // Physics state
    let isDragging = false;
    let startX = 0;
    let currentDragDelta = 0;


    function initShelf() {
        shelfSection = document.querySelector('.shelf-hero-section');
        if (!shelfSection) return;

        track = shelfSection.querySelector('.shelf-track');
        if (!track) return;

        const bookEls = Array.from(shelfSection.querySelectorAll('.book-tape'));
        if (bookEls.length === 0) return;

        books = bookEls.map((el, index) => {
            return {
                el: el,
                index: index,
                id: el.dataset.id,
                title: el.dataset.title,
                author: el.dataset.author,
                category: el.dataset.category,
                year: el.dataset.year,
                pages: el.dataset.pages,
                available: el.dataset.available,
                cover: el.dataset.cover,
                desc: el.dataset.desc,
                // Current smooth render values
                currentX: 0,
                currentY: 0,
                currentZ: 0,
                currentRotY: 0,
                currentRotX: 0
            };
        });

        // Set default active to center or first book
        activeIndex = Math.min(Math.floor(books.length / 2), 4);
        targetIndex = activeIndex;

        setupGeometry();
        setupEvents();
        updateCaption();
        updateCounter();

        // Start 60fps render loop
        requestAnimationFrame(renderLoop);
    }

    function setupGeometry() {
        const height = parseFloat(getComputedStyle(document.documentElement).getPropertyValue('--shelf-h')) || 410;
        const wCover = parseFloat(getComputedStyle(document.documentElement).getPropertyValue('--book-w')) || (height * 0.68);
        const wDepth = parseFloat(getComputedStyle(document.documentElement).getPropertyValue('--book-d')) || 34;

        const hz = wDepth / 2;
        const hw = wCover / 2;
        const hh = height / 2;

        books.forEach((b) => {
            const cv = b.el.querySelector('.book-face.cv');
            const back = b.el.querySelector('.book-face.back');
            const sp = b.el.querySelector('.book-face.sp');
            const top = b.el.querySelector('.book-face.top-face');
            const side = b.el.querySelector('.book-face.side-face');

            // 1. Front Cover (Facing Z+)
            if (cv) {
                cv.style.transform = `translateZ(${hz.toFixed(2)}px)`;
            }
            // 2. Back Cover (Facing Z-)
            if (back) {
                back.style.transform = `rotateY(180deg) translateZ(${hz.toFixed(2)}px)`;
            }
            // 3. Spine on Left Edge
            if (sp) {
                sp.style.transform = `rotateY(-90deg) translateZ(${hw.toFixed(2)}px)`;
            }
            // 4. Side Face (Right Pages Edge)
            if (side) {
                side.style.transform = `rotateY(90deg) translateZ(${hw.toFixed(2)}px)`;
            }
            // 5. Top Face (Top Pages Edge)
            if (top) {
                top.style.transform = `rotateX(90deg) translateZ(${hh.toFixed(2)}px)`;
            }
        });
    }

    function calculateTargetValues(book, index, currentTargetIdx, dragOffset) {
        const height = parseFloat(getComputedStyle(document.documentElement).getPropertyValue('--shelf-h')) || 410;
        const wCover = parseFloat(getComputedStyle(document.documentElement).getPropertyValue('--book-w')) || (height * 0.68);
        
        // Dynamic step and center gap based on screen size
        const isMobile = window.innerWidth < 768;
        const centerGap = isMobile ? (wCover * 0.65) : (wCover * 0.82);
        const stepX = isMobile ? 80 : 125;

        let targetX = 0;
        let targetY = 0;
        let targetZ = 0;
        let targetRotY = 0;
        let targetRotX = 0;

        if (index === currentTargetIdx) {
            // Active Center Book: FRONT COVER PROMINENT, slightly tilted in 3D
            targetX = 0;
            targetY = -12;
            targetZ = 100;
            targetRotY = -2.5;
            targetRotX = 1.5;
        } else if (index < currentTargetIdx) {
            // Left Books: Front cover angled gracefully towards center
            const dist = currentTargetIdx - index;
            targetX = -centerGap - (dist - 1) * stepX;
            targetY = 0;
            targetZ = -dist * 20;
            targetRotY = 22.0;
            targetRotX = 0;
        } else {
            // Right Books: Front cover angled towards viewer (+24deg), paper edge rotated to the back
            const dist = index - currentTargetIdx;
            targetX = centerGap + (dist - 1) * stepX;
            targetY = 0;
            targetZ = -dist * 20;
            targetRotY = 22.0;
            targetRotX = 0;
        }

        // Apply drag offset
        targetX += dragOffset;

        return { targetX, targetY, targetZ, targetRotY, targetRotX };
    }

    function renderLoop() {
        const lerpFactor = 0.14;

        books.forEach((b, index) => {
            const { targetX, targetY, targetZ, targetRotY, targetRotX } = calculateTargetValues(
                b,
                index,
                targetIndex,
                currentDragDelta
            );

            b.currentX += (targetX - b.currentX) * lerpFactor;
            b.currentY += (targetY - b.currentY) * lerpFactor;
            b.currentZ += (targetZ - b.currentZ) * lerpFactor;
            b.currentRotY += (targetRotY - b.currentRotY) * lerpFactor;
            b.currentRotX += (targetRotX - b.currentRotX) * lerpFactor;

            const isActive = index === targetIndex;
            b.el.classList.toggle('active', isActive);

            // Apply 3D transform to book container
            b.el.style.transform = `translate3d(${b.currentX.toFixed(2)}px, ${b.currentY.toFixed(2)}px, ${b.currentZ.toFixed(2)}px) rotateY(${b.currentRotY.toFixed(2)}deg) rotateX(${b.currentRotX.toFixed(2)}deg)`;

            // Depth sorting (z-index): active is highest, books closer to center are higher
            b.el.style.zIndex = isActive ? 100 : Math.max(1, 50 - Math.abs(index - targetIndex));
        });

        requestAnimationFrame(renderLoop);
    }

    function selectBook(index) {
        if (index < 0 || index >= books.length) return;
        targetIndex = index;
        activeIndex = index;
        updateCaption();
        updateCounter();
    }

    function updateCaption() {
        const b = books[targetIndex];
        if (!b) return;

        const titleEl = document.getElementById('shelfCaptionTitle');
        const authorEl = document.getElementById('shelfCaptionAuthor');
        const catEl = document.getElementById('shelfCaptionCat');
        const yearEl = document.getElementById('shelfCaptionYear');
        const pagesEl = document.getElementById('shelfCaptionPages');
        const availEl = document.getElementById('shelfCaptionAvail');
        const detailBtn = document.getElementById('shelfDetailBtn');
        const borrowBtn = document.getElementById('shelfBorrowBtn');

        if (titleEl) titleEl.innerText = b.title;
        if (authorEl) authorEl.innerHTML = `<i class="bi bi-person me-1"></i> ${b.author}`;
        if (catEl) catEl.innerHTML = `<i class="bi bi-tag me-1"></i> ${b.category}`;
        if (yearEl) yearEl.innerHTML = `<i class="bi bi-calendar me-1"></i> Năm ${b.year}`;
        if (pagesEl) pagesEl.innerHTML = `<i class="bi bi-file-text me-1"></i> ${b.pages} trang`;
        if (availEl) {
            const count = parseInt(b.available) || 0;
            availEl.innerHTML = count > 0 
                ? `<span class="badge bg-success-subtle text-success px-2 py-1"><i class="bi bi-check-circle me-1"></i> Còn ${count} quyển</span>`
                : `<span class="badge bg-danger-subtle text-danger px-2 py-1"><i class="bi bi-x-circle me-1"></i> Hết sách</span>`;
        }

        if (detailBtn) detailBtn.href = `/Books/Details/${b.id}`;
        if (borrowBtn) borrowBtn.href = `/BorrowSlips/Create?bookId=${b.id}`;
    }

    function updateCounter() {
        const counterEl = document.getElementById('shelfCounter');
        if (counterEl) {
            const current = (targetIndex + 1).toString().padStart(2, '0');
            const total = books.length.toString().padStart(2, '0');
            counterEl.innerText = `${current} / ${total}`;
        }
    }

    function openDetailSheet() {
        const b = books[targetIndex];
        if (!b) return;

        const modal = document.getElementById('shelfDetailModal');
        if (!modal) return;

        document.getElementById('modalSheetTitle').innerText = b.title;
        document.getElementById('modalSheetAuthor').innerText = b.author;
        document.getElementById('modalSheetCat').innerText = b.category;
        document.getElementById('modalSheetYear').innerText = `Năm ${b.year}`;
        document.getElementById('modalSheetDesc').innerText = b.desc || 'Chưa có thông tin mô tả chi tiết.';
        document.getElementById('modalSheetImg').src = b.cover;
        document.getElementById('modalSheetDetailLink').href = `/Books/Details/${b.id}`;
        document.getElementById('modalSheetBorrowLink').href = `/BorrowSlips/Create?bookId=${b.id}`;

        modal.classList.add('active');
        document.body.style.overflow = 'hidden';
    }

    function closeDetailSheet() {
        const modal = document.getElementById('shelfDetailModal');
        if (modal) {
            modal.classList.remove('active');
            document.body.style.overflow = '';
        }
    }

    function setupEvents() {
        // Book clicks
        books.forEach((b, index) => {
            b.el.addEventListener('click', () => {
                if (Math.abs(currentDragDelta) > 10) return; // ignore click on drag
                if (index === targetIndex) {
                    openDetailSheet();
                } else {
                    selectBook(index);
                }
            });

        });

        // Prev & Next buttons
        const prevBtn = document.getElementById('shelfPrevBtn');
        const nextBtn = document.getElementById('shelfNextBtn');
        if (prevBtn) {
            prevBtn.addEventListener('click', () => {
                if (targetIndex > 0) selectBook(targetIndex - 1);
                else selectBook(books.length - 1);
            });
        }
        if (nextBtn) {
            nextBtn.addEventListener('click', () => {
                if (targetIndex < books.length - 1) selectBook(targetIndex + 1);
                else selectBook(0);
            });
        }

        // Category filter tabs
        const tabBtns = shelfSection.querySelectorAll('.shelf-tab-btn');
        tabBtns.forEach((btn) => {
            btn.addEventListener('click', () => {
                tabBtns.forEach(b => b.classList.remove('active'));
                btn.classList.add('active');
                const cat = btn.dataset.category;
                if (!cat || cat === 'all') {
                    selectBook(0);
                } else {
                    const matchIndex = books.findIndex(b => b.category && b.category.toLowerCase().includes(cat.toLowerCase()));
                    if (matchIndex !== -1) {
                        selectBook(matchIndex);
                    }
                }
            });
        });

        // Keyboard navigation
        window.addEventListener('keydown', (e) => {
            if (document.activeElement && ['INPUT', 'TEXTAREA'].includes(document.activeElement.tagName)) return;
            if (e.key === 'ArrowLeft') {
                if (targetIndex > 0) selectBook(targetIndex - 1);
            } else if (e.key === 'ArrowRight') {
                if (targetIndex < books.length - 1) selectBook(targetIndex + 1);
            } else if (e.key === 'Enter') {
                const modal = document.getElementById('shelfDetailModal');
                if (modal && !modal.classList.contains('active')) {
                    openDetailSheet();
                }
            } else if (e.key === 'Escape') {
                closeDetailSheet();
            }
        });

        // Mouse Drag / Touch Swipe on Shelf
        shelfSection.addEventListener('mousedown', (e) => {
            if (e.target.closest('button') || e.target.closest('a') || e.target.closest('input')) return;
            isDragging = true;
            startX = e.clientX;
            currentDragDelta = 0;
        });

        window.addEventListener('mousemove', (e) => {
            if (!isDragging) return;
            const delta = e.clientX - startX;
            currentDragDelta = delta * 0.45;
        });

        window.addEventListener('mouseup', () => {
            if (!isDragging) return;
            isDragging = false;
            if (currentDragDelta < -40 && targetIndex < books.length - 1) {
                targetIndex++;
            } else if (currentDragDelta > 40 && targetIndex > 0) {
                targetIndex--;
            }
            currentDragDelta = 0;
            updateCaption();
            updateCounter();
        });

        // Touch support for mobile
        shelfSection.addEventListener('touchstart', (e) => {
            if (e.target.closest('button') || e.target.closest('a') || e.target.closest('input')) return;
            isDragging = true;
            startX = e.touches[0].clientX;
            currentDragDelta = 0;
        }, { passive: true });

        shelfSection.addEventListener('touchmove', (e) => {
            if (!isDragging) return;
            const delta = e.touches[0].clientX - startX;
            currentDragDelta = delta * 0.6;
        }, { passive: true });

        shelfSection.addEventListener('touchend', () => {
            if (!isDragging) return;
            isDragging = false;
            if (currentDragDelta < -35 && targetIndex < books.length - 1) {
                targetIndex++;
            } else if (currentDragDelta > 35 && targetIndex > 0) {
                targetIndex--;
            }
            currentDragDelta = 0;
            updateCaption();
            updateCounter();
        });

        // Window resize
        window.addEventListener('resize', () => {
            setupGeometry();
        });

        // Modal close
        const closeBtn = document.getElementById('shelfCloseModalBtn');
        const modalBackdrop = document.getElementById('shelfDetailModal');
        if (closeBtn) closeBtn.addEventListener('click', closeDetailSheet);
        if (modalBackdrop) {
            modalBackdrop.addEventListener('click', (e) => {
                if (e.target === modalBackdrop) closeDetailSheet();
            });
        }

        window.openCurrentBookDetailSheet = openDetailSheet;
    }



    // Export & Init on DOM ready
    window.addEventListener('DOMContentLoaded', initShelf);
    window.initShelf3D = initShelf;
})();
