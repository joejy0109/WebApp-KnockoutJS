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
    comopileForAngular : function(selector) {
        if (angular) {
            var $target = $("[ng-app]");
            angular.element($target).injector().invoke(function ($compile) {
                var scope = angular.element($target).scope();
                $compile(selector)(scope);
                scope.$apply();
            });
        }
    },
    confirmMessage: undefined,
    layoutSet: [
        '<div id="myModal" class="modal fade" role="dialog">' +
            '<div class="modal-dialog">' +
                '<div class="modal-content" />' +
            '</div>' +
        '</div>',
        '<div id="myModal" class="modal fade" role="dialog">' +
            '<div class="modal-dialog normal"> ' +
                '<div class="modal-content pop-logout">' +
                '</div>' +
            '</div>' +
        '</div>',
        '<div id="myModal" class="modal fade" role="dialog">' +
            '<div class="modal-dialog wide"> ' +
                '<div class="modal-content">' +
                '</div>' +
            '</div>' +
        '</div>',
    ],
    index: 0,

    /**
    * @description
    * modal layout 디자인 형식을 선택한다.
    *
    * @param {number} layoutIndex : 디자인 인덱스 번호 [0:default, 1:small wide, 2: wide] (required)
    * 
    * @returns {object} : current modal object.
    */
    init: function (layoutIndex) {
        if ($.isNumeric(layoutIndex))
            this.index = layoutIndex;
        return this;
    },

    /**
    * @description
    * 지정된 {string} templateUrl에 대하여 html를 호출하여 layout popup을 띄운다.    
    *
    * @param {string} templateUrl : Url. (required)
    * @param {JSON} params : POST 전송 parameters. (optional)
    * @param {function} callback : popup 호출 후 callback function. (optional)
    * 
    * @returns {object} : current modal object.
    */
    open: function (templateUrl, params, callback) {
        var $this = this;
        if ($.isFunction(params)) {
            callback = params;
            params = undefined;
        }
        $("body").off("submit", "#modalLayer form");

        if ($("#modalLayer").length == 0) {
            var baseLayout = $('<div id="modalLayer" />');
            baseLayout.append($this.layoutSet[$this.index]);
            baseLayout.find(":last").append("<div id='dyModalContent' />");
            baseLayout.appendTo('body');
            //console.log(tempLayout.html());
           
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
                $this.comopileForAngular($(this));
                if (typeof callback === "function") {
                    callback($(this));
                }
            } else {
                alert(xhr.responseText);
            }
        });
        return this;
    },

    /**
    * @description
    * submit 처리 전에 confirm 메세지 창을 표시한다.   
    *
    * @param {string} confirmMessage : 확이 메세지    
    * 
    * @returns {object} : current modal object.
    */
    confirm: function(confirmMessage) {       
        this.confirmMessage = confirmMessage;
        return this;
    },

    /**
    * @description
    * 호출된 layout popup의 form submit을 수행한다. 
    *
    * @param {boolean} processContinue : submit 이후 자동으로 창을 닫을지 여부. (optional: 기본값 false)
    * @param {JSON} params : POST 전송 parameters. (optional)
    * @param {function} success : submit 후 success callback function. (optional)
    * @param {function} fail : submit 호출 후 failed callback function. (optional)  
    */
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
    },

    /**
   * @description
   * 쓰지 말것      
   */
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

    
};
