﻿angular.module('ForecastApp')
  .controller('GlossaryCtrl', function ($rootScope) {      
      // Close loading 
      window.loading_screen.finish();

      /*
      * Method that render the data in the screen
      * (string) section: Section name to draw
      */
      $rootScope.drawFunction = function (section) {
          
      }
  });