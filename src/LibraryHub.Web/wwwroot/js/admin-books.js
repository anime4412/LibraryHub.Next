const form = document.querySelector("#book-form");
const message = document.querySelector("#admin-message");
const idField = document.querySelector("#book-id");

const openForm = (button = null) => {
  form.hidden = false;
  form.reset();
  idField.value = button?.dataset.id || "";
  document.querySelector("#book-form-title").textContent = button ? "Sửa sách" : "Thêm sách";
  if (button) {
    for (const field of form.elements) {
      if (!field.name) continue;
      const key = field.name.replace(/[A-Z]/g, (value) => `-${value.toLowerCase()}`);
      if (button.dataset[field.name] !== undefined) field.value = button.dataset[field.name];
      else if (button.dataset[key] !== undefined) field.value = button.dataset[key];
    }
  }
  form.scrollIntoView({ behavior: "smooth" });
};

document.querySelector("#new-book")?.addEventListener("click", () => openForm());
document.querySelectorAll(".edit-book").forEach((button) => button.addEventListener("click", () => openForm(button)));
["#close-book-form", "#cancel-book"].forEach((selector) => document.querySelector(selector)?.addEventListener("click", () => { form.hidden = true; }));

form?.addEventListener("submit", async (event) => {
  event.preventDefault();
  const submit = form.querySelector("[type=submit]");
  if (!LibraryHub.setBusy(submit, true)) return;
  const id = idField.value;
  try {
    const coverFile = document.querySelector("#cover-file").files[0];
    if (coverFile) {
      const upload = new FormData();
      upload.append("file", coverFile);
      const uploaded = await LibraryHub.request("/api/v1/uploads/book-cover", { method: "POST", body: upload });
      form.elements.coverImagePath.value = uploaded.path;
    }
    await LibraryHub.request(id ? `/api/v1/books/${id}` : "/api/v1/books", {
      method: id ? "PUT" : "POST",
      body: JSON.stringify(LibraryHub.formBody(form))
    });
    LibraryHub.message(message, id ? "Đã cập nhật sách." : "Đã thêm sách.");
    setTimeout(() => location.reload(), 600);
  } catch (error) {
    LibraryHub.message(message, error.message, true);
  } finally {
    LibraryHub.setBusy(submit, false);
  }
});

let dialogOpener = null;
const openDialog = (dialog, opener, initialFocus) => {
  dialogOpener = opener;
  dialog.scrollTop = 0;
  dialog.showModal();
  requestAnimationFrame(() => initialFocus?.focus());
};
const closeDialog = (dialog) => {
  dialog.close();
  if (dialogOpener?.isConnected) dialogOpener.focus();
  dialogOpener = null;
};
document.querySelectorAll(".lifecycle-dialog").forEach((dialog) => {
  dialog.querySelectorAll(".dialog-cancel").forEach((button) => button.addEventListener("click", () => closeDialog(dialog)));
  dialog.addEventListener("keydown", (event) => {
    if (event.key === "Tab") {
      LibraryHub.trapDialogFocus(event, dialog);
      return;
    }
    if (event.key !== "Escape") return;
    event.preventDefault();
    closeDialog(dialog);
  });
  dialog.addEventListener("cancel", (event) => {
    event.preventDefault();
    closeDialog(dialog);
  });
  dialog.addEventListener("click", (event) => {
    if (event.target === dialog) closeDialog(dialog);
  });
  dialog.addEventListener("close", () => {
    if (dialogOpener?.isConnected) dialogOpener.focus();
    dialogOpener = null;
  });
});

const suspendDialog = document.querySelector("#suspend-book-dialog");
const suspendForm = document.querySelector("#suspend-book-form");
const suspensionType = document.querySelector("#suspension-type");
const earlyRecallOption = document.querySelector("#early-recall-option");
const earlyReturn = document.querySelector("#request-early-return");
const recallDeadlineRow = document.querySelector("#recall-deadline-row");
const recallDeadline = document.querySelector("#recall-deadline");
const suspensionWarning = document.querySelector("#suspension-warning");
const confirmSuspension = document.querySelector("#confirm-suspension");
const canEmergency = document.querySelector(".admin-shell")?.dataset.canEmergency === "true";
let lifecycleBookId = null;
let lifecycleBookName = "";
let lifecycleImpact = null;

