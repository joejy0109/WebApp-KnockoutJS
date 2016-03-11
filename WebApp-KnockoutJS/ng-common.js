var $app = angular.module('myApp', []);
$app.factory('httpInsterceptor', function ($location, $q, $window) {
    return {
        request: function (config) {
            var token = $($window.document.forms[0]).find("input[name='__RequestVerificationToken']").val();
            if (token)
                config.headers["__RequestVerificationToken"] = token;
            config.headers["X-Requested-With"] = "XMLHttpRequest";

            return config;
        }
    }
});
$app.config(function ($httpProvider) { $httpProvider.interceptors.push('httpInsterceptor'); });
$app.filter('trim', function () {
    return function (value) {
        if (!angular.isString(value)) {
            return value;
        }
        return value.replace(/^\s+|\s+$/g, "");
    }
});
$app.filter('toNumber', function () {
    return function (value) {
        if (!Number(value)) {
            return value
        }
        return Number(value);
    }
});
// ex '20140105' | mask : '####.##.##'
$app.filter('mask', function () {
    return function (value, m) {
        if (!angular.isString(value)) {
            return value;
        }
        var m;
        var l = (m = m.split("")).length
        var s = value.split("");
        var j = 0;
        var h = "";

        for (var i = -1; ++i < l;) {
            if (m[i] != "#") {
                if (m[i] == "\\" && (h += m[++i])) continue;
                h += m[i];
                i + l == l && (s[j - 1] += h, h = "");
            } else {
                if (!s[j] && !(h = "")) break;
                (s[j] = h + s[j++]) && (h = "");
            }
        }
        return s.join("") + h;
    }
});
$app.directive('format', ['$filter', function ($filter) {
    return {
        require: "?ngModel",
        link: function (scope, elem, attrs, ctrl) {
            if (!ctrl)
                return;

            var args = attrs.format.split(':');
            var length = args[1] ? scope.$eval(args[1]) : 999999999;

            ctrl.$formatters.unshift(function () {
                //return $filter(attrs.format)(ctrl.$modelValue);
                return $filter(args[0])(ctrl.$modelValue);
            });

            elem.bind('keyup', function (event) {
                if (length != undefined)
                    console.log(length);

                var plainNumber = elem.val().replace(/[^\d]/g, "").replace(/\B(?=(\d{3})+(?!\d))/g, ",");
                elem.val(plainNumber);
            });
        }
    }
}]);

$(function () {
    $.validator.setDefaults({
        onsubmit: true,
        onkeyup: false,
        onfocusout: false,
        showErrors: function (errMap, errList) {
            if (errList.length > 0) {
                var errors = errList.map(function (elem) {
                    return elem.message;
                }).join('\r\n');
                alert(errors)
            }
        }
    });
});
