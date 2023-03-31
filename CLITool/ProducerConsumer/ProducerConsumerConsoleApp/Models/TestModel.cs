using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace ProducerConsumerConsoleApp.Models
{
  internal class TestModel
  {
    public TestModel(string name)
    {
      Name = name;
    }

    public string Name { get; set; }
  }
}
