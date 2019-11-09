using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using SiteAwake.WakeUpCallProcessor;
using System.IO;
using SiteAwake.Domain.Infrastructure;

namespace SiteAwake.TestHarness
{
    interface IRunner
    {
        bool Run();
    }

    class Runner : IRunner
    {
        private readonly IManager Manager;
        
        public Runner(IManager manager)
        {
            Manager = manager;
        }
        
        public bool Run()
        {
            try
            {
                var task1 = Task.Run(() =>
                {
                    Manager.Run();
                });

                task1.Wait();

                return true;
            }
            catch (Exception ex)
            {
                System.Diagnostics.Debug.WriteLine(ex.ToString());
            }

            return false;
        }
    }
}
