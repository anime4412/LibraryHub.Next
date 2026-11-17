const circulationMessage = document.querySelector("#admin-message");
const walkinForm = document.querySelector("#walkin-form");
document.querySelector("#toggle-walkin-btn")?.addEventListener("click", () => {
  if (!walkinForm) return;
  walkinForm.hidden = !walkinForm.hidden;
  if (!walkinForm.hidden) walkinForm.scrollIntoView({ behavior: "smooth", block: "nearest" });
});
document.querySelector("#close-walkin-btn")?.addEventListener("click", () => {
  if (walkinForm) walkinForm.hidden = true;
});
walkinForm?.addEventListener("submit", async (event) => {
  event.preventDefault();
  const submit = walkinForm.querySelector("[type=submit]");
  if (!LibraryHub.setBusy(submit, true)) return;
  try {
    walkinForm.dataset.idempotencyKey ||= crypto.randomUUID();
    const payload = {
      readerId: Number(walkinForm.elements.readerId.value),
      lines: [{ bookId: Number(walkinForm.elements.bookId.value), quantity: Number(walkinForm.elements.quantity.value) }],
      note: walkinForm.elements.note.value || null,
      deliverImmediately: walkinForm.elements.deliverImmediately.checked
    };
    const res = await LibraryHub.request("/api/v1/borrows/walk-in", {
      method: "POST",
      headers: { "Idempotency-Key": walkinForm.dataset.idempotencyKey },
      body: JSON.stringify(payload)
    });
    delete walkinForm.dataset.idempotencyKey;
    LibraryHub.message(circulationMessage, `Đã lập phiếu ${res.code} tại quầy.`);
    setTimeout(() => location.reload(), 650);
  } catch (error) {
    LibraryHub.message(circulationMessage, error.message, true);
  } finally {
    LibraryHub.setBusy(submit, false);
  }
});
document.querySelectorAll(".circulation-action").forEach((button) => button.addEventListener("click", async () => {
  if (!LibraryHub.setBusy(button, true)) return;
  try {
    await LibraryHub.request(button.dataset.endpoint, { method: "POST", body: button.dataset.body || undefined });
    LibraryHub.message(circulationMessage, "Đã cập nhật trạng thái phiếu.");
    setTimeout(() => location.reload(), 500);
  } catch (error) { LibraryHub.message(circulationMessage, error.message, true); }
  finally { LibraryHub.setBusy(button, false); }
}));
const syncReturnLineState = (line) => {
  const condition = Number(line.querySelector(".return-condition")?.value);
  const damageInput = line.querySelector(".return-damage");
  const hint = line.querySelector(".return-hint");
  if (!damageInput) return;
  if (condition === 1) {
    damageInput.disabled = false;
    damageInput.min = "1";
    if (!Number(damageInput.value)) damageInput.value = "25";
    if (hint) hint.textContent = "Hư hỏng: tính % giá sách, sách không tự hoàn vào kho khả dụng.";
  } else if (condition === 2) {
    damageInput.value = "0";
    damageInput.min = "0";
    damageInput.disabled = true;
    if (hint) hint.textContent = "Mất sách: phạt 100% giá sách và trừ khỏi tổng kho.";
  } else {
    damageInput.value = "0";
    damageInput.min = "0";
    damageInput.disabled = true;
    if (hint) hint.textContent = "Nguyên vẹn: hoàn kho khả dụng, chỉ tính trễ hạn nếu có.";
  }
};

document.querySelectorAll(".return-line").forEach((line) => {
  syncReturnLineState(line);
  line.querySelector(".return-condition")?.addEventListener("change", () => syncReturnLineState(line));
});

