(() => {
  const key = "libraryhub-borrow-cart";

  const read = () => {
    try {
      const parsed = JSON.parse(localStorage.getItem(key) || "[]");
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
      localStorage.removeItem(key);
      return [];
    }
  };

  const add = (item) => {
    const bookId = Number(item?.bookId);
    const title = String(item?.title || "Sách");
    const availableQuantity = Number(item?.availableQuantity);
    if (!Number.isInteger(bookId) || bookId <= 0 || !Number.isInteger(availableQuantity) || availableQuantity <= 0)
      return { added: false, error: true, message: `“${title}” hiện không còn bản khả dụng.` };

    const cart = read();
    if (cart.some(entry => entry.bookId === bookId))
      return { added: false, error: false, message: `“${title}” đã có trong giỏ mượn.` };

    cart.push({ bookId, quantity: 1 });
    localStorage.setItem(key, JSON.stringify(cart));
    window.LibraryHub?.syncCartBadge?.();
    return { added: true, error: false, message: `Đã thêm “${title}” vào giỏ mượn.` };
  };

  window.LibraryHubBorrowCart = Object.freeze({ add, read });
})();
