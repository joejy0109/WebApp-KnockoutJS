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

            ctrl.$formatters.unshift(function (a) {
                return $filter(attrs.format)(ctrl.$modelValue);
            });

            elem.bind('keyup', function (event) {
                var plainNumber = elem.val().replace(/[^\d]/g, "").replace(/\B(?=(\d{3})+(?!\d))/g, ",");
                elem.val(plainNumber);
            });
        }
    }
}]);
