using DemoTestProject.Interfaces;
using DemoTestProject.Logic;
using Microsoft.Extensions.Configuration;
using Microsoft.Extensions.DependencyInjection;
using Microsoft.Extensions.Hosting;
using Microsoft.Extensions.Logging;
using Serilog;
using System;
using System.Collections.Generic;
using System.Configuration;
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
      // https://github.com/serilog/serilog-settings-configuration

      var configuration = new ConfigurationBuilder()
       .AddJsonFile("appsettings.json")
       .Build();

      var builder = Host.CreateDefaultBuilder();
      builder
        .ConfigureAppConfiguration(config =>
        {
          config.AddConfiguration(configuration);
        })
        .ConfigureServices((_, services) =>
        services.AddTransient<IFeaturesManager, FeaturesManager>()
      );

      var logger = new LoggerConfiguration()
        .ReadFrom.Configuration(configuration)
        .CreateLogger();

      builder.UseSerilog(logger);

      var host = builder.Build();

      Manager.Host = host;
    }
  }
}
