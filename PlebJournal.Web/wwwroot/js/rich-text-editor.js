async function initRte() {
    const editor = await ClassicEditor.create(document.querySelector('#note-body'),{
        height: '200px'
    });
    editor.model.document.on('change', function() {
        const data = editor.getData();
        editor.updateSourceElement(data);
    });
}

initRte();