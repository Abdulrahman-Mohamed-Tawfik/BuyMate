document.querySelectorAll('.delete-form').forEach(f => { f.addEventListener('submit', e => { e.preventDefault(); Swal.fire({ title: 'Are you sure?', text: 'Do you want to delete this product?!', icon: 'warning', showCancelButton: true, confirmButtonText: 'Yes, delete it', cancelButtonText: 'Cancel' }).then(res => { if (res.isConfirmed) f.submit(); }); }); });

// Filter sidebar toggle (for mobile)
document.addEventListener('DOMContentLoaded', function () {
    var toggle = document.getElementById('filterToggle');
    var sidebar = document.getElementById('filterSidebar');
    if (toggle && sidebar) {
        toggle.addEventListener('click', function (e) {
            e.preventDefault();
            sidebar.classList.toggle('hidden');
        });
    }
});