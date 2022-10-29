using Microsoft.Extensions.Logging;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace DemoTestProject.Logic
{
  public interface ICalculationService
  {
    int AddTwoPositiveNumbers(int a, int b);
  }

  public class CalculationService : ICalculationService
  {
    private readonly ILogger<CalculationService> _logger;

    public CalculationService(ILogger<CalculationService> logger)
    {
      _logger = logger;
    }

    public int AddTwoPositiveNumbers(int a, int b)
    {
      if (a <= 0 || b <= 0)
      {
        _logger.LogError("Arguments should be both positive.");
        return 0;
      }
      _logger.LogInformation("Adding {a} and {b}", a, b);
      return a + b;
    }
  }
}
