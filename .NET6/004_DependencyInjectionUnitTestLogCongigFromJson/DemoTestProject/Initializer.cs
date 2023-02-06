using DemoTestProject.Interfaces;
using DemoTestProject.Logic;
using Microsoft.Extensions.Configuration;
using Microsoft.Extensions.DependencyInjection;
using Microsoft.Extensions.Hosting;
using Microsoft.Extensions.Logging;
using Microsoft.VisualStudio.TestPlatform.TestHost;
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
      var args = Environment.GetCommandLineArgs();

      // https://github.com/serilog/serilog-settings-configuration

      var configuration = new ConfigurationBuilder()
       .AddJsonFile("appsettings.json")
       //.AddJsonFile($"appsettings.{env.EnvironmentName}.json", true, true);
       .AddEnvironmentVariables()
       .Build();

      var builder = Host.CreateDefaultBuilder();
      builder
        .ConfigureAppConfiguration((hostingContext, config) =>
        {
          // Environments.Development
          // Environments.Staging
          // Environments.Production

          IHostEnvironment env = hostingContext.HostingEnvironment;
          config.AddConfiguration(configuration);
          config.AddJsonFile($"appsettings.Pro.json", true, true);
          //config.AddCommandLine(args);
        })
        .ConfigureServices((_, services) =>
        services
        .AddTransient<IFeaturesManager, FeaturesManager>()
        .AddTransient<ICalculationService, CalculationService>()
      );

      var logger = new LoggerConfiguration()
        .ReadFrom.Configuration(configuration)
        .CreateLogger();

      builder.UseSerilog(logger);

      var host = builder.Build();

      Manager.Host = host;

      //foreach (var item in args)
      //{
      //  Manager.Host.Services.GetRequiredService<ILogger<Program>>().LogDebug($"arg: {item}");
      //  //Console.WriteLine($"arg: {item}");
      //}
    }
  }
}
