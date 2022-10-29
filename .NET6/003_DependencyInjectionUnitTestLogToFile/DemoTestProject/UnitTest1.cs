using Microsoft.Extensions.DependencyInjection;
using Microsoft.Extensions.Logging;
using Microsoft.VisualStudio.TestPlatform.TestHost;

namespace DemoTestProject
{
  [TestClass]
  public class UnitTest1
  {
    [TestMethod]
    [TestCategory("DemoCategory")]
    [TestCategory("ProductCategory")]
    public void TestMethod1()
    {
      //Console.WriteLine("Console write text");

      Manager.Host.Services.GetRequiredService<ILogger<Program>>().LogWarning("ILogger<Program> warning hello");
      Manager.Host.Services.GetRequiredService<ILogger<Program>>().LogDebug("ILogger<Program> debug hello");
    }
  }
}