using Common;
using Common.Model;
using Newtonsoft.Json.Linq;
using NoEnvironmentTestsFixerSCOracle;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Text.Json.Nodes;
using System.Threading.Tasks;

namespace NoEnvironmentTestsFixerSCOracle.Model
{
  public class TestModel : BaseTest
  {
    public TestModel(JObject jsonObject, string testFullPath) : base(jsonObject, testFullPath)
    {
      Init();
    }

    public List<FileModel> CreateDbFiles { get; set; } = new();

    public List<FileModel> CleanDbFiles { get; set; } = new();

    public void Patch(List<Tuple<string, string>> oldNewDbNames, PatchSession patchSession)
    {
      // Path a test info.
      PatchDatabaseNames(oldNewDbNames);
      PatchServerName(oldNewDbNames);

      // Patch the attached separate files(scomp, dcomp, dgen, etalon.sql and etc.) to a test.
      PatchDocTemplates(oldNewDbNames, patchSession);
    }

    public void Patch()
    {
      PatchTimeout();
      PatchEnterprise();
    }

    public void AddCleanSection(string createFileName, List<Tuple<string, string>> oldNewDbNames)
    {
      string cleanUpFileName = createFileName.Insert(createFileName.IndexOf(".sql"), "_CleanUp");

      var postrunActions = this.JsonObject.SelectTokens("post_run[*].actions[*].run", errorWhenNoMatch: false)!.FirstOrDefault();
      if (postrunActions != null && postrunActions.Count() > 0)
      {
        var postRunArray = (JArray)this.JsonObject.SelectTokens("post_run[*].actions", errorWhenNoMatch: false)!.FirstOrDefault();

        string toAddRun =
          $@"
{{
                ""run"" : {{
                            ""code"" : {{
                                         ""type"" : ""cmd"",
                                         ""code"" : ""%Oracle_Studio% /execute /connection:{Constants.AffordableConnectionName} /inputfile:\""{cleanUpFileName}\""""
                            }}  
                }},
                ""exit_codes"":[
                  0
                ]
             }}
          ";

        postRunArray.Add(JObject.Parse(toAddRun));
      }
      else
      {
        string cleanUpSection =
  $@"[{{
""when"":""Always"",
""actions"": [
             {{
                ""run"" : {{
                            ""code"" : {{
                                         ""type"" : ""cmd"",
                                         ""code"" : ""%Oracle_Studio% /execute /connection:{Constants.AffordableConnectionName} /inputfile:\""{cleanUpFileName}\""""
                            }}  
                }},
                ""exit_codes"":[
                  0
                ],
                ""timeout"" : 120000
             }}     
  ]  
    
}}]";
        JsonObject!["post_run"] = JToken.Parse(cleanUpSection);
        // this.JsonObject!.Remove("post_run");
        // this.JsonObject!.Add("post_run", JProperty.Parse(cleanUpSection));
      }


      Console.ForegroundColor = ConsoleColor.Green;
      Console.WriteLine($"The clean_up section has been added to an environment!");
      Console.ResetColor();

      CreateCleanUpFile(cleanUpFileName, oldNewDbNames);
    }

    private void CreateCleanUpFile(string cleanUpFileName, List<Tuple<string, string>> oldNewNames)
    {
      if (!File.Exists(cleanUpFileName))
      {
        StringBuilder sb = new();

        foreach (var databaseName in oldNewNames)
        {
          string cleanSql =

$@"ALTER SESSION SET CURRENT_SCHEMA = SYS;
/

DECLARE
  SCHEMA_NAME VARCHAR2(255) := '{databaseName.Item2.ToUpper()}';
  ROW_COUNT   NUMBER;
BEGIN
  FOR R IN (SELECT SID,
                   SERIAL#
      FROM V$SESSION
      WHERE UPPER(USERNAME) LIKE UPPER(SCHEMA_NAME) || '%')
  LOOP
    EXECUTE IMMEDIATE 'ALTER SYSTEM DISCONNECT SESSION ''' || R.SID || ',' || R.SERIAL# || '''' || ' IMMEDIATE';
    EXECUTE IMMEDIATE 'ALTER SYSTEM KILL SESSION ''' || R.SID || ',' || R.SERIAL# || '''';
  END LOOP;

  SELECT COUNT(*)
    INTO ROW_COUNT
    FROM DBA_USERS
    WHERE USERNAME = SCHEMA_NAME;
  IF ROW_COUNT > 0
  THEN
    EXECUTE IMMEDIATE 'DROP USER ' || SCHEMA_NAME || ' CASCADE';
  END IF;
END;
/
";

          sb.AppendLine(cleanSql);
        }

        var environmentPath = Path.GetDirectoryName(TestFullPath)!;
        var cleanUpFileFullPath = Path.Combine(environmentPath, cleanUpFileName);
        using (StreamWriter sw = File.CreateText(cleanUpFileFullPath))
        {
          sw.Write(sb.ToString());
        }
        Console.ForegroundColor = ConsoleColor.Green;
        Console.WriteLine($"The clean_up file {cleanUpFileFullPath} has been created!");
        Console.ResetColor();
      }
    }

    private void Init()
    {
      string currentDir = Path.GetDirectoryName(TestFullPath)!;

      // СОЗДАНИЕ БАЗЫ ДАННЫХ.
      foreach (var token in JsonObject!.SelectTokens("pre_run[*].run.code.code", errorWhenNoMatch: false)!.
        Where(t => ((string)t!).Contains(".sql", StringComparison.OrdinalIgnoreCase)
         && !((string)t!).Contains("output.sql", StringComparison.OrdinalIgnoreCase))!)
      {
        var createDbCommandLine = (string)token!;
        // Получаем имя базы данных из теста like Databases.sql
        var createDbFileName = FilesHelper.ExtractFileName(createDbCommandLine)!;
        // Полный путь к файлу создания базы данных.
        string initDbFullFileName = Path.Combine(currentDir, createDbFileName);
        CreateDbFiles.Add(new FileModel() { FullPath = initDbFullFileName, FileName = createDbFileName });
      }

      // УДАЛЕНИЕ БАЗЫ ДАННЫХ.
      foreach (var token in JsonObject.SelectTokens("post_run[*].actions[*].run.code.code", errorWhenNoMatch: false)!.
        Where(t => ((string)t!).Contains(".sql", StringComparison.OrdinalIgnoreCase)
        && !((string)t!).Contains("output.sql", StringComparison.OrdinalIgnoreCase))!)
      {
        var cleanDbCommandLine = (string)token!;
        // Получаем имя базы данных из теста like CleanUp.sql
        var cleanUpDbFileName = FilesHelper.ExtractFileName(cleanDbCommandLine)!;
        // Полный путь к файлу создания базы данных.
        string cleanDbFullFileName = Path.Combine(currentDir, cleanUpDbFileName);
        CleanDbFiles.Add(new FileModel() { FullPath = cleanDbFullFileName, FileName = cleanUpDbFileName });
      }

      if (CleanDbFiles.Count == 0)
      {
        Console.WriteLine();
        Console.ForegroundColor = ConsoleColor.Yellow;
        Console.WriteLine($"$ The test with id={Id} in the file ${TestFullPath} does not have a clean.sql file!");
        Console.ResetColor();
      }
    }
  }
}
