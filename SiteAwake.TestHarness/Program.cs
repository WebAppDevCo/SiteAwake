using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using SiteAwake.WakeUpCallProcessor;
using SiteAwake.WakeUpCallProcessor.Infrastructure;
using SiteAwake.Domain.Infrastructure;
using SiteAwake.DomainProvider.Infrastructure;
using System.IO;
using Microsoft.Practices.Unity;

namespace SiteAwake.TestHarness
{
    class Program
    {
        static void Main(string[] args)
        {            
            try
            {
                IUnityContainer unitycontainer = new UnityContainer();
                unitycontainer.RegisterType<IContext, SiteAwakeDbContext>();
                unitycontainer.RegisterType<IEmailManager, EmailManager>();
                unitycontainer.RegisterType<IWorker, Worker>();
                unitycontainer.RegisterType<IManager, Manager>();
                unitycontainer.RegisterType<IRunner, Runner>();

                Logger.Log(System.Reflection.MethodBase.GetCurrentMethod().DeclaringType, Logger.EventType.Info, "Application Start", null);

                Runner runner = unitycontainer.Resolve<Runner>();
                runner.Run();

            }
            catch (Exception ex)
            {
                System.Diagnostics.Debug.WriteLine(ex.ToString());
            }
        }
    }
}
