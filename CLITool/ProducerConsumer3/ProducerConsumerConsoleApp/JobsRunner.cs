using System;
using System.Collections.Generic;
using System.Diagnostics;
using System.Linq;
using System.Text;
using System.Threading.Channels;
using System.Threading.Tasks;

namespace ProducerConsumerConsoleApp
{
  internal class JobsRunner
  {
    private readonly JobsProvider jobsProvider;
    private readonly int maxDegreeOfParallelism;
    private Channel<IRunnable> channel;

    public JobsRunner(int maxDegreeOfParallelism)
    {
      this.maxDegreeOfParallelism = maxDegreeOfParallelism;
      this.channel = Channel.CreateBounded<IRunnable>(new BoundedChannelOptions(1)
      {
        FullMode = BoundedChannelFullMode.Wait
      });
    }

    public void Complete()
    {
      channel.Writer.Complete();
    }

    public async Task AddJob(IRunnable job)
    {
      await channel.Writer.WriteAsync(job);
    }

    public Task Start()
    {
      //var saveDocumentTask = Task.Run(async () =>
      //{
      //  var job = await this.jobsProvider.NextAsync();
      //  await channel.Writer.WriteAsync(job);
      //});

      var task = Task.Run(async () =>
      {

        var tasks = Enumerable.Range(0, maxDegreeOfParallelism).Select(_ => Task.Run(async () =>
          {
            while (await channel.Reader.WaitToReadAsync())
            {
              while (channel.Reader.TryRead(out var job))
              {
                //Console.WriteLine($"Start runnning: {job}");
                // сдедать Job disposable.
                // тогда в методе dispose освобождаем ресурсы. 
                job.Run();
                //channel.Writer.TryWrite(new EnvironmentModel("111"));

                //await Task.Delay(Random.Shared.Next(1000, 4000));
                //Console.WriteLine($"Completing runnning: {job}");
              }
            }
          }))
          .ToArray();

        await Task.WhenAll(tasks);
      }
      );

      return task;
    }

    //public async Task StartAsync(RunnableQueue runnables, CancellationToken cancellationToken, int parallelDegree)
    //{

    //  //BackgroundService

    //  var channel = Channel.CreateBounded<IRunnable>(new BoundedChannelOptions(parallelDegree)
    //  {
    //    FullMode = BoundedChannelFullMode.Wait
    //  });

    //  var saveDocumentTask = Task.Run(async () =>
    //  {


    //    foreach (var todo in runnables)
    //    {
    //      await channel.Writer.WriteAsync(todo);
    //    }

    //    channel.Writer.Complete();
    //  });


    //  var tasks = Enumerable.Range(0, parallelDegree).Select(_ => Task.Run(async () =>
    //    {
    //      while (await channel.Reader.WaitToReadAsync())
    //      {
    //        while (channel.Reader.TryRead(out var data))
    //        {
    //          Console.WriteLine($"Start runnning: {data}");
    //          data.Run();
    //          //channel.Writer.TryWrite(new EnvironmentModel("111"));

    //          await Task.Delay(Random.Shared.Next(1000, 4000));
    //          Console.WriteLine($"Completing runnning: {data}");
    //        }
    //      }
    //    }))
    //    .ToArray();

    //  await Task.WhenAll(tasks);
    //}


  }
}
