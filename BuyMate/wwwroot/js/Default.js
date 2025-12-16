// Finds the parent container of this component (usually the li or div in navbar)
document.currentScript.parentElement.addEventListener('click', function (e) {
    const cart = document.getElementById('miniCart');
    // Toggle display
    cart.style.display = cart.style.display === 'none' ? 'block' : 'none';
    e.stopPropagation();
});

// Close when clicking outside
document.addEventListener('click', function (e) {
    const cart = document.getElementById('miniCart');
    if (cart.style.display === 'block' && !cart.contains(e.target) && !e.target.closest('.indicator')) {
        cart.style.display = 'none';
    }
});