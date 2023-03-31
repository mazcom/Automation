using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace ProducerConsumerConsoleApp.Models
{
  internal class EnvironmentModel
  {
    public EnvironmentModel(string name)
    {
      Name = name;
    }

    public string Name { get; set; }
    public List<TestModel> Tests { get; set; } = new();
  }
}
