// product-ajax.js
function loadProducts(url) {
    $.ajax({
        url: url,
        type: 'GET',
        success: function (result) {
            $('#product-container').html(result);
            initializeSetBg(); // Reinitialize set-bg after loading new content
        },
        error: function (err) {
            console.error('Error loading products:', err);
        }
    });
}

function initializeSetBg() {
    $('.set-bg').each(function () {
        var bg = $(this).data('setbg');
        $(this).css('background-image', 'url(' + bg + ')');
    });
}

// Handle pagination clicks
$(document).on('click', '.product__pagination a', function (e) {
    e.preventDefault();
    var url = $(this).attr('href');
    loadProducts(url);
    // Update URL without page refresh
    history.pushState(null, '', url);
});

// Handle category selection
$(document).on('click', '.category-link', function (e) {
    e.preventDefault();
    var url = $(this).attr('href');
    loadProducts(url);
    history.pushState(null, '', url);
});

// Handle search form submission
$('#search-form').on('submit', function (e) {
    e.preventDefault();
    var query = $('#search-input').val();
    var url = '/Product/Search?query=' + encodeURIComponent(query);
    loadProducts(url);
    history.pushState(null, '', url);
});

// Handle browser back/forward buttons
$(window).on('popstate', function () {
    loadProducts(window.location.href);
});