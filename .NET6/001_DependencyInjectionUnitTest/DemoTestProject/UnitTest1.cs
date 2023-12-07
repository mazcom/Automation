using Microsoft.Extensions.DependencyInjection;
using Microsoft.Extensions.Logging;
using Microsoft.VisualStudio.TestPlatform.TestHost;

namespace DemoTestProject
{

  // Copyright (c) Microsoft Corporation. All rights reserved.
  // Licensed under the MIT license. See LICENSE file in the project root for full license information.

  using System;
  using System.Collections.Generic;
  using System.Diagnostics.CodeAnalysis;
    
  /// <summary>
  /// TestCategory attribute; used to specify the category of a unit test.
  /// </summary>
  [AttributeUsage(AttributeTargets.Method | AttributeTargets.Class | AttributeTargets.Assembly, AllowMultiple = true)]
  public sealed class TestFunctionalityAttribute : TestCategoryBaseAttribute
  {
    /// <summary>
    /// Initializes a new instance of the <see cref="TestCategoryAttribute"/> class and applies the category to the test.
    /// </summary>
    /// <param name="testCategory">
    /// The test Category.
    /// </param>
    public TestFunctionalityAttribute(string testCategory)
    {
      List<string> categories = new(1)
        {
            testCategory,
        };
      TestCategories = categories;
    }

    /// <summary>
    /// Gets the test categories that has been applied to the test.
    /// </summary>
    public override IList<string> TestCategories { get; }
  }

  [AttributeUsage(AttributeTargets.Method, AllowMultiple = true)]
  public class CustomTraitPropertyAttribute : TestPropertyAttribute
  {
    /// <summary>
    /// Initializes a new instance of the <see cref="CustomTraitPropertyAttribute"/> class.
    /// </summary>
    /// <param name="traitData">
    /// Data associated with trait property
    /// </param>
    public CustomTraitPropertyAttribute(int traitData)
        : base("CustomTraitProperty", traitData.ToString())
    {
    }
  }


  [TestClass]
  public class UnitTest1
  {
    public TestContext TestContext { get; set; }

    [TestMethod]
    [TestCategory("DemoCategory")]
    [TestCategory("ProductCategory")]
    [TestProperty("TestResult.DisplayName", "DC222222222222222")]
    [TestProperty("Severity", "1")]
    [TestFunctionality("MyTest")]
    [Description("DCaaaaaaaaaaaaaaaaa")]
    [CustomTraitProperty(22222)]
    public void TestMethod1()
    {
      

      //Console.WriteLine("Console write text");

      //TestContext.WriteLine("DC111111111111111");

      Manager.Host.Services.GetRequiredService<ILogger<Program>>().LogWarning($"ILogger from test method {nameof(TestMethod1)} hello");
    }
  }
}