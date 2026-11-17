(() => {
  const host = document.querySelector("#borrow-cart-continuation");
  if (!host) return;

  let flash;
  if (host.dataset.canAdd === "true") {
    flash = LibraryHubBorrowCart.add({
      bookId: Number(host.dataset.bookId),
      title: host.dataset.bookTitle,
      availableQuantity: Number(host.dataset.availableQuantity)
    });
  } else {
    flash = { error: true, message: host.dataset.message || "Không thể thêm sách vào giỏ mượn." };
  }

  try {
    sessionStorage.setItem("libraryhub-flash-message", JSON.stringify({
      text: flash.message,
      error: flash.error === true,
      bookId: Number(host.dataset.bookId) || null,
      addedToCart: host.dataset.canAdd === "true" && flash.error !== true
    }));
  } catch {
    // Việc chuyển hướng vẫn phải tiếp tục nếu trình duyệt chặn sessionStorage.
  }
  location.replace(host.dataset.returnUrl || "/");
})();
