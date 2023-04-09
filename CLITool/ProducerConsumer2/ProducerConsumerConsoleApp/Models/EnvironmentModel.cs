using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace ProducerConsumerConsoleApp.Models
{
  internal class EnvironmentModel: IRunnable
  {
    public EnvironmentModel(string name)
    {
      Name = name;
    }

    public string Name { get; set; }
    public List<TestModel> Tests { get; set; } = new();

    private EnvironmentStatus Status
    {
      get;
      set;
    }

    public override string ToString() => Name;
    

    public IEnumerable<IRunnable> Children
    {
      get
      {
        // add exception if environment was not run.
        if (Name.Contains("2"))
        {
          return Enumerable.Empty<IRunnable>();
        }

        if (Status == EnvironmentStatus.BuildSuccess)
        {
          return Tests;
        }

        return Enumerable.Empty<IRunnable>();
      }
    }

    public bool IsDone { get; private set; }

    public void Run()
    {
      //Console.WriteLine($"Runnable with Name {Name} was run");
      Status = EnvironmentStatus.BuildSuccess;
      IsDone = true;
    }
  }
}
