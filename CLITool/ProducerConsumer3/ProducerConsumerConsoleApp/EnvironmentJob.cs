using ProducerConsumerConsoleApp.Models;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace ProducerConsumerConsoleApp
{
  internal class EnvironmentJob : BaseJob
  {
    private readonly EnvironmentModel environment;
    private readonly Func<EnvironmentRunInfo> func;

    public EnvironmentJob(TestRunner testRunner, EnvironmentModel environment, Func<EnvironmentRunInfo> func) : base(testRunner)
    {
      this.environment = environment;
      this.func = func;
    }

    //public Func<string, CancellationToken, ValueTask> HowToMakeCoffee { get; private set; }
    public EnvironmentRunInfo? RunInfo { get; private set; }

    public EnvironmentModel Environment => this.environment;

    protected override void RunInternal()
    {
      Console.WriteLine($"Start running {Environment.Name}");
      RunInfo = func();
      RunInfo.Status = EnvironmentStatus.BuildSuccess;
      Console.WriteLine($"End running {Environment.Name}");
    }
  }
}
