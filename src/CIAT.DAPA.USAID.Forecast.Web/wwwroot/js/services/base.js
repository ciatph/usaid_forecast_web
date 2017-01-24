﻿'use strict';

/**
 * @ngdoc service
 * @name ForecastApp.ForecastFactory
 * @description
 * # ForecastFactory
 * Factory in the ForecastApp.
 */
angular.module('ForecastApp')
    .factory('GeographicFactory', ['$http', 'config', function ($http, config) {
        var dataFactory = {};
        /*
        * Method that request all geographic information available from the forecast service
        */
        dataFactory.get = function () {
            if (dataFactory.raw == null) {
                dataFactory.raw = $http.get(config.api_fs + config.api_fs_geographic);
            }
            return dataFactory.raw;
        }
        return dataFactory;
    }])
    .factory('MunicipalityFactory', ['config', function (config) {
        var dataFactory = {};

        /*
        * Method that filter all municipalities available
        * (object) raw: Json with all geographic data
        */
        dataFactory.listAll = function (raw) {
            var data = raw.map(function (item) {
                return item.municipalities;
            });
            var m = [];
            for (var i = 0; i < data.length; i++)
                for (var j = 0; j < data[i].length; j++)
                    m.push(data[i][j]);
            return m;
        }

        /*
        * Method that filter the municipalities by their ids
        * (object) raw: Json with all geographic data
        * (string[]) ids: Id of the municipalities
        */
        dataFactory.listByIds = function (raw, ids) {
            var data = raw.map(function (item) {
                return item.municipalities;
            });
            var m = [];
            for (var i = 0; i < data.length; i++)
                for (var j = 0; j < data[i].length; j++) {
                    if (ids.includes(data[i][j].id))
                        m.push(data[i][j]);
                }
            return m;
        }

        return dataFactory;
    }])
    .factory('WeatherStationFactory', ['config', function (config) {
        var dataFactory = {};

        /*
        * Method that filter all weather stations by their municipality
        * (object) raw: Json with all geographic data
        * (string) municipality: Name of the municipality
        */
        dataFactory.getByMunicipality = function (raw, municipality) {
            var data = raw.filter(function (item) {
                var municipalities = item.municipalities.filter(function (item2) { return item2.name === municipality; });
                return municipalities.length > 0;
            });
            if (data == null)
                return null;
            // Map to get only weather station
            var ws = data.map(function (item) {
                var municipalities = item.municipalities.filter(function (item2) { return item2.name === municipality; });
                return municipalities[0].weather_stations[0];
            });
            return ws[0];
        }

        return dataFactory;
    }])
    .factory('AgronomicFactory', ['$http', 'config', function ($http, config) {
        var dataFactory = {};
        /*
        * Method that request all agronomic information available from the forecast service
        */
        dataFactory.get = function () {
            if (dataFactory.raw == null) {
                dataFactory.raw = $http.get(config.api_fs + config.api_fs_agronomic);
            }
            return dataFactory.raw;
        }
        return dataFactory;
    }])
    .factory('CultivarsFactory', ['config', function (config) {
        var dataFactory = {};
        /*
        * Method that filter all cultivars of the crop
        * (object) raw: Json with all agronomic data
        * (string) crop: Crop's name
        */
        dataFactory.getByCrop = function (raw, crop) {
            var data = raw.filter(function (item) {
                return item.cp_name.toLowerCase() === crop.toLowerCase();
            })[0];            
            return data.cultivars;
        }

        return dataFactory;
    }]);
