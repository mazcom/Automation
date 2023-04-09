using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace ProducerConsumerConsoleApp.Models
{
  internal class WorkItem
  {
    /// <summary>
    ///     A func that defines how the coffee should be made
    /// </summary>
    //public Func<string, CancellationToken, ValueTask> Work { get; set; }
    public Action Work { get; set; }

    /// <summary>
    ///     For whom the coffee should be made
    /// </summary>
    //public string Customer { get; set; }
  }
}
