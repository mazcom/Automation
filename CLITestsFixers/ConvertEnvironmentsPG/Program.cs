// See https://aka.ms/new-console-template for more information

using Common;
using ConvertEnvironmentsPG;
using Newtonsoft.Json;
using Newtonsoft.Json.Linq;
using System.Diagnostics;
using System.IO;
using System.Linq;
using System.Text.Json.Nodes;

Console.ResetColor();
Console.WriteLine(@"Please, enter a path to the environments like D:\Projects\commandlinetestsMaster2\Environments\PostgreSQL\Studio\SchemaComparer\");
string pathToEnvironments = @"D:\Projects\commandlinetestsMaster2\Environments\PostgreSQL\Studio\SchemaComparer\";
//string pathToTests = Console.ReadLine()!;

var newPathToEnvironments = Path.GetFullPath(Path.Combine(pathToEnvironments, @"..\_SchemaComparer"));

if (!Directory.Exists(pathToEnvironments))
{
  Console.ForegroundColor = ConsoleColor.Red;
  Console.WriteLine($"Directory ${pathToEnvironments} does not exist!");
  Console.ResetColor();
  return;
}

JArray newJsonObjects = new JArray();

string[] environmentFiles = Directory.GetFiles(pathToEnvironments,
            "*.environments",
            SearchOption.AllDirectories);
int count = 0;
int notFoundcount = 0;
foreach (var environmentFile in environmentFiles)
{
  //JArray jsonObjects;
  using (StreamReader sr = new(environmentFile))
  {
    string json = sr.ReadToEnd();
    JArray jsonObjects = JArray.Parse(json);
    foreach (JObject jsonObject in jsonObjects)
    {
      var environmentId = Guid.Parse(jsonObject["id"]!.Value<string>()!);
      var environmentName = jsonObject["name"]!.Value<string>()!;
      var environmentDescription = jsonObject["description"]!;

      var cleanUpFileName = environmentName.ToCamelCase() + "_CleanUp.sql";
      var createFileName = environmentName.ToCamelCase() + "_Create.sql";

      // sc or dc
      var dbPrefix1 = "sc1_";
      var dbPrefix2 = "sc2_";

      var dbName1 = dbPrefix1 + environmentId.ToString().Replace("-", "_");
      var dbName2 = dbPrefix2 + environmentId.ToString().Replace("-", "_");
      var newSourceFileName = environmentName.ToCamelCase() + "_Source.sql";
      var newTargetFileName = environmentName.ToCamelCase() + "_Target.sql";


      var defaultConnectionName = "%pglast%";

      //environmentName = environmentName.Replace(" ", "-");

      if (environmentName.Contains("RedShift", StringComparison.OrdinalIgnoreCase))
      {
        Debug.WriteLine($"skip environment {environmentName}");
        continue;
      }

      var nodes = jsonObject!.SelectTokens("build[*].run.code.code", errorWhenNoMatch: false)!.
        Where(t => 
              ((string)t!).Contains("SourceObjects.sql", StringComparison.OrdinalIgnoreCase)
              || ((string)t!).Contains("TargetObjects.sql", StringComparison.OrdinalIgnoreCase)
              || ((string)t!).Contains("Source.sql", StringComparison.OrdinalIgnoreCase)
              || ((string)t!).Contains("Target.sql", StringComparison.OrdinalIgnoreCase))!.ToList();

      string oldCreateDBObjectsSqlFileName1 = null;
      string oldCreateDBObjectsSqlFileName2 = null;

      if (nodes.Count == 2)
      {
        oldCreateDBObjectsSqlFileName1 = RegexHelper.ExtractSqlFileNameFromCommandLine((string)nodes[0]!);
        oldCreateDBObjectsSqlFileName2 = RegexHelper.ExtractSqlFileNameFromCommandLine((string)nodes[1]!);

        ArgumentNullException.ThrowIfNull(oldCreateDBObjectsSqlFileName1);
        ArgumentNullException.ThrowIfNull(oldCreateDBObjectsSqlFileName2);
      }
      else
      {
        //oldCreatesqlFileName1 = "Could_not_parse_filename1";
        //oldCreatesqlFileName2 = "Could_not_parse_filename2";
        notFoundcount++;
      }

      string jsonString =
$@"{{
  ""id"": ""{environmentId}"",
  ""name"": ""{environmentName}"",
  ""description"": ""{environmentDescription}"",
  ""build"": [

             {{
                ""run"" : {{
                            ""code"" : {{
                                         ""type"" : ""cmd"",
                                         ""code"" : ""%dbforgesql% /execute /connection:{defaultConnectionName} /inputfile:\""{cleanUpFileName}\""""
                            }}  
                }},
                ""exit_codes"":[
                  0
                ],
                ""timeout"" : 120000
             }},

             {{
                ""run"" : {{
                            ""code"" : {{
                                         ""type"" : ""cmd"",
                                         ""code"" : ""%dbforgesql% /execute /connection:{defaultConnectionName} /inputfile:\""{createFileName}\""""
                            }}  
                }},
                ""exit_codes"":[
                  0
                ],
                ""timeout"" : 120000
             }},

             {{
                ""run"" : {{
                            ""code"" : {{
                                         ""type"" : ""cmd"",
                                         ""code"" : ""%dbforgesql% /execute /connection:{defaultConnectionName} /inputfile:\""{newSourceFileName}\"" /database:{dbName1}""
                            }}  
                }},
                ""exit_codes"":[
                  0
                ],
                ""timeout"" : 120000
             }},

             {{
                ""run"" : {{
                            ""code"" : {{
                                         ""type"" : ""cmd"",
                                         ""code"" : ""%dbforgesql% /execute /connection:{defaultConnectionName} /inputfile:\""{newTargetFileName}\"" /database:{dbName2}""
                            }}  
                }},
                ""exit_codes"":[
                  0
                ],
                ""timeout"" : 120000
             }}

  ],  
  ""clean_up"": [
  {{
      ""when"":""Always"",
      ""actions"": [
             {{
                ""run"" : {{
                            ""code"" : {{
                                         ""type"" : ""cmd"",
                                         ""code"" : ""%dbforgesql% /execute /connection:{defaultConnectionName} /inputfile:\""{cleanUpFileName}\""""
                            }}  
                }},
                ""exit_codes"":[
                  0
                ],
                ""timeout"" : 120000
             }}     
       ]    
  }}     
  ]
}}
";

      newJsonObjects.Add(JObject.Parse(jsonString));

      // Copy old Create DB objects fileName1
      if (!string.IsNullOrEmpty(oldCreateDBObjectsSqlFileName1) && !string.IsNullOrEmpty(oldCreateDBObjectsSqlFileName2))
      {
        string oldDir = Path.GetDirectoryName(environmentFile);

        string oldFile1 = Path.Combine(oldDir, oldCreateDBObjectsSqlFileName1);
        string newFile1 = Path.Combine(newPathToEnvironments, newSourceFileName);
        File.Copy(oldFile1, newFile1, overwrite: true);

        string oldFile2 = Path.Combine(oldDir, oldCreateDBObjectsSqlFileName2);
        string newFile2 = Path.Combine(newPathToEnvironments, newTargetFileName);
        File.Copy(oldFile2, newFile2, overwrite: true);
      }


      // Copy old Create DB objects fileName2



      //foreach (var commandLine in jsonObject!.SelectTokens("build[*].run.code.code", errorWhenNoMatch: false)!.
      //  Where(t => ((string)t!).Contains("TargetDB.sql", StringComparison.OrdinalIgnoreCase)).ToList())
      //{
      //  var sqlFileName = RegexHelper.ExtractSqlFileNameFromCommandLine((string)commandLine!);

      //  Debug.WriteLine("hello");
      //  //var createDbCommandLine = (string)token!;
      //  //// Получаем имя базы данных из теста like Databases.sql
      //  //var createDbFileName = FilesHelper.ExtractFileName(createDbCommandLine)!;
      //  //// Полный путь к файлу создания базы данных.
      //  //string initDbFullFileName = Path.Combine(currentDir, createDbFileName);
      //  //CreateDbFiles.Add(new FileModel() { FullPath = initDbFullFileName, FileName = createDbFileName });
      //}
    }
  }
}


//Dictionary<string, int> allIssues = new();

// Retrieve all tests id.
//foreach (var testFile in testFiles)
//{
//  //JArray jsonObjects;
//  using (StreamReader sr = new(testFile))
//  {
//    string json = sr.ReadToEnd();
//    JArray jsonObjects = JArray.Parse(json);
//    foreach (JObject jsonObject in jsonObjects)
//    {
//      var issue = (string)jsonObject.SelectToken("issue")!;
//      if (!string.IsNullOrEmpty(issue))
//      {
//        if (!allIssues.ContainsKey(issue))
//        {
//          allIssues.Add(issue, 1);
//        }
//        else
//        {
//          allIssues[issue] += 1;
//        }
//      }
//    }
//  }
//}

//Console.WriteLine("Issues:");
//var sorted = allIssues.OrderByDescending(x => x.Value).ToDictionary(x => x.Key, x => x.Value);
//foreach (var issue in sorted)
//{
//  Console.WriteLine($"https://jira.devart.com/browse/{issue.Key}. Occurrence: {issue.Value}");
//}


if (!Directory.Exists(newPathToEnvironments))
{
  Directory.CreateDirectory(newPathToEnvironments);
}

var newEnvironmentFileName = Path.Combine(newPathToEnvironments, "_PostgreSqlSchemaComparer.environments");
File.WriteAllText(newEnvironmentFileName, newJsonObjects.ToString());


Console.WriteLine();
Console.ForegroundColor = ConsoleColor.Green;
//Console.WriteLine($"Total issues count = {allIssues.Count}");
Console.WriteLine($"Completed!");
Console.ResetColor();
