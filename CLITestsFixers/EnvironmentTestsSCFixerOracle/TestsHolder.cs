﻿using EnvironmentTestsSCFixerOracle.Model;
using Newtonsoft.Json.Linq;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace EnvironmentTestsSCFixerOracle
{
  internal class TestsHolder
  {
    // Пары имён файлов тестов и объектов: c:\Projects\Tests\_definition.tests и JArray.
    private List<Tuple<string, JArray>> rawJsonTestsArray = new();

    public TestsHolder(string pathToTests)
    {
      string[] testFiles = Directory.GetFiles(pathToTests,
            "_definition.tests",
            SearchOption.AllDirectories);

      foreach (var file in testFiles)
      {
        var jsonObjects = JArray.Parse(File.ReadAllText(file));

        int foundTestsInFileCount = 0;
        foreach (JObject jsonObject in jsonObjects)
        {
          var environment = Guid.Parse(jsonObject["environment"]!.Value<string>()!);

          // Skip empty tests
          if (environment == Guid.Empty)
          {
            continue;
          }

          AllTests.Add(new Test(jsonObject, file));
          foundTestsInFileCount++;
        }

        if (foundTestsInFileCount > 0)
        {
          rawJsonTestsArray.Add(new Tuple<string, JArray>(file, jsonObjects));
        }
      }
    }

    public List<Test> AllTests { get; } = new();

    public void SaveChanges()
    {
      foreach (var environmen in rawJsonTestsArray)
      {
        File.WriteAllText(environmen.Item1, environmen.Item2.ToString());
      }
    }
  }
}
