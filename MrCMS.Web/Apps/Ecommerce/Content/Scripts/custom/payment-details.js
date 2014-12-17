﻿(function ($) {
    function setPaymentMethod(event) {
        var checked = $('input[name="PaymentMethodSystemName"]:checked').val();
        if (checked) {
            var termsAndConditionsAccepted = $('#TermsAndConditionsAccepted:checked').length > 0;
            var data = {
                paymentMethod: checked,
                termsAndConditionsAccepted: termsAndConditionsAccepted
            };
            $.post('/Apps/Ecommerce/PaymentDetails/SetPaymentMethod', data, function (url) {
                if (!url) {
                    $('#payment-confirmation').html('');
                } else {
                    $.get(url, function(response) {
                        $('#payment-confirmation').html(response);
                        $.validator.unobtrusive.parse('form');
                    });
                }
            });
        }
    }
    function submitSetAsShippingForm(event) {
        var form = $(event.target).parents('form');
        form.submit();
    }
    function setAddress(event) {
        var address = $.parseJSON($(this).val());
        if (address != null) {
            for (var key in address) {
                if ($('#billing-address #' + key).length) {
                    $('#billing-address #' + key).val(address[key]);
                }
            }
        }
    }
    function setTermsAndConditions(event) {
        var accept = $('#TermsAndConditionsAccepted:checked').length > 0;
        $.post('/Apps/Ecommerce/terms-and-conditions/set', { accept: accept }, setPaymentMethod);
    }

    $(function () {
        $(document).on('change', '#SameAsShipping', submitSetAsShippingForm);
        $(document).on('change', "select#existing-addresses", setAddress);
        $(document).on('change', 'input[name="PaymentMethodSystemName"]', setPaymentMethod);
        $(document).on('change', 'input[name="TermsAndConditionsAccepted"]', setTermsAndConditions);
        $(document).on('add-discount', setPaymentMethod);
        $(document).on('remove-discount', setPaymentMethod);
        setPaymentMethod();
    });
})(jQuery);
