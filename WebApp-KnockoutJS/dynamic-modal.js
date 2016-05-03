/**
 * @license dynamicModal v0.9
 * (c) shince 2016
 * author:  Jeongyong, Jo.
 * License: 모든 라이센스는 작성자인 Jeongyong, Jo에 권한이 있으며 무단 도용 
 *          및 소스 복제는 허용되지 않습니다.
 */

if (typeof jQuery === "undefined") {
    throw new Error("dynamic Modal requires jQuery lib");
}


/* ========================================================================
 * dynamic Modal: bootstrap.modal.js v0.9 
 * ======================================================================== 
 *
 * bootstrap의 modal을 기반으로 하는 동적 레이어 팝업(모달) 라이브러리.
 * bootstrap의 기본 css(디자인)을 기반으로 하며 동적 출력시 문제점이 발견되어 
 * 레이어 외곽의 디자인을 동적으로 처리하도록 커스터마이징 됨.
 *
 * 함수:
 *
 *  init    =>  내부에 지정된 외곽 디자인 선택
 *  open    =>  원격지의 html를 레이어로 출력. 매개변수로 JSON 타입의 데이터 전달 가능
                만약 매개변수가 제공되면 HttpGet에서 HttpPost로 변경 됨.
 *  confirm =>  submit 직전 확인 메세지 출력.
 *  submit  =>  원격지로 부터 출력된 html 내의 form submit을 수행.
 * ======================================================================== */

var dynamicModal = (function () {
    
    var layoutCount = 0;
    var layoutArray = [];

    var getFormData = function (frm, params) {
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
    };

    var comopileForAngular = function (selector) {
        if (angular) {
            var $target = $("[ng-app]");
            angular.element($target).injector().invoke(['$compile', function ($compile) {
                var scope = angular.element($target).scope();
                $compile(selector)(scope);
                scope.$apply();
            }]);
        }
    };

    var layoutSet = [
        '<div class="modal fade" role="dialog">' +
            '<div class="modal-dialog">' +
                '<div class="modal-content" />' +
            '</div>' +
        '</div>',
        '<div class="modal fade" role="dialog">' +
            '<div class="modal-dialog normal"> ' +
                '<div class="modal-content pop-logout">' +
                '</div>' +
            '</div>' +
        '</div>',
        '<div class="modal fade" role="dialog">' +
            '<div class="modal-dialog wide"> ' +
                '<div class="modal-content">' +
                '</div>' +
            '</div>' +
        '</div>',
        '<div class="modal fade" role="dialog">' +
            '<div class="modal-dialog normal"> ' +
                '<div class="modal-content">' +
                '</div>' +
            '</div>' +
        '</div>'
    ];

    var complete = function () { };

    return new function () {
        var $confirmMessage = undefined;
        var $layoutIndex = 0;

        this.init = function (layoutIndex) {
            $layoutIndex = $.isNumeric(layoutIndex) ? layoutIndex : 0;
            return this;
        };

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
        this.open = function (templateUrl, params, callback) {
            if ($.isFunction(params)) {
                callback = params;
                params = undefined;
            }

            var layoutIndex = $layoutIndex || 0;
            $layoutIndex = 0;

            var layout = $('<div id="modalLayer' + (++layoutCount) + '" />');
            layout.append(layoutSet[layoutIndex]);
            layout.find(":last").append("<div id='dyModalContent" + layoutCount + "' />");
            layout.appendTo('body');
            layout.on("hide.bs.modal", function (e) {
                $confirmMessage = undefined;
                setTimeout(function () {                                        
                    layoutArray.pop().remove();
                    layoutCount--;                    
                }, 500);                
            });       

            $.ajax({
                type: params != undefined ? "post" : "get",
                url: templateUrl,
                data: JSON.stringify(params),
                contentType: "application/json; charset=UTF-8",
            }).done(function (res, status, xhr) {
                layout.find("[id^='dyModalContent']").html(res);
                layoutArray.push(layout);
                $.validator.unobtrusive.parse("#modalLayer" + layoutCount + " form");
                $.validator.methods.number = function (e) { return true; }; 
                layout.find('div:eq(0)').modal({ backdrop: layoutArray.length == 1 });          
                comopileForAngular($("#dyModalContent" + layoutCount));
                if (typeof callback === "function") {
                    callback(layout.find('div:eq(0)'));
                    callback = null;
                }
            }).fail(function (xhr, status, thrown) {
                //alert(xhr.responseText);
            }).complete(function () {
                setTimeout(function () {
                    layout.find("input:text:first").focus();
                }, 500);
                if (typeof callback === "function") {
                    callback(layout.find('div:eq(0)'));
                }
            });

            return this;
        };
       

        /**
        * @description
        * submit 처리 전에 confirm 메세지 창을 표시한다.   
        *
        * @param {string} msg : 확인 메세지    
        * 
        * @returns {object} : current modal object.
        */
        this.confirm = function (msg) {
            $confirmMessage = msg;
            return this;
        };

        /**
        * @description
        * 호출된 layout popup의 form submit을 수행한다. 
        *
        * @param {boolean} processContinue : submit 이후 자동으로 창을 닫을지 여부. (optional: 기본값 false)
        * @param {JSON} params : POST 전송 parameters. (optional)
        * @param {function} success : submit 후 success callback function. (optional)
        * @param {function} fail : submit 호출 후 failed callback function. (optional)  
        */
        this.submit = function (processContinue, params, success, fail) {            

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

            $("body").off("submit", "#modalLayer" + layoutCount + " form")
                .on("submit", "#modalLayer" + layoutCount + " form", function (e) {

                    if (!processContinue)
                        e.preventDefault();

                    if ($confirmMessage != undefined) {
                        if (!confirm($confirmMessage)) {
                            return;
                        }   
                    }

                    var frm = $(this);
                    jQuery.ajax({
                        url: frm.attr('action'),
                        type: 'POST',
                        dataType: 'json',
                        data: getFormData(frm, params),
                        processData: false,
                        contentType: 'application/json; charset=UTF-8',
                        headers: { "__RequestVerificationToken": frm.find("input[name='__RequestVerificationToken']").val() }
                    }).done(function (res, status, xhr) {
                        var currentModal = layoutArray[layoutCount - 1];
                        if ($.isFunction(success))
                            success(res, currentModal);
                        currentModal.find('div:eq(0)').modal('hide');
                    }).fail(function (xhr, status, thrown) {
                        if (status == "parsererror")
                            layoutArray[layoutCount - 1]
                                .find('[id^="dyModalContent"]')
                                .html(xhr.responseText);
                        //else
                        //    alert(xhr.responseText);
                    }).complete(function () {
                        //if (typeof complete === 'function') {
                        //    complete();
                        //}
                        comopileForAngular(layoutArray[layoutCount - 1]);                        
                    });
                });
            return this;
        };

        this.complete = function (func) {
            complete = func;
        };
    };
})();
