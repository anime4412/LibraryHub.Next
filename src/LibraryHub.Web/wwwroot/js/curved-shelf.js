(() => {
  const root = document.getElementById('vhsArchiveHero');
  const books = Array.isArray(window.__LIBRARYHUB_SHELF_BOOKS__) ? window.__LIBRARYHUB_SHELF_BOOKS__ : [];
  if (!root || books.length === 0) return;

  const reducedMotion = window.matchMedia('(prefers-reduced-motion: reduce)').matches;
  const isFinePointer = window.matchMedia('(pointer: fine)').matches;

  const viewport = document.getElementById('vhsShelfViewport');
  const stage = document.getElementById('vhs3DStage');
  const probe = document.getElementById('vhsHeightProbe');
  const playRing = document.getElementById('vhsPlayRing');
  const titleWrap = document.getElementById('vhsTitleWrap');
  const subWrap = document.getElementById('vhsSubWrap');
  const yearWrap = document.getElementById('vhsYearWrap');
  const arrowPrev = document.getElementById('vhsArrowPrev');
  const arrowNext = document.getElementById('vhsArrowNext');
  const timelineEl = document.getElementById('vhsTimeline');
  const timelineDot = document.getElementById('vhsTimelineDot');
  const timelineTicks = document.getElementById('vhsTimelineTicks');
  const bookCounter = document.getElementById('vhsBookCounter');
  const cursor = document.getElementById('vhsCursor');

  // Detail Sheet elements
  const detailSheet = document.getElementById('vhsDetailSheet');
  const detailClose = document.getElementById('vhsDetailClose');
  const detailSub = document.getElementById('vhsDetailSub');
  const detailTitle = document.getElementById('vhsDetailTitle');
  const detailMeta = document.getElementById('vhsDetailMeta');
  const detailSynopsis = document.getElementById('vhsDetailSynopsis');
  const detailCredit = document.getElementById('vhsDetailCredit');
  const detailLink = document.getElementById('vhsDetailLink');
  const detailBorrowBtn = document.getElementById('vhsDetailBorrowBtn');
  const posterImg = document.getElementById('vhsPosterImg');
  const inkBlobs = [0, 1, 2, 3, 4, 5, 6].map((i) => ({
    el: document.getElementById(`vhsInkBlob${i}`),
    delay: [0, 120, 220, 300, 360, 420, 180][i]
  }));

  const SPINE_PALETTES = [
    ['#1B3F9E', '#ECE6DA', '#E04D36'],
    ['#183D35', '#F3EDE1', '#D4AF37'],
    ['#7C2D23', '#F5EFE4', '#E5B85C'],
    ['#1E2430', '#EDE7DB', '#5C85D6'],
    ['#3D2B1F', '#F4EDE0', '#D97B48'],
    ['#263A29', '#F0EAE0', '#C7A951'],
    ['#3A2442', '#F3EDF4', '#D79E5B'],
    ['#14293D', '#EBF1F6', '#D46046']
  ];

  function makeFallbackCover(book, idx) {
    const pal = SPINE_PALETTES[idx % SPINE_PALETTES.length];
    const c = document.createElement('canvas');
    c.width = 474;
    c.height = 720;
    const ctx = c.getContext('2d');

    ctx.fillStyle = pal[0];
    ctx.fillRect(0, 0, c.width, c.height);

    // Subtle paper grain
    ctx.fillStyle = 'rgba(255,255,255,0.03)';
    for (let i = 0; i < 1600; i++) {
      const x = Math.abs((Math.sin(i * 12.9898) * 43758.5453) % 1) * c.width;
      const y = Math.abs((Math.sin(i * 78.233) * 43758.5453) % 1) * c.height;
      ctx.fillRect(x, y, 2, 2);
    }

    ctx.strokeStyle = pal[2];
    ctx.lineWidth = 3;
    ctx.strokeRect(24, 24, c.width - 48, c.height - 48);

    ctx.fillStyle = pal[1];
    ctx.textAlign = 'center';
    ctx.font = '600 13px "Inter Tight", sans-serif';
    ctx.fillText('LIBRARYHUB · ARCHIVE EDITION', c.width / 2, 70);

    // Title wrapping
    const words = String(book.title || '').split(/\s+/);
    const lines = [];
    let cur = '';
    ctx.font = '700 40px "Newsreader", serif';
    for (const w of words) {
      const t = cur ? `${cur} ${w}` : w;
      if (ctx.measureText(t).width > c.width - 96 && cur) {
        lines.push(cur);
        cur = w;
      } else {
        cur = t;
      }
    }
    if (cur) lines.push(cur);

    let y = 250;
    for (const l of lines.slice(0, 4)) {
      ctx.fillText(l, c.width / 2, y);
      y += 48;
    }

    ctx.fillStyle = pal[2];
    ctx.fillRect(c.width / 2 - 36, y + 18, 72, 2);

    ctx.fillStyle = pal[1];
    ctx.font = '500 20px "Inter Tight", sans-serif';
    ctx.fillText(String(book.author || '').slice(0, 32), c.width / 2, c.height - 96);

    ctx.font = '600 12px "Inter Tight", sans-serif';
    ctx.fillStyle = pal[2];
    ctx.fillText(`${book.year || 'ARCHIVE'} · ${book.category || ''}`.toUpperCase(), c.width / 2, c.height - 62);
    return c.toDataURL('image/png');
  }

  function makeSpineCanvas(book, idx, overrideBg = null) {
    const pal = SPINE_PALETTES[idx % SPINE_PALETTES.length];
    const bg = overrideBg || pal[0];
    const c = document.createElement('canvas');
    c.width = 160;
    c.height = 960;
    const ctx = c.getContext('2d');

    ctx.fillStyle = bg;
    ctx.fillRect(0, 0, c.width, c.height);

    // Edge wear & cardboard crease shading
    const grad = ctx.createLinearGradient(0, 0, c.width, 0);
    grad.addColorStop(0, 'rgba(0,0,0,0.42)');
    grad.addColorStop(0.18, 'rgba(255,255,255,0.08)');
    grad.addColorStop(0.82, 'rgba(0,0,0,0.05)');
    grad.addColorStop(1, 'rgba(0,0,0,0.45)');
    ctx.fillStyle = grad;
    ctx.fillRect(0, 0, c.width, c.height);

    // Compute brightness to ensure high-contrast ink/cream spine text
    let isLightBg = false;
    if (overrideBg) {
      const m = overrideBg.match(/\d+/g);
      if (m && m.length >= 3) {
        const lum = 0.299 * +m[0] + 0.587 * +m[1] + 0.114 * +m[2];
        isLightBg = lum > 145;
      }
    }

    const ink = isLightBg ? '#181818' : pal[1];
    const accent = isLightBg ? '#1F3FB0' : pal[2];

    // Top miniature geometric illustration badge
    ctx.strokeStyle = accent;
    ctx.lineWidth = 3;
    ctx.strokeRect(32, 38, c.width - 64, 96);
    ctx.fillStyle = accent;
    ctx.beginPath();
    ctx.arc(c.width / 2, 86, 22, 0, Math.PI * 2);
    ctx.fill();

    // Vertical condensed serif title
    ctx.save();
    ctx.translate(c.width / 2, c.height / 2 - 20);
    ctx.rotate(-Math.PI / 2);
    ctx.fillStyle = ink;
    ctx.textAlign = 'center';
    ctx.textBaseline = 'middle';
    ctx.font = '700 44px "Newsreader", Georgia, serif';
    ctx.fillText(String(book.title || '').slice(0, 22), 0, 0);
    ctx.restore();

    // Author surname + Year box at bottom
    const surname = String(book.author || 'ARCHIVE').trim().split(/\s+/).pop().toUpperCase().slice(0, 10);
    ctx.fillStyle = accent;
    ctx.textAlign = 'center';
    ctx.font = '600 18px "Inter Tight", sans-serif';
    ctx.fillText(surname, c.width / 2, c.height - 135);

    ctx.strokeStyle = ink;
    ctx.lineWidth = 2.5;
    ctx.strokeRect(30, c.height - 102, c.width - 60, 48);
    ctx.fillStyle = ink;
    ctx.font = '700 20px "Inter Tight", sans-serif';
    ctx.fillText(book.year > 0 ? String(book.year) : 'BOOK', c.width / 2, c.height - 71);

    return c.toDataURL('image/png');
  }

  function sampleCoverBg(img) {
    try {
      const s = document.createElement('canvas');
      s.width = 16;
      s.height = 16;
      const sx = s.getContext('2d');
      sx.drawImage(img, 0, 0, 16, 16);
      const d = sx.getImageData(0, 0, 16, 16).data;
      let r = 0, g = 0, b = 0, n = 0;
      for (let i = 0; i < d.length; i += 4) {
        if (d[i + 3] < 128) continue;
        r += d[i]; g += d[i + 1]; b += d[i + 2]; n++;
      }
      if (!n) return null;
      return `rgb(${Math.round(r / n)}, ${Math.round(g / n)}, ${Math.round(b / n)})`;
    } catch {
      return null;
    }
  }

  // Dynamic book count N (every newly added book automatically appears in the curved shelf!)
  const N = books.length;
  const COPIES = Math.max(3, Math.ceil(36 / N));
  const TOTAL = COPIES * N;
  const startBookOffset = N > 8 ? Math.min(5, Math.floor(N / 3)) : 0;
  const initialVirtual = Math.floor(COPIES / 2) * N + startBookOffset;

  // Timeline year range computed cleanly from real book years
  const validYears = books.map((b) => b.year).filter((y) => y > 0);
  const minYearRaw = validYears.length ? Math.min(...validYears) : 1955;
  const maxYearRaw = validYears.length ? Math.max(...validYears) : 2026;
  const minYear = Math.floor(minYearRaw / 5) * 5;
  const maxYear = Math.max(minYear + 10, Math.ceil(maxYearRaw / 5) * 5);
  const yearSpan = Math.max(1, maxYear - minYear);
  const labelStep = yearSpan <= 40 ? 5 : yearSpan <= 85 ? 10 : yearSpan <= 165 ? 20 : 25;
  const minorStep = labelStep >= 20 ? 5 : 1;

  // Build Timeline Ruler Ticks
  if (timelineTicks) {
    timelineTicks.replaceChildren();
    for (let y = minYear; y <= maxYear; y += minorStep) {
      const pct = ((y - minYear) / yearSpan) * 100;
      const isMultiple = y % labelStep === 0;
      const isEdge = y === minYear || y === maxYear;
      const tooCloseToMin = y !== minYear && ((y - minYear) / yearSpan) * 100 < 6.5;
      const tooCloseToMax = y !== maxYear && ((maxYear - y) / yearSpan) * 100 < 6.5;
      const shouldLabel = isEdge || (isMultiple && !tooCloseToMin && !tooCloseToMax);
      const isMajor = isMultiple || isEdge;
      const tick = document.createElement('div');
      tick.className = `vhs-tick ${isMajor ? 'vhs-tick-major' : 'vhs-tick-minor'}`;
      tick.style.left = `${pct}%`;
      timelineTicks.appendChild(tick);

      if (shouldLabel) {
        const lbl = document.createElement('div');
        lbl.className = 'vhs-tick-label';
        if (y === minYear) lbl.classList.add('align-left');
        if (y === maxYear) lbl.classList.add('align-right');
        lbl.style.left = `${pct}%`;
        lbl.textContent = String(y);
        timelineTicks.appendChild(lbl);
      }
    }
  }

  // Build Virtual Tapes & Preload Cover Images
  const tapes = [];
  const fallbackCovers = books.map((b, idx) => makeFallbackCover(b, idx));
  const spineUrls = books.map((b, idx) => makeSpineCanvas(b, idx));

  books.forEach((b, idx) => {
    if (!b.cover) return;
    const img = new Image();
    img.crossOrigin = 'anonymous';
    img.onload = () => {
      const sampled = sampleCoverBg(img);
      if (!sampled) return;
      const tintedSpine = makeSpineCanvas(b, idx, sampled);
      spineUrls[idx] = tintedSpine;
      tapes.forEach((t) => {
        if (t.bookIdx === idx) t.spEl.style.backgroundImage = `url("${tintedSpine}")`;
      });
    };
    img.src = b.cover;
  });

  for (let copy = 0; copy < COPIES; copy++) {
    for (let i = 0; i < N; i++) {
      const v = copy * N + i;
      const b = books[i];

      const tapeEl = document.createElement('div');
      tapeEl.className = 'vhs-tape';

      const spEl = document.createElement('div');
      spEl.className = 'vhs-face vhs-sp';
      spEl.style.backgroundImage = `url("${spineUrls[i]}")`;

      const cvEl = document.createElement('div');
      cvEl.className = 'vhs-face vhs-cv';
      cvEl.style.backgroundImage = b.cover
        ? `url("${b.cover}"), url("${fallbackCovers[i]}")`
        : `url("${fallbackCovers[i]}")`;

      tapeEl.append(spEl, cvEl);
      stage.appendChild(tapeEl);
      tapes.push({ v, bookIdx: i, el: tapeEl, spEl, cvEl, lift: 0 });
    }
  }

  // Arc Geometry State
  let H = 320, ws = 54, d = 168, gap = 11, U = 416;
  const P = 1400;
  let XS = [];

  function recomputeXS() {
    XS = [0];
    let acc = 0;
    const steps = Math.ceil((3 * U) / 2);
    for (let i = 1; i <= steps; i++) {
      const u = i * 2;
      acc += 2 / (1 + Math.pow(u / U, 1.6));
      XS[i] = acc;
    }
  }

  function syncMeasurements() {
    H = probe.clientHeight || 320;
    ws = (H * 116) / 720;
    d = (H * 474) / 720;
    gap = H * 0.032;
    U = window.innerWidth <= 767 ? H * 1.15 : H * 1.45;
    recomputeXS();
    root.style.setProperty('--ws', `${ws}px`);
    root.style.setProperty('--d', `${d}px`);
  }
  syncMeasurements();
  window.addEventListener('resize', syncMeasurements);

  const smoothstep = (val) => {
    const x = Math.max(0, Math.min(1, val));
    return x * x * (3 - 2 * x);
  };

  function rel(v, curS) {
    let r = (v - curS) % TOTAL;
    if (r < -TOTAL / 2) r += TOTAL;
    if (r >= TOTAL / 2) r -= TOTAL;
    return r;
  }

  const B = (r) => (Math.abs(r) < 1 ? (r + Math.sin(Math.PI * r) / Math.PI) / 2 : r > 0 ? 0.5 : -0.5);
  const F = (r) => r * (ws + gap) + (d - ws + 1.4 * gap) * B(r);
  const scl = (u) => 1 / (1 + Math.pow(Math.abs(u) / U, 1.6));
  function xsc(u) {
    const idx = Math.min(XS.length - 1, Math.round(Math.abs(u) / 2));
    return (u >= 0 ? 1 : -1) * (XS[idx] || 0);
  }

  // Interactive Motion State
  let s = initialVirtual;
  let T = initialVirtual;
  let activeBookIdx = ((initialVirtual % N) + N) % N;
  let timelineYear = books[activeBookIdx].year > 0 ? books[activeBookIdx].year : maxYear;
  let isDetailOpen = false;
  let isDragging = false;
  let hasMovedDrag = false;
  let dragStartX = 0;
  let dragStartT = 0;
  let mouseX = window.innerWidth / 2;
  let mouseY = window.innerHeight / 2;
  let cursorX = mouseX;
  let cursorY = mouseY;
  let tiltX = 0;
  let tiltY = 0;
  let hoveredTape = null;
  let detailAnimations = [];

  // Mask Slide Caption Swap (no opacity fades, mask clip slide per prompt)
  function swapMaskedText(wrapEl, text, className, dir) {
    if (!wrapEl) return;
    const oldItems = [...wrapEl.querySelectorAll('.vhs-mask-item')];
    if (oldItems.length === 1 && oldItems[0].textContent === text) return;

    const nextEl = document.createElement('span');
    nextEl.className = `vhs-mask-item ${className}`;
    nextEl.textContent = text;
    wrapEl.appendChild(nextEl);

    if (reducedMotion || dir === 0) {
      oldItems.forEach((o) => o.remove());
      return;
    }

    const duration = 650;
    const easing = 'cubic-bezier(0.2, 0.8, 0.2, 1)';
    nextEl.animate(
      [
        { transform: `translateY(${dir * 105}%)` },
        { transform: 'translateY(0)' }
      ],
      { duration, easing, fill: 'both' }
    );

    oldItems.forEach((old) => {
      const currentTransform = getComputedStyle(old).transform;
      const startTransform = currentTransform && currentTransform !== 'none' ? currentTransform : 'translateY(0)';
      const anim = old.animate(
        [
          { transform: startTransform },
          { transform: `translateY(${-dir * 105}%)` }
        ],
        { duration, easing, fill: 'both' }
      );
      anim.onfinish = () => old.remove();
    });
  }

  function updateCaption(bookIdx, dir) {
    const b = books[bookIdx];
    if (!b) return;
    swapMaskedText(titleWrap, b.title, 'vhs-title', dir);
    swapMaskedText(subWrap, `${b.author || 'THƯ VIỆN'} / ${b.category || 'SÁCH'}`, 'vhs-subtitle vhs-lbl', dir);
    const yearPart = b.year > 0 ? String(b.year) : 'SÁCH THƯ VIỆN';
    const availPart = b.available > 0 ? `CÒN ${b.available} CUỐN` : 'HẾT SÁCH';
    swapMaskedText(yearWrap, `${yearPart} — ${b.category || 'VĂN HỌC'} · ${availPart}`, 'vhs-year-genre', dir);

    if (bookCounter) bookCounter.textContent = `${bookIdx + 1} / ${N}`;
  }
  updateCaption(activeBookIdx, 0);

  // Inputs
  root.addEventListener('pointerenter', () => {
    if (isFinePointer) {
      root.classList.add('is-cursor-active');
      cursor?.classList.add('visible');
    }
  });
  root.addEventListener('pointerleave', () => {
    root.classList.remove('is-cursor-active');
    cursor?.classList.remove('visible');
  });

  window.addEventListener('pointermove', (e) => {
    mouseX = e.clientX;
    mouseY = e.clientY;
    if (!isDragging || isDetailOpen) return;
    const dx = e.clientX - dragStartX;
    if (Math.abs(dx) > 5) hasMovedDrag = true;
    T = dragStartT - dx / (ws + gap);
  });

  root.addEventListener('pointerdown', (e) => {
    if (isDetailOpen) return;
    if (e.target.closest('.vhs-arrow, .vhs-timeline, .vhs-play-ring, .vhs-topbar, .vhs-detail')) return;
    isDragging = true;
    hasMovedDrag = false;
    dragStartX = e.clientX;
    dragStartT = T;
  });

  window.addEventListener('pointerup', () => {
    if (!isDragging) return;
    isDragging = false;
    T = Math.round(T);
  });

  // Wheel horizontal / Shift+wheel only so vertical scroll to Catalog works seamlessly
  let wheelTimer = null;
  root.addEventListener('wheel', (e) => {
    if (isDetailOpen) return;
    if (!e.shiftKey && Math.abs(e.deltaX) <= Math.abs(e.deltaY)) return;
    e.preventDefault();
    const delta = Math.abs(e.deltaX) > Math.abs(e.deltaY) ? e.deltaX : e.deltaY;
    T += (delta / (ws + gap)) * 0.5;
    clearTimeout(wheelTimer);
    wheelTimer = setTimeout(() => { T = Math.round(T); }, 160);
  }, { passive: false });

  window.addEventListener('keydown', (e) => {
    if (e.target.closest('input, textarea, select')) return;
    if (isDetailOpen) {
      if (e.key === 'Escape') closeDetail();
      return;
    }
    if (e.key === 'ArrowLeft') T = Math.round(T - 1);
    else if (e.key === 'ArrowRight') T = Math.round(T + 1);
    else if (e.key === 'Enter' && root.matches(':hover')) openDetail();
  });

  arrowPrev?.addEventListener('click', () => { T = Math.round(T - 1); });
  arrowNext?.addEventListener('click', () => { T = Math.round(T + 1); });
  playRing?.addEventListener('click', openDetail);

  tapes.forEach((t) => {
    t.el.addEventListener('pointerenter', () => { hoveredTape = t; });
    t.el.addEventListener('pointerleave', () => { if (hoveredTape === t) hoveredTape = null; });
    t.el.addEventListener('click', () => {
      if (hasMovedDrag || isDetailOpen) return;
      const r = rel(t.v, s);
      if (Math.abs(r) < 0.5) openDetail();
      else T = Math.round(T + r);
    });
  });

  timelineEl?.addEventListener('click', (e) => {
    const rect = timelineEl.getBoundingClientRect();
    const pct = Math.max(0, Math.min(1, (e.clientX - rect.left) / rect.width));
    const clickedYear = minYear + pct * yearSpan;

    // Nearest book to clicked year
    let bestIdx = 0;
    let bestDiff = Infinity;
    books.forEach((b, idx) => {
      const by = b.year > 0 ? b.year : maxYear;
      const diff = Math.abs(by - clickedYear);
      if (diff < bestDiff) {
        bestDiff = diff;
        bestIdx = idx;
      }
    });

    const curIdx = ((Math.round(T) % N) + N) % N;
    let stepDiff = bestIdx - curIdx;
    if (stepDiff > N / 2) stepDiff -= N;
    if (stepDiff < -N / 2) stepDiff += N;
    T = Math.round(T + stepDiff);
  });

  // Active front cover DOM element lookup
  function getActiveCoverEl() {
    let best = null;
    let bestAbs = Infinity;
    for (const t of tapes) {
      const ar = Math.abs(rel(t.v, s));
      if (ar < bestAbs) {
        bestAbs = ar;
        best = t.cvEl;
      }
    }
    return best;
  }

  // Detail Sheet (Ink Bleed Poster + Clip-Path Grow)
  function openDetail() {
    if (isDetailOpen) return;
    T = Math.round(T);
    s = T;
    const idx = ((Math.round(s) % N) + N) % N;
    const book = books[idx];
    if (!book || !detailSheet) return;

    isDetailOpen = true;
    root.classList.remove('is-cursor-active');
    cursor?.classList.remove('visible');

    detailSub.textContent = `${book.author || 'THƯ VIỆN'} · ${book.category || 'SÁCH'}`;
    detailTitle.textContent = book.title;
    detailMeta.textContent = `${book.year > 0 ? book.year : 'SÁCH THƯ VIỆN'} / ${book.category || 'CHUNG'} / ${book.pages || 200} TRANG · ISBN ${book.isbn || ''}`;
    detailSynopsis.textContent = book.description || 'Tác phẩm tuyển chọn thuộc bộ sưu tập LibraryHub.';
    detailCredit.textContent = `Tác giả: ${book.author || 'LibraryHub Archive'} · ${book.available > 0 ? `Còn ${book.available} cuốn sẵn sàng` : 'Hiện hết sách, có thể đặt trước'}`;
    detailLink.href = `/books/${book.id}`;

    if (detailBorrowBtn) {
      detailBorrowBtn.dataset.bookId = String(book.id);
      detailBorrowBtn.disabled = book.available <= 0;
      detailBorrowBtn.textContent = book.available > 0 ? '+ THÊM VÀO GIỎ MƯỢN' : 'HẾT SÁCH';
    }

    posterImg.setAttribute('href', book.cover || fallbackCovers[idx]);
    inkBlobs.forEach((b) => b.el?.setAttribute('r', '0'));

    detailAnimations.forEach((a) => a.cancel());
    detailAnimations = [];

    const coverEl = getActiveCoverEl();
    const rect = coverEl ? coverEl.getBoundingClientRect() : { top: 180, left: window.innerWidth / 2 - 110, right: window.innerWidth / 2 + 110, bottom: 520 };
    const top = Math.max(0, rect.top);
    const right = Math.max(0, window.innerWidth - rect.right);
    const bottom = Math.max(0, window.innerHeight - rect.bottom);
    const left = Math.max(0, rect.left);

    detailSheet.classList.add('is-open');
    detailSheet.setAttribute('aria-hidden', 'false');

    const durSheet = reducedMotion ? 1 : 1050;
    detailAnimations.push(
      detailSheet.animate(
        [
          { clipPath: `inset(${top}px ${right}px ${bottom}px ${left}px round 3px)` },
          { clipPath: 'inset(0px 0px 0px 0px round 0px)' }
        ],
        { duration: durSheet, easing: 'cubic-bezier(0.76, 0, 0.18, 1)', fill: 'both' }
      )
    );

    const textEls = detailSheet.querySelectorAll('.vhs-detail-anim');
    textEls.forEach((el, i) => {
      detailAnimations.push(
        el.animate(
          [{ transform: 'translateY(110%)' }, { transform: 'translateY(0)' }],
          {
            duration: reducedMotion ? 1 : 900,
            delay: reducedMotion ? 0 : 620 + i * 75,
            easing: 'cubic-bezier(0.2, 0.8, 0.2, 1)',
            fill: 'both'
          }
        )
      );
    });

    if (reducedMotion) {
      inkBlobs.forEach((b) => b.el?.setAttribute('r', '720'));
    } else {
      inkBlobs.forEach((b) => {
        if (!b.el) return;
        const startAt = performance.now() + 520 + b.delay;
        const stepInk = (now) => {
          if (!isDetailOpen) return;
          const p = Math.min(1, Math.max(0, (now - startAt) / 1250));
          const eased = 1 - Math.pow(1 - p, 3);
          b.el.setAttribute('r', String(eased * 720));
          if (p < 1) requestAnimationFrame(stepInk);
        };
        requestAnimationFrame(stepInk);
      });
    }
  }

  function closeDetail() {
    if (!isDetailOpen || !detailSheet) return;
    const coverEl = getActiveCoverEl();
    const rect = coverEl ? coverEl.getBoundingClientRect() : { top: 180, left: window.innerWidth / 2 - 110, right: window.innerWidth / 2 + 110, bottom: 520 };
    const top = Math.max(0, rect.top);
    const right = Math.max(0, window.innerWidth - rect.right);
    const bottom = Math.max(0, window.innerHeight - rect.bottom);
    const left = Math.max(0, rect.left);

    const anim = detailSheet.animate(
      [
        { clipPath: 'inset(0px 0px 0px 0px round 0px)' },
        { clipPath: `inset(${top}px ${right}px ${bottom}px ${left}px round 3px)` }
      ],
      { duration: reducedMotion ? 1 : 850, easing: 'cubic-bezier(0.76, 0, 0.18, 1)', fill: 'both' }
    );
    anim.onfinish = () => {
      isDetailOpen = false;
      detailSheet.classList.remove('is-open');
      detailSheet.setAttribute('aria-hidden', 'true');
      detailAnimations.forEach((a) => a.cancel());
      detailAnimations = [];
      anim.cancel();
    };
  }

  detailClose?.addEventListener('click', closeDetail);

  detailBorrowBtn?.addEventListener('click', async () => {
    const bookId = Number(detailBorrowBtn.dataset.bookId);
    if (!bookId || !window.LibraryHub || !window.LibraryHubBorrowCart) return;
    const tokenInput = document.querySelector('#catalog-antiforgery input[name="__RequestVerificationToken"]');
    if (!window.LibraryHub.setBusy(detailBorrowBtn, true, 'ĐANG THÊM…')) return;
    try {
      const formData = new FormData();
      formData.append('BookId', String(bookId));
      formData.append('ReturnUrl', `${location.pathname}${location.search}${location.hash}`);
      if (tokenInput?.value) formData.append('__RequestVerificationToken', tokenInput.value);
      const item = await window.LibraryHub.request('/borrow-cart/add', { method: 'POST', body: formData });
      const res = window.LibraryHubBorrowCart.add(item);
      window.LibraryHub.notify(res.message, res.error);
    } catch (error) {
      if (!error.authenticationRedirected) {
        window.LibraryHub.notify(error.message || 'Không thể thêm sách vào giỏ mượn.', true);
      }
    } finally {
      window.LibraryHub.setBusy(detailBorrowBtn, false);
    }
  });

  // Main Single rAF Loop
  let lastTime = performance.now();
  let isHeroVisible = true;
  const heroObserver = new IntersectionObserver((entries) => {
    isHeroVisible = entries.some((e) => e.isIntersecting);
  }, { threshold: 0.02 });
  heroObserver.observe(root);

  function tick(now) {
    requestAnimationFrame(tick);
    if (!isHeroVisible && !isDetailOpen) {
      lastTime = now;
      return;
    }

    const dt = Math.min(0.05, Math.max(0.001, (now - lastTime) / 1000));
    lastTime = now;

    // 1. Spring physics
    const kSpring = isDragging ? 18 : 5.2;
    if (reducedMotion) {
      s = T;
    } else {
      s += (T - s) * (1 - Math.exp(-kSpring * dt));
    }

    // Wrap loops cleanly without drift
    if (Math.abs(s) > TOTAL * 2) {
      const shift = Math.round(s / N) * N;
      s -= shift;
      T -= shift;
    }

    // 2. Active book detection & masked caption swap
    const rounded = ((Math.round(s) % N) + N) % N;
    if (rounded !== activeBookIdx) {
      const dir = T >= s ? 1 : -1;
      activeBookIdx = rounded;
      updateCaption(activeBookIdx, dir);
    }

    // 3. Cover tilt
    const heroRect = root.getBoundingClientRect();
    const shelfCy = heroRect.top + heroRect.height * 0.41;
    const W = heroRect.width;
    const onActiveCover =
      !isDetailOpen &&
      Math.abs(T - s) < 0.3 &&
      Math.abs(mouseX - (heroRect.left + W / 2)) < 0.55 * d &&
      Math.abs(mouseY - shelfCy) < 0.55 * H;

    const targetTiltY = onActiveCover
      ? ((mouseX - (heroRect.left + W / 2)) / (0.55 * d)) * 8
      : ((mouseX - (heroRect.left + W / 2)) / Math.max(1, W)) * 4;
    const targetTiltX = onActiveCover ? -((mouseY - shelfCy) / (H / 2)) * 5 : 0;

    const tiltAlpha = reducedMotion ? 1 : 1 - Math.exp(-7 * dt);
    tiltX += (targetTiltX - tiltX) * tiltAlpha;
    tiltY += (targetTiltY - tiltY) * tiltAlpha;

    // 4. Per-tape 3D positioning along receding bowl arc
    for (const t of tapes) {
      const r = rel(t.v, s);
      const u = F(r);
      if (Math.abs(u) > U * 1.95) {
        t.el.style.display = 'none';
        continue;
      }
      t.el.style.display = 'block';

      const o = smoothstep(Math.max(0, 1 - Math.abs(r)));
      const sc = scl(u);
      const k = 1 / sc;

      const isHoverTarget = hoveredTape === t && Math.abs(r) > 0.5 && !isDetailOpen;
      const liftAlpha = reducedMotion ? 1 : 1 - Math.exp(-12 * dt);
      t.lift += ((isHoverTarget ? 1 : 0) - t.lift) * liftAlpha;

      const z = P * (1 - k) + o * H * 0.1;
      const x = xsc(u) * k;
      const y = (H * 0.5 * (1 - sc) * 0.42 + o * H * 0.012 - t.lift * H * 0.05) * k;
      const rotY = (u >= 0 ? 1 : -1) * Math.min(1, Math.abs(u) / U) * 26 * (1 - o) - 90 * o + tiltY * o;
      const rotX = tiltX * o;

      t.el.style.transform = `translate3d(${x.toFixed(2)}px, ${y.toFixed(2)}px, ${z.toFixed(2)}px) rotateX(${rotX.toFixed(2)}deg) rotateY(${rotY.toFixed(2)}deg)`;
      t.el.style.zIndex = String(Math.round(3000 + z));

      const spShade = Math.min(0.6, o * 0.55 + (1 - sc) * 0.32);
      const cvShade = (1 - o) * 0.82;
      t.spEl.style.setProperty('--shade', spShade.toFixed(3));
      t.cvEl.style.setProperty('--shade', cvShade.toFixed(3));
    }

    // 5. Play ring placement & scale
    if (playRing) {
      const ringScale = isDetailOpen ? 0 : smoothstep(Math.max(0, 1 - Math.abs(T - s) * 3));
      const ringLeft = W / 2 - d / 2 - 6;
      const ringTop = heroRect.height * 0.41 + 0.36 * H;
      playRing.style.left = `${ringLeft.toFixed(1)}px`;
      playRing.style.top = `${ringTop.toFixed(1)}px`;
      playRing.style.transform = `translate(-50%, -50%) scale(${ringScale.toFixed(3)})`;
      playRing.style.pointerEvents = ringScale > 0.5 ? 'auto' : 'none';
    }

    // 6. Timeline dot interpolation
    if (timelineDot) {
      const fIdx0 = ((Math.floor(s) % N) + N) % N;
      const fIdx1 = (fIdx0 + 1) % N;
      const frac = s - Math.floor(s);
      const y0 = books[fIdx0].year > 0 ? books[fIdx0].year : maxYear;
      const y1 = books[fIdx1].year > 0 ? books[fIdx1].year : maxYear;
      const interpYear = Math.abs(y1 - y0) > yearSpan * 0.5
        ? (frac < 0.5 ? y0 : y1)
        : y0 + (y1 - y0) * frac;

      if (Math.abs(interpYear - timelineYear) > 6 || reducedMotion) {
        timelineYear = interpYear;
      } else {
        timelineYear += (interpYear - timelineYear) * (1 - Math.exp(-14 * dt));
      }
      const pct = Math.max(0, Math.min(100, ((timelineYear - minYear) / yearSpan) * 100));
      timelineDot.style.left = `${pct.toFixed(2)}%`;
    }

    // 7. Custom cursor smoothing & state
    if (cursor && isFinePointer && root.classList.contains('is-cursor-active')) {
      const cAlpha = 1 - Math.exp(-16 * dt);
      cursorX += (mouseX - cursorX) * cAlpha;
      cursorY += (mouseY - cursorY) * cAlpha;
      cursor.style.transform = `translate3d(${cursorX.toFixed(1)}px, ${cursorY.toFixed(1)}px, 0)`;

      const elAt = document.elementFromPoint(mouseX, mouseY);
      const overCover = !!elAt?.closest('.vhs-cv') && Math.abs(T - s) < 0.3 && !isDetailOpen;
      const overSpine = !!elAt?.closest('.vhs-sp') && !overCover && !isDetailOpen;
      const overBtn = !!elAt?.closest('button, a, .vhs-timeline, .vhs-play-ring');

      cursor.classList.toggle('open', overCover);
      cursor.classList.toggle('spine', overSpine && !overBtn);
      cursor.classList.toggle('btn', overBtn && !overCover);
    }
  }

  requestAnimationFrame(tick);
})();
