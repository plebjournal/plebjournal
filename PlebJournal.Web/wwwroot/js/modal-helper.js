function closeModal() {
  Array.from(document.getElementsByClassName("modal"))
    .forEach((modal) => {
      modal.classList.remove("show");
      modal.remove();
    });
}

function handleKeyPress(event) {
    // Check if the pressed key is the "Escape" key (code 27)
    if (event.keyCode === 27) {
        closeModal();
    }
}

htmx.on("tx-created", closeModal);
htmx.on("tx-updated", closeModal);
htmx.on("tx-deleted", closeModal);
htmx.on("note-created", closeModal);
document.addEventListener("keydown", handleKeyPress);