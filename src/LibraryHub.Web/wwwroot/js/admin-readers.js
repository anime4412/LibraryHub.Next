const readerForm = document.querySelector("#reader-form");
const readerMessage = document.querySelector("#admin-message");
document.querySelectorAll(".edit-reader").forEach(button => button.addEventListener("click", () => {
  readerForm.hidden = false;
  for (const field of readerForm.elements) {
    if (!field.name) continue;
    const key = field.name.replace(/[A-Z]/g, x => `-${x.toLowerCase()}`);
    field.value = button.dataset[field.name] ?? button.dataset[key] ?? "";
  }
  readerForm.elements.isVerified.checked = button.dataset.isVerified === "true";
  readerForm.scrollIntoView({ behavior: "smooth" });
}));
document.querySelector("#cancel-reader")?.addEventListener("click", () => readerForm.hidden = true);
readerForm?.addEventListener("submit", async event => {
  event.preventDefault();
  const button = readerForm.querySelector("[type=submit]");
  if (!LibraryHub.setBusy(button, true)) return;
  const values = LibraryHub.formBody(readerForm);
  const id = values.id;
  const operational = {
    fullName: values.fullName,
    dateOfBirth: values.dateOfBirth || null,
    phone: values.phone || null,
    address: values.address || null,
    identityNumber: values.identityNumber || null,
    expiresOn: values.expiresOn,
    maxBorrowBooks: values.maxBorrowBooks,
    type: values.type,
    gender: values.gender,
    isVerified: readerForm.elements.isVerified.checked,
    commentLockMode: values.commentLockMode,
    commentBlockedUntil: values.commentBlockedUntil || null,
    bio: values.bio || null,
    activeBadge: values.activeBadge || null,
    avatarPath: values.avatarPath || null,
    coverImagePath: values.coverImagePath || null,
    backgroundPath: values.backgroundPath || null
  };
  try {
    await LibraryHub.request(`/api/v1/readers/${id}`, { method: "PUT", body: JSON.stringify(operational) });
    LibraryHub.message(readerMessage, "Đã cập nhật hồ sơ độc giả.");
  } catch (error) { LibraryHub.message(readerMessage, error.message, true); }
  finally { LibraryHub.setBusy(button, false); }
});
