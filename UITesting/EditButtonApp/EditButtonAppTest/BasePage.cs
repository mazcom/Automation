using OpenQA.Selenium.Appium.Windows;
using SeleniumExtras.PageObjects;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace EditButtonAppTest
{
  public class BasePage
  {
    private readonly WindowsDriver<WindowsElement> driver;

    public BasePage(WindowsDriver<WindowsElement> driver)
    {
      this.driver = driver;
    }

    public virtual void Open()
    {
      //PageFactory.InitElements(this.driver, this);
    }

  }
}
