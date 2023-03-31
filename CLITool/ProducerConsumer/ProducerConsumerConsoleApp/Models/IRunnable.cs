using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace ProducerConsumerConsoleApp.Models
{
  internal interface IRunnable
  {
    void Run();
    IEnumerable<IRunnable> Children { get; }
  }
}
