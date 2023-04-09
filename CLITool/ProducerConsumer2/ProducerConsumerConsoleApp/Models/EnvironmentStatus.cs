using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace ProducerConsumerConsoleApp.Models
{
  [Flags]
  public enum EnvironmentStatus
  {
    None = 0,
    Stopped = 1,
    NotFound = 2,
    BuildSuccess = 4,
    BuildFailure = 8,
    CleanupSuccess = 16,
    CleanupFailure = 32,
    Bad = 64,
    BuildStarted = 128,
    CleanupStarted = 256,
    StartClosing = 512,
    Closed = 1024
  }
}
