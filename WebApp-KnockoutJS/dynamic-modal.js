var dynamicModal = modal = {
    getFormData: function (frm, params) {
        var data = {};
        $.map(frm.serializeArray(), function (v, i) {
            data[v['name']] = v['value'];
        });      

        if (params !== undefined) {
            $.map(params, function (val, key) {
                data[key] = val;                
            });
        }
        return JSON.stringify(data);
    },   
    modalObj: null,
    confirmMessage: undefined,
    defaultLayout: '<div id="myModal" class="modal fade" role="dialog">' + 
                        '<div class="modal-dialog">' +
                            '<div class="modal-content" />'+
                        '</div>' +
                    '</div>',
    layout: undefined,
    init: function (outsideSelector) {
        this.layout = outsideSelector;
        return this;
    },
    open: function (templateUrl, params, callback) {
        var $this = this;
        if ($.isFunction(params)) {
            callback = params;
            params = undefined;
        }
        $("body").off("submit", "#modalLayer form");

        if ($("#modalLayer").length == 0) {
            var tempLayout = $('<div id="modalLayer" />');
            if ($this.layout !== undefined) {               
                tempLayout.append($this.layout);
            }
            else {
                tempLayout.append($this.defaultLayout);
            }
            tempLayout.find(":last").append("<div id='dyModalContent' />");            
            $('body').append(tempLayout);

            //console.log(tempLayout.html());

            //$("body").append('<div id="modalLayer">'+
            //                    '<div id="myModal" class="modal fade" role="dialog">' + 
            //                        '<div class="modal-dialog">' +
            //                            '<div id="dyModalContent" class="modal-content"></div>' +
            //                        '</div>' +
            //                     '</div>' +
            //                  '</div>');

            $("#modalLayer").on("hide.bs.modal", function (e) {
                $(e.target).removeData("bs.modal");
                //$(e.target).remove();
            });
            $this.modalObj = $("#modalLayer");
        }
        $("#dyModalContent").load(templateUrl, params, function (res, status, xhr) {
            if (xhr.status == 200) {
                $.validator.unobtrusive.parse("#modalLayer form");
                $.validator.methods.number = function (e) { return true; };
                $("#modalLayer div:eq(0)").modal('show');
                if (typeof callback === "function") {
                    callback($(this));
                }
            } else {
                alert(xhr.responseText);
            }
        });
        return this;
    },
    confirm: function(confirmMessage) {       
        this.confirmMessage = confirmMessage;
        return this;
    },


    // test용 쓰지 말것
    submit2: function(confirmMsg) {
        return function (processContinue, params, success, fail) {
            if (typeof processContinue !== "boolean") {
                fail = success;
                success = params;
                params = processContinue;
                processContinue = false;
            }
            if ($.isFunction(params)) {
                fail = success;
                success = params;
                params = undefined;
            }
            var $this = this;

            $("body").on("submit", "#modalLayer form", function (e) {
                if (!processContinue)
                    e.preventDefault();
                if (confirmMsg != undefined) {
                    if (!confirm(confirmMsg)) {
                        return;
                    }
                }
                var frm = $(this);
                jQuery.ajax({
                    url: frm.attr('action'),
                    type: 'POST',
                    dataType: 'json',
                    data: $this.getFormData(frm, params),
                    processData: false,
                    contentType: 'application/json; charset=UTF-8',
                    headers: {
                        "__RequestVerificationToken": frm.find("input[name='__RequestVerificationToken']").val()
                    },
                }).done(function (res, status, xhr) {
                    if ($.isFunction(success))
                        success(res, $this.modalObj);
                    $("#modalLayer div:eq(0)").modal('hide');
                }).error(function (xhr, textStatus, errThrown) {
                    //alert(textStatus);
                }).fail(function (xhr, textStatus, errThrown) {
                    if (textStatus == "parsererror") {
                        $("#dyModalContent").html(xhr.responseText);
                    }
                });
            });
        };
    },

    submit: function (processContinue, params, success, fail) {

        if (typeof processContinue !== "boolean") {
            fail = success;
            success = params;
            params = processContinue;
            processContinue = false;
        }
        if ($.isFunction(params)) {
            fail = success;
            success = params;
            params = undefined;
        }
        var $this = this;
                
        $("body").off("submit", "#modalLayer form").on("submit", "#modalLayer form", function (e) {
            if (!processContinue)
                e.preventDefault();
            if ($this.confirmMessage != undefined) {
                if (!confirm($this.confirmMessage)) {
                    $this.confirmMessage = undefined;
                    return;
                }
            }
            var frm = $(this);
            jQuery.ajax({
                url: frm.attr('action'),
                type: 'POST',
                dataType: 'json',
                data: $this.getFormData(frm, params),
                processData: false,
                contentType: 'application/json; charset=UTF-8',
                headers: {
                    "__RequestVerificationToken": frm.find("input[name='__RequestVerificationToken']").val()
                },
            }).done(function (res, status, xhr) {
                if ($.isFunction(success))
                    success(res, $this.modalObj);                
                $("#modalLayer div:eq(0)").modal('hide');
            }).error(function (xhr, textStatus, errThrown) {
                //alert(textStatus);
            }).fail(function (xhr, textStatus, errThrown) {
                if (textStatus == "parsererror") {
                    $("#dyModalContent").html(xhr.responseText);
                }                
            });
        });
    }
};
