'use strict';
angular.module('SiteAwake.WebApplication.Common')
.constant('API_ENDPOINT', '')
.controller('SignUpController', ['$scope', '$window', 'HomeService', 'UtilitiesService', 'API_ENDPOINT', function ($scope, $window, homeService, utilitiesService, API_ENDPOINT) {
    var signUp = this;

    var termsError = 'Please agree to the "Terms & Conditions" above.';

    signUp.email = '';
    signUp.password = '';
    signUp.protocol = 'http://';
    signUp.url = '';
    signUp.wakeupInterval = 4;
    signUp.feedbackMessage = '';
    signUp.processingForm = false;
    signUp.agreedToTerms = false;
    
    signUp.agreeToTerms = function (e) {
        e.preventDefault();

        signUp.agreedToTerms = true;

        if (signUp.feedbackMessage == termsError) {
            signUp.feedbackMessage = '';
        }

        $('#modalTermsAndConditions').modal('hide');
    };

    signUp.onAgreedToTermsCheckChanged = function () {
        if (signUp.feedbackMessage == termsError) {
            signUp.feedbackMessage = '';
        }
    };

    signUp.onSignUpClick = function (e) {
        e.preventDefault();

        if (signUp.showConfirm) {
            return false;
        }

        if (!signUp.agreedToTerms) {
            signUp.feedbackMessage = termsError;
            return false;
        }

        signUp.feedbackMessage = '';

        signUp.processingForm = true;

        signUp.showConfirm = false;

        var model = {
            Email: signUp.email,
            Password: signUp.password,
            Url: signUp.protocol + signUp.url.toLowerCase().replace(/http:\/\//g, '').replace(/https:\/\//g, ''),
            WakeUpIntervalMinutes: signUp.wakeupInterval,
            ErrorMessage: null
        };

        homeService.signUp(model).then(function successCallback(response) {
            if (response != null && response != undefined) {
                if (response.data.ErrorMessage != null) {
                    signUp.feedbackMessage = response.data.ErrorMessage;
                }
                else {
                    signUp.showConfirm = true;
                }
            }
            else {
                signUp.feedbackMessage = 'An unspecified error occurred.';
            }

            signUp.processingForm = false;
        }, function errorCallback(response) {
            signUp.feedbackMessage = 'An unspecified error occurred.';
            signUp.processingForm = false;
            console.log(response);
        });
    };

    signUp.onResendVerificationClick = function (e) {
        e.preventDefault();

        if (!signUp.showConfirm) {
            return false;
        }

        signUp.feedbackMessage = '';

        //signUp.processingForm = true;

        signUp.showConfirm = true;

        var model = {
            Email: signUp.email,
            Password: signUp.password,
            Url: signUp.protocol + signUp.url.toLowerCase().replace(/http:\/\//g, '').replace(/https:\/\//g, ''),
            WakeUpIntervalMinutes: signUp.wakeupInterval,
            ErrorMessage: null
        };

        homeService.resendVerification(model).then(function successCallback(response) {
            if (response != null && response != undefined) {
                if (response.data.ErrorMessage != null) {
                    signUp.feedbackMessage = response.data.ErrorMessage;
                }
                else {
                    alert('Verification Sent');
                }
            }
            else {
                signUp.feedbackMessage = 'An unspecified error occurred.';
            }

            signUp.processingForm = false;
        }, function errorCallback(response) {
            signUp.feedbackMessage = 'An unspecified error occurred.';
            //signUp.processingForm = false;
            console.log(response);
        });
    };

    signUp.initialize = function () {

    };
    
    signUp.initialize();
}])
.directive('participateKeyPressed', function () {
    return function (scope, element, attrs) {
        element.bind('keydown keypress', function (event) {
            if (event.which === 13) {
                event.preventDefault();
                $('#btnSignUp').click();
            }
        });
    };
});