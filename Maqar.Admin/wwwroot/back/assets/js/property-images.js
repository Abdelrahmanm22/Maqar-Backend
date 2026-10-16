// Property images on Create/Edit.
//
// 1. Upload: a Dropzone drag/drop UI synced to the hidden <input name="NewImages" multiple> through the
//    DataTransfer API, so files travel with the normal form POST instead of Dropzone's own XHR upload
//    (ported from Travel's attachment-dropzones.js). The limits mirror PropertyImageRules, which is what
//    the server actually enforces.
// 2. Edit only: cover / reorder / remove buttons on the existing-images strip. Each is an AJAX POST, after
//    which the strip is re-fetched from Property/Images, so unsaved form fields are never lost.
(function () {
    'use strict';

    var script = document.currentScript;

    // ─── Upload ────────────────────────────────────────────────────────────────
    if (typeof Dropzone !== 'undefined') {
        Dropzone.autoDiscover = false;

        var zone = document.getElementById('property-dropzone');
        var input = document.getElementById('property-images-input');

        if (zone && input) {
            var maxFiles = parseInt(zone.getAttribute('data-max-files'), 10);
            if (isNaN(maxFiles) || maxFiles < 0) maxFiles = 15;

            var dz = new Dropzone(zone, {
                url: '#',
                autoProcessQueue: false,
                addRemoveLinks: true,
                acceptedFiles: '.jpg,.jpeg,.png,.webp',
                maxFilesize: 5,
                maxFiles: maxFiles,
                thumbnailWidth: 160,
                thumbnailHeight: 120,
                dictRemoveFile: 'Remove',
                dictFileTooBig: 'This image is larger than 5MB.',
                dictInvalidFileType: 'Only JPG, PNG and WebP images are allowed.',
                dictMaxFilesExceeded: 'A listing can have at most 15 images.',
            });

            var sync = function () {
                var dt = new DataTransfer();
                dz.files.forEach(function (file) {
                    if (file.status !== Dropzone.ERROR) dt.items.add(file);
                });
                input.files = dt.files;
            };

            dz.on('addedfile', sync);
            dz.on('removedfile', sync);
            dz.on('error', sync);

            if (maxFiles === 0) {
                zone.classList.add('dz-disabled');
                zone.querySelector('.dz-message h6').textContent = 'This listing already has the maximum number of images.';
                dz.disable();
            }
        }
    }

    // ─── Existing images (Edit) ────────────────────────────────────────────────
    var strip = document.getElementById('property-images');
    if (!strip || !script) return;

    var base = script.getAttribute('data-action-base');
    var propertyId = script.getAttribute('data-property-id');
    var token = $('input[name="__RequestVerificationToken"]').val();

    function refresh() {
        $.get(strip.getAttribute('data-refresh-url')).done(function (html) {
            strip.innerHTML = html;
        });
    }

    function post(button) {
        var data = {
            id: propertyId,
            imageId: button.getAttribute('data-image-id'),
            __RequestVerificationToken: token,
        };
        var direction = button.getAttribute('data-direction');
        if (direction) data.direction = direction;

        $.post(base + button.getAttribute('data-action'), data)
            .done(function (res) {
                if (res.success) {
                    toastr.success(res.message);
                    refresh();
                } else {
                    toastr.error(res.message);
                }
            })
            .fail(function () { toastr.error('Something went wrong. Please try again.'); });
    }

    $(strip).on('click', '.js-image-action', function () {
        var button = this;

        if (button.getAttribute('data-confirm') !== 'true') {
            post(button);
            return;
        }

        Swal.fire({
            title: 'Remove this image?',
            text: 'It is deleted right away — this can\'t be undone.',
            icon: 'warning',
            showCancelButton: true,
            confirmButtonColor: '#f46a6a',
            cancelButtonColor: '#74788d',
            confirmButtonText: 'Yes, remove it'
        }).then(function (result) {
            if (result.isConfirmed) post(button);
        });
    });
})();
