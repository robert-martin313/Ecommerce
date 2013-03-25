﻿$(document).ready(function () {
    $().dropdown();
    $("[rel='tooltip']").tooltip();

    if ($('#nav-tabs').length) {
        $("#nav-tabs li:eq(" + getTabNumber() + ") a").tab('show');
    }

    var currentTab = $.cookie('selected-tab-' + location.pathname);
    if (location.hash !== '') {
        $('.inner-content a[href="' + location.hash + '"]').tab('show');
    } else if (currentTab) {
        $('.inner-content a[href="' + currentTab + '"]').tab('show');
    } else if ($('.inner-content li a[data-toggle="tab"]').length > 0 && $('.inner-content li.active a[data-toggle="tab"]').length == 0) {
        $('.inner-content a[data-toggle="tab"]').eq(0).click();
    }

    $('.inner-content a[data-toggle="tab"]').on('shown', function (e) {
        $.cookie('selected-tab-' + location.pathname, e.target.hash, { expires: 1, path: "/" });
    });

    $(".datepicker").datepicker();

    try {
        $('#fileupload').fileupload();
    } catch (e) {
    }

    function getTabNumber() {
        var val = $('#controller-name').val();
        if (val !== undefined) {
            switch (val.toLowerCase()) {
                case 'mediacategory':
                    return 1;
                case 'layout':
                    return 2;
                case 'webpage':
                default:
                    return 0;
            }
        }
        return 0;
    }

    $(".web-tree").treeview({
        animated: "medium",
        persist: "cookie",
        cookieId: "navigationtreeWeb",
        toggle: function () {
        }
    });


    if ($.cookie('selected-site')) {
        var cookie = $.cookie('selected-site');
        $('#web #Site').val(cookie);
        $('#layout #Site').val(cookie);
        setVisibleSite(cookie);
    }

    $('#web #Site').change(function () {
        var val = $(this).val();
        setVisibleSite(val);
        $('#layout #Site').val(val);
    });
    $('#layout #Site').change(function () {
        var val = $(this).val();
        setVisibleSite(val);
        $('#web #Site').val(val);
    });

    function setVisibleSite(id) {
        if ($('#web .filetree[data-site-id=' + id + ']').length) {
            $('#web .filetree').hide();
            $('#web .filetree[data-site-id=' + id + ']').show();
        }
        if ($('#layout .filetree[data-site-id=' + id + ']').length) {
            $('#layout .filetree').hide();
            $('#layout .filetree[data-site-id=' + id + ']').show();
        }
        $.cookie('selected-site', id, { expires: 1, path: '/Admin' });
    }

    $(".web-media").treeview({
        animated: "medium",
        persist: "cookie",
        cookieId: "navigationtreeMedia",
        toggle: function () {
        }
    });
    $(".layout-tree").treeview({
        animated: "medium",
        persist: "cookie",
        cookieId: "navigationtreeLayout",
        toggle: function () {
        }
    });

    $(document).on('click', '.date-time-picker', function () {
        var that = $(this);
        if (!that.hasClass('hasDatepicker')) {
            that.datetimepicker({
                dateFormat: 'dd/mm/yy',
                timeFormat: 'hh:mm'
            }).blur().focus();
        }
    });
    $(".date-time-picker");

    // Show menu when an ahref is click
    $(".browser li").not(".browser > li").not(':has(ul)').contextMenu({
        menu: 'edit-menu'
    }, function (action, el, pos) {
        processMenuAction(action, el, pos);
    });

    // Show menu when an ahref is click
    $(".browser li").not(".browser > li").not(':has(ul)').filter('li[data-can-add-child=False]').contextMenu({
        menu: 'edit-no-add-menu'
    }, function (action, el, pos) {
        processMenuAction(action, el, pos);
    });

    // Show menu when an ahref is click
    $(".browser li").not(".browser > li").not(':has(ul)').filter('li[data-sortable=False]').contextMenu({
        menu: 'edit-no-sort-menu'
    }, function (action, el, pos) {
        processMenuAction(action, el, pos);
    });

    // Show menu when an ahref is click
    $(".browser li").not(".browser > li").not(':has(ul)').filter('li[data-can-add-child=False]').filter('li[data-custom-sortable=False]').contextMenu({
        menu: 'edit-no-add-no-sort-menu'
    }, function (action, el, pos) {
        processMenuAction(action, el, pos);
    });

    // Show menu when an ahref is click
    $(".browser li").not(".browser > li").has('ul').contextMenu({
        menu: 'edit-no-delete-menu'
    }, function (action, el, pos) {
        processMenuAction(action, el, pos);
    });

    // Show menu when an ahref is click
    $(".browser li").not(".browser > li").has('ul').filter('li[data-can-add-child=False]').contextMenu({
        menu: 'edit-no-add-no-delete-menu'
    }, function (action, el, pos) {
        processMenuAction(action, el, pos);
    });

    // Show menu when an ahref is click
    $(".browser li").not(".browser > li").has('ul').filter('li[data-sortable=False]').contextMenu({
        menu: 'edit-no-delete-no-sort-menu'
    }, function (action, el, pos) {
        processMenuAction(action, el, pos);
    });

    // Show menu when an ahref is click
    $(".browser li").not(".browser > li").has('ul').filter('li[data-can-add-child=False]').filter('li[data-sortable=False]').contextMenu({
        menu: 'edit-no-add-no-delete-no-sort-menu'
    }, function (action, el, pos) {
        processMenuAction(action, el, pos);
    });

    function processMenuAction(action, el, pos) {
        var element = $(el);
        var id = element.data('id');
        var sortId = element.data('sort-id');
        var controller = element.data('controller');
        var href = element.data('href');
        if (href != null) {
            location.href = href;
        } else {
            switch (action) {
                case "add":
                    location.href = '/Admin/' + controller + '/Add/' + id;
                    break;
                case "edit":
                    location.href = '/Admin/' + controller + '/Edit/' + id;
                    break;
                case "delete":
                    getRemoteModel('/Admin/' + controller + '/Delete/' + id);
                    break;
                case "sort":
                    location.href = '/Admin/' + controller + '/Sort/' + sortId;
                    break;
            }
        }
    }

    function processAdd(element, id, controller) {
        if (controller == 'Webpage' && $(element).data('parent-id') == '') {
        } else {

        }
    }

    // Show menu when an ahref is click
    $(".browser > li").contextMenu({
        menu: 'edit-root-menu'
    }, function (action, el, pos) {
        var element = $(el);
        var controller = element.data('controller');
        switch (action) {
            case "add":
                location.href = '/Admin/' + controller + '/Add';
                break;
            case "sort":
                location.href = '/Admin/' + controller + '/Sort';
                break;
        }
    });

    // Show menu when an ahref is click
    $(".user-list > li").contextMenu({
        menu: 'edit-user-root-menu'
    }, function (action, el, pos) {
        var controller = $(el).data('controller');
        switch (action) {
            case "add":
                location.href = '/Admin/' + controller + '/Add/';
                break;
        }
    });

    // Show menu when an ahref is click
    $(".user-list li").not('.user-list > li').contextMenu({
        menu: 'edit-user-menu'
    }, function (action, el, pos) {
        var id = $(el).data('id');
        var controller = $(el).data('controller');
        switch (action) {
            case "edit":
                location.href = '/Admin/' + controller + '/Edit/' + id;
                break;
        }
    });

    //LiveSearch
    var globalTimeout = null;
    $('#term').keyup(function (e) {
        var tb = this;
        if (globalTimeout != null) clearTimeout(globalTimeout);
        setTimeout(function () { callLiveSearch(tb); }, 250);
    });

    function callLiveSearch(textbox) {
        globalTimeout = null;
        $.getJSON('/Admin/Search/GetSearchResults?term=' + textbox.value, function (data1) { insertCallback(data1); });
    }

    function insertCallback(data1) {
        $("#liveSearchResultsContainer").remove();
        if (data1.length > 0) {
            var results = '<ul id=\'livesearchresults\'>';
            $.each(data1, function (index, value) {
                var doctype = "WebPage";
                if (value.DocumentType.indexOf("MediaCategory") >= 0)
                    doctype = "MediaCategory";
                else if (value.DocumentType.indexOf("Layout") >= 0)
                    doctype = "Layout";

                results = results + "<li>" +
				    "<a href='/Admin/" + doctype + "/Edit/" + value.DocumentId + "'>" + value.DisplayName +
				    "</a><br /><span class='grey font-small'>Updated: " + value.LastUpdated + "</span></li>";
            });
            var position = $("#term").offset();
            $('body').append("<div id='liveSearchResultsContainer' style='top:42px; left:" + position.left + "px'>" + results + " </ul></div>");
            registerHideEvent();
        }
    }

    function registerHideEvent() {
        window.jQuery(document.body).click(function (event) {
            var clicked = window.jQuery(event.target);
            if (!(clicked.is('#livesearchresults') || clicked.parents('#livesearchresults').length || clicked.is('input'))) {
                $("#liveSearchResultsContainer").fadeOut('slow').remove();
            }
        });
    }

    if ($().mediaselector) {
        $('.media-selector').mediaselector();
    }
    $(document).on('click', '[data-toggle="fb-modal"]', function () {
        var clone = $(this).clone();
        clone.attr('data-toggle', '');
        clone.hide();
        clone.fancybox({
            type: 'iframe',
            padding: 0,
            height: 0,
            'onComplete': function () {
                $('#fancybox-frame').load(function () { // wait for frame to load and then gets it's height
                    $(this).contents().find('form').attr('target', '_parent').css('margin', '0');
                    $('#fancybox-content').height($(this).contents()[0].documentElement.scrollHeight);
                    $.fancybox.center();
                });
            }
        }).click().remove();
        return false;
    });

    // Support for AJAX loaded modal window.
    // Focuses on first input textbox after it loads the window.
    $(document).on('click', '[data-toggle="modal"]', function (e) {
        e.preventDefault();
        launchModal($(this));
        return false;
    });

    function split(val) {
        return val.split(/,\s*/);
    }

    function extractLast(term) {
        return split(term).pop();
    }

    $("#TagList").bind("keydown", function (event) {
        if (event.keyCode === $.ui.keyCode.TAB &&
		    $(this).data("autocomplete").menu.active) {
            event.preventDefault();
        }
    }).autocomplete(autoCompleteSettings("/Admin/Tag/Search"));

    $(document).on('click', '#InheritFrontEndRolesFromParent', function() {
        $('#front-end-roles').toggle(!$('#InheritFrontEndRolesFromParent').is(':checked'));
    });

    $(document).on('click', '#InheritAdminRolesFromParent', function() {
        $('#admin-roles').toggle(!$('#InheritAdminRolesFromParent').is(':checked'));
    });

    $("#FrontEndRoles").bind("keydown", function (event) {
        if (event.keyCode === $.ui.keyCode.TAB &&
		    $(this).data("autocomplete").menu.active) {
            event.preventDefault();
        }
    }).autocomplete(autoCompleteSettings("/Admin/Role/Search"));

    $("#AdminRoles").bind("keydown", function (event) {
        if (event.keyCode === $.ui.keyCode.TAB &&
		    $(this).data("autocomplete").menu.active) {
            event.preventDefault();
        }
    }).autocomplete(autoCompleteSettings("/Admin/Role/Search"));
    
    function autoCompleteSettings(url) {
        return {
            source: function(request, response) {
                $.getJSON(url, {
                    term: extractLast(request.term),
                    id: $('#Id').val()
                }, response);
            },
            search: function() {
                // custom minLength
                var term = extractLast(this.value);
                if (term.length < 2) {
                    return false;
                }
            },
            focus: function() {
                // prevent value inserted on focus
                return false;
            },
            select: function(event, ui) {
                var terms = split(this.value);
                // remove the current input
                terms.pop();
                // add the selected item
                terms.push(ui.item.value);
                // add placeholder to get the comma-and-space at the end
                terms.push("");
                this.value = terms.join(", ");
                return false;
            }
        };
    }

    $('[data-action=save]').click(function (e) {
        e.preventDefault();
        var formId = $(this).data('form-id');
        $('#' + formId).submit();
    });

    $('[data-action=post-link]').click(function (e) {
        e.preventDefault();
        var self = $(this);
        var url = self.attr('href') || self.data('link');
        if (url != null) {
            post_to_url(url, {});
        }
    });
    $('#publish-status-change').click(function (e) {
        e.preventDefault();
        var self = $(this);
        var url = self.attr('href') || self.data('link');
        if (url != null) {
            post_to_url_ajax(url, {}, 'POST', function () {
                $('#edit-document').submit();
            });
        }
        return false;
    });

    $(window).resize(function () {
        $('.modal').each(function (index, element) {
            resizeModal($(element));
        });
    });
    $('#move-to').draggable({
        revert: "invalid",
        opacity: 0.5,
        distance: 50,
        helper: "clone"
    });
    $('#web a').droppable({
        activeClass: "ui-state-hover",
        hoverClass: "ui-state-active",
        accept: function (el) {
            if (!el.is('#move-to'))
                return false;
            if ($('#Id').val() == $(this).parent().data('id'))
                return false;
            var any = true;
            var children = $(this).parent('li[data-id!=""]');
            children.each(function (index, element) {
                if ($('#Id').val() == $(element).data('id'))
                    any = false;
            });
            if (!any)
                return any;

            var parents = $(this).parents('li[data-id!=""]');
            parents.each(function (index, element) {
                if ($('#Id').val() == $(element).data('id'))
                    any = false;
            });
            return any;
        },
        drop: function (event, ui) {
            $.post('/Admin/Webpage/SetParent', { id: $('#Id').val(), parentId: $(this).parent().data('id') }, function () {
                window.location.reload();
            });
        },
        tolerance: 'pointer'
    });

    $(document).on('click', 'div[data-paging-type="async"] .pagination a[href]', function () {
        var self = $(this);
        $.get(this.href, function (response) {
            self.parents('div[data-paging-type="async"]').replaceWith(response);
        });
        return false;
    });

    $(document).on('click', 'div[data-paging-type="async"] button[data-action=update]', function () {
        var self = $(this);
        var data = self.parents('div[data-paging-type="async"]').find('input, select, textarea').serialize();
        $.get($(this).data('url'), data, function (response) {
            self.parents('div[data-paging-type="async"]').replaceWith(response);
        });
        return false;
    });

    $(document).on('click', 'a.more-link', function () {
        return false;
    });

    $(document).on('change', '#admin-site-selector', function () {
        location.href = $(this).val();
    });

});