document.querySelectorAll(".open-return").forEach((button) => button.addEventListener("click", () => {
  const form = button.closest(".record-details")?.querySelector(".return-form");
  if (!form) return;
  form.hidden = false;
  form.querySelectorAll(".return-line").forEach(syncReturnLineState);
}));
document.querySelectorAll(".cancel-return").forEach((button) => button.addEventListener("click", () => button.closest(".return-form").hidden = true));
document.querySelectorAll(".return-form").forEach((form) => form.addEventListener("submit", async (event) => {
  event.preventDefault();
  const button = form.querySelector("[type=submit]");
  if (!LibraryHub.setBusy(button, true)) return;
  const lines = [...form.querySelectorAll(".return-line")].map((line) => {
    const condition = Number(line.querySelector(".return-condition").value);
    const damagePercent = condition === 1 ? Number(line.querySelector(".return-damage").value) : 0;
    return {
      borrowDetailId: Number(line.dataset.detailId),
      quantity: Number(line.querySelector(".return-quantity").value),
      condition,
      damagePercent,
      note: line.querySelector(".return-note").value || null
    };
  });
  try {
    form.dataset.idempotencyKey ||= crypto.randomUUID();
    const result = await LibraryHub.request(form.dataset.endpoint, {
      method: "POST", headers: { "Idempotency-Key": form.dataset.idempotencyKey },
      body: JSON.stringify({ lines })
    });
    delete form.dataset.idempotencyKey;
    LibraryHub.message(circulationMessage, result.fineId
      ? `Đã trả sách, phát sinh phạt ${Number(result.fineAmount).toLocaleString("vi-VN")}đ.`
      : "Đã ghi nhận trả sách, không phát sinh phạt.");
    setTimeout(() => location.reload(), 700);
  } catch (error) { LibraryHub.message(circulationMessage, error.message, true); }
  finally { LibraryHub.setBusy(button, false); }
}));

document.querySelectorAll(".fine-collect").forEach((button) => button.addEventListener("click", async () => {
  const remaining = Number(button.dataset.remaining || 0);
  const code = button.dataset.code || "";
  if (!await LibraryHub.askConfirmation({
    title: "Xác nhận thu phạt tại quầy",
    message: `Ghi nhận độc giả đã thanh toán đủ ${remaining.toLocaleString("vi-VN")}đ cho khoản phạt ${code}?`,
    confirmText: "Đã thu đủ"
  })) return;
  if (!LibraryHub.setBusy(button, true)) return;
  try {
    await LibraryHub.request(`/api/v1/fines/${button.dataset.id}/collect`, {
      method: "POST",
      body: JSON.stringify({ amount: remaining })
    });
    LibraryHub.message(circulationMessage, `Đã xác nhận thu đủ khoản phạt ${code}.`);
    setTimeout(() => location.reload(), 550);
  } catch (error) {
    LibraryHub.setBusy(button, false);
    LibraryHub.message(circulationMessage, error.message, true);
  }
}));

document.querySelectorAll(".fine-waive").forEach((button) => button.addEventListener("click", async () => {
  const code = button.dataset.code || "";
  if (!await LibraryHub.askConfirmation({
    title: "Miễn giảm khoản phạt",
    message: `Xác nhận miễn toàn bộ dư nợ cho khoản phạt ${code}? Thao tác sẽ thông báo tới độc giả.`,
    confirmText: "Miễn phạt",
    danger: true
  })) return;
  if (!LibraryHub.setBusy(button, true)) return;
  try {
    await LibraryHub.request(`/api/v1/fines/${button.dataset.id}/waive`, {
      method: "POST",
      body: JSON.stringify({ reason: "Thủ thư xác nhận miễn tại quầy" })
    });
    LibraryHub.message(circulationMessage, `Đã miễn khoản phạt ${code}.`);
    setTimeout(() => location.reload(), 550);
  } catch (error) {
    LibraryHub.setBusy(button, false);
    LibraryHub.message(circulationMessage, error.message, true);
  }
}));
document.querySelectorAll(".reservation-status").forEach((select) => {
  select.value = select.dataset.current;
  select.addEventListener("change", async () => {
    select.disabled = true;
    try {
      await LibraryHub.request(`/api/v1/reservations/${select.dataset.id}/status`, {
        method: "PATCH", body: JSON.stringify({ status: Number(select.value) })
      });
      select.dataset.current = select.value;
      LibraryHub.message(circulationMessage, "Đã cập nhật đặt trước.");
      setTimeout(() => location.reload(), 500);
    } catch (error) {
      select.value = select.dataset.current;
      LibraryHub.message(circulationMessage, error.message, true);
    }
    finally { select.disabled = false; }
  });
});
