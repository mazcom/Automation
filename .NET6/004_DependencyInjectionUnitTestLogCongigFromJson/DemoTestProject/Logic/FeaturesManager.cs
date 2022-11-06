using DemoTestProject.Interfaces;
using Microsoft.Extensions.Configuration;

namespace DemoTestProject.Logic
{
  internal class FeaturesManager : IFeaturesManager
  {
    public FeaturesManager(IConfiguration configuration)
    {

    }

    public bool IsEnabled(string featureName)
    {
      Console.WriteLine($"Console writes feature name: {featureName}");
      return true;
    }
  }
}
