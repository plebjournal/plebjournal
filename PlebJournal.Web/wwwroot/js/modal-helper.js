function closeModal() {
  Array.from(document.getElementsByClassName("modal"))
    .forEach((modal) => {
      modal.classList.remove("show");
      modal.remove();
    });
}

htmx.on("tx-created", closeModal);
htmx.on("tx-deleted", closeModal);