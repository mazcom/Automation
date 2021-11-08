using NUnit.Framework;
using OpenQA.Selenium.Appium;
using OpenQA.Selenium.Appium.Service;
using OpenQA.Selenium.Appium.Windows;
using OpenQA.Selenium.Remote;
using System;
using System.Threading;

namespace EditButtonAppTest
{
    public class Tests
    {
        private WindowsDriver<WindowsElement> driver;

        [SetUp]
        public void Setup()
        {
            var options = new AppiumOptions();
            options.AddAdditionalCapability("app", @"D:\Automation\SampleApps\EditButtonApp\EditButtonApp\bin\Debug\EditButtonApp.exe");
            options.AddAdditionalCapability("deviceName", "WindowsPC");
            driver = new WindowsDriver<WindowsElement>(new Uri("http://127.0.0.1:4723"), options);
            driver.Manage().Timeouts().ImplicitWait = TimeSpan.FromSeconds(5);
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

        [Test]
        public void Test1()
        {
            driver.FindElementByAccessibilityId("edText").SendKeys("Sample text");
            Thread.Sleep(TimeSpan.FromSeconds(1));
            driver.FindElementByAccessibilityId("btnClear").Click();
            Thread.Sleep(TimeSpan.FromSeconds(1));
            
            Assert.Pass();
        }

        [Test]
        public void Test2()
        {
            
            Assert.Pass();
        }
    }
}