using Microsoft.Extensions.DependencyInjection;
using Microsoft.FeatureManagement;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace EditButtonAppTest
{
  public class Startup
  {
    public void ConfigureServices(IServiceCollection services)
    {
      services.AddFeatureManagement();
    }
  }
}
