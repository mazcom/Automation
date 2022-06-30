using Common;
using Newtonsoft.Json.Linq;
using System.Text;

namespace EnvironmentTestsFixer.Model
{
  internal class TestsEnvironment
  {
    private readonly JObject jsonObject;
    private readonly string environmentFullPath;

    public TestsEnvironment(JObject jsonObject, string environmentFullPath)
    {
      this.jsonObject = jsonObject;
      this.environmentFullPath = environmentFullPath;
      Id = Guid.Parse(jsonObject["id"]!.Value<string>()!);
      Name = jsonObject["name"]!.Value<string>()!;
    }

    public Guid Id { get; }
    public string Name { get; }

    public List<Test> Tests { get; } = new();

    public List<Tuple<string, string>> OldNewDatabaseNames { get; } = new();

    public bool Patch() => PatchDatabaseInfo();

    public string PatchError { get; private set; }

    private bool PatchDatabaseInfo()
    {
      // СОЗДАНИЕ БАЗЫ ДАННЫХ.
      var createDbCommandLineNode = jsonObject.SelectTokens("build[*].run.code.code", errorWhenNoMatch: false)!.
        Where(t => ((string)t!).Contains(".sql", StringComparison.OrdinalIgnoreCase))!.FirstOrDefault();

      if (createDbCommandLineNode != null) 
      {
        var createDbCommandLine = (string)createDbCommandLineNode!;

        var environmentPath = Path.GetDirectoryName(this.environmentFullPath)!;
        var sqlFileName = RegexHelper.ExtractSqlFileName(createDbCommandLine)!;
        var createdatabaseFileFullPath = Path.Combine(environmentPath, sqlFileName);

        // Меняем имена баз данных в файлах создания баз данных.  
        if (DBReplacer.GenerateNamesAndReplaceInSqlFile(createdatabaseFileFullPath, out List<Tuple<string, string>> oldNewNames, out bool alreadyPatched))
        {
          oldNewNames.ForEach(cf =>
          {
            OldNewDatabaseNames.Add(cf);
          });

          Console.ForegroundColor = ConsoleColor.Green;
          Console.WriteLine($"The file {createdatabaseFileFullPath} has been patched!");
          Console.ResetColor();

          string currentServerName = RegexHelper.ExtractServerName(createDbCommandLine)!;
          if (currentServerName != null)
          {
            ((JValue)createDbCommandLineNode).Value = createDbCommandLine.Replace(currentServerName, Constants.AffordableConnectionName);
          }
        }
        else
        {
          if (alreadyPatched)
          {
            PatchError = "The environment seems to be already patched.";
            return false;
          }
        }
        DBReplacer.TryToReplacePassword(createdatabaseFileFullPath);
        PatchCleanSection(sqlFileName);
      }
      return true;
    }

    private void PatchCleanSection(string createFileName)
    {
      // check whether the clean_up property already exists
      var cleanUpNode = this.jsonObject.SelectTokens("clean_up", errorWhenNoMatch: false)!.FirstOrDefault();
      if (cleanUpNode != null)
      {
        // do nothing if it does. 
        return;
      } 

      string cleanUpFileName = createFileName.Insert(createFileName.IndexOf(".sql"), "_CleanUp");
      string cleanUpSection =
$@"[{{
""when"":""Always"",
""actions"": [
             {{
                ""run"" : {{
                            ""code"" : {{
                                         ""type"" : ""cmd"",
                                         ""code"" : ""%dbforgesql% /execute /connection:{Constants.AffordableConnectionName} /inputfile:\""{cleanUpFileName}\""""
                            }}  
                }},
                ""exit_codes"":[
                  0
                ],
                ""timeout"" : 120000
             }}     
  ]  
    
}}]";
      this.jsonObject.Add("clean_up", JProperty.Parse(cleanUpSection));

      Console.ForegroundColor = ConsoleColor.Green;
      Console.WriteLine($"The clean_up section has been added to an environment!");
      Console.ResetColor();

      CreateCleanUpFile(cleanUpFileName);
    }

    private void CreateCleanUpFile(string cleanUpFileName)
    {
      if (!File.Exists(cleanUpFileName))
      {
        StringBuilder sb = new();

        foreach (var databaseName in OldNewDatabaseNames)
        {
          string cleanSql =

$@"USE [master]
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
";
          if (sb.Length > 0)
          {
            sb.AppendLine(Environment.NewLine);
          }

          sb.AppendLine(cleanSql);
        }

        var environmentPath = Path.GetDirectoryName(this.environmentFullPath)!;
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
  }
}
