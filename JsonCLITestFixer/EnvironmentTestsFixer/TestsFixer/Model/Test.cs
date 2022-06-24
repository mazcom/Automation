using Newtonsoft.Json.Linq;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace TestsFixer.Model
{
  internal class Test
  {
    private readonly JObject jsonObject;
    private TestsEnvironment environment;

    public Test(JObject jsonObject)
    {
      this.jsonObject = jsonObject;
      Id = Guid.Parse(jsonObject["id"]!.Value<string>()!);
      EnvironmentId = Guid.Parse(jsonObject["environment"]!.Value<string>()!);
      Name = jsonObject["name"]!.Value<string>()!;
      Description = jsonObject["description"]!.Value<string>()!;
    }

    public Guid Id { get; }
    public Guid EnvironmentId { get; }
    public string Name { get; }
    public string Description { get; set; }

    public void SetEnvironment(TestsEnvironment environment)
    {
      this.environment = environment;
    }

    public void Patch()
    {
      PatchDatabaseNames();
      PatchEnterprise();
    }

    public void PatchDatabaseNames()
    {
      foreach (JValue token in this.jsonObject.SelectTokens("pre_run[*].run.code.code", errorWhenNoMatch: false)!.
        Where(t => ((string)t!).Contains("/database:", StringComparison.OrdinalIgnoreCase))!)
      {
        var createDbCommandLine = (string)token!;

        for (int i = 0; i < environment.OldDatabaseNames.Count; i++)
        {
          var oldDatabase = environment.OldDatabaseNames[i];
          var newDatabase = environment.NewDatabaseNames[i];

          if (!createDbCommandLine.Contains(oldDatabase))
          {
            continue;
          }

          createDbCommandLine = createDbCommandLine.Replace(oldDatabase, newDatabase);
          token.Value = createDbCommandLine;
          break;
        }
      }
    }
    public void PatchEnterprise()
    {
      var enterprise = this.jsonObject.SelectTokens("assert.files_equal[*].condition", errorWhenNoMatch: false)!.FirstOrDefault();
      if (enterprise == null)
      {
        var filesEqualNode = this.jsonObject.SelectTokens("assert.files_equal", errorWhenNoMatch: false)!.FirstOrDefault();
        if (filesEqualNode != null)
        {
          var etalonNode = filesEqualNode.SelectTokens("[*].etalon", errorWhenNoMatch: false)!.FirstOrDefault();
          var actualNode = filesEqualNode.SelectTokens("[*].actual", errorWhenNoMatch: false)!.FirstOrDefault();

          string addenterpriseSection =
$@"{{
""condition"": ""Enterprise"",
""etalon"": ""{etalonNode!.Value<string>()}"",
""actual"": ""{actualNode!.Value<string>()}""    
}}";
          ((JArray)filesEqualNode).Add(JObject.Parse(addenterpriseSection));
        }
      }
    }
  }
}
