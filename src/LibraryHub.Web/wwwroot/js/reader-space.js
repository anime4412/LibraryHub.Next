const messageBox = document.querySelector("#ui-message");

document.querySelectorAll(".api-form").forEach((form) => {
  form.addEventListener("submit", async (event) => {
    event.preventDefault();
    const button = form.querySelector("button[type=submit]");
    if (!LibraryHub.setBusy(button, true)) return;
    try {
      await LibraryHub.request(form.dataset.endpoint, {
        method: form.dataset.method || "POST",
        body: JSON.stringify(LibraryHub.formBody(form))
      });
      LibraryHub.message(messageBox, form.dataset.success || "Đã cập nhật.");
      setTimeout(() => location.reload(), 700);
    } catch (error) {
      LibraryHub.message(messageBox, error.message, true);
    } finally {
      LibraryHub.setBusy(button, false);
    }
  });
});

document.querySelector("#read-all")?.addEventListener("click", async event => {
  const button = event.currentTarget;
  if (!LibraryHub.setBusy(button, true)) return;
  try {
    await LibraryHub.request("/api/v1/notifications/read-all", { method: "POST" });
    document.querySelectorAll(".notification-row").forEach((x) => x.classList.remove("unread"));
    LibraryHub.message(messageBox, "Đã đánh dấu toàn bộ thông báo là đã đọc.");
  } catch (error) { LibraryHub.message(messageBox, error.message, true); }
  finally {
    LibraryHub.setBusy(button, false);
    button.disabled = document.querySelectorAll(".notification-row.unread").length === 0;
  }
});

const cartKey = "libraryhub-borrow-cart";
const readCart = () => {
  try {
    const parsed = JSON.parse(localStorage.getItem(cartKey) || "[]");
    if (!Array.isArray(parsed)) throw new Error("invalid cart");
    const grouped = new Map();
    for (const item of parsed) {
      const bookId = Number(item?.bookId);
      const quantity = Number(item?.quantity);
      if (!Number.isInteger(bookId) || bookId <= 0 || !Number.isInteger(quantity) || quantity <= 0) continue;
      grouped.set(bookId, (grouped.get(bookId) || 0) + quantity);
    }
    return [...grouped].map(([bookId, quantity]) => ({ bookId, quantity }));
  } catch {
    localStorage.removeItem(cartKey);
    return [];
  }
};
const writeCart = (items) => {
  localStorage.setItem(cartKey, JSON.stringify(items));
  window.LibraryHub?.syncCartBadge?.();
};
const cartHost = document.querySelector("#borrow-cart-items");
const borrowSubmitButton = document.querySelector("#submit-borrow");
const updateBorrowSubmitState = () => {
  if (borrowSubmitButton && borrowSubmitButton.dataset.busy !== "true")
    borrowSubmitButton.disabled = readCart().length === 0;
};

async function renderCart() {
  if (!cartHost) return;
  const cart = readCart();
  if (!cart.length) {
    cartHost.className = "empty-inline";
    cartHost.textContent = "Chưa có sách trong giỏ. Chọn “Thêm vào giỏ mượn” tại trang chi tiết sách.";
    updateBorrowSubmitState();
    return;
  }
  const books = await Promise.all(cart.map((item) => LibraryHub.request(`/api/v1/books/${item.bookId}`)));
  cartHost.className = "cart-list";
  cartHost.replaceChildren();
  books.forEach((book, index) => {
    const row = document.createElement("div");
    row.className = "cart-row";
    const details = document.createElement("div");
    const title = document.createElement("strong");
    title.textContent = book.title;
    const availability = document.createElement("span");
    availability.textContent = `Còn ${Number(book.availableQuantity)} cuốn`;
    details.append(title, availability);

    const label = document.createElement("label");
    label.append("Số lượng");
    const input = document.createElement("input");
    const maximum = Math.max(1, Number(book.availableQuantity) || 0);
    input.className = "cart-quantity";
    input.type = "number";
    input.min = "1";
    input.max = String(maximum);
    input.value = String(Math.min(maximum, cart[index].quantity));
    input.addEventListener("change", () => {
      const current = readCart();
      const quantity = Math.min(maximum, Math.max(1, Number(input.value) || 1));
      input.value = String(quantity);
      current[index].quantity = quantity;
      writeCart(current);
    });
    label.append(input);

    const remove = document.createElement("button");
    remove.className = "icon-button cart-remove";
    remove.type = "button";
    remove.setAttribute("aria-label", `Xóa ${book.title} khỏi giỏ mượn`);
    remove.textContent = "×";
    remove.addEventListener("click", () => {
      const current = readCart();
      current.splice(index, 1);
      writeCart(current);
      renderCart().catch(error => LibraryHub.message(messageBox, error.message, true));
    });
    row.append(details, label, remove);
    cartHost.append(row);
  });
  updateBorrowSubmitState();
}

