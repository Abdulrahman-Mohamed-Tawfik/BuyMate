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