using Newtonsoft.Json.Linq;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using TestsFixer.Model;

namespace TestsFixer
{
  internal class EnvironmentsHolder
  {
    // Пары имён файлов тестов и объектов: c:\Projects\Environments\*.environments и JArray.
    private List<Tuple<string, JArray>> rawJsonEnvironmentsArray = new();

    public EnvironmentsHolder(IEnumerable<Test> tests, string environmentsPath)
    {
      string[] environmentFiles = Directory.GetFiles(environmentsPath,
            "*.environments",
            SearchOption.AllDirectories);

      HashSet<Guid> environmentsUsedInTests = new HashSet<Guid>(tests.Select(t => t.EnvironmentId));

      foreach (var fileName in environmentFiles)
      {
        var jsonObjects = JArray.Parse(File.ReadAllText(fileName));
        var environmentsInFile = jsonObjects.Select(e => Guid.Parse(e.SelectToken("id")!.ToString())).ToArray();

        // Проверяем, что энвайронменты из тестов имеются в текущем файле.
        var isContained = environmentsUsedInTests.Intersect(environmentsInFile).Count() > 0;
        if (isContained)
        {
          HashSet<Guid> processedIds = new();
          foreach (JObject jsonObject in jsonObjects)
          {
            var environmentId = Guid.Parse(jsonObject["id"]!.Value<string>()!);
            if (environmentsUsedInTests.Contains(environmentId) && !processedIds.Contains(environmentId))
            {
              processedIds.Add(environmentId);
              TestsEnvironment environment = new(jsonObject, fileName);

              // Заполняем тестами текущий environment тестами, которые на него смотрят.
              foreach (var test in tests)
              {
                if (environment.Id == test.EnvironmentId)
                {
                  test.SetEnvironment(environment);
                  environment.Tests.Add(test);
                }
              }

              Environments.Add(environment);
            }
          }
          rawJsonEnvironmentsArray.Add(new Tuple<string, JArray>(fileName, jsonObjects));
        }
      }
    }

    public List<TestsEnvironment> Environments { get; } = new();

    public void SaveChanges()
    {
      foreach (var environmen in rawJsonEnvironmentsArray)
      {
        File.WriteAllText(environmen.Item1, environmen.Item2.ToString());
      }
    }
  }
}
