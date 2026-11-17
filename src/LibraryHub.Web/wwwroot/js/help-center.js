const helpSearch = document.querySelector("#help-search");
const helpSections = [...document.querySelectorAll("[data-help-section]")];
const helpStatus = document.querySelector("#help-search-status");

helpSearch?.addEventListener("input", () => {
  const query = helpSearch.value.trim().toLocaleLowerCase("vi");
  let visible = 0;
  helpSections.forEach((section) => {
    const matches = !query || section.textContent.toLocaleLowerCase("vi").includes(query);
    section.hidden = !matches;
    if (matches) visible += 1;
  });
  if (helpStatus) {
    helpStatus.textContent = query
      ? `Tìm thấy ${visible} mục hướng dẫn phù hợp.`
      : "";
  }
});
