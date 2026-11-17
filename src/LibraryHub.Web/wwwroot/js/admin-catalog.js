const catalogMessage = document.querySelector("#admin-message");
document.querySelectorAll(".catalog-module").forEach((module) => {
  const form = module.querySelector(".catalog-form");
  const fileInput = form.querySelector(".catalog-image-file");
  const previewWrap = form.querySelector(".catalog-image-preview");
  const previewImg = previewWrap?.querySelector("img");
  let previewObjectUrl = null;

  const setPreview = (src) => {
    if (!previewWrap || !previewImg) return;
    if (previewObjectUrl) {
      URL.revokeObjectURL(previewObjectUrl);
      previewObjectUrl = null;
    }
    if (!src) {
      previewImg.removeAttribute("src");
      previewWrap.hidden = true;
      return;
    }
    previewImg.src = src;
    previewWrap.hidden = false;
  };

  fileInput?.addEventListener("change", () => {
    const file = fileInput.files?.[0];
    if (!file) {
      setPreview(form.elements.imagePath?.value?.trim() || "");
      return;
    }
    if (previewObjectUrl) URL.revokeObjectURL(previewObjectUrl);
    previewObjectUrl = URL.createObjectURL(file);
    if (previewImg && previewWrap) {
      previewImg.src = previewObjectUrl;
      previewWrap.hidden = false;
    }
  });

  form.elements.imagePath?.addEventListener("input", () => {
    if (!fileInput?.files?.length) {
      setPreview(form.elements.imagePath.value.trim());
    }
  });

  const show = (button = null) => {
    form.hidden = false;
    form.reset();
    if (fileInput) fileInput.value = "";
    form.elements.id.value = button?.dataset.id || "";
    if (button) for (const field of form.elements) {
      if (!field.name || field.name === "id" || field.type === "file") continue;
      const dashed = field.name.replace(/[A-Z]/g, (x) => `-${x.toLowerCase()}`);
      field.value = button.dataset[field.name] ?? button.dataset[dashed] ?? "";
    }
    setPreview(form.elements.imagePath?.value?.trim() || "");
  };
  module.querySelector(".add-catalog").addEventListener("click", () => show());
  module.querySelector(".cancel-catalog").addEventListener("click", () => { form.hidden = true; setPreview(""); });
  module.querySelectorAll(".edit-catalog").forEach((x) => x.addEventListener("click", () => show(x)));
  form.addEventListener("submit", async (event) => {
    event.preventDefault();
    const submit = form.querySelector("[type=submit]");
    if (!LibraryHub.setBusy(submit, true)) return;
    try {
      const selectedFile = fileInput?.files?.[0];
      if (selectedFile && form.elements.imagePath) {
        const upload = new FormData();
        upload.append("file", selectedFile);
        const uploaded = await LibraryHub.request("/api/v1/uploads/category-image", {
          method: "POST",
          body: upload
        });
        form.elements.imagePath.value = uploaded.path;
      }
      const body = LibraryHub.formBody(form);
      const id = body.id;
      delete body.id;
      for (const field of form.querySelectorAll("input[type=number]")) body[field.name] = Number(field.value || 0);
      if (body.dateOfBirth === "") body.dateOfBirth = null;
      await LibraryHub.request(id ? `${module.dataset.endpoint}/${id}` : module.dataset.endpoint, { method: id ? "PUT" : "POST", body: JSON.stringify(body) });
      LibraryHub.message(catalogMessage, "Dữ liệu danh mục đã được lưu.");
      setTimeout(() => location.reload(), 500);
    } catch (error) { LibraryHub.message(catalogMessage, error.message, true); }
    finally { LibraryHub.setBusy(submit, false); }
  });
  module.querySelectorAll(".delete-catalog").forEach((button) => button.addEventListener("click", async () => {
    const itemName = button.dataset.name?.trim();
    const itemLabel = module.dataset.label?.trim() || "bản ghi danh mục";
    if (!await LibraryHub.askConfirmation({
      title: "Xóa bản ghi danh mục",
      message: itemName
        ? `Bạn sắp xóa mềm ${itemLabel} “${itemName}”. Dữ liệu lịch sử vẫn được giữ để đối soát.`
        : "Bản ghi sẽ ngừng xuất hiện trong danh mục đang hoạt động; dữ liệu lịch sử vẫn được giữ.",
      confirmText: "Xóa bản ghi",
      danger: true
    })) return;
    if (!LibraryHub.setBusy(button, true)) return;
    try {
      await LibraryHub.request(`${module.dataset.endpoint}/${button.dataset.id}`, { method: "DELETE" });
      button.closest(".catalog-row").remove();
      LibraryHub.message(catalogMessage, "Đã xóa bản ghi.");
    } catch (error) {
      LibraryHub.setBusy(button, false);
      LibraryHub.message(catalogMessage, error.message, true);
    }
  }));
});
