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
  public class EnvironmentGeneralPage : BasePage
  {

    [FindsBy(How = How.Name, Using = "cbSkins")]
    private IWebElement cbSkins;

    [FindsBy(How = How.Id, Using = "chkMainFormSkin")]
    private IWebElement chkMainFormSkin;

    [FindsBy(How = How.Id, Using = "chkEnableMenuAnimation")]
    private IWebElement chkEnableMenuAnimation;

    public EnvironmentGeneralPage(WindowsDriver<WindowsElement> driver) : base(driver)
    {
    }

    public string CurrentSkinName => cbSkins.Text;

    public bool MainFormSkinChecked => chkMainFormSkin.Enabled;

    public bool EnableMenuAnimationChecked => chkEnableMenuAnimation.Enabled;

    public void ChangeSkinTo(string skinName)
    {
      // change skin code
    }
  }
}
