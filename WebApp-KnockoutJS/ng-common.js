var $app = angular.module('myApp', [])
.factory('httpInsterceptor', ['$location', '$window', '$q', function ($location, $window, $q) {
    return {
        request: function (config) {
            var token = $('body').find("input[name='__RequestVerificationToken']:first").val();
            if (token)
                config.headers["__RequestVerificationToken"] = token;
            config.headers["X-Requested-With"] = "XMLHttpRequest";
            return config;
        },
        responseError: function (error) {
            alert(error.data);
            return $q.reject(error);
        }
    }
}])
.config(['$httpProvider', function ($httpProvider) {
    $httpProvider.interceptors.push('httpInsterceptor');
}])
.filter('trim', function () {
    return function (value) {
        if (!angular.isString(value)) {
            return value;
        }
        return value.replace(/^\s+|\s+$/g, "");
    }
})
.filter('num', ['$filter', function ($filter) {
    return function (value) {
        var temp = value.toString().replace(/[^\d]/g, "");
        var result = $filter('number')(temp);
        return result;
    }
}])
.filter('toDate', ['$filter', function ($filter) {
    return function (value, format) {
        if (value == undefined)
            return value;

        var dateString = value.replace(/(\d{4})(\d{2})(\d{2})/gi, "$1-$2-$3");
        var date = new Date(dateString);

        if (!angular.isDate(date))
            return value;

        var result = $filter('date')(date, format);
        return result;
    }
}])
.filter('toNumber', function () {
    return function (value) {
        if (!Number(value)) {
            return value
        }
        return Number(value);
    }
})
.filter('html', ['$sce', function ($sce) {
    return function (value) {
        return $sce.trustAsHtml(value);
    };
}])
// ex '20140105' | mask : '####.##.##'
.filter('mask', function () {
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
})
.directive('format', ['$filter', function ($filter) {
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
}])
.directive('krInput', ['$parse', function ($parse) {
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

// text highlighting
$app.filter('highlight', ['$sce', function ($sce) {
    return function (text, phrase) {
        if (phrase)
            text = text.replace(new RegExp('(' + phrase + ')', 'gi'), '<span style="background-color:yellow">$1</span>');
        return $sce.trustAsHtml(text);
    };
}]);
