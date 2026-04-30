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

window.toggleWishlist = function (btn, productId) {
    if (!productId) return;

    const inWishlist = btn.getAttribute('data-in-wishlist') === 'true';
    const icon = btn.querySelector('i');

    // Helper to show modern toast notification
    const showToast = (message, type = 'success') => {
        if (typeof Swal !== 'undefined') {
            Swal.fire({
                toast: true,
                position: 'bottom-end',
                showConfirmButton: false,
                timer: 3000,
                timerProgressBar: true,
                icon: type,
                title: message,
                didOpen: (toast) => {
                    toast.addEventListener('mouseenter', Swal.stopTimer)
                    toast.addEventListener('mouseleave', Swal.resumeTimer)
                }
            });
        }
    };

    // Optimistic UI update
    if (inWishlist) {
        // Was in wishlist, remove it
        btn.classList.remove('text-danger', 'border-danger');
        btn.classList.add('text-secondary');
        if (icon) {
            icon.classList.remove('fas', 'text-danger');
            icon.classList.add('far');
        }
        btn.setAttribute('data-in-wishlist', 'false');

        fetch('/Wishlist/RemoveFromWishlist', {
            method: 'POST',
            headers: {
                'Content-Type': 'application/x-www-form-urlencoded',
            },
            body: `itemId=${productId}`
        }).then(res => {
            if (!res.ok) throw new Error('Failed');
            return res.json();
        }).then(data => {
            if (data.success) {
                showToast('Removed from wishlist!', 'info');
            } else {
                throw new Error('Failed');
            }
        }).catch(err => {
            // Revert UI
            btn.classList.remove('text-secondary');
            btn.classList.add('text-danger', btn.classList.contains('btn-outline-dark') ? 'border-danger' : 'text-danger');
            if (icon) {
                icon.classList.remove('far');
                icon.classList.add('fas', 'text-danger');
            }
            btn.setAttribute('data-in-wishlist', 'true');
            showToast('Failed to remove from wishlist.', 'error');
        });
    } else {
        // Was not in wishlist, add it
        btn.classList.remove('text-secondary');
        const isOutline = btn.classList.contains('btn-outline-dark');
        btn.classList.add('text-danger');
        if (isOutline) btn.classList.add('border-danger');

        if (icon) {
            icon.classList.remove('far');
            icon.classList.add('fas', 'text-danger');
        }
        btn.setAttribute('data-in-wishlist', 'true');

        fetch('/Wishlist/AddtoWishlist', {
            method: 'POST',
            headers: {
                'Content-Type': 'application/x-www-form-urlencoded',
            },
            body: `productId=${productId}`
        }).then(res => res.json()).then(data => {
            if (!data.success) {
                if (data.message === "Invalid user id.") { // Likely unauthenticated
                    window.location.href = '/Account/Login?returnUrl=' + encodeURIComponent(window.location.pathname);
                } else {
                    throw new Error('Failed');
                }
            } else {
                showToast('Added to wishlist!', 'success');
            }
        }).catch(err => {
            // Revert UI
            btn.classList.remove('text-danger', 'border-danger');
            btn.classList.add('text-secondary');
            if (icon) {
                icon.classList.remove('fas', 'text-danger');
                icon.classList.add('far');
            }
            btn.setAttribute('data-in-wishlist', 'false');
            if (err.message && err.message !== '') {
                console.error("Failed to update wishlist", err);
                showToast('Failed to add to wishlist.', 'error');
            }
        });
    }
};