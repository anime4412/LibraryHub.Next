const modMsg = document.querySelector("#admin-message");

const runModAction = async (button, endpoint, body, okMessage) => {
  if (!LibraryHub.setBusy(button, true)) return;
  try {
    await LibraryHub.request(endpoint, { method: "POST", body: JSON.stringify(body) });
    LibraryHub.message(modMsg, okMessage);
    setTimeout(() => location.reload(), 550);
  } catch (error) {
    LibraryHub.setBusy(button, false);
    LibraryHub.message(modMsg, error.message, true);
  }
};

document.querySelectorAll(".mod-hide").forEach((btn) =>
  btn.addEventListener("click", () =>
    runModAction(btn, `/api/v1/moderation/reports/${btn.dataset.id}/resolve`,
      { hideComment: true, lockReaderDays: null, resolution: "Đã ẩn bình luận vi phạm" },
      "Đã ẩn bình luận và đánh dấu hoàn tất.")));

document.querySelectorAll(".mod-hide-lock").forEach((btn) =>
  btn.addEventListener("click", () =>
    runModAction(btn, `/api/v1/moderation/reports/${btn.dataset.id}/resolve`,
      { hideComment: true, lockReaderDays: 3, resolution: "Đã ẩn bình luận và khóa bình luận 3 ngày" },
      "Đã ẩn bình luận và khóa quyền bình luận 3 ngày.")));

document.querySelectorAll(".mod-reject").forEach((btn) =>
  btn.addEventListener("click", () =>
    runModAction(btn, `/api/v1/moderation/reports/${btn.dataset.id}/reject`,
      { reason: "Bình luận hợp lệ" },
      "Đã bỏ qua báo cáo.")));
