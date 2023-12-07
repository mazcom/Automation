using Common;
using Common.Model;
using Newtonsoft.Json.Linq;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace NoEnvironmentTestsFixer.Model
{
  public class TestModel: BaseTest
  {
    public TestModel(JObject jsonObject, string testFullPath): base( jsonObject, testFullPath)
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
      string cleanUpSection =
$@"[{{
""when"":""Always"",
""actions"": [
             {{
                ""run"" : {{
                            ""code"" : {{
                                         ""type"" : ""cmd"",
                                         ""code"" : ""%MySQL_Studio% /execute /connection:{Constants.AffordableConnectionName} /inputfile:\""{cleanUpFileName}\""""
                            }}  
                }},
                ""exit_codes"":[
                  0
                ],
                ""timeout"" : 120000
             }}     
  ]  
    
}}]";
      this.JsonObject!["post_run"] = JValue.Parse(cleanUpSection);
      // this.JsonObject!.Remove("post_run");
      // this.JsonObject!.Add("post_run", JProperty.Parse(cleanUpSection));

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
$@"DROP DATABASE IF EXISTS {databaseName.Item2};";

/*$@"USE [master]
GO
DECLARE @db_name NVARCHAR(255);
SET @db_name = N'{databaseName.Item2}';
IF EXISTS (SELECT 1 FROM sys.databases d WHERE d.name = @db_name)
BEGIN
EXEC msdb.dbo.sp_delete_database_backuphistory @db_name;
EXEC (N'ALTER DATABASE '+@db_name+N' SET SINGLE_USER WITH ROLLBACK IMMEDIATE');
EXEC (N'DROP DATABASE '+@db_name);
END;
GO
";*/
          /*if (sb.Length > 0)
          {
            sb.AppendLine(Environment.NewLine);
          }*/

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
      foreach (var token in this.JsonObject!.SelectTokens("pre_run[*].run.code.code", errorWhenNoMatch: false)!.
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
      foreach (var token in this.JsonObject.SelectTokens("post_run[*].actions[*].run.code.code", errorWhenNoMatch: false)!.
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
