using DemoTestProject.Interfaces;
using DemoTestProject.Logic;
using Microsoft.Extensions.DependencyInjection;
using Microsoft.Extensions.Hosting;
using Microsoft.Extensions.Logging;
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

      var builder = Host.CreateDefaultBuilder();
      builder.ConfigureServices((_, services) =>
        services.AddTransient<IFeaturesManager, FeaturesManager>()
        .AddLogging(config => config.AddConsole())
      );

      builder.Build();
    }
  }
}
