// Please see documentation at https://learn.microsoft.com/aspnet/core/client-side/bundling-and-minification
// for details on configuring this project to bundle and minify static web assets.

// Write your JavaScript code.



const errorAlert = document.getElementById('errorAlert');
if (errorAlert)
{
    setTimeout(() => errorAlert.remove(), 5000);
}


alertBox = document.getElementById('successAlert');
progress = document.getElementById('progressBar');

if (alertBox)
{
    let width = 0;
    const interval = setInterval(() =>
    {
        width += 1; // 1% per tick
        progress.style.width = width + "%";
        if (width >= 100)
        {
            clearInterval(interval);
            alertBox.style.transition = "opacity 0.5s";
            alertBox.style.opacity = 0;
            setTimeout(() => alertBox.remove(), 500); // remove from DOM
        }
    }, 30); // 30ms per tick = ~3 seconds total
}


document.addEventListener('DOMContentLoaded', function ()
{
    const dropdowns = document.querySelectorAll('.dropdown');

    dropdowns.forEach(dropdown =>
    {
        const label = dropdown.querySelector('label');

        label.addEventListener('click', function (e)
        {
            e.stopPropagation(); // prevent closing immediately
            dropdown.classList.toggle('open');
        });
    });

    // Close dropdown when clicking outside
    document.addEventListener('click', function ()
    {
        dropdowns.forEach(dropdown => dropdown.classList.remove('open'));
    });
});

// Accessible mobile menu toggle
window.addEventListener('DOMContentLoaded', function ()
{
    var toggleBtn = document.querySelector('.mobile-menu-toggle');
    var mobileMenu = document.getElementById('mobileMenu');
    if (!toggleBtn || !mobileMenu) return;

    function hide()
    {
        if (!mobileMenu.classList.contains('hidden'))
        {
            mobileMenu.classList.add('hidden');
            toggleBtn.setAttribute('aria-expanded', 'false');
        }
    }

    function toggle()
    {
        var isHidden = mobileMenu.classList.contains('hidden');
        mobileMenu.classList.toggle('hidden');
        toggleBtn.setAttribute('aria-expanded', isHidden ? 'true' : 'false');
    }

    toggleBtn.addEventListener('click', function (e)
    {
        e.stopPropagation();
        toggle();
    });

    document.addEventListener('click', function (e)
    {
        if (!mobileMenu.contains(e.target) && !toggleBtn.contains(e.target))
        {
            hide();
        }
    });

    document.addEventListener('keydown', function (e)
    {
        if (e.key === 'Escape') hide();
    });
});