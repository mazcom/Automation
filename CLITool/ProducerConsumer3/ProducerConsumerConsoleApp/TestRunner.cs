using ProducerConsumerConsoleApp.Models;
using System;
using System.Collections;
using System.Collections.Concurrent;
using System.Collections.Generic;
using System.Diagnostics;
using System.Linq;
using System.Text;
using System.Threading;
using System.Threading.Channels;
using System.Threading.Tasks;

namespace ProducerConsumerConsoleApp
{
  internal class TestRunner
  {
    private readonly IEnumerable<EnvironmentModel> environments;
    private readonly int maxDegreeOfParallelism;

    private ConcurrentDictionary<IRunnable, IRunnable> inProcessJobs = new();

    private ConcurrentQueue<IRunnable> toRunJobs;

    private IEnumerator<EnvironmentModel> suitesEnumerator;
    private JobsRunner jobsRunner;

    public TestRunner(IEnumerable<EnvironmentModel> environments, int maxDegreeOfParallelism)
    {
      this.environments = environments;
      this.maxDegreeOfParallelism = maxDegreeOfParallelism;

      // lazy
      this.suitesEnumerator = environments.GetEnumerator();
      this.toRunJobs = new ConcurrentQueue<IRunnable>();
      this.jobsRunner = new JobsRunner(maxDegreeOfParallelism);
    }



    public async Task Run()
    {


      //var actions = new List<Action>();
      //var funcs = new List<Func<TestSuiteRunInfo>>();

      //while (suitesEnumerator.MoveNext())
      //{
      //  var testSuite = suitesEnumerator.Current;

      //  //TestSuiteRunInfo result = this.testSuiteManager.Run(testsRunOptions, testSuite, cancellationToken, this.progress);
      //  //funcs.Add(() => this.testSuiteManager.Run(testsRunOptions, testSuite, cancellationToken, this.progress));

      //  //Action line = () => funcs[0]();

      //  //var s = funcs[0]();
      //  //actions.Add(() => this.testSuiteManager.Run(testsRunOptions, testSuite, cancellationToken, this.progress));
      //}

      var task = Task.Run(async () =>
      {
        var consumerTask = this.jobsRunner.Start();

        while (true)
        {
          await WaitUntilJobAwailable();

          if (this.toRunJobs.Count > 0)
          {
            if (this.toRunJobs.TryDequeue(out var job))
            {
              inProcessJobs.TryAdd(job, job);
              await this.jobsRunner.AddJob(job);
              continue;
            }
          }

          bool has = suitesEnumerator.MoveNext();
          if (has)
          {
            Func<EnvironmentRunInfo> func = () => new EnvironmentRunInfo();

            var environmentModel = suitesEnumerator.Current;
            var envJob = new EnvironmentJob(this, environmentModel, func);
            inProcessJobs.TryAdd(envJob, envJob);
            await this.jobsRunner.AddJob(envJob);
            continue;
          }

          if (!has && inProcessJobs.Count == 0 && this.toRunJobs.Count == 0)
          {
            this.jobsRunner.Complete();
            await consumerTask;
            break;
          }
        }

        //while (suitesEnumerator.MoveNext())
        //{
        //  var testSuite = suitesEnumerator.Current;
        //}

        //var job = await this.jobsProvider.NextAsync();
        //await channel.Writer.WriteAsync(job);
      });

      await task;
    }

    public void Done(IRunnable job)
    {
      switch (job)
      {
        case EnvironmentJob envJob:
          OnEnvironmentJobDone(envJob);
          break;

        case TestJob testJob: // Type pattern with discard (_)
          OnTestJobDone(testJob);
          break;

        default:
          //favoriteTask = "Listen to music";
          break;
      }

      inProcessJobs.TryRemove(job, out var _);
    }

    private void OnEnvironmentJobDone(EnvironmentJob envJob)
    {
      if (envJob.RunInfo!.Status.HasFlag(EnvironmentStatus.BuildSuccess))
      {
        foreach (var test in envJob.Environment.Tests)
        {
          Func<TestRunInfo> func = () => new TestRunInfo();
          var testJob = new TestJob(this, test, func);
          toRunJobs.Enqueue(testJob);
        }
      }

      inProcessJobs.Remove(envJob, out var _);
    }

    private void OnTestJobDone(TestJob testJob)
    {
      inProcessJobs.Remove(testJob, out var _);
    }

    private async Task WaitUntilJobAwailable(CancellationToken ct = default)
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


  }
}
