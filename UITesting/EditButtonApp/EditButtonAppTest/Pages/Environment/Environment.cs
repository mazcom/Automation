using OpenQA.Selenium;
using OpenQA.Selenium.Appium.Windows;
using SeleniumExtras.PageObjects;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace EditButtonAppTest.Pages.Environment
{
  public class Environment : BasePage
  {

    public Environment(WindowsDriver<WindowsElement> driver) : base(driver)
    {
      EnvironmentGeneralPage = new(driver);
      EnvironmentOutputPage = new(driver);
    }

    public EnvironmentGeneralPage EnvironmentGeneralPage { get; }

    public EnvironmentOutputPage EnvironmentOutputPage { get; }

  }
}
