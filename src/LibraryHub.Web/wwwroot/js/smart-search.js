(() => {
  const HISTORY_KEY = 'libraryhub_smart_search_history_v1';

  // Quick-Login Popover in Header & 1-Click Role Cards on /login
  const quickToggle = document.getElementById('quick-login-toggle');
  const quickMenu = document.getElementById('quick-login-menu');

  const closeQuickLogin = () => {
    if (!quickMenu || !quickToggle) return;
    quickMenu.hidden = true;
    quickToggle.setAttribute('aria-expanded', 'false');
  };

  quickToggle?.addEventListener('click', (e) => {
    e.stopPropagation();
    const opening = quickMenu.hidden;
    quickMenu.hidden = !opening;
    quickToggle.setAttribute('aria-expanded', opening ? 'true' : 'false');
  });

  document.addEventListener('click', (e) => {
    if (quickMenu && !quickMenu.hidden && !e.target.closest('.quick-login-wrap')) {
      closeQuickLogin();
    }
  });

  document.querySelectorAll('.js-quick-login-role').forEach((btn) => {
    btn.addEventListener('click', () => {
      const email = btn.dataset.email || '';
      const password = btn.dataset.password || '';
      const loginForm = document.querySelector('form.auth-card');
      const emailInput = loginForm?.querySelector('#Email, input[name="Email"]');
      const passInput = loginForm?.querySelector('#Password, input[name="Password"]');
      if (loginForm && emailInput && passInput) {
        emailInput.value = email;
        passInput.value = password;
        if (window.LibraryHub?.setBusy) window.LibraryHub.setBusy(btn, true, 'Đang đăng nhập…');
        loginForm.submit();
      }
    });
  });

  // Recent Search History Helpers
  const readHistory = () => {
    try {
      const parsed = JSON.parse(localStorage.getItem(HISTORY_KEY) || '[]');
      return Array.isArray(parsed) ? parsed.slice(0, 5) : [];
    } catch { return []; }
  };

  const pushHistory = (q) => {
    const clean = String(q || '').trim();
    if (!clean) return;
    const next = [clean, ...readHistory().filter((x) => x.toLowerCase() !== clean.toLowerCase())].slice(0, 5);
    try { localStorage.setItem(HISTORY_KEY, JSON.stringify(next)); } catch {}
  };

  const escapeHtml = (str) => String(str ?? '')
    .replace(/&/g, '&amp;')
    .replace(/</g, '&lt;')
    .replace(/>/g, '&gt;')
    .replace(/"/g, '&quot;');

  const renderSmartMarkup = (data, query) => {
    const categories = data?.categories || [];
    const books = data?.books || [];
    const suggestions = data?.suggestions || [];
    const hasQuery = Boolean(String(query || '').trim());
    const history = !hasQuery ? readHistory() : [];

    let html = '';

    if (history.length > 0) {
      html += `<div class="smart-group"><div class="smart-group-head"><span>Tìm kiếm gần đây</span></div><div class="smart-chips">`;
      history.forEach((term) => {
        html += `<button type="button" class="smart-chip js-smart-fill" data-query="${escapeHtml(term)}">${escapeHtml(term)}</button>`;
      });
      html += `</div></div>`;
    }

    if (categories.length > 0) {
      html += `<div class="smart-group"><div class="smart-group-head"><span>Danh mục gợi ý</span></div><div class="smart-chips">`;
      categories.forEach((cat) => {
        html += `<a class="smart-chip smart-chip-cat" href="/?q=${encodeURIComponent(cat.name)}#catalog">${escapeHtml(cat.name)} <small>(${cat.bookCount})</small></a>`;
      });
      html += `</div></div>`;
    }

    if (books.length > 0) {
      html += `<div class="smart-group"><div class="smart-group-head"><span>${hasQuery ? 'Đầu sách khớp thông minh' : 'Gợi ý đọc nổi bật'}</span></div><div class="smart-book-list">`;
      books.forEach((b) => {
        const badge = b.availableQuantity > 0
          ? `<span class="smart-stock is-available">Còn ${b.availableQuantity}</span>`
          : `<span class="smart-stock is-out">Đặt trước</span>`;
        const thumb = b.coverImagePath
          ? `<img src="${escapeHtml(b.coverImagePath)}" alt="" loading="lazy" />`
          : `<span>${escapeHtml((b.title || 'S').slice(0, 1))}</span>`;
        html += `<a class="smart-book-row" href="/books/${b.id}">
          <span class="smart-book-thumb">${thumb}</span>
          <span class="smart-book-meta">
            <strong>${escapeHtml(b.title)}</strong>
            <small>${escapeHtml(b.author)} · ${escapeHtml(b.category)}${b.year > 0 ? ` · ${b.year}` : ''}</small>
          </span>
          ${badge}
        </a>`;
      });
      html += `</div></div>`;
    } else if (hasQuery) {
      html += `<div class="smart-empty">Không thấy kết quả khớp trực tiếp. Thử nhấn <strong>Enter</strong> để tìm toàn bộ hoặc chọn gợi ý dưới đây.</div>`;
    }

    if (suggestions.length > 0 && !books.length) {
      html += `<div class="smart-group"><div class="smart-group-head"><span>Gợi ý phổ biến</span></div><div class="smart-chips">`;
      suggestions.forEach((term) => {
        html += `<button type="button" class="smart-chip js-smart-fill" data-query="${escapeHtml(term)}">${escapeHtml(term)}</button>`;
      });
      html += `</div></div>`;
    }

    return html;
  };

  const fetchSmart = async (q) => {
    const res = await fetch(`/api/v1/search/smart?q=${encodeURIComponent(q || '')}`, {
      headers: { Accept: 'application/json' }
    });
    if (!res.ok) throw new Error('Smart search failed');
    return await res.json();
  };

  // Global Smart Search Command Palette (#smart-search-dialog)
  const palette = document.getElementById('smart-search-dialog');
  const paletteInput = document.getElementById('smart-search-input');
  const paletteResults = document.getElementById('smart-search-results');
  const paletteForm = document.getElementById('smart-search-form');
  const openTriggers = document.querySelectorAll('#open-smart-search, .js-open-smart-search');
  let paletteTimer = null;

  const loadPalette = async (q) => {
    if (!paletteResults) return;
    try {
      const data = await fetchSmart(q);
      paletteResults.innerHTML = renderSmartMarkup(data, q);
      wireFillButtons(paletteResults, paletteInput, loadPalette);
    } catch {}
  };

  const openPalette = (initialQuery = '') => {
    if (!palette) return;
    closeQuickLogin();
    if (!palette.open) palette.showModal();
    if (paletteInput) {
      if (initialQuery) paletteInput.value = initialQuery;
      paletteInput.focus();
      paletteInput.select();
    }
    loadPalette(paletteInput?.value || '');
  };

  const wireFillButtons = (container, inputEl, reloadFn) => {
    container?.querySelectorAll('.js-smart-fill').forEach((btn) => {
      btn.addEventListener('click', () => {
        if (!inputEl) return;
        inputEl.value = btn.dataset.query || '';
        inputEl.focus();
        reloadFn(inputEl.value);
      });
    });
    container?.querySelectorAll('.smart-book-row, .smart-chip-cat').forEach((link) => {
      link.addEventListener('click', () => {
        if (inputEl?.value) pushHistory(inputEl.value);
      });
    });
  };

  openTriggers.forEach((btn) => {
    btn.addEventListener('click', (e) => {
      e.preventDefault();
      openPalette();
    });
  });

  palette?.querySelector('[data-smart-close]')?.addEventListener('click', () => palette.close());
  palette?.addEventListener('click', (e) => {
    if (e.target === palette) palette.close();
  });

  paletteInput?.addEventListener('input', () => {
    clearTimeout(paletteTimer);
    paletteTimer = setTimeout(() => loadPalette(paletteInput.value), 140);
  });

  paletteForm?.addEventListener('submit', () => {
    if (paletteInput?.value) pushHistory(paletteInput.value);
  });

  // Keyboard navigation inside Smart Search Palette + Global Ctrl/Cmd + K
  let activeResultIndex = -1;
  palette?.addEventListener('keydown', (e) => {
    const items = [...(paletteResults?.querySelectorAll('.smart-book-row, .smart-chip') || [])];
    if (e.key === 'ArrowDown' && items.length) {
      e.preventDefault();
      activeResultIndex = (activeResultIndex + 1) % items.length;
      items.forEach((el, i) => el.classList.toggle('is-active', i === activeResultIndex));
      items[activeResultIndex]?.scrollIntoView({ block: 'nearest' });
    } else if (e.key === 'ArrowUp' && items.length) {
      e.preventDefault();
      activeResultIndex = (activeResultIndex - 1 + items.length) % items.length;
      items.forEach((el, i) => el.classList.toggle('is-active', i === activeResultIndex));
      items[activeResultIndex]?.scrollIntoView({ block: 'nearest' });
    } else if (e.key === 'Enter' && activeResultIndex >= 0 && items[activeResultIndex]) {
      e.preventDefault();
      items[activeResultIndex].click();
    }
  });

  document.addEventListener('keydown', (e) => {
    if ((e.metaKey || e.ctrlKey) && e.key.toLowerCase() === 'k') {
      e.preventDefault();
      openPalette();
    } else if (e.key === 'Escape') {
      closeQuickLogin();
    }
  });

  // Inline Smart Dropdown below #hero-search-input on /
  const heroInput = document.getElementById('hero-search-input');
  const heroDropdown = document.getElementById('hero-smart-dropdown');
  const heroForm = heroInput?.closest('form');
  let heroTimer = null;

  if (heroInput && heroDropdown) {
    const loadHeroSmart = async (q) => {
      try {
        const data = await fetchSmart(q);
        heroDropdown.innerHTML = renderSmartMarkup(data, q);
        heroDropdown.hidden = false;
        wireFillButtons(heroDropdown, heroInput, loadHeroSmart);
      } catch {}
    };

    heroInput.addEventListener('focus', () => {
      loadHeroSmart(heroInput.value);
    });
    heroInput.addEventListener('input', () => {
      clearTimeout(heroTimer);
      heroTimer = setTimeout(() => loadHeroSmart(heroInput.value), 160);
    });
    heroForm?.addEventListener('submit', () => {
      if (heroInput.value) pushHistory(heroInput.value);
    });
    document.addEventListener('click', (e) => {
      if (!e.target.closest('.hero-search-shell')) {
        heroDropdown.hidden = true;
      }
    });
  }
})();
