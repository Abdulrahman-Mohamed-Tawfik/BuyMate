// Please see documentation at https://learn.microsoft.com/aspnet/core/client-side/bundling-and-minification
// for details on configuring this project to bundle and minify static web assets.

// Write your JavaScript code.

(function () {
    'use strict';

    document.addEventListener('DOMContentLoaded', function () {
        // 1. Alert handling
        const errorAlert = document.getElementById('errorAlert');
        if (errorAlert) {
            setTimeout(() => errorAlert.remove(), 5000);
        }

        const alertBox = document.getElementById('successAlert');
        const progress = document.getElementById('progressBar');

        if (alertBox && progress) {
            let width = 0;
            const interval = setInterval(() => {
                width += 1; // 1% per tick
                progress.style.width = width + "%";
                if (width >= 100) {
                    clearInterval(interval);
                    alertBox.style.transition = "opacity 0.5s";
                    alertBox.style.opacity = 0;
                    setTimeout(() => alertBox.remove(), 500); // remove from DOM
                }
            }, 30); // 30ms per tick = ~3 seconds total
        }

        // 2. Dropdown handling
        const dropdowns = document.querySelectorAll('.dropdown');
        dropdowns.forEach(dropdown => {
            const label = dropdown.querySelector('label');
            if (!label) return;

            label.addEventListener('click', function (e) {
                e.stopPropagation();
                dropdown.classList.toggle('open');
            });
        });

        document.addEventListener('click', function () {
            dropdowns.forEach(dropdown => dropdown.classList.remove('open'));
        });

        // 3. Accessible mobile menu toggle
        const toggleBtn = document.querySelector('.mobile-menu-toggle');
        const mobileMenu = document.getElementById('mobileMenu');
        if (toggleBtn && mobileMenu) {
            function hide() {
                if (!mobileMenu.classList.contains('hidden')) {
                    mobileMenu.classList.add('hidden');
                    toggleBtn.setAttribute('aria-expanded', 'false');
                }
            }

            function toggle() {
                const isHidden = mobileMenu.classList.contains('hidden');
                mobileMenu.classList.toggle('hidden');
                toggleBtn.setAttribute('aria-expanded', isHidden ? 'true' : 'false');
            }

            toggleBtn.addEventListener('click', function (e) {
                e.stopPropagation();
                toggle();
            });

            document.addEventListener('click', function (e) {
                if (!mobileMenu.contains(e.target) && !toggleBtn.contains(e.target)) {
                    hide();
                }
            });

            document.addEventListener('keydown', function (e) {
                if (e.key === 'Escape') hide();
            });
        }
    });
})();