const updateSuspensionOptions = () => {
  const emergency = suspensionType.value === "1";
  earlyRecallOption.hidden = !emergency;
  recallDeadlineRow.hidden = !emergency || !earlyReturn.checked;
  recallDeadline.required = emergency && earlyReturn.checked;
  if (!recallDeadline.required) recallDeadline.value = "";
};
suspensionType?.addEventListener("change", updateSuspensionOptions);
earlyReturn?.addEventListener("change", updateSuspensionOptions);

document.querySelectorAll(".suspend-book").forEach((button) => button.addEventListener("click", async () => {
  lifecycleBookId = Number(button.dataset.id);
  lifecycleBookName = button.dataset.name || "sách đã chọn";
  suspendForm.reset();
  document.querySelector("#suspend-book-name").textContent = `Đầu sách: ${lifecycleBookName}`;
  document.querySelector("#impact-active-borrowers").textContent = "…";
  document.querySelector("#impact-pending-borrows").textContent = "…";
  document.querySelector("#impact-held-reservations").textContent = "…";
  confirmSuspension.disabled = true;
  suspensionWarning.textContent = "Đang kiểm tra các dữ liệu sẽ bị ảnh hưởng…";
  openDialog(suspendDialog, button, document.querySelector("#suspension-reason"));
  updateSuspensionOptions();
  try {
    lifecycleImpact = await LibraryHub.request(`/api/v1/books/${lifecycleBookId}/lifecycle-impact`);
    document.querySelector("#impact-active-borrowers").textContent = lifecycleImpact.activeBorrowerCount;
    document.querySelector("#impact-pending-borrows").textContent = lifecycleImpact.pendingBorrowRequestCount;
    document.querySelector("#impact-held-reservations").textContent = lifecycleImpact.heldReservationCount;
    const normalOption = suspensionType.querySelector('option[value="0"]');
    normalOption.disabled = lifecycleImpact.activeBorrowerCount > 0;
    if (lifecycleImpact.activeBorrowerCount > 0) {
      if (canEmergency) {
        suspensionType.value = "1";
        suspensionWarning.textContent = "Đang có độc giả mượn sách. Chỉ có thể thu hồi khẩn cấp; phiếu đang mượn vẫn được giữ nguyên.";
        confirmSuspension.disabled = false;
      } else {
        suspensionWarning.textContent = "Không thể ngừng phục vụ thông thường vì đang có độc giả mượn. Tài khoản này không có quyền thu hồi khẩn cấp.";
        confirmSuspension.disabled = true;
      }
    } else {
      suspensionType.value = "0";
      suspensionWarning.textContent = "Các yêu cầu chưa giao và đặt trước đang hoạt động sẽ bị hủy; không trừ kho và không tạo phạt.";
      confirmSuspension.disabled = false;
    }
    updateSuspensionOptions();
  } catch (error) {
    suspensionWarning.textContent = error.message;
    confirmSuspension.disabled = true;
  }
}));

let suspensionIdempotencyKey = null;
suspendForm?.addEventListener("submit", async (event) => {
  event.preventDefault();
  const reason = document.querySelector("#suspension-reason").value.trim();
  const type = Number(suspensionType.value);
  if (!reason) return;
  let deadline = null;
  if (type === 1 && earlyReturn.checked) {
    const parsed = new Date(recallDeadline.value);
    if (!recallDeadline.value || Number.isNaN(parsed.valueOf()) || parsed < new Date()) {
      suspensionWarning.textContent = "Hạn trả sớm phải từ thời điểm hiện tại trở đi.";
      return;
    }
    deadline = parsed.toISOString();
  }
  if (!LibraryHub.setBusy(confirmSuspension, true)) return;
  suspensionIdempotencyKey ??= crypto.randomUUID();
  try {
    await LibraryHub.request(`/api/v1/books/${lifecycleBookId}/suspend`, {
      method: "POST",
      headers: { "Idempotency-Key": suspensionIdempotencyKey },
      body: JSON.stringify({ type, reason, recallDeadline: deadline })
    });
    suspensionIdempotencyKey = null;
    closeDialog(suspendDialog);
    LibraryHub.message(message, type === 1 ? "Đã thu hồi và ngừng phục vụ sách." : "Đã ngừng phục vụ sách.");
    setTimeout(() => location.reload(), 700);
  } catch (error) {
    suspensionWarning.textContent = error.message;
  } finally {
    LibraryHub.setBusy(confirmSuspension, false);
  }
});

