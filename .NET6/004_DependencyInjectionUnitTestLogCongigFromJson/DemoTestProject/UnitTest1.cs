//using DemoTestProject.Interfaces;
//using DemoTestProject.Logic;
//using Microsoft.Extensions.Configuration;
//using Microsoft.Extensions.DependencyInjection;
//using Microsoft.Extensions.Logging;
//using Microsoft.VisualStudio.TestPlatform.TestHost;
//using Microsoft.VisualStudio.TestTools.UnitTesting;

//namespace DemoTestProject
//{
//  [TestClass]
//  public class UnitTest1
//  {
//    //public UnitTest1(ILogger<UnitTest1> logger)
//    //{

//    //}

//    [TestMethod]
//    [TestCategory("DemoCategory")]
//    [TestCategory("ProductCategory")]
//    public void TestMethod1()
//    {

//      Manager.Host.Services.GetRequiredService<ILogger<Program>>().LogWarning("ILogger<Program> warning hello");
//      Manager.Host.Services.GetRequiredService<ILogger<Program>>().LogDebug("ILogger<Program> debug hello");

//      Environment.SetEnvironmentVariable("Edition", "Pro");
//      Manager.Host.Services.GetRequiredService<ILogger<Program>>().LogDebug(Environment.GetEnvironmentVariable("Edition"));
//      Manager.Host.Services.GetRequiredService<ILogger<Program>>().LogDebug(Environment.GetEnvironmentVariable("ASPNETCORE_ENVIRONMENT"));


//      var featureManager = Manager.Host.Services.GetRequiredService<IFeaturesManager>();

//      var service = Manager.Host.Services.GetRequiredService<ICalculationService>();
//      service.AddTwoPositiveNumbers(1, 2);
//    }
//  }
//}