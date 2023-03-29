using DemoTestProject.Interfaces;
using DemoTestProject.Logic;
using Microsoft.Extensions.Configuration;
using Microsoft.Extensions.DependencyInjection;
using Microsoft.Extensions.Logging;
using Microsoft.VisualStudio.TestPlatform.TestHost;
using Microsoft.VisualStudio.TestTools.UnitTesting;

namespace DemoTestProject
{
  [TestClass]
  public class UnitTests
  {
    [TestMethod]
    [TestCategory("SQLCategory")]
    [TestCategory("OracleCategory")]
    [TestCategory("PgCategory")]
    public void TestMethod1()
    {
      Console.WriteLine("hello from UnitTest1.TestMethod1()");
      Do();
    }

    protected virtual void Do()
    {

    }
  }

  [TestClass]
  public class DerivedUnitTests : UnitTests
  {
    protected override void Do()
    {

    }
  }
}