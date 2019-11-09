'use strict';
angular.module('SiteAwake.WebApplication.Common')
.constant('API_ENDPOINT', '')
.service('HomeService', ['$http', '$q', 'API_ENDPOINT', function ($http, $q, API_ENDPOINT) {
    var homeService = this;

    $http.defaults.useXDomain = true;
    delete $http.defaults.headers.common['X-Requested-With'];
       
    homeService.sendMessage = function (model) {
        var deferObject;
        var promise =
            $http({
                method: 'POST',
                url: API_ENDPOINT + '/Home/SendMessage',
                dataType: 'json',
                data: model
            });

        deferObject = deferObject || $q.defer();

        promise.then(
            // OnSuccess function
            function (response) {
                // This code will only run if we have a successful promise.
                deferObject.resolve(response);
            },
            // OnFailure function
            function (response) {
                // This code will only run if we have a failed promise.
                deferObject.reject(response);
            });

        return deferObject.promise;
    };

    homeService.signUp = function (model) {
        var deferObject;
        var promise =
            $http({
                method: 'POST',
                url: API_ENDPOINT + '/Account/Create',
                dataType: 'json',
                data: model
            });

        deferObject = deferObject || $q.defer();

        promise.then(
            // OnSuccess function
            function (response) {
                // This code will only run if we have a successful promise.
                deferObject.resolve(response);
            },
            // OnFailure function
            function (response) {
                // This code will only run if we have a failed promise.
                deferObject.reject(response);
            });

        return deferObject.promise;
    };

    homeService.resendVerification = function (model) {
        var deferObject;
        var promise =
            $http({
                method: 'POST',
                url: API_ENDPOINT + '/Account/ResendVerification',
                dataType: 'json',
                data: model
            });

        deferObject = deferObject || $q.defer();

        promise.then(
            // OnSuccess function
            function (response) {
                // This code will only run if we have a successful promise.
                deferObject.resolve(response);
            },
            // OnFailure function
            function (response) {
                // This code will only run if we have a failed promise.
                deferObject.reject(response);
            });

        return deferObject.promise;
    };
}]);