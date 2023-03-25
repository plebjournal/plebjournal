
// Allow HTMX to swap in HTTP 422 responses 
const configureHtmx = () =>
  document.body.addEventListener('htmx:beforeSwap', function(evt) {
    if (evt.detail.xhr.status === 422) {
      evt.detail.shouldSwap = true;
      evt.detail.isError = false;
    }
  });