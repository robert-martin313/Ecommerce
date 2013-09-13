﻿$(document).ready(function () {

    $(document).on('click', '#pb-start-task', function () {
        $("#pb-start-task").hide();
        $("#pb").show();
        updateProgressBar();
        $.post(taskUrl, {
            taskId: taskId,
            from: from,
            to: to
        }, function (data) {
            return false;
        });
    });

    function updateProgressBar() {
            $.ajax({
                url: "/Admin/Apps/Amazon/App/ProgressBarStatus",
                type: "GET",
                cache: false,
                data: {
                    taskId: taskId,
                    from:from,
                    to:to
                },
                dataType: "json",
                success: function (data) {
                    refreshMessages();
                    var percentComplete = parseInt(data.PercentComplete);
                    if (data.Status === "Error") {
                        $("#pb-start-task").show();
                        $("#pb").hide();
                    } else {
                        if (percentComplete == null || percentComplete == 100) {
                            $("#pb .progress").removeClass("active");
                            $("#pb .progress .bar").css("width", "100%");
                            $("#pb-start-task").show();
                            $("#pb").hide();
                            setTimeout(refreshMessages, 1000);
                        } else {
                            $("#pb .progress").addClass("active");
                            $("#pb .progress .bar").css("width", percentComplete + "%");
                            setTimeout(updateProgressBar, 1000);
                        }
                    }
                }
            });
        return false;
    }
    
    function refreshMessages() {
        $("#pb-status").show();
        $.get("/Admin/Apps/Amazon/App/ProgressBarMessages", {
            taskId: taskId,
            from: from,
            to: to
        }, function (data) {
            $('#progress-bar-messages').replaceWith(data);
        });
    }

});