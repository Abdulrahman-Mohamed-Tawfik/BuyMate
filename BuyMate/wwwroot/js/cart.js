function showAlert(options) {
    Swal.fire(options);
}

function showConfirm(options, onConfirm) {
    Swal.fire(options).then((result) => {
        if (result.isConfirmed) onConfirm();
    });
}

async function fetchCartApi(url, params) {
    const token = document.getElementById('__RequestVerificationToken')?.value || document.querySelector('input[name="__RequestVerificationToken"]')?.value;
    const headers = {
        'Content-Type': 'application/x-www-form-urlencoded',
        'X-Requested-With': 'XMLHttpRequest',
        'Accept': 'application/json'
    };

    if (token) headers['RequestVerificationToken'] = token;

    const response = await fetch(url, {
        method: 'POST',
        headers: headers,
        body: params.toString()
    });

    if (response.redirected) {
        window.location.href = response.url;
        return null;
    }
    if (response.status === 401) {
        window.location.href = '/User/Login';
        return null;
    }

    if (!response.ok) {
        throw new Error('Network response was not ok');
    }

    return response.json();
}

window.addToCart = function (productId, evt) {
    // Ensure we stop propagation only for the provided event
    if (evt && evt.stopPropagation) evt.stopPropagation();

    var quantityEl = document.getElementById('quantity');
    var quantity = quantityEl ? (parseInt(quantityEl.value, 10) || 1) : 1;

    const params = new URLSearchParams();
    params.append('productId', productId);
    params.append('quantity', quantity);

    fetchCartApi('/Cart/AddToCart', params)
        .then(data => {
            if (!data) return;

            if (data.success) {
                var badge = document.getElementById('cartBadge');
                var total = document.getElementById('miniCartTotal');
                var numCount = document.getElementById('num-count');

                if (badge) {
                    badge.innerText = data.newCount;
                    badge.style.display = data.newCount ? 'inline-flex' : 'none';
                }
                if (numCount) numCount.innerText = data.newCount + (data.newCount === 1 ? " item" : " items");
                if (total) total.innerText = '$' + parseFloat(data.totalPrice).toFixed(2);

                showAlert({ title: data.message, icon: 'success' });
            } else {
                showAlert({ title: data.message, icon: 'error' });
            }
        })
        .catch(error => console.error('Error:', error));
};

// Change item quantity by delta (+1 or -1) from the cart page
function changeItemQuantity(itemId, delta) {
    var input = document.getElementById('qty-' + itemId);
    if (!input) return;

    var stock = parseInt(input.getAttribute('data-stock'), 10) || 99;
    var current = parseInt(input.value, 10) || 1;
    var next = current + delta;

    if (next < 1) next = 1;
    if (next > stock) {
        showAlert({ title: `Only ${stock} units available in stock.`, icon: 'warning' });
        next = stock;
    }

    if (next === current) return;

    // Update UI immediately
    input.value = next;

    // Call server update and pass previous value so we can revert if server rejects
    updateQuantity(itemId, next, current);
}

function updateQuantity(itemId, quantity, previousValue) {
    const params = new URLSearchParams();
    params.append('itemId', itemId);
    params.append('quantity', quantity);

    fetchCartApi('/Cart/UpdateQuantity', params)
        .then(data => {
            if (!data) return;
            if (data.success) {
                // Update DOM with new totals
                var itemTotal = document.getElementById('item-total-' + itemId);
                if (itemTotal) itemTotal.innerText = '$' + parseFloat(data.itemTotal).toFixed(2);

                var cartSubtotal = document.getElementById('cart-subtotal');
                if (cartSubtotal) cartSubtotal.innerText = '$' + parseFloat(data.subtotal).toFixed(2);

                var cartTotal = document.getElementById('cart-total');
                if (cartTotal) cartTotal.innerText = '$' + parseFloat(data.total).toFixed(2);

                // Update Mini Cart elements
                var badge = document.getElementById('cartBadge');
                var numCount = document.getElementById('num-count');
                var miniCartTotal = document.getElementById('miniCartTotal');

                if (badge) {
                    badge.innerText = data.newCount;
                    badge.style.display = data.newCount > 0 ? 'inline-block' : 'none';
                }
                if (numCount) numCount.innerText = data.newCount + (data.newCount === 1 ? " item" : " items");
                if (miniCartTotal) miniCartTotal.innerText = '$' + parseFloat(data.total).toFixed(2);

                // Re-enable/disable buttons
                var input = document.getElementById('qty-' + itemId);
                if (input) {
                    var val = parseInt(input.value, 10);
                    var stock = parseInt(input.getAttribute('data-stock'), 10) || 99;
                    var btns = input.parentElement.querySelectorAll('button');
                    if (btns.length >= 2) {
                        btns[0].disabled = val <= 1;
                        btns[1].disabled = val >= stock;
                    }
                }
            } else {
                showAlert({ title: data.message ?? 'Could not update quantity', icon: 'warning' });
                var input = document.getElementById('qty-' + itemId);
                if (input && typeof previousValue !== 'undefined') input.value = previousValue;
            }
        })
        .catch(error => {
            console.error('Error:', error);
            var input = document.getElementById('qty-' + itemId);
            if (input && typeof previousValue !== 'undefined') input.value = previousValue;
        });
}

function removeFromCart(itemId) {
    showConfirm({
        title: 'Remove this item?',
        text: 'This action cannot be undone.',
        icon: 'warning',
        showCancelButton: true,
        confirmButtonText: 'Remove',
        cancelButtonText: 'Cancel'
    }, () => {
        const params = new URLSearchParams();
        params.append('itemId', itemId);

        fetchCartApi('/Cart/Remove', params)
            .then(data => {
                if (!data) return;
                if (data.success || data.success === undefined) { 
                    // some responses like Remove return {success: true, ...}
                    location.reload();
                } else {
                    console.error('Remove failed:', data.message);
                    showAlert({ title: data.message ?? 'Remove failed', icon: 'error' });
                }
            })
            .catch(error => console.error('Error:', error));
    });
}