﻿using ProducerConsumerConsoleApp.Models;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace ProducerConsumerConsoleApp
{
  internal class EnvironmentModel
  {
    public EnvironmentModel(string name)
    {
      Name = name;
    }

    public string Name { get; set; }
    public List<TestModel> Tests { get; set; } = new();

    private EnvironmentStatus Status
    {
      get;
      set;
    }

    public override string ToString() => Name;
    

    public IEnumerable<IJob> Children
    {
      get
      {
        // add exception if environment was not run.
        if (Name.Contains("2"))
        {
          return Enumerable.Empty<IJob>();
        }

        if (Status == EnvironmentStatus.BuildSuccess)
        {
          return Tests;
        }

        return Enumerable.Empty<IJob>();
      }
    }

    public bool IsDone { get; private set; }

    public void Run()
    {
      //Console.WriteLine($"Runnable with Name {Name} was run");
      Status = EnvironmentStatus.BuildSuccess;
      IsDone = true;
    }
  }
}
