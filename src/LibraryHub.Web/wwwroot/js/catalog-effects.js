import { createBook3DViewer } from './book3d-stage.js?v=3';

const updateHeaderCartBadge = () => {
  const badge = document.querySelector('#header-cart-badge');
  if (!badge || !window.LibraryHubBorrowCart) return;
  const cart = window.LibraryHubBorrowCart.read();
  const count = cart.reduce((sum, item) => sum + (Number(item.quantity) || 1), 0);
  badge.textContent = String(count);
  badge.hidden = count === 0;
  badge.closest('.cart-link')?.classList.toggle('has-items', count > 0);
};

// 1. Hero 3D Book
const heroStage = document.querySelector('#hero-book3d-stage');
if (heroStage) {
  const viewer = createBook3DViewer(heroStage, {
    title: 'LIBRARYHUB',
    subtitle: 'THE ARCHITECTURAL EDITION',
    author: 'CURATED KNOWLEDGE',
    badge: 'SCHOLASTIC LUXURY · VOL. I',
    paddingFactor: 1.38
  });
  document.querySelector('#hero-book3d-reset')?.addEventListener('click', () => viewer?.resetView());
}

// 2. Book Detail Page 2D / 3D Switcher
const detailHost = document.querySelector('.book-detail');
const detailStage = document.querySelector('#detail-book3d-stage');
const detailToggle = document.querySelector('#toggle-detail-3d');
if (detailHost && detailStage && detailToggle) {
  let detailViewer = null;
  detailToggle.addEventListener('click', () => {
    const active3d = detailStage.hidden === false;
    if (active3d) {
      detailStage.hidden = true;
      detailHost.querySelector('.detail-cover-media')?.removeAttribute('hidden');
      detailToggle.textContent = 'Xem sách 3D';
      detailToggle.setAttribute('aria-pressed', 'false');
    } else {
      detailHost.querySelector('.detail-cover-media')?.setAttribute('hidden', '');
      detailStage.hidden = false;
      detailToggle.textContent = 'Xem bìa 2D';
      detailToggle.setAttribute('aria-pressed', 'true');
      if (!detailViewer) {
        detailViewer = createBook3DViewer(detailStage, {
          title: detailHost.dataset.bookTitle || 'LIBRARYHUB',
          subtitle: detailHost.dataset.bookCategory || 'CATALOG EDITION',
          author: detailHost.dataset.bookAuthor || '',
          badge: detailHost.dataset.bookIsbn || 'EDITION',
          coverUrl: detailHost.dataset.bookCover || null,
          paddingFactor: 1.38,
          particles: false
        });
      }
    }
  });
}

// 3. 3D Card Tilt & Radial Specular Sheen (pointer-fine & motion-ok only)
const allowTilt =
  window.matchMedia('(pointer: fine)').matches &&
  !window.matchMedia('(prefers-reduced-motion: reduce)').matches;

if (allowTilt) {
  document.querySelectorAll('.book-card[data-tilt-card]').forEach((card) => {
    const cover = card.querySelector('.cover');
    if (!cover) return;

    let raf = 0;
    let targetRx = 0;
    let targetRy = 0;
    let mx = 50;
    let my = 50;

    const applyTransform = () => {
      raf = 0;
      cover.style.transform = `perspective(900px) rotateX(${targetRx.toFixed(2)}deg) rotateY(${targetRy.toFixed(2)}deg) translateZ(6px)`;
      cover.style.setProperty('--mx', `${mx.toFixed(1)}%`);
      cover.style.setProperty('--my', `${my.toFixed(1)}%`);
    };

    card.addEventListener('pointermove', (e) => {
      const rect = cover.getBoundingClientRect();
      if (!rect.width || !rect.height) return;
      const px = (e.clientX - rect.left) / rect.width;
      const py = (e.clientY - rect.top) / rect.height;
      mx = px * 100;
      my = py * 100;
      targetRy = (px - 0.5) * 11;
      targetRx = (0.5 - py) * 11;
      if (!raf) raf = requestAnimationFrame(applyTransform);
    });

    card.addEventListener('pointerleave', () => {
      if (raf) cancelAnimationFrame(raf);
      raf = 0;
      cover.style.transform = '';
      cover.style.removeProperty('--mx');
      cover.style.removeProperty('--my');
    });
  });
}

