using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace ProducerConsumerConsoleApp
{
  internal abstract class BaseJob : IJob
  {
    private readonly TestRunner testRunner;

    public BaseJob(TestRunner testRunner)
    {
      this.testRunner = testRunner;
    }

    public void Run()
    {
      try
      {
        RunInternal();
      }
      finally 
      {
        testRunner.Done(this);
      }
    }

    protected abstract void RunInternal();
    
  }
}
