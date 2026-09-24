// CoreDeck Community - Client Scripts
document.addEventListener('DOMContentLoaded', function () {
    // Delegated Wishlist Toggle Handling
    document.addEventListener('click', async function (e) {
        const btn = e.target.closest('.wishlist-toggle-btn');
        if (!btn) return;

        e.preventDefault();
        const productId = btn.getAttribute('data-product-id');
        if (!productId) return;

        try {
            const response = await fetch('/Wishlist/ToggleWishlist', {
                method: 'POST',
                headers: {
                    'Content-Type': 'application/x-www-form-urlencoded',
                    'X-Requested-With': 'XMLHttpRequest'
                },
                body: new URLSearchParams({ productId: productId })
            });

            if (response.redirected) {
                window.location.href = response.url;
                return;
            }

            const data = await response.json();
            if (data.success) {
                // Update all buttons matching this product id on the page
                const allMatchingBtns = document.querySelectorAll(`.wishlist-toggle-btn[data-product-id="${productId}"]`);
                allMatchingBtns.forEach(b => {
                    const icon = b.querySelector('i');
                    if (icon) {
                        if (data.isWishlisted) {
                            icon.classList.remove('far');
                            icon.classList.add('fas', 'text-danger');
                            b.classList.add('active', 'border-danger', 'text-danger');
                        } else {
                            icon.classList.remove('fas', 'text-danger');
                            icon.classList.add('far');
                            b.classList.remove('active', 'border-danger', 'text-danger');
                        }
                    }
                    const label = b.querySelector('.wishlist-label');
                    if (label) {
                        label.textContent = data.isWishlisted ? 'Saved to Wishlist' : 'Save to Wishlist';
                    }
                    b.setAttribute('title', data.isWishlisted ? 'Remove from Wishlist' : 'Save to Wishlist');
                });
            } else {
                if (data.message && data.message.includes('Authentication')) {
                    window.location.href = '/Account/Login?returnUrl=' + encodeURIComponent(window.location.pathname);
                } else if (data.message) {
                    alert(data.message);
                }
            }
        } catch (err) {
            // Fallback: submit enclosing form if present
            const form = btn.closest('form');
            if (form) {
                form.submit();
            }
        }
    });
});
