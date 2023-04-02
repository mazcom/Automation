using System.Collections;

namespace ProducerConsumerConsoleApp.Models
{
  internal class RunnableQueue : IEnumerable<IRunnable>
  {
    private readonly IEnumerable<EnvironmentModel> environments;

    public RunnableQueue(IEnumerable<EnvironmentModel> environments)
    {
      this.environments = environments;
    }

    IEnumerator IEnumerable.GetEnumerator() => GetEnumerator();

    public IEnumerator<IRunnable> GetEnumerator()
    {
      var inProcessEnvironmets = new List<IRunnable>();

      foreach (var env in environments)
      {
        inProcessEnvironmets.Add(env);
        yield return env;

        if (TryGetTestsFromAnyDoneEnvironment(inProcessEnvironmets, out var tasks))
        {
          foreach (var task in tasks)
          {
            yield return task;
          }
        }
      }

      while (inProcessEnvironmets.Count > 0)
      {
        if (TryGetTestsFromAnyDoneEnvironment(inProcessEnvironmets, out var tasks))
        {
          foreach (var task in tasks)
          {
            yield return task;
          }
        }
        else
        {
          Thread.Sleep(100);
        }
      }
    }

    private bool TryGetTestsFromAnyDoneEnvironment(List<IRunnable> inProcessEnvironmets, out IEnumerable<IRunnable> tasks)
    {
      tasks = null!;

      if (inProcessEnvironmets.FirstOrDefault(i => i.IsDone) is IRunnable env)
      {
        tasks = env.Children;
        inProcessEnvironmets.Remove(env);
        return true;
      }

      return false;
    }
  }
}
