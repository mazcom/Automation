using NUnit.Framework;
using OpenQA.Selenium;
using OpenQA.Selenium.Appium;
using OpenQA.Selenium.Appium.Service;
using OpenQA.Selenium.Appium.Windows;
using OpenQA.Selenium.Interactions;
using OpenQA.Selenium.Remote;
using OpenQA.Selenium.Support.Events;
using OpenQA.Selenium.Support.UI;
using System;
using System.IO;
using System.Threading;
using Microsoft.Extensions.DependencyInjection;
using Microsoft.Extensions.Hosting;


using Keys = OpenQA.Selenium.Keys;
using Microsoft.Extensions.Configuration;
using Microsoft.FeatureManagement;
using System.Collections.Generic;
using OpenQA.Selenium.Html5;

namespace EditButtonAppTest
{
  public class Tests
  {
    private WindowsDriver<WindowsElement> driver;
    private EventFiringWebDriver eventFiringWebDriver;

    [SetUp]
    public void Setup()
    {

      var featureManagement = new Dictionary<string, string> { { "FeatureManagement:Beta", "true" } };

      IConfigurationRoot configuration = new ConfigurationBuilder().AddInMemoryCollection(featureManagement).Build();

      IServiceCollection services = new ServiceCollection();
      services.AddFeatureManagement(configuration);

      var serviceProvider = services.BuildServiceProvider();
      IFeatureManager featureManager = serviceProvider.GetRequiredService<IFeatureManager>();

      bool enabled = featureManager.IsEnabledAsync("Beta1").Result;

      var options = new AppiumOptions();

      string appFullPath = Path.GetFullPath(Directory.GetParent(Directory.GetCurrentDirectory()).Parent.FullName + "..//..//..\\EditButtonApp\\bin\\Debug\\EditButtonApp.exe");
      options.AddAdditionalCapability("app", appFullPath);
      options.AddAdditionalCapability("deviceName", "WindowsPC");

      Console.WriteLine("Trying to find a driver...");
      driver = new WindowsDriver<WindowsElement>(new Uri("http://127.0.0.1:4723"), options, TimeSpan.FromSeconds(100));
      eventFiringWebDriver = new EventFiringWebDriver(driver);

      Console.WriteLine("Driver is found...");
    }

    [TearDown]
    public void TestCleanup()
    {
      if (driver != null)
      {
        driver.Quit();
        driver = null;
      }
    }

    static IHostBuilder CreateHostBuilder(string[] args) =>
        Host.CreateDefaultBuilder(args)
            .ConfigureServices((_, services) =>
                services.AddTransient<TransientDisposable>()
                        .AddScoped<ScopedDisposable>()
                        .AddSingleton<SingletonDisposable>());



    [Test]
    public void TestMethod()
    {
      // Case 1
      //var element = driver.FindElement(By.Name("SuchElementNotExists"));

      // Case 2
      var manyElement = driver.FindElements(By.Name("SuchElementNotExists"));
    }
  }





























  public sealed class TransientDisposable : IDisposable
  {
    public void Dispose() => Console.WriteLine($"{nameof(TransientDisposable)}.Dispose()");
  }

  public sealed class ScopedDisposable : IDisposable
  {
    public void Dispose() => Console.WriteLine($"{nameof(ScopedDisposable)}.Dispose()");
  }

  public sealed class SingletonDisposable : IDisposable
  {
    public void Dispose() => Console.WriteLine($"{nameof(SingletonDisposable)}.Dispose()");
  }
}