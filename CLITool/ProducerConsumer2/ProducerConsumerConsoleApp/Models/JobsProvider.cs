using System;
using System.Collections;
using System.Collections.Concurrent;
using System.Collections.Generic;
using System.Linq;
using System.Runtime.CompilerServices;
using System.Text;
using System.Threading.Tasks;

namespace ProducerConsumerConsoleApp.Models
{
  internal class JobsProvider
  {
    private readonly int maxDegreeOfParallelism = 0;

    private ConcurrentDictionary<IRunnable, object> inProcessJobs;

    public JobsProvider(int maxDegreeOfParallelism)
    {
      this.maxDegreeOfParallelism = maxDegreeOfParallelism;
    }

    //public IRunnable Next()
    //{
    //  // Job Factory
    //  inProcessJobs.TryAdd(null!, null!);
    //  return null;
    //}

    public async Task<IRunnable> NextAsync()
    {
      await WaitUntilJobAwailable();
      // Job Factory
      inProcessJobs.TryAdd(null!, null!);
      return null;
    }

    //public bool WaitUntilJobAwailable()
    //{
    //  return true;
    //}

    public async Task WaitUntilJobAwailable(CancellationToken ct = default)
    {
      Func<bool> condition = () => true;
      try
      {
        //while (condition())
        while (inProcessJobs.Count >= this.maxDegreeOfParallelism)
        {
          await Task.Delay(25, ct).ConfigureAwait(false);
        }
      }
      catch (TaskCanceledException)
      {
        // ignore: Task.Delay throws this exception when ct.IsCancellationRequested = true
        // In this case, we only want to stop polling and finish this async Task.
      }
    }

    public void Done(IRunnable job)
    {
      bool result = inProcessJobs.TryRemove(job, out var removedItem);
      
      //return null;
    }

  }
}
