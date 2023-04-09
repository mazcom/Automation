using ProducerConsumerConsoleApp.Models;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace ProducerConsumerConsoleApp
{
  internal class TestJob : BaseJob
  {
    private readonly TestModel testModel;
    private readonly Func<TestRunInfo> func;

    public TestJob(TestRunner testRunner, TestModel testModel, Func<TestRunInfo> func) : base(testRunner)
    {
      this.testModel = testModel;
      this.func = func;
    }

    public TestRunInfo? RunInfo { get; private set; }

    public TestModel TestModel => this.testModel;

    protected override void RunInternal()
    {
      Console.WriteLine($"Start running {TestModel.Name}");
      RunInfo = func();
      RunInfo.Status = TestStatus.Success;
      Console.WriteLine($"End running {TestModel.Name}");
    }
  }
}
