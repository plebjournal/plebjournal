htmx.on("showMessage", (e) => {
  const toastElement = document.getElementById("toast")
  const toastBody = document.getElementById("toast-body-header")

  const toast = new bootstrap.Toast(toastElement, { delay: 4000 })
  
  toastBody.innerText = e.detail.value
  toast.show()
})