const restoreDialog = document.querySelector("#restore-book-dialog");
const restoreForm = document.querySelector("#restore-book-form");
const restoreReason = document.querySelector("#restore-reason");
const confirmRestore = document.querySelector("#confirm-restore");
document.querySelectorAll(".restore-book").forEach((button) => button.addEventListener("click", () => {
  lifecycleBookId = Number(button.dataset.id);
  lifecycleBookName = button.dataset.name || "sách đã chọn";
  restoreForm.reset();
  document.querySelector("#restore-book-name").textContent = `Đầu sách: ${lifecycleBookName}`;
  openDialog(restoreDialog, button, restoreReason);
}));
let restoreIdempotencyKey = null;
restoreForm?.addEventListener("submit", async (event) => {
  event.preventDefault();
  if (!LibraryHub.setBusy(confirmRestore, true)) return;
  restoreIdempotencyKey ??= crypto.randomUUID();
  try {
    await LibraryHub.request(`/api/v1/books/${lifecycleBookId}/restore`, {
      method: "POST",
      headers: { "Idempotency-Key": restoreIdempotencyKey },
      body: JSON.stringify({ reason: restoreReason.value.trim() })
    });
    restoreIdempotencyKey = null;
    closeDialog(restoreDialog);
    LibraryHub.message(message, "Sách đã chuyển sang trạng thái đang bảo trì / chờ kiểm tra.");
    setTimeout(() => location.reload(), 700);
  } catch (error) {
    LibraryHub.message(message, error.message, true);
  } finally {
    LibraryHub.setBusy(confirmRestore, false);
  }
});

const maintenanceDialog = document.querySelector("#maintenance-book-dialog");
const maintenanceForm = document.querySelector("#maintenance-book-form");
const maintenanceTotal = document.querySelector("#maintenance-total");
const maintenanceAvailable = document.querySelector("#maintenance-available");
const confirmMaintenance = document.querySelector("#confirm-maintenance");
document.querySelectorAll(".complete-maintenance").forEach((button) => button.addEventListener("click", () => {
  lifecycleBookId = Number(button.dataset.id);
  lifecycleBookName = button.dataset.name || "sách đã chọn";
  maintenanceForm.reset();
  maintenanceTotal.value = button.dataset.total;
  maintenanceAvailable.value = button.dataset.available;
  document.querySelector("#maintenance-book-name").textContent = `Đầu sách: ${lifecycleBookName}`;
  openDialog(maintenanceDialog, button, maintenanceTotal);
}));
let maintenanceIdempotencyKey = null;
maintenanceForm?.addEventListener("submit", async (event) => {
  event.preventDefault();
  const actualTotalQuantity = Number(maintenanceTotal.value);
  const availableQuantityAfterInspection = Number(maintenanceAvailable.value);
  if (availableQuantityAfterInspection > actualTotalQuantity) {
    LibraryHub.message(message, "Số bản khả dụng không được vượt tổng số bản thực tế.", true);
    return;
  }
  if (!LibraryHub.setBusy(confirmMaintenance, true)) return;
  maintenanceIdempotencyKey ??= crypto.randomUUID();
  try {
    await LibraryHub.request(`/api/v1/books/${lifecycleBookId}/complete-maintenance`, {
      method: "POST",
      headers: { "Idempotency-Key": maintenanceIdempotencyKey },
      body: JSON.stringify({ actualTotalQuantity, availableQuantityAfterInspection })
    });
    maintenanceIdempotencyKey = null;
    closeDialog(maintenanceDialog);
    LibraryHub.message(message, "Đã hoàn tất bảo trì và cập nhật tồn kho.");
    setTimeout(() => location.reload(), 700);
  } catch (error) {
    LibraryHub.message(message, error.message, true);
  } finally {
    LibraryHub.setBusy(confirmMaintenance, false);
  }
});

document.querySelectorAll(".permanent-delete-book").forEach((button) => button.addEventListener("click", async () => {
  const bookName = button.dataset.name?.trim() || "sách đã chọn";
  if (!await LibraryHub.askConfirmation({
    title: "Xóa vĩnh viễn sách",
    message: `Chỉ sách chưa từng phát sinh dữ liệu nghiệp vụ mới được xóa. Bạn có chắc muốn xóa vĩnh viễn “${bookName}”?`,
    confirmText: "Xóa vĩnh viễn",
    danger: true
  })) return;
  if (!LibraryHub.setBusy(button, true)) return;
  try {
    await LibraryHub.request(`/api/v1/books/${button.dataset.id}/permanent`, { method: "DELETE" });
    LibraryHub.message(message, "Đã xóa vĩnh viễn sách chưa từng được sử dụng.");
    button.closest("tr").remove();
  } catch (error) {
    LibraryHub.message(message, error.message, true);
    LibraryHub.setBusy(button, false);
  }
}));
