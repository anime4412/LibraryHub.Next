const root = document.documentElement;
const savedTheme = localStorage.getItem("library-theme");
if (savedTheme) root.dataset.theme = savedTheme;
document.querySelector("#theme-toggle")?.addEventListener("click", () => {
  root.dataset.theme = root.dataset.theme === "dark" ? "light" : "dark";
  localStorage.setItem("library-theme", root.dataset.theme);
});

document.querySelector("#menu-toggle")?.addEventListener("click", event => {
  const navigation = document.querySelector("#main-navigation");
  if (!navigation) return;
  const open = navigation.classList.toggle("open");
  event.currentTarget.setAttribute("aria-expanded", String(open));
  event.currentTarget.setAttribute("aria-label", open ? "Đóng trình đơn" : "Mở trình đơn");
});

document.querySelectorAll("[data-password-toggle]").forEach((button) => {
  button.addEventListener("click", () => {
    const input = button.parentElement?.querySelector("input");
    if (!input) return;
    const showing = input.type === "text";
    input.type = showing ? "password" : "text";
    button.textContent = showing ? "Hiện" : "Ẩn";
    button.setAttribute("aria-label", showing ? "Hiện mật khẩu" : "Ẩn mật khẩu");
  });
});

const ensureFeedbackUi = () => {
  let region = document.querySelector("#toast-region");
  if (!region) {
    region = document.createElement("div");
    region.id = "toast-region";
    region.className = "toast-region";
    region.setAttribute("aria-live", "polite");
    document.body.append(region);
  }
  let dialog = document.querySelector("#library-dialog");
  if (!dialog) {
    dialog = document.createElement("dialog");
    dialog.id = "library-dialog";
    dialog.className = "library-dialog";
    dialog.setAttribute("aria-labelledby", "library-dialog-title");
    dialog.setAttribute("aria-describedby", "library-dialog-message");
    dialog.innerHTML = `<form method="dialog" class="dialog-card">
      <h2 id="library-dialog-title"></h2>
      <p id="library-dialog-message"></p>
      <div class="dialog-actions">
        <button class="button button-secondary" value="cancel">Hủy</button>
        <button class="button dialog-confirm" value="confirm">Xác nhận</button>
      </div>
    </form>`;
    document.body.append(dialog);
  }
  return { region, dialog };
};

const notify = (text, error = false) => {
  const { region } = ensureFeedbackUi();
  const toast = document.createElement("div");
  toast.className = `toast ${error ? "toast-error" : "toast-success"}`;
  toast.setAttribute("role", error ? "alert" : "status");
  const icon = document.createElement("span");
  icon.className = "toast-badge";
  icon.setAttribute("aria-hidden", "true");
  icon.textContent = error ? "!" : "✓";
  const copy = document.createElement("span");
  copy.className = "toast-text";
  copy.textContent = text;
  const close = document.createElement("button");
  close.type = "button";
  close.setAttribute("aria-label", "Đóng thông báo");
  close.textContent = "×";
  const dismiss = () => {
    toast.classList.add("is-leaving");
    setTimeout(() => toast.remove(), 220);
  };
  close.addEventListener("click", dismiss);
  toast.append(icon, copy, close);
  region.append(toast);
  setTimeout(dismiss, 4500);
};

const trapDialogFocus = (event, dialog) => {
  if (event.key !== "Tab") return;
  const focusable = [...dialog.querySelectorAll(
    'a[href], button:not([disabled]), input:not([disabled]), select:not([disabled]), textarea:not([disabled]), [tabindex]:not([tabindex="-1"])')]
    .filter(element => !element.hidden && element.getClientRects().length > 0);
  if (!focusable.length) {
    event.preventDefault();
    dialog.focus();
    return;
  }
  const first = focusable[0];
  const last = focusable[focusable.length - 1];
  if (event.shiftKey && (document.activeElement === first || !dialog.contains(document.activeElement))) {
    event.preventDefault();
    last.focus();
  } else if (!event.shiftKey && document.activeElement === last) {
    event.preventDefault();
    first.focus();
  }
};

