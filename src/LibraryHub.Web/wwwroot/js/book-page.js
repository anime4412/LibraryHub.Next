const host = document.querySelector(".book-detail");
const message = document.querySelector("#book-message");
document.querySelector("#borrow-cart-add-form")?.addEventListener("submit", async event => {
  event.preventDefault();
  const form = event.currentTarget;
  const button = form.querySelector("[type=submit]");
  if (!LibraryHub.setBusy(button, true)) return;
  LibraryHub.saveWorkpoint?.({ selector: "#book-actions", action: "borrow", bookId: Number(host?.dataset.bookId) });
  try {
    const item = await LibraryHub.request(form.action, {
      method: "POST",
      body: new FormData(form)
    });
    const result = LibraryHubBorrowCart.add(item);
    LibraryHub.message(message, result.message, result.error);
  } catch (error) {
    if (!error.authenticationRedirected) LibraryHub.message(message, error.message, true);
  } finally {
    LibraryHub.setBusy(button, false);
  }
});
document.querySelector("#share-book")?.addEventListener("click", async event => {
  const button = event.currentTarget;
  if (!LibraryHub.setBusy(button, true)) return;
  try {
    await LibraryHub.request(`/api/v1/share?url=${encodeURIComponent(location.href)}`);
    if (navigator.share) {
      await navigator.share({ title: host.dataset.bookTitle, url: location.href });
      LibraryHub.message(message, "Đã mở bảng chia sẻ.");
    } else {
      await navigator.clipboard.writeText(location.href);
      LibraryHub.message(message, "Đã sao chép liên kết sách.");
    }
  } catch (error) {
    if (error.name !== "AbortError") LibraryHub.message(message, "Không thể chia sẻ lúc này. Vui lòng sao chép địa chỉ trên thanh trình duyệt.", true);
  } finally { LibraryHub.setBusy(button, false); }
});
document.querySelectorAll(".api-button").forEach((button) => button.addEventListener("click", async () => {
  let completedLabel = null;
  if (!LibraryHub.setBusy(button, true)) return;
  LibraryHub.saveWorkpoint?.({ selector: button.id ? `#${button.id}` : "#book-actions", action: button.id || "api", bookId: Number(host?.dataset.bookId) });
  try {
    const body = button.dataset.body || "{}";
    const result = await LibraryHub.request(button.dataset.endpoint, { method: button.dataset.method || "POST", body });
    if (button.id === "favorite-book" && result) completedLabel = result.favorite ? "♥ Đã yêu thích" : "♡ Yêu thích";
    LibraryHub.message(message, "Thao tác đã hoàn thành.");
  } catch (error) {
    if (!error.authenticationRedirected) LibraryHub.message(message, error.message, true);
  }
  finally {
    LibraryHub.setBusy(button, false);
    if (completedLabel) button.textContent = completedLabel;
  }
}));

document.querySelectorAll(".comment-like-btn").forEach((btn) => {
  btn.addEventListener("click", async () => {
    const id = btn.dataset.commentId;
    LibraryHub.saveWorkpoint?.({ selector: `#comment-${id}` });
    try {
      const res = await LibraryHub.request(`/api/v1/comments/${id}/like`, { method: "POST" });
      if (res) btn.innerHTML = `${res.liked ? "♥" : "♡"} <span>${res.likes}</span>`;
    } catch (error) {
      if (!error.authenticationRedirected) LibraryHub.message(message, error.message, true);
    }
  });
});

document.querySelectorAll(".comment-report-btn").forEach((btn) => {
  btn.addEventListener("click", async () => {
    const id = Number(btn.dataset.commentId);
    const reason = prompt("Nhập lý do báo cáo vi phạm (tối đa 500 ký tự):", "Nội dung không phù hợp");
    if (!reason || !reason.trim()) return;
    if (!LibraryHub.setBusy(btn, true)) return;
    try {
      await LibraryHub.request("/api/v1/comments/report", {
        method: "POST",
        body: JSON.stringify({ bookCommentId: id, profileCommentId: null, reason: reason.trim() })
      });
      LibraryHub.message(message, "Đã gửi báo cáo bình luận tới bộ phận kiểm duyệt.");
      btn.textContent = "Đã báo cáo";
      btn.disabled = true;
    } catch (error) {
      LibraryHub.setBusy(btn, false);
      if (!error.authenticationRedirected) LibraryHub.message(message, error.message, true);
    }
  });
});
const bindCommunityForm = (selector, endpoint, success, reload = false) => {
  document.querySelector(selector)?.addEventListener("submit", async event => {
    event.preventDefault();
    const button = event.currentTarget.querySelector("[type=submit]");
    if (!LibraryHub.setBusy(button, true)) return;
    try {
      await LibraryHub.request(endpoint, {
        method: "POST", body: JSON.stringify(LibraryHub.formBody(event.currentTarget))
      });
      LibraryHub.message(message, success);
      if (reload) setTimeout(() => location.reload(), 500);
    } catch (error) { LibraryHub.message(message, error.message, true); }
    finally { LibraryHub.setBusy(button, false); }
  });
};
bindCommunityForm("#rating-form", `/api/v1/books/${host.dataset.bookId}/ratings`, "Đã lưu đánh giá.");
bindCommunityForm("#reading-form", `/api/v1/reading/${host.dataset.bookId}`, "Đã lưu tiến độ đọc.");
bindCommunityForm("#comment-form", `/api/v1/books/${host.dataset.bookId}/comments`, "Đã đăng bình luận.", true);
