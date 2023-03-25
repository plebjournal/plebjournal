function closeModal() {
  var container = document.getElementById("modal-container")
  var backdrop = document.getElementById("modal-backdrop")
  var modal = document.getElementById("bought-btc-form")

  modal.classList.remove("show")
  backdrop.classList.remove("show")

  backdrop.remove();
  modal.remove();
}

htmx.on("tx-created", (e) => {
  console.log(e)
  closeModal();
});