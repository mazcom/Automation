using System;
using System.Collections;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using System.Xml.Linq;

namespace ProducerConsumerConsoleApp.Models
{
  internal class RunnableQueue : IEnumerable<IRunnable>
  {
    private readonly IEnumerable<EnvironmentModel> environments;

    public RunnableQueue(IEnumerable<EnvironmentModel> environments) 
    {
      this.environments = environments;
    }

    IEnumerator IEnumerable.GetEnumerator()  => GetEnumerator();

    public IEnumerator<IRunnable> GetEnumerator()
    {
      var queue = new Queue<IRunnable>();

      foreach (var env in environments)
      {
        queue.Enqueue(env);

        while (queue.Any())
        {
          yield return queue.Dequeue();

          foreach (var child in env.Tests)
          {
            yield return child;
            //queue.Enqueue(child);
          }
        }
      }
    }
  }
}
