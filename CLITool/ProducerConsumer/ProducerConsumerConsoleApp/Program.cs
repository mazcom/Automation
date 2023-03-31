// See https://aka.ms/new-console-template for more information
using ProducerConsumerConsoleApp;
using ProducerConsumerConsoleApp.Models;
using System.Diagnostics;
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

int messageLimit = 1;

var channel = Channel.CreateBounded<IRunnable>(new BoundedChannelOptions(messageLimit * 2)
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


var translateDocumentTasks = Enumerable
        .Range(0, messageLimit) // 7 translation tasks
        .Select(_ => Task.Run(async () =>
        {
          while (await channel.Reader.WaitToReadAsync())
          {
            while (channel.Reader.TryRead(out var data))
            {
              //data.Run();

              Console.WriteLine($"Start runnning: {data}");
              data.Run();
              //Random.Shared.Next(1000, 4000)
              await Task.Delay(3000);
              Console.WriteLine($"Completing runnning: {data}");
              
              //var document = await ReadAndTranslateDocument(documentId);
              //await savingChannel.Writer.WriteAsync(document);
            }
          }
        }))
        .ToArray();

await Task.WhenAll(translateDocumentTasks);

//Task[] consumers = new Task[messageLimit];
//for (int i = 0; i < consumers.Length; i++)
//{
//  consumers[i] = Task.Factory.StartNew(async () =>
//  {
//    while (await channel.Reader.WaitToReadAsync())
//    {
//      if (channel.Reader.TryRead(out
//              var data))
//      {
//        data.Run();
//        //Console.WriteLine($" Data read from Consumer No.{Task.CurrentId} is {data}");
//      }
//    }
//  });
//}

//Task.WaitAll(consumers);

Console.ReadKey();
//producer.Wait();
//Task.WaitAll(consumer);
//Console.ReadKey();





