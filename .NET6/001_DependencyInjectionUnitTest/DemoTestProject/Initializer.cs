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
  public class Initializer
  {
    [AssemblyInitialize]
    public static void Initialize(TestContext testContext)
    {

      Log.Logger = new LoggerConfiguration()
        .WriteTo.Console() // write log to the console
        .CreateBootstrapLogger();


      var builder = Host.CreateDefaultBuilder();
      builder.ConfigureServices((_, services) =>
        services.AddTransient<IFeaturesManager, FeaturesManager>()
        //.AddLogging(config => config.AddConsole())
      );

      builder.UseSerilog();

      //builder.ConfigureLogging(logging =>
      //{
      //  logging.ClearProviders();
      //  logging.AddConsole();
      //});

      var host =  builder.Build();
      Manager.Host = host;


    }
  }
}
