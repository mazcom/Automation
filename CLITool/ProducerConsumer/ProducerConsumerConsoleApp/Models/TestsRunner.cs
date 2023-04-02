using System;
using System.Collections.Generic;
using System.Diagnostics;
using System.Linq;
using System.Text;
using System.Threading.Channels;
using System.Threading.Tasks;

namespace ProducerConsumerConsoleApp.Models
{
  internal class TestsRunner
  {

    public async Task StartAsync(RunnableQueue runnables, CancellationToken cancellationToken, int parallelDegree)
    {

      var channel = Channel.CreateBounded<IRunnable>(new BoundedChannelOptions(parallelDegree)
      {
        FullMode = BoundedChannelFullMode.Wait
      });

      var saveDocumentTask = Task.Run(async () =>
      {
        foreach (var todo in runnables)
        {
          await channel.Writer.WriteAsync(todo);
        }

        channel.Writer.Complete();
      });


      var tasks = Enumerable
        .Range(0, parallelDegree)
        .Select(_ => Task.Run(async () =>
        {
          while (await channel.Reader.WaitToReadAsync())
          {
            while (channel.Reader.TryRead(out var data))
            {
              Console.WriteLine($"Start runnning: {data}");
              data.Run();
              await Task.Delay(Random.Shared.Next(1000, 4000));
              Console.WriteLine($"Completing runnning: {data}");
            }
          }
        }))
        .ToArray();

      await Task.WhenAll(tasks);
    }
  }
}
