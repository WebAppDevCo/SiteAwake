/*
geolocation code requires the following page level reference
<script type="text/javascript" src="https://maps.googleapis.com/maps/api/js"></script>
*/
'use strict';
angular.module('SiteAwake.WebApplication.Common')
    .service('UtilitiesService',
        function () {
            var utilitiesService = this;

            //utilitiesService.zipcode = '';

            //utilitiesService.initialize = function () {
            //    //set zipcode based on current location
            //    if ("geolocation" in navigator) {
            //        navigator.geolocation.getCurrentPosition(
            //            function (position) {
            //                var point = new google.maps.LatLng(position.coords.latitude, position.coords.longitude);

            //                new google.maps.Geocoder().geocode({
            //                    'latLng': point
            //                }, function (res, status) {
            //                    if (status === google.maps.GeocoderStatus.OK && typeof res[0] !== 'undefined') {
            //                        //console.log(res);
            //                        var zip = res[0].formatted_address.match(/,\s\w{2}\s(\d{5})/);
            //                        if (zip) {
            //                            //console.log(zip);
            //                            utilitiesService.zipcode = zip[1];
            //                            //console.log(utilitiesService.zipcode);
            //                            //$scope.$apply(); //binds value to UI
            //                        } else {
            //                            console.log('Failed to parse zip code');
            //                        }
            //                    } else {
            //                        console.log('Failed to call maps api');
            //                    }
            //                });
            //            },
            //            function (error) {
            //                console.log(error.message);
            //            }
            //        );
            //    } else {
            //        console.log('GeoLocation unsupported! Please update your browser!');
            //    }
            //}

            //utilitiesService.initialize();
        }
    );