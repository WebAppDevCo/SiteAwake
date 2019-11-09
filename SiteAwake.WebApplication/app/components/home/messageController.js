'use strict';
angular.module('SiteAwake.WebApplication.Common')
.constant('API_ENDPOINT', '')
.controller('MessageController', ['$scope', '$window', 'HomeService', 'UtilitiesService', 'API_ENDPOINT', function ($scope, $window, homeService, utilitiesService, API_ENDPOINT) {
    var message = this;
    
    message.email = '';
    message.phone = '';
    message.name = '';
    message.message = '';
    message.showConfirm = false;

    message.onSendMessageClick = function (e) {
        e.preventDefault();

        if (message.showConfirm) {
            return false;
        }

        message.showConfirm = false;

        var model = {
            Email: message.email,
            Phone: message.phone,
            Name: message.name,
            Message: message.message
        };

        homeService.sendMessage(model).then(function successCallback(response) {
            if (response != null && response != undefined) {
                console.log(response);
            }
        }, function errorCallback(response) {
            console.log(response);
        });

        message.showConfirm = true;
    };
    
    message.onClose = function (e) {
        e.preventDefault();

        $('#modalDialogMessage').modal('hide');
    };

    message.initialize = function () {

    }; 
    
    message.initialize();
}])
.directive('sendKeyPressed', function () {
    return function (scope, element, attrs) {
        element.bind('keydown keypress', function (event) {
            if (event.which === 13) {
                event.preventDefault();
                $('#btnSendMessage').click();
            }
        });
    };
});