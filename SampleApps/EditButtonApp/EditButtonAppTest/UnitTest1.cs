using NUnit.Framework;
using OpenQA.Selenium;
using OpenQA.Selenium.Appium;
using OpenQA.Selenium.Appium.Service;
using OpenQA.Selenium.Appium.Windows;
using OpenQA.Selenium.Interactions;
using OpenQA.Selenium.Remote;
using System;
using System.IO;
using System.Threading;
using Keys = OpenQA.Selenium.Keys;

namespace EditButtonAppTest
{
    public class Tests
    {
        private WindowsDriver<WindowsElement> driver;

        [SetUp]
        public void Setup()
        {
            Console.WriteLine("Start running testst...");


            //string[] folders = System.IO.Directory.GetDirectories(@"C:\Program Files (x86)", "*", System.IO.SearchOption.AllDirectories);
            //foreach (string folder in folders)
            //{
            //    Console.WriteLine(folder);
            //}


            var options = new AppiumOptions();
         
            options.AddAdditionalCapability("app", @"D:\MyProjects\Automation\SampleApps\EditButtonApp\EditButtonApp\bin\Debug\EditButtonApp.exe");
            //options.AddAdditionalCapability("app", @"D:\Automation\Automation\SampleApps\EditButtonApp\EditButtonApp\bin\Debug\EditButtonApp.exe");
            //options.AddAdditionalCapability("app", @"C:\app\EditButtonApp\bin\Debug\EditButtonApp.exe");
            //options.AddAdditionalCapability("platformName", "Windows");
            options.AddAdditionalCapability("deviceName", "WindowsPC");
            //options.AddAdditionalCapability("ms:waitForAppLaunch", "50");

            //var fileName = @"C:\app\EditButtonApp\bin\Debug\EditButtonApp.exe";
            //if (File.Exists(fileName))
            //{
            //    Console.WriteLine($"File ${fileName} exists");
            //}
            //else
            //{
            //    Console.WriteLine("File ${fileName} DOES NOT exists");
            //}

            Console.WriteLine("Trying to find a driver...");
            driver = new WindowsDriver<WindowsElement>(new Uri("http://127.0.0.1:4723"), options, TimeSpan.FromSeconds(5));
            Console.WriteLine("Driver is found...");

            //driver.Manage().Timeouts().ImplicitWait = TimeSpan.FromSeconds(60);
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
            Console.WriteLine("Start running Test1...");

            var edText = driver.FindElementByAccessibilityId("edText");
            edText.SendKeys("Sample text");
            //driver.FindElementByAccessibilityId("edText").SendKeys("Sample text");


            // Создаём Desctop сессию в которой и ищем наше context menu. 
            //var options = new AppiumOptions();
            //options.AddAdditionalCapability("app", "Root");
            //var DesktopSession = new WindowsDriver<WindowsElement>(new Uri("http://127.0.0.1:4723"), options);

            //edText.SendKeys(Keys.Shift + Keys.F10);
            //new Actions(driver).ContextClick(edText).Release().Perform();

            // Рабочий вариант
            //Actions actions = new Actions(driver);
            //actions.ContextClick().SendKeys(Keys.Shift + Keys.F10).Build().Perform();

            //var currDataInClipBoard = Clipboard.GetText(TextDataFormat.Text);

            //Clipboard.SetText("blabla", TextDataFormat.Text);

            //Actions actions = new Actions(driver);
            //actions.MoveToElement(edText).KeyDown(Keys.Shift).KeyDown(Keys.Insert).KeyUp(Keys.Shift).Build().Perform();

            

            //Clipboard.SetText(currDataInClipBoard, TextDataFormat.Text);


            //System.Threading.Thread.Sleep(2000000);


            //Actions action = new Actions(driver).ContextClick(edText);
            //action.Build().Perform();


            //var menu = DesktopSession.FindElementByName("Context");
            //var suMenu =  menu.FindElement(By.Name("Item 1"));
            //suMenu.Click();


            ////Thread.Sleep(TimeSpan.FromSeconds(100000));
            //driver.FindElementByAccessibilityId("btnClear").Click();
            //Thread.Sleep(TimeSpan.FromSeconds(1));

            //driver.FindElementByAccessibilityId("edText").SendKeys("Sample text");


            Assert.IsTrue(true);
        }

        [Test]
        public void Test2()
        {

            Assert.IsTrue(true);
        }
    }
}