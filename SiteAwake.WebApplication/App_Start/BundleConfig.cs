using System.Web;
using System.Web.Optimization;

namespace SiteAwake.WebApplication
{
    public class BundleConfig
    {
        // For more information on bundling, visit http://go.microsoft.com/fwlink/?LinkId=301862
        public static void RegisterBundles(BundleCollection bundles)
        {
            bundles.Add(new ScriptBundle("~/bundles/jquery").Include(
                        "~/Scripts/jquery-{version}.js"
                        /*"~/Assets/plugins/jquery-ui-1.11.4/jquery-ui.min.js"*/));

            bundles.Add(new ScriptBundle("~/bundles/jqueryval").Include(
                      "~/Scripts/jquery.validate*"));

            // Use the development version of Modernizr to develop with and learn from. Then, when you're
            // ready for production, use the build home at http://modernizr.com to pick only the tests you need.
            bundles.Add(new ScriptBundle("~/bundles/modernizr").Include(
                        "~/Scripts/modernizr-*"));

            bundles.Add(new ScriptBundle("~/bundles/bootstrap").Include(
                      "~/Scripts/bootstrap.js",
                      "~/Scripts/respond.js"));

            bundles.Add(new StyleBundle("~/Content/css").Include(
                      "~/Content/bootstrap.css",
                      "~/Assets/plugins/font-awesome-4.6.3/css/font-awesome.min.css",
                      //"~/plugins/jquery-ui-1.11.4.custom/jquery-ui.css",
                      "~/Content/site.css"
                      ));

            bundles.Add(new StyleBundle("~/Content/css-account-admin").Include(
                      "~/Content/bootstrap.css",
                      "~/Assets/plugins/font-awesome-4.6.3/css/font-awesome.min.css",
                      //"~/plugins/jquery-ui-1.11.4.custom/jquery-ui.css",
                      "~/Content/site.css",
                      "~/Content/account-admin.css"
                      ));
        }
    }
}
