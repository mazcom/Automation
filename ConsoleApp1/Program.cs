using Microsoft.Extensions.DependencyInjection;
using Microsoft.Extensions.Hosting;
using System.Collections.Concurrent;
using System.Diagnostics;
using System.Runtime.InteropServices;

class Program
{
  static void Main(string[] args)
  {
    // Create some random data to process in parallel.
    // There is a good probability this data will cause some exceptions to be thrown.
    byte[] data = new byte[1000];
    Random r = Random.Shared;
    r.NextBytes(data);

    // ProcessDataInParallel(data);

    try
    {
      ProcessDataInParallel(data);
    }
    catch (AggregateException ae)
    {
      var ignoredExceptions = new List<Exception>();
      // This is where you can choose which exceptions to handle.
      foreach (var ex in ae.Flatten().InnerExceptions)
      {
        Console.WriteLine(ex.Message);
        //if (ex is ArgumentException) Console.WriteLine(ex.Message);
        //else ignoredExceptions.Add(ex);
      }
      //if (ignoredExceptions.Count > 0)
      //{
      //  throw new AggregateException(ignoredExceptions);
      //}
    }
    //catch (Exception ae)
    //{
    //}

    Console.WriteLine("Press any key to exit.");
    //Console.ReadKey();
  }

  private static void ProcessDataInParallel(byte[] data)
  {
    // Use ConcurrentQueue to enable safe enqueueing from multiple threads.
    var exceptions = new ConcurrentQueue<Exception>();

    // Execute the complete loop and capture all exceptions.
    Parallel.ForEach(data, d =>
    {
      try
      {
        // Cause a few exceptions, but not too many.
        if (d < 3) throw new ArgumentException($"Value is {d}. Value must be greater than or equal to 3.");
        else Console.Write(d + " ");
      }
      // Store the exception and continue with the loop.
      catch (Exception e)
      {
        exceptions.Enqueue(e);
      }
    });
    Console.WriteLine();

    // Throw the exceptions here after the loop completes.
    if (!exceptions.IsEmpty)
    {
      throw new AggregateException(exceptions);
    }
  }

  //static async Task Main(string[] args)
  //{
  //  using IHost host = CreateHostBuilder(args).Build();

  //  ExemplifyDisposableScoping(host.Services, "Scope 1");
  //  Console.WriteLine();

  //  ExemplifyDisposableScoping(host.Services, "Scope 2");
  //  Console.WriteLine();

  //  await host.RunAsync();
  //}

  static IHostBuilder CreateHostBuilder(string[] args) =>
      Host.CreateDefaultBuilder(args)
          .ConfigureServices((_, services) =>
              services.AddTransient<TransientDisposable>()
                      .AddScoped<ScopedDisposable>()
                      .AddSingleton<SingletonDisposable>());

  static void ExemplifyDisposableScoping(IServiceProvider services, string scope)
  {
    Console.WriteLine($"{scope}...");

    //using IServiceScope serviceScope = services.CreateScope();
    using (IServiceScope serviceScope = services.CreateScope())
    {
      IServiceProvider provider = serviceScope.ServiceProvider;

      _ = provider.GetRequiredService<TransientDisposable>();
      _ = provider.GetRequiredService<ScopedDisposable>();
      _ = provider.GetRequiredService<SingletonDisposable>();
    }
  }
}

public sealed class TransientDisposable : IDisposable
{
  public void Dispose() => Console.WriteLine($"{nameof(TransientDisposable)}.Dispose()");
}

public sealed class ScopedDisposable : IDisposable
{
  public void Dispose() => Console.WriteLine($"{nameof(ScopedDisposable)}.Dispose()");
}

public sealed class SingletonDisposable : IDisposable
{
  public void Dispose() => Console.WriteLine($"{nameof(SingletonDisposable)}.Dispose()");
}