let borrowIdempotencyKey = null;
document.querySelector("#submit-borrow")?.addEventListener("click", async event => {
  const cart = readCart();
  if (!cart.length) return LibraryHub.message(messageBox, "Giỏ mượn đang trống.", true);
  const button = event.currentTarget;
  if (!LibraryHub.setBusy(button, true)) return;
  borrowIdempotencyKey ??= crypto.randomUUID();
  try {
    const result = await LibraryHub.request("/api/v1/borrows", {
      method: "POST",
      headers: { "Idempotency-Key": borrowIdempotencyKey },
      body: JSON.stringify({ lines: cart, note: document.querySelector("#borrow-note").value || null })
    });
    borrowIdempotencyKey = null;
    writeCart([]);
    await renderCart();
    LibraryHub.message(messageBox, `Đã tạo phiếu ${result.code}.`);
    setTimeout(() => location.reload(), 800);
  } catch (error) { LibraryHub.message(messageBox, error.message, true); }
  finally {
    LibraryHub.setBusy(button, false);
    updateBorrowSubmitState();
  }
});

updateBorrowSubmitState();
renderCart().catch((error) => LibraryHub.message(messageBox, error.message, true));

document.querySelectorAll(".cancel-borrow-btn").forEach((button) => {
  button.addEventListener("click", async () => {
    const code = button.dataset.code || "";
    if (!await LibraryHub.askConfirmation({
      title: "Hủy phiếu mượn chờ duyệt",
      message: `Xác nhận hủy phiếu ${code}? Bạn có thể tạo lại phiếu mới bất cứ lúc nào.`,
      confirmText: "Hủy phiếu",
      danger: true
    })) return;
    if (!LibraryHub.setBusy(button, true)) return;
    try {
      await LibraryHub.request(`/api/v1/borrows/${button.dataset.id}/cancel`, { method: "POST" });
      LibraryHub.message(messageBox, `Đã hủy phiếu ${code}.`);
      setTimeout(() => location.reload(), 650);
    } catch (error) {
      LibraryHub.setBusy(button, false);
      LibraryHub.message(messageBox, error.message, true);
    }
  });
});

document.querySelectorAll(".cancel-reservation-btn").forEach((button) => {
  button.addEventListener("click", async () => {
    const title = button.dataset.title || "";
    if (!await LibraryHub.askConfirmation({
      title: "Hủy đặt trước sách",
      message: `Hủy lượt giữ chỗ cho “${title}”?`,
      confirmText: "Hủy đặt trước",
      danger: true
    })) return;
    if (!LibraryHub.setBusy(button, true)) return;
    try {
      await LibraryHub.request(`/api/v1/reservations/${button.dataset.id}/cancel`, { method: "POST" });
      LibraryHub.message(messageBox, `Đã hủy đặt trước “${title}”.`);
      setTimeout(() => location.reload(), 650);
    } catch (error) {
      LibraryHub.setBusy(button, false);
      LibraryHub.message(messageBox, error.message, true);
    }
  });
});

const featuredForm = document.querySelector("#featured-books-form");
if (featuredForm) {
  const countEl = document.querySelector("#featured-count");
  const checkboxes = [...featuredForm.querySelectorAll('input[name="featuredBookIds"]')];
  const syncCount = () => {
    const checked = checkboxes.filter((c) => c.checked);
    if (countEl) countEl.textContent = `${checked.length}/4 đã chọn`;
  };
  checkboxes.forEach((cb) => cb.addEventListener("change", () => {
    const checked = checkboxes.filter((c) => c.checked);
    if (checked.length > 4) {
      cb.checked = false;
      LibraryHub.message(messageBox, "Chỉ được chọn tối đa 4 sách nổi bật.", true);
    }
    syncCount();
  }));
  featuredForm.addEventListener("submit", async (event) => {
    event.preventDefault();
    const btn = featuredForm.querySelector("[type=submit]");
    if (!LibraryHub.setBusy(btn, true)) return;
    const bookIds = checkboxes.filter((c) => c.checked).map((c) => Number(c.value));
    try {
      await LibraryHub.request("/api/v1/me/featured-books", {
        method: "PUT",
        body: JSON.stringify({ bookIds })
      });
      LibraryHub.message(messageBox, "Đã lưu tủ sách nổi bật trên hồ sơ công khai.");
    } catch (error) {
      LibraryHub.message(messageBox, error.message, true);
    } finally {
      LibraryHub.setBusy(btn, false);
    }
  });
}
