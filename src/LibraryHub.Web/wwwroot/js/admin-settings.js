const settingsMessage = document.querySelector("#admin-message");
document.querySelector("#settings-form")?.addEventListener("submit", async event => {
  event.preventDefault();
  const button = event.currentTarget.querySelector("[type=submit]");
  if (!LibraryHub.setBusy(button, true)) return;
  try {
    await LibraryHub.request("/api/v1/settings", {
      method: "PUT", body: JSON.stringify(LibraryHub.formBody(event.currentTarget))
    });
    LibraryHub.message(settingsMessage, "Đã lưu cấu hình nghiệp vụ.");
  } catch (error) { LibraryHub.message(settingsMessage, error.message, true); }
  finally { LibraryHub.setBusy(button, false); }
});
