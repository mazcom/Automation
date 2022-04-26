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
using Keys = OpenQA.Selenium.Keys;

namespace EditButtonAppTest
{
    public class Tests
    {
        private WindowsDriver<WindowsElement> driver;
        private EventFiringWebDriver eventFiringWebDriver;

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
            driver = new WindowsDriver<WindowsElement>(new Uri("http://127.0.0.1:4723"), options, TimeSpan.FromSeconds(1));
            eventFiringWebDriver = new EventFiringWebDriver(driver);
            eventFiringWebDriver.FindElementCompleted += EventFiringWebDriver_FindElementCompleted;  

            //driver = new WindowsDriver<WindowsElement>(options);
            //string sss = ((WindowsDriver<WindowsElement>) driver.WrappedDriver).Capabilities.ToString();
            //string sss = driver.Capabilities.ToString();

            Console.WriteLine("Driver is found...");

            //driver.Manage().Timeouts().ImplicitWait = TimeSpan.FromSeconds(60);
        }

        private void EventFiringWebDriver_FindElementCompleted(object sender, FindElementEventArgs e)
        {
            //throw new NotImplementedException();
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


        public static bool AreElementsPresent(WindowsDriver<WindowsElement> driver, By locator)
        {
            return driver.FindElements(locator).Count > 0;
        }

        public static bool IsElementPresent(WindowsDriver<WindowsElement> driver, By locator)
        {
            var restoreValue = driver.Manage().Timeouts().ImplicitWait;
            try
            {
                driver.Manage().Timeouts().ImplicitWait = TimeSpan.FromSeconds(10);
                return driver.FindElements(locator).Count > 0;
            }
            finally
            {
                driver.Manage().Timeouts().ImplicitWait = restoreValue;
            }
        }

        public static bool IsElementPresentNotPresent(WindowsDriver<WindowsElement> driver, By locator)
        {
            var restoreValue = driver.Manage().Timeouts().ImplicitWait;
            try
            {
                driver.Manage().Timeouts().ImplicitWait = TimeSpan.FromSeconds(0);
                return driver.FindElements(locator).Count > 0;
            }
            finally
            {
                driver.Manage().Timeouts().ImplicitWait = restoreValue;
            }
        }

        public static bool IsElementDisapointed(WindowsDriver<WindowsElement> driver, WindowsElement element)
        {
            try
            {
                var wait = new WebDriverWait(driver, TimeSpan.FromSeconds(10));
                //wait.IgnoreExceptionTypes(typeof(WebDriverTimeoutException));
                //wait.IgnoreExceptionTypes(typeof(WebDriverException));
                wait.Until(SeleniumExtras.WaitHelpers.ExpectedConditions.StalenessOf(element));
                return true;
            }
            catch (Exception ex)
            {
                return false;
            }
        }

        public static bool WaitUntilElementIsVisible(WindowsDriver<WindowsElement> driver, By locator)
        {
            try
            {
                var wait = new WebDriverWait(driver, TimeSpan.FromSeconds(10));
                wait.IgnoreExceptionTypes(typeof(WebDriverException));
                wait.Until(SeleniumExtras.WaitHelpers.ExpectedConditions.ElementIsVisible(locator));
                return true;
            }
            catch (Exception ex)
            {
                return false;
            }
        }

        public static bool WaitUntilElementIsNotVisible(WindowsDriver<WindowsElement> driver, By locator)
        {
            try
            {
                var wait = new WebDriverWait(driver, TimeSpan.FromSeconds(10));
                wait.IgnoreExceptionTypes(typeof(WebDriverException));
                //wait.Until(SeleniumExtras.WaitHelpers.ExpectedConditions.InvisibilityOfElementLocated);
                wait.Until(d => d.FindElements(locator).Count == 0);
                return true;
            }
            catch (Exception ex)
            {
                return false;
            }
        }

        [Test]
        public void TestMethod()
        {
            var isVisible = WaitUntilElementIsVisible(driver, By.Name("edText"));
        }

        public static bool IsElementPresentWait(WindowsDriver<WindowsElement> driver, By locator)
        {
            try
            {
                var wait = new WebDriverWait(driver, TimeSpan.FromSeconds(10));
                wait.IgnoreExceptionTypes(typeof(WebDriverException));
                wait.Until(driver => driver.FindElement(locator));
                //wait.Until(ExpectedConditions.StalenessOf(element));
                return true;
            }
            catch (WebDriverTimeoutException ex)
            {
                return false;
            }
        }

        public static bool IsElementPresent2(WindowsDriver<WindowsElement> driver, By locator)
        {
            try
            {
                driver.FindElement(locator);
                return true;
            }
            catch (NoSuchElementException)
            {
                return false;
            }
        }




        [Test]
        public void Test1()
        {
            Console.WriteLine("Start running Test1...");

            //driver.Manage().Timeouts().ImplicitWait = TimeSpan.FromSeconds(5);

            // var x = WaitUntilElementIsNotVisible(driver, By.Name("edText"));

            var edText = driver.FindElementByAccessibilityId("edText");
            //var edText = driver.FindElement(By.Name("Text:"));
            edText.SendKeys("Sample text");



            //new Actions(driver).dra

            //Thread.Sleep(4000);
            //var x = IsElementDisapointed(driver, edText);
            //Thread.Sleep(7000);


            var x2 = IsElementPresent(driver, By.Name("sdfsdfsdf"));

            //var x2 = AreElementsPresent(driver, By.Name("namersdsd"));

            //driver.FindElementByAccessibilityId("edText").SendKeys("Sample text");


            //var options = new AppiumOptions();
            //options.AddAdditionalCapability("app", "Root");
            //var DesktopSession = new WindowsDriver<WindowsElement>(new Uri("http://127.0.0.1:4723"), options);

            //edText.SendKeys(Keys.Shift + Keys.F10);
            //new Actions(driver).ContextClick(edText).Release().Perform();

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