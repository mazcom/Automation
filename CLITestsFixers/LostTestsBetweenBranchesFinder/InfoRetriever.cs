using Newtonsoft.Json.Linq;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace LostTestsBetweenBranchesFinder
{
  internal static class InfoRetriever
  {

    public static Dictionary<Guid, string> Retrieve(string pathToTests)
    {
      var result = new Dictionary<Guid, string>();

      string[] testFiles = Directory.GetFiles(pathToTests,
            "_definition.tests",
            SearchOption.AllDirectories);

      foreach (var testFile in testFiles)
      {
        using (StreamReader sr = new(testFile))
        {
          string json = sr.ReadToEnd();
          JArray jsonObjects = JArray.Parse(json);
          foreach (JObject jsonObject in jsonObjects)
          {
            var id = (Guid)jsonObject.SelectToken("id")!;
            if (!result.ContainsKey(id))
            {
              result.Add(id, testFile);
            }
          }
        }
      }

      return result;
    }
  }
}
