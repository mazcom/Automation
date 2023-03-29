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
    }

    [TestMethod]
    [TestCategory("Common")]
    public void TestMethod2()
    {
      Console.WriteLine("hello from UnitTest1.TestMethod2()");
    }

    [TestMethod]
    [TestCategory("PgCategory")]
    public void TestMethod3()
    {
      Console.WriteLine("hello from UnitTest1.TestMethod3()");
    }
  }
}