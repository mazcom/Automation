using EditButtonAppTest.Pages.Environment;
using OpenQA.Selenium.Appium.Windows;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace EditButtonAppTest
{
  public class OptionsWindow
  {
    public OptionsWindow(WindowsDriver<WindowsElement> driver)
    {
      Environment = new(driver);
    }

    public Environment Environment { get; }
    //public E Environment { get; }
  }
}
