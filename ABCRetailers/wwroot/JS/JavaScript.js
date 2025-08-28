// Initialize tooltips
document.addEventListener('DOMContentLoaded', function () {
    // Enable Bootstrap tooltips
    const tooltipTriggerList = [].slice.call(document.querySelectorAll('[data-bs-toggle="tooltip"]'));
    const tooltipList = tooltipTriggerList.map(function (tooltipTriggerEl) {
        return new bootstrap.Tooltip(tooltipTriggerEl);
    });

    // Enable Bootstrap popovers
    const popoverTriggerList = [].slice.call(document.querySelectorAll('[data-bs-toggle="popover"]'));
    const popoverList = popoverTriggerList.map(function (popoverTriggerEl) {
        return new bootstrap.Popover(popoverTriggerEl);
    });

    // Real-time price calculation for orders
    const calculateOrderTotal = function () {
        const quantity = parseInt($('#Quantity').val()) || 0;
        const unitPrice = parseFloat($('#UnitPrice').val()) || 0;
        const totalPrice = quantity * unitPrice;
        $('#TotalPrice').val(totalPrice.toFixed(2));
    };

    $('#Quantity, #UnitPrice').on('input', calculateOrderTotal);

    // File upload preview
    $('#ProofOfPayment').on('change', function () {
        const fileName = $(this).val().split('\\').pop();
        $(this).next('.custom-file-label').html(fileName);
    });

    // Auto-dismiss alerts
    setTimeout(function () {
        $('.alert').fadeTo(500, 0).slideUp(500, function () {
            $(this).remove();
        });
    }, 5000);

    // Smooth scrolling for anchor links
    $('a[href^="#"]').on('click', function (event) {
        event.preventDefault();
        $('html, body').animate({
            scrollTop: $($.attr(this, 'href')).offset().top
        }, 500);
    });
});

// AJAX function for real-time product pricing
function getProductPrice(productId) {
    if (productId) {
        $.getJSON('/Order/GetProductPrice', { productId: productId }, function (data) {
            $('#UnitPrice').val(data.price);
            $('#StockAvailable').text(data.stock);
            calculateOrderTotal();
        });
    }
}