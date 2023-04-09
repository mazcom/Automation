using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace ProducerConsumerConsoleApp
{
  internal interface IRunnable
  {
    void Run();
    //IEnumerable<IRunnable> Children { get; }

    //bool IsDone { get; }
  }
}