const askConfirmation = ({ title, message, confirmText = "Xác nhận", danger = false }) => {
  const { dialog } = ensureFeedbackUi();
  if (dialog.open) return Promise.resolve(false);
  const previousFocus = document.activeElement;
  dialog.querySelector("#library-dialog-title").textContent = title;
  dialog.querySelector("#library-dialog-message").textContent = message;
  const confirmButton = dialog.querySelector(".dialog-confirm");
  confirmButton.textContent = confirmText;
  confirmButton.classList.toggle("danger-button", danger);
  dialog.showModal();
  dialog.querySelector('[value="cancel"]').focus();
  return new Promise(resolve => {
    const finish = accepted => {
      dialog.removeEventListener("close", onClose);
      dialog.removeEventListener("cancel", onCancel);
      dialog.removeEventListener("keydown", onKeyDown);
      dialog.removeEventListener("click", onClick);
      if (previousFocus instanceof HTMLElement && previousFocus.isConnected) previousFocus.focus();
      resolve(accepted);
    };
    const onClose = () => finish(dialog.returnValue === "confirm");
    const onCancel = event => {
      event.preventDefault();
      dialog.close("cancel");
    };
    const onKeyDown = event => {
      if (event.key === "Tab") {
        trapDialogFocus(event, dialog);
        return;
      }
      if (event.key !== "Escape") return;
      event.preventDefault();
      dialog.close("cancel");
    };
    const onClick = event => {
      if (event.target === dialog) dialog.close("cancel");
    };
    dialog.addEventListener("close", onClose);
    dialog.addEventListener("cancel", onCancel);
    dialog.addEventListener("keydown", onKeyDown);
    dialog.addEventListener("click", onClick);
  });
};

const workpointKey = "libraryhub-workpoint";
const saveWorkpoint = (extra = {}) => {
  try {
    sessionStorage.setItem(workpointKey, JSON.stringify({
      path: `${location.pathname}${location.search}`,
      scrollY: window.scrollY,
      selector: extra.selector || (document.querySelector("#book-actions") ? "#book-actions" : null),
      action: extra.action || null,
      bookId: extra.bookId || null,
      ts: Date.now()
    }));
  } catch {
    // Ignore storage quota / privacy mode errors
  }
};

