using System;
using System.Collections.Generic;
using System.Linq;
using System.Web;
using System.Web.Http;
using System.Web.Mvc;
using System.Web.Optimization;
using System.Web.Routing;
using System.IO;
using log4net;
using log4net.Config;
using System.Reflection;

namespace SiteAwake.WebApplication
{
    public class WebApiApplication : System.Web.HttpApplication
    {
        private readonly ILog Logger = LogManager.GetLogger(MethodBase.GetCurrentMethod().DeclaringType);

        protected void Application_Start()
        {
            AreaRegistration.RegisterAllAreas();
            GlobalConfiguration.Configure(WebApiConfig.Register);
            FilterConfig.RegisterGlobalFilters(GlobalFilters.Filters);
            RouteConfig.RegisterRoutes(RouteTable.Routes);
            BundleConfig.RegisterBundles(BundleTable.Bundles);
            XmlConfigurator.Configure(new FileInfo(Server.MapPath("~/Log4Net.config")));
        }
        
        protected void Application_Error(object sender, EventArgs e)
        {
            Logger.Error(Server.GetLastError().Message, Server.GetLastError());
        }
    }
}
