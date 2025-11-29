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