window.LibraryHub = {
  saveWorkpoint,
  async request(url, options = {}) {
    const headers = { ...(options.headers || {}) };
    const method = String(options.method || "GET").toUpperCase();
    if (!['GET', 'HEAD', 'OPTIONS'].includes(method) && !headers["X-LibraryHub-Request"])
      headers["X-LibraryHub-Request"] = "XMLHttpRequest";
    if (options.body && !(options.body instanceof FormData) && !headers["Content-Type"]) headers["Content-Type"] = "application/json";
    const response = await fetch(url, { credentials: "same-origin", ...options, headers });
    if (response.status === 401) {
      let problem;
      try { problem = await response.json(); } catch { problem = {}; }
      saveWorkpoint();
      const targetReturn = `${location.pathname}${location.search}${document.querySelector("#book-actions") ? "#book-actions" : location.hash}`;
      const fallback = `/login?returnUrl=${encodeURIComponent(targetReturn)}`;
      let loginUrl = fallback;
      try {
        const candidate = new URL(problem.loginUrl || fallback, location.origin);
        if (candidate.origin === location.origin)
          loginUrl = `${candidate.pathname}${candidate.search}${candidate.hash}`;
      } catch {
        loginUrl = fallback;
      }
      location.assign(loginUrl);
      const error = new Error(problem.message || "Bạn cần đăng nhập.");
      error.authenticationRedirected = true;
      throw error;
    }
    if (response.status === 403) throw new Error("Bạn không có quyền thực hiện thao tác này.");
    if (!response.ok) {
      let problem;
      try { problem = await response.json(); } catch { problem = {}; }
      const validation = problem.errors
        ? Object.values(problem.errors).flat().join(" ")
        : "";
      throw new Error(problem.detail || validation || problem.message || problem.title || `Yêu cầu thất bại (${response.status}).`);
    }
    if (response.status === 204) return null;
    const type = response.headers.get("content-type") || "";
    return type.includes("json") ? response.json() : response.text();
  },
  message(element, text, error = false) {
    if (text) notify(text, error);
    if (!element) return;
    element.hidden = false;
    element.className = `alert ${error ? "alert-error" : "alert-success"}`;
    element.setAttribute("role", error ? "alert" : "status");
    element.setAttribute("aria-live", error ? "assertive" : "polite");
    element.textContent = text;
    element.scrollIntoView({ behavior: "smooth", block: "nearest" });
  },
  setBusy(button, busy, text = "Đang xử lý…") {
    if (!button) return false;
    if (busy) {
      if (button.dataset.busy === "true") return false;
      button.dataset.busy = "true";
      button.dataset.busyHtml = button.innerHTML;
      button.disabled = true;
      button.setAttribute("aria-busy", "true");
      button.textContent = text;
      return true;
    }
    button.disabled = false;
    button.removeAttribute("aria-busy");
    delete button.dataset.busy;
    if (button.dataset.busyHtml !== undefined) {
      button.innerHTML = button.dataset.busyHtml;
      delete button.dataset.busyHtml;
    }
    return true;
  },
  formBody(form) {
    const result = {};
    for (const [key, value] of new FormData(form)) {
      const field = form.elements[key];
      const numericSelect = field?.tagName === "SELECT" && /^-?\d+(\.\d+)?$/.test(value);
      result[key] = field?.type === "number" || numericSelect ? Number(value) : value;
    }
    return result;
  },
  notify,
  askConfirmation,
  trapDialogFocus,
  syncCartBadge() {
    const badge = document.querySelector("#header-cart-badge");
    if (!badge) return;
    try {
      const parsed = JSON.parse(localStorage.getItem("libraryhub-borrow-cart") || "[]");
      const count = Array.isArray(parsed)
        ? parsed.reduce((sum, item) => sum + (Number(item?.quantity) || 1), 0)
        : 0;
      badge.textContent = String(count);
      badge.hidden = count === 0;
      badge.closest(".cart-link")?.classList.toggle("has-items", count > 0);
    } catch {
      badge.hidden = true;
    }
  }
};

window.LibraryHub.syncCartBadge();
window.addEventListener("storage", event => {
  if (event.key === "libraryhub-borrow-cart") window.LibraryHub.syncCartBadge();
});

try {
  const flash = JSON.parse(sessionStorage.getItem("libraryhub-flash-message") || "null");
  sessionStorage.removeItem("libraryhub-flash-message");
  if (flash?.text) {
    notify(String(flash.text), flash.error === true);
    const bookMsg = document.querySelector("#book-message");
    if (bookMsg && flash.addedToCart) {
      bookMsg.hidden = false;
      bookMsg.className = "alert alert-success";
      bookMsg.innerHTML = "";
      bookMsg.append(`${flash.text} `);
      const link = document.createElement("a");
      link.href = "/me/cart";
      link.style.fontWeight = "700";
      link.style.textDecoration = "underline";
      link.textContent = "Mở giỏ mượn & gửi phiếu →";
      bookMsg.append(link);
    }
  }
} catch {
  sessionStorage.removeItem("libraryhub-flash-message");
}

// Khôi phục đúng vị trí đang xem/mượn sau khi đăng nhập quay lại
try {
  const savedWp = JSON.parse(sessionStorage.getItem(workpointKey) || "null");
  if (savedWp && Date.now() - Number(savedWp.ts || 0) < 10 * 60 * 1000) {
    const currentPath = `${location.pathname}${location.search}`;
    if (savedWp.path === currentPath) {
      sessionStorage.removeItem(workpointKey);
      requestAnimationFrame(() => {
        const target = (savedWp.selector && document.querySelector(savedWp.selector))
          || (savedWp.bookId && document.querySelector(`.book-card[data-book-id="${savedWp.bookId}"]`))
          || document.querySelector("#book-actions");
        if (target) {
          target.scrollIntoView({ behavior: "smooth", block: "center" });
          target.classList.add("is-resumed-highlight");
          setTimeout(() => target.classList.remove("is-resumed-highlight"), 2400);
        } else if (typeof savedWp.scrollY === "number" && savedWp.scrollY > 40) {
          window.scrollTo({ top: savedWp.scrollY, behavior: "smooth" });
        }
      });
    }
  } else if (savedWp) {
    sessionStorage.removeItem(workpointKey);
  }
} catch {
  sessionStorage.removeItem(workpointKey);
}

