function addToCart(productId) {
    fetch('/Cart/AddToCart', {
        method: 'POST',
        headers: {
            'Content-Type': 'application/x-www-form-urlencoded',
        },
        body: `productId=${productId}&quantity=1`
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

function removeFromWishlist(productId) {
    if (typeof Swal !== 'undefined') {
        Swal.fire({
            title: 'Are you sure?',
            text: "Do you want to remove this item from your wishlist?",
            icon: 'warning',
            showCancelButton: true,
            confirmButtonColor: '#dc3545',
            cancelButtonColor: '#6c757d',
            confirmButtonText: 'Yes, remove it!'
        }).then((result) => {
            if (result.isConfirmed) {
                fetch('/Wishlist/RemoveFromWishlist', {
                    method: 'POST',
                    headers: {
                        'Content-Type': 'application/x-www-form-urlencoded',
                    },
                    body: `itemId=${productId}`
                })
                .then(response => {
                    if (response.ok) {
                        Swal.fire({
                            title: 'Removed!',
                            text: 'Item has been removed from your wishlist.',
                            icon: 'success',
                            timer: 1500,
                            showConfirmButton: false
                        }).then(() => {
                            location.reload();
                        });
                    } else {
                        Swal.fire('Error!', 'Failed to remove item from wishlist.', 'error');
                    }
                })
                .catch(error => {
                    console.error('Error:', error);
                    Swal.fire('Error!', 'An error occurred while removing the item.', 'error');
                });
            }
        });
    } else {
        // Fallback if Swal is not loaded
        if (confirm('Are you sure you want to remove this item from your wishlist?')) {
            fetch('/Wishlist/RemoveFromWishlist', {
                method: 'POST',
                headers: {
                    'Content-Type': 'application/x-www-form-urlencoded',
                },
                body: `itemId=${productId}`
            })
                .then(response => {
                    if (response.ok) {
                        location.reload();
                    } else {
                        alert('Failed to remove item from wishlist.');
                    }
                })
                .catch(error => {
                    console.error('Error:', error);
                });
        }
    }
}