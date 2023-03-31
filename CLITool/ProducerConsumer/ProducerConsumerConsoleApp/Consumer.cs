using ProducerConsumerConsoleApp.Models;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Channels;
using System.Threading.Tasks;

namespace ProducerConsumerConsoleApp
{
  internal class Consumer
  {
    private readonly ChannelReader<IRunnable> _channelReader;
    public Consumer(ChannelReader<IRunnable> channelReader)
    {
      _channelReader = channelReader;
    }
    public async Task ConsumeWorkAsync()
    {
      await foreach (var todo in _channelReader.ReadAllAsync())
      {
        todo.Run();
        Console.WriteLine($"Completing todo: {todo}");
        await Task.Delay(1000);
      }

      Console.WriteLine("All items read");
    }
  }
}
