using DemoTestProject.Interfaces;

namespace DemoTestProject.Logic
{
  internal class FeaturesManager : IFeaturesManager
  {
    public bool IsEnabled(string featureName)
    {
      Console.WriteLine($"Console writes feature name: {featureName}");
      return true;
    }
  }
}
