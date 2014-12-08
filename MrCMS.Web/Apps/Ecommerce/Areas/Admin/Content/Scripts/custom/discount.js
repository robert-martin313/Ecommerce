﻿(function($) {
    var selector = '#RequiresCode',
        limitationsSelector = '[data-discount-limitations]',
        applicationsSelector = '[data-discount-applications]';

    function reloadLimitations(event) {
        var limitations = $(limitationsSelector);
        if (limitations.length == 0)
            return;
        limitations.each(function(index, element) {
            var list = $(element);
            var url = list.data('discount-limitations');
            $.get(url, function(response) {
                list.replaceWith(response);
            });
        });
    }
    function reloadApplications(event) {
        var applications = $(applicationsSelector);
        if (applications.length == 0)
            return;
        applications.each(function(index, element) {
            var list = $(element);
            var url = list.data('discount-applications');
            $.get(url, function(response) {
                list.replaceWith(response);
            });
        });
    }

    function showHideCodeField(event) {
        event.preventDefault();
        var required = $(event.target);
        $('[data-code-field]').toggle(required.is(':checked'));
    }

    $(function() {
        $(document).on('change', selector, showHideCodeField);
        $(document).on('reload-limitations', reloadLimitations);
        $(document).on('reload-applications', reloadApplications);
        $(selector).change();
    });
})(jQuery);