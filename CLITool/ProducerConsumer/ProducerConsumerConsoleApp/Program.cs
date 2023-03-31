// See https://aka.ms/new-console-template for more information
using ProducerConsumerConsoleApp;
using ProducerConsumerConsoleApp.Models;
using System.Threading.Channels;

Console.WriteLine("Hello, World!");


var environmets = new List<EnvironmentModel>()
{
  new EnvironmentModel("Env1") {
    Tests = new List<TestModel>
      {
        new TestModel("SC1"),
        new TestModel("SC2"),
        new TestModel("SC3")
      }},
  new EnvironmentModel("Env2") {
    Tests = new List<TestModel>
      {
        new TestModel("Datagenerator1"),
        new TestModel("Datagenerator2")
      }},
  new EnvironmentModel("Env3") {
    Tests = new List<TestModel>
      {
        new TestModel("Import1")
      }},
};


//var boundedChannel = Channel.CreateBounded<string>(new BoundedChannelOptions(1)
//{
//  FullMode = BoundedChannelFullMode.Wait
//});

int messageLimit = 3;

var channel = Channel.CreateBounded<IRunnable>(new BoundedChannelOptions(messageLimit)
{
  FullMode = BoundedChannelFullMode.Wait
});


var producer = new Producer( channel.Writer, new RunnableQueue(environmets));
var consumer = new Consumer(channel.Reader);

_ = Task.Factory.StartNew(async () => await producer.ProduceWorkAsync());

//await consumer.ConsumeWorkAsync();

//Console.ReadKey();


//Task producer = Task.Factory.StartNew(() =>
//{
//  foreach (var runnable in environmets)
//  {
//    channel.Writer.TryWrite(runnable);
//  }
//  channel.Writer.Complete();
//});



Task[] consumers = new Task[messageLimit];
for (int i = 0; i < consumers.Length; i++)
{
  consumers[i] = Task.Factory.StartNew(async () =>
  {
    while (await channel.Reader.WaitToReadAsync())
    {
      if (channel.Reader.TryRead(out
              var data))
      {
        data.Run();
        //Console.WriteLine($" Data read from Consumer No.{Task.CurrentId} is {data}");
      }
    }
  });
}

Task.WaitAll(consumers);

Console.ReadKey();
//producer.Wait();
//Task.WaitAll(consumer);
//Console.ReadKey();





