(function () {
    var toggle = document.getElementById('showPasswordRegister');
    if (!toggle) return;
    toggle.addEventListener('change', function () {
        var show = this.checked;
        ['Password', 'ConfirmPassword'].forEach(function (id) {
            var el = document.getElementById(id);
            if (el) el.type = show ? 'text' : 'password';
        });
    });
})();