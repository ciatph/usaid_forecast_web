﻿using CIAT.DAPA.USAID.Forecast.Web.Models.Forecast;
using CIAT.DAPA.USAID.Forecast.Web.Models.Tools;
using Microsoft.AspNetCore.Hosting;
using Microsoft.AspNetCore.Mvc;
using Microsoft.Extensions.Options;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Threading.Tasks;

namespace CIAT.DAPA.USAID.Forecast.Web.Controllers
{
    public abstract class WebBaseController : Controller
    {
        protected IHostingEnvironment hostingEnvironment { get; set; }
        /// <summary>
        /// Get or set the object to get information from forecast service
        /// </summary>
        protected WebAPIForecast apiForecast { get; set; }

        /// <summary>
        /// Method Construct
        /// </summary>
        /// <param name="settings">Settings options</param>
        /// <param name="hostingEnvironment">Host Enviroment</param>
        public WebBaseController(IOptions<Settings> settings, IHostingEnvironment environment) : base()
        {
            hostingEnvironment = environment;
            apiForecast = new WebAPIForecast()
            {
                root = settings.Value.api_fs,
            };
        }

        /// <summary>
        /// Method that load the paths of the web apis
        /// </summary>
        protected void loadAPIs()
        {
            ViewBag.api_fs = apiForecast.root;
        }

        /// <summary>
        /// Method that stablish the months of the forecast
        /// </summary>
        protected void loadMonthsClimate()
        {
            string dates = string.Empty;
            DateTime start = DateTime.Now.AddMonths(-1);
            for (int i = 1; i <= 6; i++)
            {
                start = start.AddMonths(1);
                dates += start.ToString("MM") + ",";
            }
            ViewBag.gv_months = dates.Substring(0, dates.Length - 1);
        }

        /// <summary>
        /// Method that stablish the months of the forecast
        /// </summary>
        protected void loadMonthsCrop()
        {
            string dates = string.Empty;
            string years = string.Empty;
            DateTime start = DateTime.Now.AddMonths(-1);            
            for (int i = 1; i <= 2; i++)
            {
                start = start.AddMonths(1);
                dates += start.ToString("yyyy-MM") + ",";
            }
            ViewBag.gv_months = dates.Substring(0, dates.Length - 1);
        }


    }
}
