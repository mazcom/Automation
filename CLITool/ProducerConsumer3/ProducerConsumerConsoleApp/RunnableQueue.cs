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
        //foreach (var task in TryGetTestsFromAnyDoneEnvironment(inProcessEnvironmets))
        //{
        //  yield return task;
        //}

        //inProcessEnvironmets.Add(env);
        //yield return env;

        foreach (var task in TryGetTestsFromAnyDoneEnvironment(inProcessEnvironmets))
        {
          yield return task;
        }
      }

      while (inProcessEnvironmets.Count > 0)
      {
        foreach (var task in TryGetTestsFromAnyDoneEnvironment(inProcessEnvironmets))
        {
          yield return task;
        }
      }
    }

    private IEnumerable<IRunnable> TryGetTestsFromAnyDoneEnvironment(List<IRunnable> inProcessEnvironmets)
    {
      //var doneEnvironmets = inProcessEnvironmets.Where(i => i.IsDone).ToArray();

      //if (doneEnvironmets.Length > 0)
      //{
      //  foreach (var env in doneEnvironmets)
      //  {
      //    inProcessEnvironmets.Remove(env);
      //  }

      //  return doneEnvironmets.SelectMany(e => e.Children);
      //}

      return Enumerable.Empty<IRunnable>();
    }
  }
}
