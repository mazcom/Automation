using DemoTestProject.Interfaces;
using DemoTestProject.Logic;
using Microsoft.Extensions.DependencyInjection;
using Microsoft.Extensions.Hosting;
using Microsoft.Extensions.Logging;
using Serilog;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace DemoTestProject
{
  [TestClass]
  internal class Initializer
  {
    [AssemblyInitialize]
    public static void Initialize(TestContext testContext)
    {

      var logger = new LoggerConfiguration()
        .MinimumLevel.Debug()
        .WriteTo.File("aaaaaabbbb.log", rollingInterval: RollingInterval.Infinite)
        //.WriteTo.Console() // можно писать одновременно и в файл и в консоль.
        .CreateLogger();


      var builder = Host.CreateDefaultBuilder();
      builder.ConfigureServices((_, services) =>
        services.AddTransient<IFeaturesManager, FeaturesManager>()
      );

      builder.UseSerilog(logger);

      var host =  builder.Build();
      Manager.Host = host;
    }
  }
}
