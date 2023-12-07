using System;
using System.Collections.Generic;

// Class representing an option on a page
public class UITestOption
{
  public string DisplayName { get; set; }
  public string Locator { get; set; } // Locator for finding the UI element
  public UITestPage AssociatedPage { get; set; } // Page associated with this option
  public List<UITestOption> SubOptions { get; set; } = new List<UITestOption>(); // Sub-options for this option

  public UITestOption(string displayName, string locator, UITestPage associatedPage)
  {
    DisplayName = displayName;
    Locator = locator;
    AssociatedPage = associatedPage;
  }
}

// Class representing a page with options
public class UITestPage
{
  public string PageName { get; set; }
  public List<UITestOption> Options { get; set; } = new List<UITestOption>();

  public UITestPage(string pageName)
  {
    PageName = pageName;
  }
}

// Class representing the structure of the options tree for UI testing
public class UITestTree
{
  public List<UITestPage> Pages { get; set; } = new List<UITestPage>();
}

// Class for comparing the standard tree with the actual tree
public class UITestTreeComparer
{
  public bool CompareTrees(UITestTree standardTree, UITestTree actualTree)
  {
    // Compare each page and option
    foreach (var standardPage in standardTree.Pages)
    {
      var actualPage = FindPage(actualTree, standardPage.PageName);
      if (actualPage == null)
      {
        Console.WriteLine($"Page {standardPage.PageName} not found in the actual tree.");
        return false;
      }

      if (!CompareOptions(standardPage.Options, actualPage.Options))
      {
        return false;
      }
    }

    return true;
  }

  private UITestPage FindPage(UITestTree tree, string pageName)
  {
    foreach (var page in tree.Pages)
    {
      if (page.PageName == pageName)
      {
        return page;
      }
    }

    return null;
  }

  private bool CompareOptions(List<UITestOption> standardOptions, List<UITestOption> actualOptions)
  {
    if (standardOptions.Count != actualOptions.Count)
    {
      Console.WriteLine("Number of options is not equal.");
      return false;
    }

    for (int i = 0; i < standardOptions.Count; i++)
    {
      var standardOption = standardOptions[i];
      var actualOption = actualOptions[i];

      if (standardOption.DisplayName != actualOption.DisplayName ||
          standardOption.Locator != actualOption.Locator)
      {
        Console.WriteLine($"Option {standardOption.DisplayName} does not match the expected values.");
        return false;
      }

      if (!CompareOptions(standardOption.SubOptions, actualOption.SubOptions))
      {
        return false;
      }
    }

    return true;
  }
}

class Program
{
  static void Main()
  {
    // Example usage:

    // Create the standard tree
    var standardTree = new UITestTree();

    var homePageStandard = new UITestPage("Home");
    var loginOptionStandard = new UITestOption("Login", "id=loginButton", homePageStandard);
    var settingsPageStandard = new UITestPage("Settings");
    var generalOptionStandard = new UITestOption("General", "id=generalSettings", settingsPageStandard);

    homePageStandard.Options.Add(loginOptionStandard);
    settingsPageStandard.Options.Add(generalOptionStandard);

    standardTree.Pages.Add(homePageStandard);
    standardTree.Pages.Add(settingsPageStandard);

    // Create the actual tree
    var actualTree = new UITestTree();

    var homePageActual = new UITestPage("Home");
    var loginOptionActual = new UITestOption("Login", "id=loginButton", homePageActual);
    var settingsPageActual = new UITestPage("Settings");
    var generalOptionActual = new UITestOption("General", "id=generalSettings", settingsPageActual);

    homePageActual.Options.Add(loginOptionActual);
    settingsPageActual.Options.Add(generalOptionActual);

    actualTree.Pages.Add(homePageActual);
    actualTree.Pages.Add(settingsPageActual);

    // Compare the trees
    var treeComparer = new UITestTreeComparer();
    bool result = treeComparer.CompareTrees(standardTree, actualTree);

    Console.WriteLine("Trees are equal: " + result);
  }
}