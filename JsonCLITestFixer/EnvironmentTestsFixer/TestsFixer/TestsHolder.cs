using Newtonsoft.Json.Linq;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using TestsFixer.Model;

namespace TestsFixer
{
  internal class TestsHolder
  {

    // Пары имён файлов тестов и объектов: c:\Projects\Tests\_definition.tests и JArray.
    private List<Tuple<string, JArray>> rawJsonArrayTests = new();

    /// <summary>
    /// </summary>
    /// <param name="testFiles">Set of the _definition.tests files</param>
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
          if(environment == Guid.Empty)
          {
            continue;
          }

          AllTests.Add(new Test(jsonObject));
          foundTestsInFileCount++;
        }

        if (foundTestsInFileCount > 0)
        {
          rawJsonArrayTests.Add(new Tuple<string, JArray>(file, jsonObjects));
        }
      }
    }

    public List<Test> AllTests { get; } = new();
  }
}
