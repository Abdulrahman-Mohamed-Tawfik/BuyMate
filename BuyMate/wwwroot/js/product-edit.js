const MAX_FILE_SIZE = 4 * 1024 * 1024; //4MB
const ALLOWED = ['.jpg', '.jpeg', '.png', '.gif', '.webp'];

document.getElementById('files').addEventListener('change', function () {
    const container = document.getElementById('imagePreview');
    const selected = document.getElementById('selectedFiles');
    container.innerHTML = '';

    // Use DataTransfer to keep only valid files
    const dt = new DataTransfer();
    const errors = [];

    if (this.files.length === 0) {
        selected.textContent = 'No files chosen';
        return;
    }

    for (const file of this.files) {
        const ext = '.' + file.name.split('.').pop().toLowerCase();
        if (!ALLOWED.includes(ext)) {
            errors.push(`${file.name}: unsupported extension`);
            continue; // skip invalid
        }
        if (file.size > MAX_FILE_SIZE) {
            errors.push(`${file.name}: exceeds4MB`);
            continue; // skip large files
        }

        // valid -> add to DataTransfer and preview
        dt.items.add(file);

        const reader = new FileReader();
        reader.onload = e => {
            const div = document.createElement('div');
            div.className = 'relative';
            div.innerHTML = `<img src="${e.target.result}" class="w-full h-32 object-cover rounded-lg border" />`;
            container.appendChild(div);
        };
        reader.readAsDataURL(file);
    }

    // replace file input selection with only valid files
    this.files = dt.files;
    if (dt.files.length === 0) selected.textContent = 'No valid files chosen';
    else selected.textContent = `${dt.files.length} file(s) selected`;

    if (errors.length) {
        Swal.fire({
            title: 'Some files were skipped',
            html: errors.join('<br/>'),
            icon: 'warning'
        });
    }
});
function removeImage(btn) {
    btn.parentElement.remove();
    updateImageOrder();
}
function validateEditForm() {
    const name = document.querySelector("input[name='Name']").value.trim();
    const brand = document.querySelector("input[name='Brand']").value.trim();
    const desc = document.querySelector("textarea[name='Description']").value.trim();
    const price = Number(document.querySelector("input[name='Price']").value);
    const stock = Number(document.querySelector("input[name='StockQuantity']").value);
    const categories = Array.from(document.getElementById('categorySelect').selectedOptions).map(o => o.value);
    const specifications = document.getElementById('specificationContainer').children;


    if (name === '') { Swal.fire('Error', 'Name is required', 'error'); return false; }
    if (brand === '') { Swal.fire('Error', 'Brand is required', 'error'); return false; }
    if (desc === '') { Swal.fire('Error', 'Description is required', 'error'); return false; }
    if (!price || price <= 0) { Swal.fire('Error', 'Price must be greater than0', 'error'); return false; }
    if (isNaN(stock) || stock < 0) { Swal.fire('Error', 'Stock must be0 or more', 'error'); return false; }
    if (categories.length == 0) { Swal.fire('Error', 'Select at least one category', 'error'); return false; }

    if (specifications.length !== 0) {

        for (let i = 0; i < specifications.length; i++) {
            const keyInput = specifications[i].querySelector(`input[name='Specifications[${i}].Key']`);
            const valueInput = specifications[i].querySelector(`input[name='Specifications[${i}].Value']`);
            const key = keyInput.value.trim();
            const value = valueInput.value.trim();
            if (!key || !value) {
                Swal.fire('Error', `Specification #${i + 1} requires both Key and Value`, `error`);
                return false;
            }
        }

        const keyInputss = Array.from(specifications).map((spec, index) => spec.querySelector(`input[name='Specifications[${index}].Key']`).value.trim().toLowerCase());

        const keysSet = new Set(keyInputss);
        if (keysSet.size !== keyInputss.length) {
            Swal.fire('Error', 'Duplicate keys found in specifications', 'error');
            return false;
        }

    }


    return true;
}

document.getElementById("addSpecBtn").addEventListener('click', function () {
    const container = document.getElementById('specificationContainer');
    const idx = container.children.length;
    const row = document.createElement('div');
    row.className = 'flex gap-3';
    row.innerHTML = `
            <input name="Specifications[${idx}].Key" class="input w-1/2" placeholder="Key (e.g., Color)" />
            <input name="Specifications[${idx}].Value" class="input w-1/2" placeholder="Value (e.g., Red)" />
            <button type="button" class="btn btn-danger removeSpecBtn">X</button>
            `;
    container.appendChild(row);
    row.querySelector('.removeSpecBtn').addEventListener('click', () => row.remove());
});

document.getElementById('editForm').addEventListener('submit', function (e) {
    e.preventDefault();
    if (!validateEditForm()) return;
    Swal.fire({ title: 'Save Changes?', icon: 'question', showCancelButton: true, confirmButtonText: 'Save' }).then(result => {
        if (!result.isConfirmed) return;
        const form = document.getElementById('editForm');

        // Prepare categories: no hidden inputs needed — FormData(form) includes selects

        // if there are new files selected, use FormData(form) to include files and all fields
        const files = document.getElementById('files').files;
        if (files && files.length) {
            const fd = new FormData(form);
            // Fetch with FormData (includes files automatically)
            fetch(form.action, { method: 'POST', body: fd })
                .then(r => { if (r.redirected) window.location = r.url; else if (!r.ok) return r.text().then(t => { throw new Error(t); }); else return r.text().then(h => { document.open(); document.write(h); document.close(); }); })
                .catch(err => Swal.fire('Error', err.message, 'error'));
        } else {
            form.submit();
        }
    });
});