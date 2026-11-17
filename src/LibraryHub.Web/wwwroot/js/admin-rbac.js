const rbacMessage = document.querySelector("#admin-message");
document.querySelectorAll(".role-permission-form").forEach(form => form.addEventListener("submit", async event => {
  event.preventDefault();
  const button = form.querySelector("[type=submit]");
  const permissionIds = [...form.querySelectorAll("[name=permissionIds]:checked")].map(x => Number(x.value));
  if (!LibraryHub.setBusy(button, true)) return;
  try {
    await LibraryHub.request(`/api/v1/roles/${form.dataset.roleId}/permissions`, {
      method: "PUT", body: JSON.stringify({ permissionIds })
    });
    LibraryHub.message(rbacMessage, "Đã cập nhật quyền của vai trò.");
  } catch (error) { LibraryHub.message(rbacMessage, error.message, true); }
  finally { LibraryHub.setBusy(button, false); }
}));
document.querySelectorAll(".save-override").forEach(button => button.addEventListener("click", async () => {
  const row = button.closest("tr");
  const permissionId = row.querySelector(".override-permission").value;
  const effect = row.querySelector(".override-effect").value;
  if (!LibraryHub.setBusy(button, true)) return;
  try {
    await LibraryHub.request(`/api/v1/employees/${button.dataset.employeeId}/permission/${permissionId}`, {
      method: effect === "" ? "DELETE" : "PUT",
      body: effect === "" ? undefined : JSON.stringify({ effect: Number(effect) })
    });
    LibraryHub.message(rbacMessage, effect === ""
      ? "Đã bỏ ghi đè; quyền hiện được kế thừa từ vai trò."
      : "Đã áp dụng quyền ghi đè.");
  } catch (error) { LibraryHub.message(rbacMessage, error.message, true); }
  finally { LibraryHub.setBusy(button, false); }
}));
