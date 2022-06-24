using Newtonsoft.Json.Linq;
using System.Text;

namespace TestsFixer.Model
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

    public List<string> NewDatabaseNames { get; } = new();
    public List<string> OldDatabaseNames { get; } = new();

    public bool Patch() => PatchDatabaseInfo();

    public string PatchError { get; private set; }

    private bool PatchDatabaseInfo()
    {
      // СОЗДАНИЕ БАЗЫ ДАННЫХ.
      foreach (var token in jsonObject.SelectTokens("build[*].run.code.code", errorWhenNoMatch: false)!.
        Where(t => ((string)t!).Contains(".sql", StringComparison.OrdinalIgnoreCase))!)
      {
        var createDbCommandLine = (string)token!;

        var environmentPath = Path.GetDirectoryName(this.environmentFullPath)!;
        var sqlFileName = FilesHelper.ExtractSqlFileName(createDbCommandLine)!;
        var createdatabaseFileFullPath = Path.Combine(environmentPath, sqlFileName);

        // Меняем имена баз данных в файлах создания баз данных.  
        if (DBNamesReplacer.GenerateNamesAndReplaceInFile(createdatabaseFileFullPath, out List<Tuple<string, string>> oldNewNames, out bool alreadyPatched))
        {
          oldNewNames.ForEach(cf =>
          {
            OldDatabaseNames.Add(cf.Item1);
            NewDatabaseNames.Add(cf.Item2);
          });

          Console.ForegroundColor = ConsoleColor.Green;
          Console.WriteLine($"The file {createdatabaseFileFullPath} has been patched!");
          Console.ResetColor();

        }
        else
        {
          if (alreadyPatched)
          {
            PatchError = "The environment seems to be already patched.";
            return false;
          }
        }

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
        return;
      } 

      string cleanUpFileName = createFileName.Insert(createFileName.IndexOf(".sql"), "_CleanUp");
      string cleanUpSection =
$@"{{
""when"":""Always"",
""actions"": [
             {{
                ""run"" : {{
                            ""code"" : {{
                                         ""type"" : ""cmd"",
                                         ""code"" : ""%dbforgesql% /execute /connection:%sqlserver2019% /inputfile:\""{cleanUpFileName}\""""
                            }}  
                }},
                ""exit_codes"":[
                  0
                ],
                ""timout"" : 120000
             }}     
  ]  
    
}}";
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

        foreach (var databaseName in NewDatabaseNames)
        {
          string cleanSql =

$@"USE [master]
GO
DECLARE @db_name NVARCHAR(255);
SET @db_name = N'{databaseName}';
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
