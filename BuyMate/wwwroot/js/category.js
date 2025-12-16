
const MAX_FILE_SIZE = 2 * 1024 * 1024; // 2MB
const ALLOWED = ['.jpg', '.jpeg', '.png', '.gif', '.webp'];

const fileInput = document.getElementById('imageFile');
const selectedFileSpan = document.getElementById('selectedFile');
const previewContainer = document.getElementById('imagePreview');
const createform = document.getElementById('createCategoryForm');

const clearBtn = document.getElementById('clearImageBtn');
const editForm = document.getElementById('editCategoryForm');
const deleteForm = document.getElementById('deleteCategoryForm');

//For Index View delete buttons
document.addEventListener('DOMContentLoaded', function () {
    document.querySelectorAll('form.swal-delete').forEach(function (form) {
        form.addEventListener('submit', function (e) {
            e.preventDefault();
            Swal.fire({
                title: form.dataset.confirm || 'Are you sure?',
                icon: 'warning',
                showCancelButton: true,
                confirmButtonText: 'Delete'
            }).then(function (result) {
                if (result.isConfirmed) {
                    form.submit();
                }
            });
        });
    });
});

createform.addEventListener('submit', function (e) {
    e.preventDefault();
    const name = document.querySelector("input[name='Name']").value.trim();
    const files = fileInput.files;

    if (!name) { Swal.fire('Error', 'Name is required', 'error'); return; }
    if (!files || files.length === 0) { Swal.fire('Error', 'Please select an image', 'error'); return; }

    Swal.fire({ title: 'Create Category?', icon: 'question', showCancelButton: true }).then(res => {
        if (!res.isConfirmed) return;

        const formData = new FormData(createform);
        fetch(createform.action, { method: 'POST', body: formData })
            .then(r => {
                if (r.redirected) { window.location = r.url; return; }
                if (!r.ok) return r.text().then(t => { throw new Error(t); });
                return r.text().then(html => { document.open(); document.write(html); document.close(); });
            })
            .catch(err => Swal.fire('Error', err.message, 'error'));
    });
});




fileInput.addEventListener('change', function () {
    previewContainer.innerHTML = '';
    if (this.files.length === 0) {
        selectedFileSpan.textContent = 'No file chosen';
        if (clearBtn != null)
            clearBtn.classList.add('hidden');
        return;
    }
    const file = this.files[0];
    const ext = '.' + file.name.split('.').pop().toLowerCase();
    const errors = [];
    if (!ALLOWED.includes(ext)) errors.push('Unsupported extension');
    if (file.size > MAX_FILE_SIZE) errors.push('File exceeds 2MB');
    if (errors.length) {
        this.value = '';
        selectedFileSpan.textContent = 'No valid file chosen';
        clearBtn.classList.add('hidden');
        Swal.fire({ title: 'Invalid file', html: errors.join('<br/>'), icon: 'warning' });
        return;
    }
    selectedFileSpan.textContent = file.name;
    if (clearBtn != null)
        clearBtn.classList.remove('hidden');
    const reader = new FileReader();
    reader.onload = e => {
        const div = document.createElement('div');
        div.className = 'relative';
        div.innerHTML = `<img src="${e.target.result}" class="w-full h-32 object-cover rounded-lg border" alt="Preview" />`;
        previewContainer.appendChild(div);
    };
    reader.readAsDataURL(file);
});


clearBtn.addEventListener('click', () => {
    fileInput.value = '';
    selectedFileSpan.textContent = 'No file chosen';
    previewContainer.innerHTML = '';
    clearBtn.classList.add('hidden');
});



editForm.addEventListener('submit', function (e) {
    e.preventDefault();
    const name = document.querySelector("input[name='Name']").value.trim();
    if (!name) { Swal.fire('Error', 'Name is required', 'error'); return; }
    Swal.fire({ title: 'Save changes?', icon: 'question', showCancelButton: true }).then(res => {
        if (!res.isConfirmed) return;
        const formData = new FormData(editForm);
        fetch(editForm.action, { method: 'POST', body: formData })
            .then(r => {
                if (r.redirected) { window.location = r.url; return; }
                if (!r.ok) return r.text().then(t => { throw new Error(t); });
                return r.text().then(html => { document.open(); document.write(html); document.close(); });
            })
            .catch(err => Swal.fire('Error', err.message, 'error'));
    });
});

deleteForm.addEventListener('submit', function (e) {
    e.preventDefault();
    Swal.fire({ title: 'Delete this category?', text: 'This action cannot be undone.', icon: 'warning', showCancelButton: true, confirmButtonColor: '#d33', confirmButtonText: 'Delete' })
        .then(res => {
            if (!res.isConfirmed) return;
            const formData = new FormData(deleteForm);
            fetch(deleteForm.action, { method: 'POST', body: formData })
                .then(r => {
                    if (r.redirected) { window.location = r.url; return; }
                    if (!r.ok) return r.text().then(t => { throw new Error(t); });
                    return r.text().then(html => { document.open(); document.write(html); document.close(); });
                })
                .catch(err => Swal.fire('Error', err.message, 'error'));
        });
});



