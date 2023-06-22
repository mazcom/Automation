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
    private readonly int maxDegreeOfParallelism;
    private Channel<IJob> channel;

    public JobsRunner(int maxDegreeOfParallelism)
    {
      this.maxDegreeOfParallelism = maxDegreeOfParallelism;
      this.channel = Channel.CreateBounded<IJob>(new BoundedChannelOptions(1)
      {
        FullMode = BoundedChannelFullMode.Wait
      });
    }

    public void Complete()
    {
      channel.Writer.Complete();
    }

    public async Task AddJob(IJob job)
    {
      await channel.Writer.WriteAsync(job);
    }

    public Task Start()
    {
      var task = Task.Run(async () =>
      {
        var tasks = Enumerable.Range(0, maxDegreeOfParallelism).Select(_ => Task.Run(async () =>
          {
            while (await channel.Reader.WaitToReadAsync())
            {
              while (channel.Reader.TryRead(out var job))
              {
                job.Run();
              }
            }
          }))
          .ToArray();

        await Task.WhenAll(tasks);
      }
      );

      return task;
    }
  }
}
