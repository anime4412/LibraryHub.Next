(() => {
  const root = document.querySelector("#lh-chatbot-root");
  if (!root) return;

  const fab = root.querySelector("#lh-chat-fab");
  const panel = root.querySelector("#lh-chat-panel");
  const closeBtn = root.querySelector("#lh-chat-close");
  const resetBtn = root.querySelector("#lh-chat-reset");
  const messagesEl = root.querySelector("#lh-chat-messages");
  const form = root.querySelector("#lh-chat-form");
  const input = root.querySelector("#lh-chat-input");
  const sendBtn = root.querySelector("#lh-chat-send");
  const startersEl = root.querySelector("#lh-chat-starters");

  const STORAGE_KEY = "lh_chatbot_history_v1";
  const ACTION_LABELS = {
    "/": "Trang chủ",
    "/#catalog": "Kho sách",
    "/me/cart": "Giỏ mượn sách",
    "/me/borrows": "Phiếu mượn của tôi",
    "/me/reservations": "Đặt trước",
    "/me/fines": "Khoản phạt",
    "/me/profile": "Hồ sơ cá nhân",
    "/support": "Hỗ trợ nhân viên",
    "/help": "Hướng dẫn"
  };

  let state = loadState();
  let busy = false;

  function loadState() {
    try {
      const raw = sessionStorage.getItem(STORAGE_KEY);
      if (raw) {
        const parsed = JSON.parse(raw);
        if (Array.isArray(parsed.messages)) {
          return { open: Boolean(parsed.open), messages: parsed.messages.slice(-18) };
        }
      }
    } catch {}
    return {
      open: false,
      messages: [
        {
          role: "model",
          text: "Xin chào! Mình là Trợ lý Thư viện AI. Bạn có thể hỏi gợi ý sách, quy định mượn trả hoặc tra cứu nhanh phiếu mượn.",
          books: [],
          actions: ["/#catalog", "/help"],
          ai: true
        }
      ]
    };
  }

  function saveState() {
    try {
      sessionStorage.setItem(STORAGE_KEY, JSON.stringify({
        open: state.open,
        messages: state.messages.slice(-18)
      }));
    } catch {}
  }

  function escapeHtml(text) {
    return String(text ?? "")
      .replaceAll("&", "&amp;")
      .replaceAll("<", "&lt;")
      .replaceAll(">", "&gt;")
      .replaceAll('"', "&quot;")
      .replaceAll("'", "&#39;");
  }

  function formatReplyText(text) {
    return escapeHtml(text)
      .replace(/\[([^\]]+)\]\((\/[a-zA-Z0-9\-_/#?=&]*)\)/g, '<a href="$2" style="color:inherit;font-weight:600;text-decoration:underline">$1</a>')
      .replace(/\*\*(.+?)\*\*/g, "<strong>$1</strong>")
      .replace(/\n/g, "<br>");
  }

  function render() {
    panel.hidden = !state.open;
    fab.setAttribute("aria-expanded", String(state.open));
    root.classList.toggle("is-open", state.open);

    messagesEl.innerHTML = state.messages.map((msg) => {
      const isUser = msg.role === "user";
      const booksHtml = Array.isArray(msg.books) && msg.books.length > 0
        ? `<div class="lh-chat-books">${msg.books.map((b) => `
            <a class="lh-chat-book" href="/books/${Number(b.id)}">
              ${b.coverImagePath ? `<img src="${escapeHtml(b.coverImagePath)}" alt="" loading="lazy" />` : `<span class="lh-chat-book-ph" aria-hidden="true">S</span>`}
              <span class="lh-chat-book-meta">
                <strong>${escapeHtml(b.title)}</strong>
                <span>${escapeHtml(b.author)} · Còn ${Number(b.availableQuantity)} cuốn</span>
              </span>
            </a>`).join("")}</div>`
        : "";
      const actionsHtml = !isUser && Array.isArray(msg.actions) && msg.actions.length > 0
        ? `<div class="lh-chat-actions">${msg.actions.map((url) => `
            <a class="lh-chat-action" href="${escapeHtml(url)}">${escapeHtml(ACTION_LABELS[url] || url)}</a>`).join("")}</div>`
        : "";
      return `
        <div class="lh-chat-msg ${isUser ? "is-user" : "is-bot"}">
          <div class="lh-chat-bubble">${formatReplyText(msg.text)}</div>
          ${booksHtml}
          ${actionsHtml}
        </div>`;
    }).join("");

    if (busy) {
      messagesEl.insertAdjacentHTML("beforeend", `
        <div class="lh-chat-msg is-bot">
          <div class="lh-chat-bubble is-typing" aria-label="Đang trả lời"><span></span><span></span><span></span></div>
        </div>`);
    }

    if (startersEl) startersEl.hidden = state.messages.length > 2;
    messagesEl.scrollTop = messagesEl.scrollHeight;
  }

  async function ask(questionText) {
    const clean = String(questionText || "").trim();
    if (!clean || busy) return;

    const history = state.messages
      .filter((m) => m.role === "user" || m.role === "model")
      .slice(-6)
      .map((m) => ({ role: m.role, text: m.text }));

    state.messages.push({ role: "user", text: clean });
    state.open = true;
    busy = true;
    input.value = "";
    sendBtn.disabled = true;
    saveState();
    render();

    try {
      const resp = await fetch("/api/v1/help/chat", {
        method: "POST",
        credentials: "same-origin",
        headers: {
          "Content-Type": "application/json",
          "X-LibraryHub-Request": "XMLHttpRequest"
        },
        body: JSON.stringify({ question: clean, history })
      });
      if (!resp.ok) throw new Error(`HTTP ${resp.status}`);
      const data = await resp.json();
      state.messages.push({
        role: "model",
        text: data.answer || "Mình chưa có phản hồi cho câu hỏi này.",
        books: Array.isArray(data.books) ? data.books : [],
        actions: Array.isArray(data.suggestedActions) ? data.suggestedActions : [],
        ai: Boolean(data.poweredByAi)
      });
    } catch {
      state.messages.push({
        role: "model",
        text: "Kết nối trợ lý AI tạm gián đoạn. Bạn có thể thử lại sau hoặc gửi yêu cầu tới nhân viên hỗ trợ.",
        books: [],
        actions: ["/support", "/help"],
        ai: false
      });
    } finally {
      busy = false;
      sendBtn.disabled = false;
      saveState();
      render();
      input.focus();
    }
  }

  fab?.addEventListener("click", () => {
    state.open = !state.open;
    saveState();
    render();
    if (state.open) input?.focus();
  });

  closeBtn?.addEventListener("click", () => {
    state.open = false;
    saveState();
    render();
  });

  resetBtn?.addEventListener("click", () => {
    sessionStorage.removeItem(STORAGE_KEY);
    state = loadState();
    state.open = true;
    saveState();
    render();
    input?.focus();
  });

  startersEl?.querySelectorAll("[data-chat-prompt]").forEach((btn) => {
    btn.addEventListener("click", () => ask(btn.dataset.chatPrompt || btn.textContent || ""));
  });

  form?.addEventListener("submit", (e) => {
    e.preventDefault();
    ask(input?.value || "");
  });

  document.addEventListener("keydown", (e) => {
    if (e.key === "Escape" && state.open) {
      state.open = false;
      saveState();
      render();
    }
  });

  render();
})();
