using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace ProducerConsumerConsoleApp.Models
{
  internal class TestModel : IJob
  {
    public TestModel(string name)
    {
      Name = name;
    }
    
    public string Name { get; set; }
    public IEnumerable<IJob> Children => Enumerable.Empty<IJob>();

    public override string ToString() => Name;

    public bool IsDone { get; private set; }

    public void Run()
    {
      IsDone = true;
    }
  }
}
