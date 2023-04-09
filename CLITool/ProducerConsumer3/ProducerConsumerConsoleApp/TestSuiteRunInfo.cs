using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace ProducerConsumerConsoleApp
{
  public sealed class TestSuiteRunInfo
  {
    private static readonly object Sync = new object();

    public EnvironmentRunInfo EnvironmentRunInfo
    {
      get;
      internal set;
    }

    public int SuccessedTestCount
    {
      get;
      private set;
    }

    public int FailedTestCount
    {
      get;
      private set;
    }

    public int BrokenTestCount
    {
      get;
      private set;
    }

    public int SkippedTestCount
    {
      get;
      private set;
    }

    public TestStatus Status
    {
      get;
      private set;
    }

    internal void Append(TestRunInfo info)
    {
      //lock (Sync)
      //{
      //  Status |= info.Status;

      //  if (info.Status.HasFlag(TestStatus.Success))
      //  {
      //    SuccessedTestCount++;
      //  }
      //  else if (info.Status.HasFlag(TestStatus.Failed))
      //  {
      //    FailedTestCount++;
      //  }
      //  else if (info.Status.HasFlag(TestStatus.Bad))
      //  {
      //    BrokenTestCount++;
      //  }
      //  else
      //  {
      //    SkippedTestCount++;
      //  }
      //}
    }
  }
}
