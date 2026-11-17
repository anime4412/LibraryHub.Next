const notificationMessage = document.querySelector("#notification-message");
const unreadCount = document.querySelector("#unread-count");
const readAllButton = document.querySelector("#read-all");
const updateReadAllState = () => {
  if (readAllButton && readAllButton.dataset.busy !== "true")
    readAllButton.disabled = Number(unreadCount?.textContent || 0) === 0;
};
const markRead = async (row, button) => {
  if (!row || !LibraryHub.setBusy(button, true)) return;
  try {
    await LibraryHub.request(`/api/v1/notifications/${row.dataset.notificationId}/read`, { method: "POST" });
    row.classList.remove("unread");
    button.remove();
    unreadCount.textContent = String(Math.max(0, Number(unreadCount.textContent) - 1));
    updateReadAllState();
  } catch (error) {
    LibraryHub.setBusy(button, false);
    LibraryHub.message(notificationMessage, error.message, true);
  }
};
document.querySelectorAll(".read-one").forEach(button => button.addEventListener("click", () =>
  markRead(button.closest(".notification-row"), button)));
document.querySelector("#read-all")?.addEventListener("click", async event => {
  const button = event.currentTarget;
  if (!LibraryHub.setBusy(button, true)) return;
  try {
    await LibraryHub.request("/api/v1/notifications/read-all", { method: "POST" });
    document.querySelectorAll(".notification-row").forEach(row => row.classList.remove("unread"));
    document.querySelectorAll(".read-one").forEach(x => x.remove());
    unreadCount.textContent = "0";
    LibraryHub.message(notificationMessage, "Đã đánh dấu tất cả thông báo là đã đọc.");
  } catch (error) { LibraryHub.message(notificationMessage, error.message, true); }
  finally {
    LibraryHub.setBusy(button, false);
    updateReadAllState();
  }
});
const notificationConnection = new SignalRLite("/hubs/notifications", (target, args) => {
  if (target.toLowerCase() !== "notification") return;
  const item = args[0];
  if (document.querySelector(`[data-notification-id="${Number(item.id)}"]`)) return;
  const row = document.createElement("article");
  row.className = "notification-row unread";
  row.dataset.notificationId = item.id;
  const content = document.createElement("div");
  const title = document.createElement("strong");
  title.textContent = item.title;
  const copy = document.createElement("p");
  copy.textContent = item.message;
  content.append(title, copy);
  const actions = document.createElement("div");
  actions.className = "notification-actions";
  const time = document.createElement("span");
  time.textContent = "Vừa xong";
  const readButton = document.createElement("button");
  readButton.className = "button button-small button-secondary read-one";
  readButton.type = "button";
  readButton.textContent = "Đã đọc";
  readButton.addEventListener("click", () => markRead(row, readButton));
  actions.append(time, readButton);
  row.append(content, actions);
  const list = document.querySelector("#notification-list");
  list.querySelector(".empty-inline")?.remove();
  list.prepend(row);
  unreadCount.textContent = String(Number(unreadCount.textContent) + 1);
  updateReadAllState();
});
notificationConnection.start().catch(() => {});
updateReadAllState();
