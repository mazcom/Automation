using DemoTestProject.Interfaces;
using Microsoft.Extensions.Configuration;

namespace DemoTestProject.Logic
{
  internal class FeaturesManager : IFeaturesManager
  {
    public FeaturesManager(IConfiguration configuration)
    {
      Console.WriteLine($"Edition: {configuration["Edition"]}");
      Console.WriteLine($"V1: {configuration["V1"]}");
    }

    public bool IsEnabled(string featureName)
    {
      Console.WriteLine($"Console writes feature name: {featureName}");
      return true;
    }
  }
}
