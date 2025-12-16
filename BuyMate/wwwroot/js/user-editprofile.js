(function () {
	var input = document.getElementById('avatarFileInput');
	var preview = document.getElementById('profilePreview');
	var nameDisp = document.getElementById('fileNameDisplay');
	if (!input) return;

	input.addEventListener('change', function () {
		var file = input.files && input.files[0];
		if (!file) {
			nameDisp.textContent = '';
			return;
		}

		// Show filename under image
		nameDisp.textContent = file.name;

		// Preview if image
		if (file.type && file.type.indexOf('image/') === 0) {
			var reader = new FileReader();
			reader.onload = function (ev) {
				preview.src = ev.target.result;
			};
			reader.readAsDataURL(file);
		}
	});
})();