// 5. Quick Add to Borrow Cart on Catalog Cards
const submitQuickAdd = async (bookId, button) => {
  if (!window.LibraryHub || !window.LibraryHubBorrowCart) return;
  const tokenInput = document.querySelector('#catalog-antiforgery input[name="__RequestVerificationToken"]');
  if (!window.LibraryHub.setBusy(button, true, 'Đang thêm…')) return;
  window.LibraryHub.saveWorkpoint?.({
    selector: `.book-card[data-book-id="${bookId}"]`,
    action: 'borrow',
    bookId
  });

  try {
    const formData = new FormData();
    formData.append('BookId', String(bookId));
    formData.append('ReturnUrl', `${location.pathname}${location.search}${location.hash}`);
    if (tokenInput?.value) {
      formData.append('__RequestVerificationToken', tokenInput.value);
    }

    const item = await window.LibraryHub.request('/borrow-cart/add', {
      method: 'POST',
      body: formData
    });
    const result = window.LibraryHubBorrowCart.add(item);
    window.LibraryHub.notify(result.message, result.error);
    updateHeaderCartBadge();
  } catch (error) {
    if (!error.authenticationRedirected) {
      window.LibraryHub.notify(error.message || 'Không thể thêm sách vào giỏ mượn.', true);
    }
  } finally {
    window.LibraryHub.setBusy(button, false);
  }
};

document.querySelectorAll('.js-quick-add-cart').forEach((button) => {
  button.addEventListener('click', (event) => {
    event.preventDefault();
    event.stopPropagation();
    const bookId = Number(button.dataset.bookId);
    if (bookId > 0) submitQuickAdd(bookId, button);
  });
});

// 6. Quick Preview Dialog with Interactive 3D Book
const previewDialog = document.querySelector('#book-preview-dialog');
if (previewDialog) {
  const preview3dHost = previewDialog.querySelector('#preview-book3d-stage');
  const titleEl = previewDialog.querySelector('[data-preview-title]');
  const authorEl = previewDialog.querySelector('[data-preview-author]');
  const metaEl = previewDialog.querySelector('[data-preview-meta]');
  const descEl = previewDialog.querySelector('[data-preview-description]');
  const stockEl = previewDialog.querySelector('[data-preview-stock]');
  const linkEl = previewDialog.querySelector('[data-preview-link]');
  const addBtn = previewDialog.querySelector('[data-preview-add]');
  let previewViewer = null;
  let currentPreviewBookId = 0;

  const openPreview = (card) => {
    const d = card.dataset;
    currentPreviewBookId = Number(d.bookId) || 0;
    const available = Number(d.bookAvailable) || 0;
    titleEl.textContent = d.bookTitle || '';
    authorEl.textContent = d.bookAuthor ? `Tác giả: ${d.bookAuthor}` : '';
    metaEl.textContent = [d.bookCategory, d.bookYear, d.bookIsbn ? `ISBN ${d.bookIsbn}` : ''].filter(Boolean).join(' · ');
    descEl.textContent = d.bookDescription || 'Khám phá chi tiết sách, tình trạng bản in và đánh giá từ bạn đọc tại thư viện.';
    stockEl.textContent = available > 0 ? `Còn ${available} cuốn sẵn sàng` : 'Hiện hết sách · Có thể đặt trước';
    stockEl.className = `availability ${available > 0 ? 'is-available' : 'is-out'}`;
    linkEl.href = `/books/${currentPreviewBookId}`;

    if (addBtn) {
      addBtn.disabled = available <= 0;
      addBtn.dataset.bookId = String(currentPreviewBookId);
    }

    if (typeof previewDialog.showModal === 'function') {
      previewDialog.showModal();
    } else {
      previewDialog.setAttribute('open', '');
    }

    const metaPayload = {
      title: d.bookTitle || 'LIBRARYHUB',
      author: d.bookAuthor || '',
      subtitle: d.bookCategory || 'COLLECTION',
      coverUrl: d.bookCover || null
    };

    if (!previewViewer && preview3dHost) {
      previewViewer = createBook3DViewer(preview3dHost, {
        ...metaPayload,
        paddingFactor: 1.38,
        particles: false
      });
    } else {
      previewViewer?.updateBook(metaPayload);
    }
  };

  document.querySelectorAll('.js-quick-preview').forEach((btn) => {
    btn.addEventListener('click', (event) => {
      event.preventDefault();
      event.stopPropagation();
      const card = btn.closest('.book-card');
      if (card) openPreview(card);
    });
  });

  addBtn?.addEventListener('click', () => {
    if (currentPreviewBookId > 0) submitQuickAdd(currentPreviewBookId, addBtn);
  });

  previewDialog.querySelector('[data-preview-close]')?.addEventListener('click', () => {
    previewDialog.close?.();
  });

  previewDialog.addEventListener('click', (event) => {
    if (event.target === previewDialog) previewDialog.close?.();
  });
}

updateHeaderCartBadge();
