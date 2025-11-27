window.addToCart = function (productId)
{
    //TODO: automatic refresh of items number and total price not working
    if (event) event.stopPropagation();

    const token = document.querySelector('input[name="__RequestVerificationToken"]')?.value;

    var quantity = document.getElementById('quantity')?.value || '1';

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
        body: 'productId=' + productId + '&quantity=' + quantity
    })
        .then(function (response)
        {
            if (response.redirected)
            {
                window.location.href = response.url;
                return;
            }
            if (response.status === 401)
            {
                window.location.href = '/User/Login';
                return;
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
                    numCount.innerText = data.newCount + " items";

                    if (data.newCount)
                        badge.style.display = 'inline-flex';
                }

                if (total)
                {
                    total.innerText = '$' + data.totalPrice;
                }

                Swal.fire({ title: data.message, icon: 'success' })
            } else
            {
                Swal.fire({ title: data.message, icon: 'failure' })
            }
        })
        .catch(function (error)
        {
            console.error('Error:', error);
        });
};

function updateQuantity(itemId, quantity)
{
    const token = document.getElementById('__RequestVerificationToken').value;

    fetch('/Cart/UpdateQuantity', {
        method: 'POST',
        headers: {
            'Content-Type': 'application/x-www-form-urlencoded',
            'RequestVerificationToken': token
        },
        body: `itemId=${itemId}&quantity=${quantity}`
    })
        .then(response =>
        {
            if (response.ok)
                location.reload();
            else
                console.error("Update failed", response.status);
        })
        .catch(error =>
        {
            console.error('Error:', error);

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

        fetch('/Cart/Remove', {
            method: 'POST',
            headers: {
                'Content-Type': 'application/x-www-form-urlencoded',
                ...(token ? { 'RequestVerificationToken': token } : {})
            },
            body: `itemId=${encodeURIComponent(itemId)}`
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