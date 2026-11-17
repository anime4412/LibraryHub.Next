const supportRoot = document.querySelector(".support-layout");
const supportMessage = document.querySelector("#support-message");
const ticketId = Number(supportRoot?.dataset.selectedTicket || 0);
const showSupportError = error => LibraryHub.message(supportMessage, error.message, true);
document.querySelector("#new-ticket-form")?.addEventListener("submit", async event => {
  event.preventDefault();
  const button = event.currentTarget.querySelector("[type=submit]");
  if (!LibraryHub.setBusy(button, true)) return;
  try {
    const result = await LibraryHub.request("/api/v1/support/tickets", {
      method: "POST", body: JSON.stringify(LibraryHub.formBody(event.currentTarget))
    });
    location.href = `/support?ticketId=${result.id}`;
  } catch (error) {
    LibraryHub.setBusy(button, false);
    showSupportError(error);
  }
});
document.querySelector("#reply-form")?.addEventListener("submit", async event => {
  event.preventDefault();
  const button = event.currentTarget.querySelector("[type=submit]");
  if (!LibraryHub.setBusy(button, true)) return;
  try {
    await LibraryHub.request(`/api/v1/support/tickets/${ticketId}/messages`, {
      method: "POST", body: JSON.stringify(LibraryHub.formBody(event.currentTarget))
    });
    event.currentTarget.reset();
  } catch (error) { showSupportError(error); }
  finally { LibraryHub.setBusy(button, false); }
});
document.querySelector("#claim-ticket")?.addEventListener("click", async event => {
  const button = event.currentTarget;
  if (!LibraryHub.setBusy(button, true)) return;
  try {
    await LibraryHub.request(`/api/v1/support/tickets/${ticketId}/claim`, { method: "POST" });
    LibraryHub.message(supportMessage, "Đã nhận xử lý yêu cầu hỗ trợ.");
  } catch (error) { showSupportError(error); }
  finally { LibraryHub.setBusy(button, false); }
});
document.querySelector("#close-ticket")?.addEventListener("click", async event => {
  const button = event.currentTarget;
  if (!LibraryHub.setBusy(button, true)) return;
  try {
    await LibraryHub.request(`/api/v1/support/tickets/${ticketId}/close`, { method: "POST" });
    location.reload();
  } catch (error) {
    LibraryHub.setBusy(button, false);
    showSupportError(error);
  }
});
const appendMessage = item => {
  if (document.querySelector(`[data-message-id="${item.id}"]`)) return;
  const bubble = document.createElement("div");
  bubble.className = `chat-bubble sender-${String(item.senderType)}`;
  bubble.dataset.messageId = item.id;
  const strong = document.createElement("strong");
  strong.textContent = Number(item.senderType) === 0 ? "Độc giả" : "Nhân viên";
  const paragraph = document.createElement("p");
  paragraph.textContent = item.content;
  bubble.append(strong, paragraph);
  document.querySelector("#chat-messages")?.append(bubble);
  bubble.scrollIntoView({ block: "nearest" });
};
if (ticketId) {
  const supportConnection = new SignalRLite("/hubs/support", (target, args) => {
    if (target.toLowerCase() === "message") appendMessage(args[0]);
  });
  supportConnection.start().then(() => {
    const deadline = Date.now() + 10_000;
    const waitForOpen = setInterval(() => {
      if (supportConnection.socket?.readyState === WebSocket.OPEN) {
        clearInterval(waitForOpen);
        supportConnection.invoke("JoinTicket", [ticketId]);
      } else if (Date.now() >= deadline || supportConnection.socket?.readyState === WebSocket.CLOSED) {
        clearInterval(waitForOpen);
      }
    }, 50);
  }).catch(() => {});
}
