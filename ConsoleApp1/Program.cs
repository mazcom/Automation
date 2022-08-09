using Microsoft.Extensions.DependencyInjection;
using Microsoft.Extensions.Hosting;
using System.Diagnostics;
using System.Runtime.InteropServices;

class Program
{
  static async Task Main(string[] args)
  {
    using IHost host = CreateHostBuilder(args).Build();

    ExemplifyDisposableScoping(host.Services, "Scope 1");
    Console.WriteLine();

    ExemplifyDisposableScoping(host.Services, "Scope 2");
    Console.WriteLine();

    await host.RunAsync();
  }

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
