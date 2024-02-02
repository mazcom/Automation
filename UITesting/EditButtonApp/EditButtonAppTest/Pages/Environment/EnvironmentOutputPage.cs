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
  public class EnvironmentOutputPage : BasePage
  {

    [FindsBy(How = How.Id, Using = "chkShowEventTime")]
    private IWebElement chkShowEventTime;

    [FindsBy(How = How.Id, Using = "chkWriteToSqlLog")]
    private IWebElement chkWriteToSqlLog;

    public EnvironmentOutputPage(WindowsDriver<WindowsElement> driver) : base(driver)
    {
    }

    public bool ShowEventTimeChecked => chkShowEventTime.Enabled;

    public bool WriteToSqlLogChecked => chkWriteToSqlLog.Enabled;

  }
}
