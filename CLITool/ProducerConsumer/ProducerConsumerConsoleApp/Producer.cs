using ProducerConsumerConsoleApp.Models;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Channels;
using System.Threading.Tasks;

namespace ProducerConsumerConsoleApp
{
  internal class Producer
  {
    //private static readonly List<string> _todoItems = new()
    //{
    //    "Make a coffee",
    //    "Read CodeMaze articles",
    //    "Go for a run",
    //    "Eat lunch"
    //};
    private readonly ChannelWriter<IRunnable> _channelWriter;
    private readonly RunnableQueue runnables;

    public Producer(ChannelWriter<IRunnable> channelWriter, RunnableQueue runnables )
    {
      _channelWriter = channelWriter;
      this.runnables = runnables;
    }
    public async Task ProduceWorkAsync()
    {
      foreach (var todo in runnables)
      {
        await _channelWriter.WriteAsync(todo);
        Console.WriteLine($"Added todo: '{todo}' to channel");
        await Task.Delay(500);
      }
      _channelWriter.Complete();
    }
  }
}