// Lưu ngữ cảnh khi bấm Đăng nhập hoặc Đăng nhập nhanh từ thanh đầu trang
document.querySelectorAll('.quick-login-item-form').forEach((qlForm) => {
  qlForm.addEventListener("submit", () => {
    const returnField = qlForm.querySelector('input[name="ReturnUrl"]');
    if (returnField && returnField.value) {
      if (document.querySelector("#book-actions") && !returnField.value.includes("#"))
        returnField.value += "#book-actions";
      saveWorkpoint();
    }
  });
});

// Thanh tiến trình chuyển trang & Animation xuất hiện theo cuộn trang
(() => {
  const bar = document.querySelector("#nav-progress-bar");
  const startBar = () => {
    if (!bar) return;
    bar.classList.remove("is-done");
    bar.classList.add("is-loading");
  };
  const resetBar = () => {
    if (!bar) return;
    bar.classList.remove("is-loading");
    bar.classList.add("is-done");
    setTimeout(() => bar.classList.remove("is-done"), 250);
  };
  window.addEventListener("pageshow", resetBar);
  document.addEventListener("click", (event) => {
    const link = event.target.closest("a[href]");
    if (!link) return;
    if (link.target === "_blank" || link.hasAttribute("download") || event.metaKey || event.ctrlKey || event.shiftKey) return;
    const href = link.getAttribute("href") || "";
    if (!href || href.startsWith("#") || href.startsWith("javascript:")) return;
    try {
      const targetUrl = new URL(href, location.origin);
      if (targetUrl.origin === location.origin && targetUrl.pathname !== location.pathname) {
        startBar();
      }
    } catch { /* ignore */ }
  });

  if (window.matchMedia("(prefers-reduced-motion: reduce)").matches) return;
  const targets = [
    ...document.querySelectorAll(".pp-hero-copy, .pp-hero-stage, .pp-pillars-bar > *, .category-card, .book-card, .metric, .panel, .admin-heading, .record-row, .data-table tbody tr")
  ].slice(0, 60);
  if (!targets.length || typeof IntersectionObserver !== "function") return;

  targets.forEach((el, idx) => {
    el.classList.add("reveal-ready");
    el.style.setProperty("--reveal-delay", `${Math.min((idx % 8) * 35, 240)}ms`);
  });

  const observer = new IntersectionObserver((entries) => {
    for (const entry of entries) {
      if (!entry.isIntersecting) continue;
      entry.target.classList.add("is-visible");
      observer.unobserve(entry.target);
    }
  }, { rootMargin: "0px 0px -24px 0px", threshold: 0.04 });

  targets.forEach((el) => observer.observe(el));
})();

document.addEventListener("submit", event => {
  if (event.defaultPrevented) return;
  const form = event.target;
  if (!(form instanceof HTMLFormElement) || String(form.method).toLowerCase() !== "post") return;
  if (form.dataset.submitting === "true") {
    event.preventDefault();
    return;
  }
  if (!form.checkValidity()) return;
  form.dataset.submitting = "true";
  LibraryHub.setBusy(form.querySelector('button[type="submit"], input[type="submit"]'), true);
});

document.querySelectorAll("img[data-image-fallback]").forEach((image) => {
  const removeBrokenImage = () => image.remove();
  if (image.complete && image.naturalWidth === 0) removeBrokenImage();
  else image.addEventListener("error", removeBrokenImage, { once: true });
});
