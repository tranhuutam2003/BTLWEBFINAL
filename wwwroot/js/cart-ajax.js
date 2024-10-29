// Thêm vào đầu file
function showMessage(message, isSuccess = true) {
    // Implement your own toast/alert message here
    alert(message);
}

// Thêm vào giỏ hàng
function addToCart(bookId) {
    $.ajax({
        url: '/Cart/AddToCart',
        type: 'POST',
        data: { bookId: bookId },
        success: function(result) {
            if (result.success) {
                showMessage(result.message, true);
                // Cập nhật số lượng sản phẩm trong giỏ hàng (nếu có hiển thị)
                if ($('#cartCount').length) {
                    $('#cartCount').text(result.cartCount);
                }
            } else {
                showMessage(result.message, false);
                if (result.requireLogin) {
                    window.location.href = '/Account/Login';
                }
            }
        },
        error: function() {
            showMessage('Có lỗi xảy ra. Vui lòng thử lại sau!', false);
        }
    });
}

// Tăng số lượng
function increaseQuantity(bookId) {
    $.ajax({
        url: '/Cart/IncreaseQuantity',
        type: 'POST',
        data: { bookId: bookId },
        success: function(result) {
            if (result.success) {
                // Cập nhật số lượng và tổng tiền của sản phẩm
                $(`#quantity-${bookId}`).text(result.quantity);
                $(`#total-${bookId}`).text(result.total.toLocaleString('vi-VN') + ' đ');
                // Cập nhật tổng tiền giỏ hàng
                $('#cart-total').text(result.cartTotal.toLocaleString('vi-VN') + ' đ');
            } else {
                showMessage(result.message, false);
            }
        },
        error: function() {
            showMessage('Có lỗi xảy ra. Vui lòng thử lại sau!', false);
        }
    });
}

// Giảm số lượng
function decreaseQuantity(bookId) {
    $.ajax({
        url: '/Cart/DecreaseQuantity',
        type: 'POST',
        data: { bookId: bookId },
        success: function(result) {
            if (result.success) {
                if (result.removed) {
                    // Nếu số lượng = 0, xóa sản phẩm khỏi DOM
                    $(`#cart-item-${bookId}`).remove();
                } else {
                    // Cập nhật số lượng và tổng tiền của sản phẩm
                    $(`#quantity-${bookId}`).text(result.quantity);
                    $(`#total-${bookId}`).text(result.total.toLocaleString('vi-VN') + ' đ');
                }
                // Cập nhật tổng tiền giỏ hàng
                $('#cart-total').text(result.cartTotal.toLocaleString('vi-VN') + ' đ');
            } else {
                showMessage(result.message, false);
            }
        },
        error: function() {
            showMessage('Có lỗi xảy ra. Vui lòng thử lại sau!', false);
        }
    });
}

// Xóa khỏi giỏ hàng
function removeFromCart(bookId) {
    if (confirm('Bạn có chắc chắn muốn xóa sản phẩm này khỏi giỏ hàng?')) {
        $.ajax({
            url: '/Cart/RemoveFromCart',
            type: 'POST',
            data: { bookId: bookId },
            success: function(result) {
                if (result.success) {
                    // Xóa sản phẩm khỏi DOM
                    $(`#cart-item-${bookId}`).remove();
                    // Cập nhật tổng tiền giỏ hàng
                    $('#cart-total').text(result.cartTotal.toLocaleString('vi-VN') + ' đ');
                    showMessage(result.message, true);
                } else {
                    showMessage(result.message, false);
                }
            },
            error: function() {
                showMessage('Có lỗi xảy ra. Vui lòng thử lại sau!', false);
            }
        });
    }
}