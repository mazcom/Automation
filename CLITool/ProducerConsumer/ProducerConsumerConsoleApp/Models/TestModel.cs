using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace ProducerConsumerConsoleApp.Models
{
  internal class TestModel : IRunnable
  {
    public TestModel(string name)
    {
      Name = name;
    }
    
    public string Name { get; set; }
    public IEnumerable<IRunnable> Children => Enumerable.Empty<IRunnable>();

    public void Run()
    {
      Console.WriteLine($"Runnable with Name {Name} was run");
    }
  }
}
