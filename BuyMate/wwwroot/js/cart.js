window.addToCart = function (productId) {
    //TODO: automatic refresh of items number and total price not working
    if (event) event.stopPropagation();

    const token = document.querySelector('input[name="__RequestVerificationToken"]')?.value;

    var quantity = document.getElementById('quantity')?.value || '1';

    const headers = {
        'Content-Type': 'application/x-www-form-urlencoded'
    };

    if (token) {
        headers['RequestVerificationToken'] = token;
    }

    fetch('/Cart/Add', {
        method: 'POST',
        headers: headers,
        body: 'productId=' + productId + '&quantity=' + quantity
    })
        .then(function (response) {
            if (response.redirected) {
                window.location.href = response.url;
                return;
            }
            if (response.status === 401) {
                window.location.href = '/User/Login';
                return;
            }
            return response.json();
        })
        .then(function (data) {
            if (!data) return; 

            if (data.success) {
                var badge = document.getElementById('cartBadge');
                var total = document.getElementById('miniCartTotal');
                var numCount = document.getElementById('num-count');
                if (badge) {
                    badge.innerText = data.newCount;
                    numCount.innerText = data.newCount + " items";

                    if (data.newCount)
                        badge.style.display = 'inline-flex';
                }

                if (total) {
                    total.innerText = '$' + data.totalPrice;
                }
                // TODO: Use a nicer toast notification here instead of alert
                alert(data.message);
            } else {
                alert("Error: " + data.message);
            }
        })
        .catch(function (error) {
            console.error('Error:', error);
        });
};