var $app = angular.module('myApp', []);
$app.factory('httpInsterceptor', function ($location, $q, $window) {
    return {
        request: function (config) {
            var token = $('body').find("input[name='__RequestVerificationToken']:first").val();
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
$app.filter('num', ['$filter', function ($filter) {
    return function (value) {
        var temp = value.toString().replace(/[^\d]/g, "");
        var result = $filter('number')(temp);
        return result;
    }
}]);
$app.filter('toDate', ['$filter', function ($filter) {
    return function (value, format) {
        var dateString = value.toString().replace(/(\d{4})(\d{2})(\d{2})/gi, "$1-$2-$3");
        var date = new Date(dateString);
        //console.log(angular.isDate(date));
        if (!angular.isDate(date))
            return value;

        var result = $filter('date')(date, format);
        return result;
    }
}]);
$app.filter('toNumber', function () {
    return function (value) {
        if (!Number(value)) {
            return value
        }
        return Number(value);
    }
});
$app.filter('html', ['$sce', function ($sce) {
    return function (value) {
        return $sce.trustAsHtml(value);
    };
}]);
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
            var length = args[1] ? scope.$eval(args[1]) : 0;
            $(elem).number(true, length);
            ctrl.$formatters.unshift(function () {return $filter(args[0])(ctrl.$modelValue);});
            elem.bind('keyup', function (event) {
                /***** jQuery number plug-in *****/
                //var plainNumber = elem.val().replace(/[^\d]/g, "").replace(/\B(?=(\d{3})+(?!\d))/g, ",");                
                //elem.val(plainNumber);
            });
        }
    }
}]);
$app.directive('krInput', ['$parse', function ($parse) {
    return {
        priority : 2,
        restrict : 'A',
        compile : function(element) {
            element.on('compositionstart',function(e){
                e.stopImmediatePropagation();
            });
        },

    };
}]);
