using System;
using System.Collections;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Channels;
using System.Threading.Tasks;

namespace ProducerConsumerConsoleApp.Models
{
  internal class WorkItemsQueueService
  {
    //private readonly int maxDegreeOfParallelism;
    private readonly Channel<WorkItem> queue;

    public WorkItemsQueueService(int maxDegreeOfParallelism)
    {
      //this.maxDegreeOfParallelism = maxDegreeOfParallelism;
      var options = new BoundedChannelOptions(maxDegreeOfParallelism)
      {
        FullMode = BoundedChannelFullMode.Wait
      };

      queue = Channel.CreateBounded<WorkItem>(options);
    }


    public async ValueTask EnqueueAsync(WorkItem workItem)
    {
      if (workItem == null)
      {
        throw new ArgumentNullException(nameof(workItem));
      }

      await queue.Writer.WriteAsync(workItem);
    }

    //public ChannelsQueue()
    //{
    //  var channel = Channel.CreateUnbounded<Action>(new UnboundedChannelOptions() { SingleReader = true });
    //  var reader = channel.Reader;
    //  _writer = channel.Writer;

    //  Task.Run(async () =>
    //  {
    //    while (await reader.WaitToReadAsync())
    //    {
    //      // Fast loop around available jobs
    //      while (reader.TryRead(out var job))
    //      {
    //        job.Invoke();
    //      }
    //    }
    //  });
    //}

    // ProcessQueuedItems

    //public async ValueTask<WorkItem> DequeueAsync(CancellationToken cancellationToken)
    //{
    //  while (await queue.Reader.WaitToReadAsync())
    //  {
    //    var workItem = await queue.Reader.ReadAsync(cancellationToken);
    //    return workItem;
    //  }
    //  //return ValueTask.CompletedTask<WorkItem>;
    //  //var workItem = await queue.Reader.ReadAsync(cancellationToken);
    //  //return workItem;
    //}

  }
}
