(function () {
    var toggle = document.getElementById('showPasswordLogin');
    if (!toggle) return;
    toggle.addEventListener('change', function () {
        var el = document.getElementById('LoginPassword');
        if (el) el.type = this.checked ? 'text' : 'password';
    });
})();