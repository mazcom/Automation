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

    public override string ToString() => Name;

    public void Run()
    {
      //Console.WriteLine($"Start runnning: {data}");
      //Thread.Sleep(3000);
      ////await Task.Delay(3000);
      //Console.WriteLine($"Completing runnning: {data}");

      //Console.WriteLine($"Runnable with Name {Name} was run");
    }
  }
}
