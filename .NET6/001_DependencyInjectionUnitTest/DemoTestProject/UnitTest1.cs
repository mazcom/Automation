using Microsoft.Extensions.Logging;

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
      Console.WriteLine("Console write text");
    }
  }
}