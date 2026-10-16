// Property Create/Edit form: shows only the fields that apply to the chosen listing and type.
// The server enforces the same rules (PropertyService clears RentalUnit on a sale and the specs on land);
// this only keeps the form honest.
(function () {
    'use strict';

    var form = document.getElementById('property-form');
    if (!form) return;

    var listingType = document.getElementById('listing-type');
    var propertyType = document.getElementById('property-type');

    function toggle(selector, visible) {
        form.querySelectorAll(selector).forEach(function (el) {
            el.classList.toggle('d-none', !visible);
        });
    }

    function sync() {
        var isRent = listingType.value === 'Rent';
        toggle('.js-rent-only', isRent);
        form.querySelector('.js-price-suffix').textContent = isRent ? 'EGP / month' : 'EGP';

        if (!isRent) {
            var unit = form.querySelector('[name="RentalUnit"]');
            if (unit) unit.value = '';
        }

        toggle('.js-not-land', propertyType.value !== 'Land');
    }

    listingType.addEventListener('change', sync);
    propertyType.addEventListener('change', sync);
    sync();

    // Disable the submit button only once client validation has passed — otherwise a validation failure
    // would leave it permanently disabled (see CLAUDE.md guardrails).
    $(form).on('submit', function () {
        if (typeof $(form).valid === 'function' && !$(form).valid()) return;
        var button = form.querySelector('button[type="submit"]');
        button.disabled = true;
        button.innerHTML = '<i class="bx bx-loader bx-spin me-1"></i> Saving…';
    });
})();
