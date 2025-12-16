function addToCart(productId) {
    if (!productId) return;
    fetch('/Cart/AddToCart', {
        method: 'POST',
        headers: {
            'Content-Type': 'application/x-www-form-urlencoded',
        },
        body: `productId=${encodeURIComponent(productId)}&quantity=1`
    })
        .then(response => {
            if (response.ok) {
                alert('Product added to cart!');
            }
        })
        .catch(error => {
            console.error('Error:', error);
        });
}