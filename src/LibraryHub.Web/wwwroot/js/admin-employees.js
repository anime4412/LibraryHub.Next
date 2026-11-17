const employeeForm = document.querySelector("#employee-form");
const employeeMessage = document.querySelector("#admin-message");
const openEmployee = button => {
  employeeForm.hidden = false;
  employeeForm.reset();
  document.querySelector("#employee-form-title").textContent = button ? "Sửa nhân viên" : "Thêm nhân viên";
  if (button) for (const field of employeeForm.elements) {
    if (!field.name) continue;
    const key = field.name.replace(/[A-Z]/g, x => `-${x.toLowerCase()}`);
    field.value = button.dataset[field.name] ?? button.dataset[key] ?? "";
  }
  employeeForm.scrollIntoView({ behavior: "smooth" });
};
document.querySelector("#new-employee")?.addEventListener("click", () => openEmployee());
document.querySelectorAll(".edit-employee").forEach(button => button.addEventListener("click", () => openEmployee(button)));
document.querySelector("#cancel-employee")?.addEventListener("click", () => employeeForm.hidden = true);
employeeForm?.elements.positionId?.addEventListener("change", event => {
  const salary = event.target.selectedOptions[0]?.dataset.salary;
  if (!employeeForm.elements.id.value && salary) employeeForm.elements.salary.value = salary;
});
employeeForm?.addEventListener("submit", async event => {
  event.preventDefault();
  const button = employeeForm.querySelector("[type=submit]");
  if (!LibraryHub.setBusy(button, true)) return;
  const body = LibraryHub.formBody(employeeForm);
  const id = body.id; delete body.id;
  body.roleId = body.roleId === "" ? null : Number(body.roleId);
  body.statusOptionId = null;
  body.leaveStartedOn = body.leaveStartedOn || null;
  body.lastUnexcusedOn = null;
  try {
    await LibraryHub.request(id ? `/api/v1/employees/${id}` : "/api/v1/employees", {
      method: id ? "PUT" : "POST", body: JSON.stringify(body)
    });
    LibraryHub.message(employeeMessage, "Đã lưu hồ sơ nhân viên.");
    setTimeout(() => location.reload(), 500);
  } catch (error) { LibraryHub.message(employeeMessage, error.message, true); }
  finally { LibraryHub.setBusy(button, false); }
});
document.querySelector("#position-form")?.addEventListener("submit", async event => {
  event.preventDefault();
  const form = event.currentTarget;
  const button = form.querySelector("[type=submit]");
  if (!LibraryHub.setBusy(button, true)) return;
  const body = LibraryHub.formBody(form);
  const id = body.id;
  delete body.id;
  body.allowSalaryOverride = form.elements.allowSalaryOverride.checked;
  body.description = null;
  try {
    await LibraryHub.request(id ? `/api/v1/positions/${id}` : "/api/v1/positions", {
      method: id ? "PUT" : "POST", body: JSON.stringify(body)
    });
    LibraryHub.message(employeeMessage, "Đã lưu chức vụ.");
    setTimeout(() => location.reload(), 500);
  } catch (error) { LibraryHub.message(employeeMessage, error.message, true); }
  finally { LibraryHub.setBusy(button, false); }
});
document.querySelectorAll(".edit-position").forEach(button => button.addEventListener("click", () => {
  const form = document.querySelector("#position-form");
  form.elements.id.value = button.dataset.id;
  form.elements.code.value = button.dataset.code;
  form.elements.name.value = button.dataset.name;
  form.elements.salaryAmount.value = button.dataset.salary;
  form.elements.allowSalaryOverride.checked = button.dataset.override === "true";
  form.scrollIntoView({ behavior: "smooth" });
}));
document.querySelector("#cancel-position")?.addEventListener("click", () => document.querySelector("#position-form").reset());
document.querySelectorAll(".delete-position").forEach(button => button.addEventListener("click", async () => {
  const positionName = button.dataset.name?.trim();
  if (!await LibraryHub.askConfirmation({
    title: "Xóa mềm chức vụ",
    message: positionName
      ? `Chức vụ “${positionName}” sẽ ngừng xuất hiện trong danh mục đang hoạt động. Dữ liệu lịch sử vẫn được giữ.`
      : "Chức vụ sẽ ngừng xuất hiện trong danh mục đang hoạt động. Dữ liệu lịch sử vẫn được giữ.",
    confirmText: "Xóa chức vụ",
    danger: true
  })) return;
  if (!LibraryHub.setBusy(button, true)) return;
  try {
    await LibraryHub.request(`/api/v1/positions/${button.dataset.id}`, { method: "DELETE" });
    button.closest(".catalog-row").remove();
    LibraryHub.message(employeeMessage, "Đã xóa mềm chức vụ.");
  } catch (error) {
    LibraryHub.setBusy(button, false);
    LibraryHub.message(employeeMessage, error.message, true);
  }
}));
document.querySelectorAll(".delete-employee").forEach(button => button.addEventListener("click", async () => {
  const employeeName = button.dataset.name?.trim();
  if (!await LibraryHub.askConfirmation({
    title: "Xóa mềm hồ sơ nhân viên",
    message: employeeName
      ? `Hồ sơ “${employeeName}” sẽ ngừng xuất hiện trong danh sách nhân sự. Tài khoản đăng nhập liên kết không tự động bị xóa.`
      : "Hồ sơ sẽ ngừng xuất hiện trong danh sách nhân sự. Tài khoản đăng nhập liên kết không tự động bị xóa.",
    confirmText: "Xóa hồ sơ",
    danger: true
  })) return;
  if (!LibraryHub.setBusy(button, true)) return;
  try {
    await LibraryHub.request(`/api/v1/employees/${button.dataset.id}`, { method: "DELETE" });
    button.closest("tr").remove();
    LibraryHub.message(employeeMessage, "Đã xóa mềm hồ sơ nhân viên.");
  } catch (error) {
    LibraryHub.setBusy(button, false);
    LibraryHub.message(employeeMessage, error.message, true);
  }
}));
