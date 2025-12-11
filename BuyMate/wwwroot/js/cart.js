window.addToCart = function (productId, evt)
{
    // Ensure we stop propagation only for the provided event
    if (evt && evt.stopPropagation) evt.stopPropagation();

    const token = document.getElementById('__RequestVerificationToken')?.value || document.querySelector('input[name="__RequestVerificationToken"]')?.value;

    var quantityEl = document.getElementById('quantity');
    var quantity = 1;
    if (quantityEl)
    {
        quantity = parseInt(quantityEl.value, 10) || 1;
    }

    // Build form body safely
    const params = new URLSearchParams();
    params.append('productId', productId);
    params.append('quantity', quantity);

    const headers = {
        'Content-Type': 'application/x-www-form-urlencoded'
    };

    if (token)
    {
        headers['RequestVerificationToken'] = token;
    }

    fetch('/Cart/AddToCart', {
        method: 'POST',
        headers: headers,
        body: params.toString()
    })
        .then(function (response)
        {
            if (response.redirected)
            {
                window.location.href = response.url;
                return null;
            }
            if (response.status === 401)
            {
                window.location.href = '/User/Login';
                return null;
            }
            return response.json();
        })
        .then(function (data)
        {
            if (!data) return;

            if (data.success)
            {
                var badge = document.getElementById('cartBadge');
                var total = document.getElementById('miniCartTotal');
                var numCount = document.getElementById('num-count');
                if (badge)
                {
                    badge.innerText = data.newCount;
                    if (numCount) numCount.innerText = data.newCount + (data.newCount === 1 ? " item" : " items");

                    if (data.newCount)
                        badge.style.display = 'inline-flex';
                    else
                        badge.style.display = 'none';
                }

                if (total)
                {
                    // Ensure two decimals
                    total.innerText = '$' + parseFloat(data.totalPrice).toFixed(2);
                }

                Swal.fire({ title: data.message, icon: 'success' })
            } else
            {
                Swal.fire({ title: data.message, icon: 'error' })
            }
        })
        .catch(function (error)
        {
            console.error('Error:', error);
        });
};

// Change item quantity by delta (+1 or -1) from the cart page
function changeItemQuantity(itemId, delta)
{
    var input = document.getElementById('qty-' + itemId);
    if (!input) return;
    var current = parseInt(input.value, 10) || 1;
    var next = current + delta;
    if (next < 1) next = 1;
    if (next > 99) next = 99; // arbitrary max guard
    // Update UI immediately
    input.value = next;

    // Call server update and pass previous value so we can revert if server rejects
    updateQuantity(itemId, next, current);
}

function updateQuantity(itemId, quantity, previousValue)
{
    const token = document.getElementById('__RequestVerificationToken')?.value;

    const params = new URLSearchParams();
    params.append('itemId', itemId);
    params.append('quantity', quantity);

    const headers = {
        'Content-Type': 'application/x-www-form-urlencoded',
        'X-Requested-With': 'XMLHttpRequest',
        'Accept': 'application/json'
    };

    if (token)
    {
        headers['RequestVerificationToken'] = token;
    }

    fetch('/Cart/UpdateQuantity', {
        method: 'POST',
        headers: headers,
        body: params.toString()
    })
        .then(response =>
        {
            if (response.redirected)
            {
                // Non-AJAX request: navigate
                window.location.href = response.url;
                return null;
            }
            if (response.status === 401)
            {
                window.location.href = '/User/Login';
                return null;
            }
            return response.json();
        })
        .then(data =>
        {
            if (!data) return;
            if (data.success)
            {
                // On success, reload to reflect totals and other changes
                location.reload();
            }
            else
            {
                // Show Swal with provided message and revert UI to previous value
                Swal.fire({ title: data.message ?? 'Could not update quantity', icon: 'warning' });
                var input = document.getElementById('qty-' + itemId);
                if (input && typeof previousValue !== 'undefined') input.value = previousValue;
            }
        })
        .catch(error =>
        {
            console.error('Error:', error);
            // revert on network error
            var input = document.getElementById('qty-' + itemId);
            if (input && typeof previousValue !== 'undefined') input.value = previousValue;
        });
}

function removeFromCart(itemId)
{
    const token = document.getElementById('__RequestVerificationToken')?.value;

    Swal.fire({
        title: 'Remove this item?',
        text: 'This action cannot be undone.',
        icon: 'warning',
        showCancelButton: true,
        confirmButtonText: 'Remove',
        cancelButtonText: 'Cancel'
    }).then((result) =>
    {
        if (!result.isConfirmed) return;

        const params = new URLSearchParams();
        params.append('itemId', itemId);

        fetch('/Cart/Remove', {
            method: 'POST',
            headers: {
                'Content-Type': 'application/x-www-form-urlencoded',
                ...(token ? { 'RequestVerificationToken': token } : {})
            },
            body: params.toString()
        })
            .then(response =>
            {
                if (response.ok)
                {
                    location.reload();
                } else
                {
                    console.error('Remove failed', response.status);
                }
            })
            .catch(error =>
            {
                console.error('Error:', error);
            });
    });
}