function resizeModal(jqElement) {
    var modal = jqElement.hasClass('modal') ? jqElement : jqElement.parents('.modal');
    var height = modal.outerHeight(),
	    windowHeight = $(window).outerHeight(),
	    width = modal.outerWidth(),
	    windowWidth = $(window).outerWidth();
    var top = (windowHeight - height) / 2,
	    left = (windowWidth - width) / 2;

    modal.css('top', top).css('left', left);
}

function launchModal(element, callback) {
    var elId = element.attr('id');
    var href = element.attr('href') || element.data('link');
    if (href.indexOf('#') == 0) {
        $(href).modal('open').on('hidden', function () {
            $(this).remove();
        });
    } else {
        getRemoteModel(href, elId, callback);
    }
}
function getRemoteModel(href, elId, callback) {
    var div = null;
    $.get(href, function (data) {
        div = $('<div class="modal">' + data + '</div>');
        div.modal({ elementId: elId, callback: callback })
		    .on('shown', function () {
		    })
		    .on('hidden', function () {
		        $(this).remove();
		    });
        $.validator.unobtrusive.parse('.modal form');
    }).success(function () {
        $('input:text:visible:first').focus();
        resizeModal(div);
        div.wrap('<div class="admin-tools" />');
    });
}

//general functions
function setCookie(name, value) {
    $.cookie(name, value, { expires: 7, path: '/' });
}

function getCookie(cookieName) {
    return $.cookie(cookieName);
}
CKEDITOR.config.toolbar = 'Basic';
CKEDITOR.replaceAll('ckedit-enabled');
CKEDITOR.on('instanceReady', function () { $(window).resize(); });