using Common;
using Common.Model;
using Newtonsoft.Json.Linq;
using System.Text;

namespace EnvironmentTestsFixerMySQL.Model
{
  internal class TestsEnvironment
  {
    private readonly JObject jsonObject;
    private readonly string environmentFullPath;

    // hack
    private static HashSet<string> processedCreateFilenames = new();

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

    public bool Patch(PatchSession patchSession) => PatchDatabaseInfo(patchSession);

    public string PatchError { get; private set; }

    private bool PatchDatabaseInfo(PatchSession patchSession)
    {
      // СОЗДАНИЕ БАЗЫ ДАННЫХ.
      var createDbCommandLineNodes = jsonObject.SelectTokens("build[*].run.code.code", errorWhenNoMatch: false)!.
        Where(t => ((string)t!).Contains(".sql", StringComparison.OrdinalIgnoreCase))!.ToArray();

      if (createDbCommandLineNodes == null || createDbCommandLineNodes.Length == 0)
      {
        return true;
      }

      List<string> sqlFileNames = new();

      foreach (var createDbCommandLineNode in createDbCommandLineNodes)
      {
        var createDbCommandLine = (string)createDbCommandLineNode!;

        var environmentPath = Path.GetDirectoryName(this.environmentFullPath)!;
        var sqlFileName = RegexHelper.ExtractSqlFileNameFromCommandLine(createDbCommandLine)!;

        if (string.IsNullOrEmpty(sqlFileName))
        {
          Console.ForegroundColor = ConsoleColor.Yellow;
          Console.WriteLine($"Attention! Could not parce a file file name from command {createDbCommandLine}!");
          Console.ResetColor();
          continue;
        }


        if (!sqlFileNames.Contains(sqlFileName))
        {
          sqlFileNames.Add(sqlFileName);
        }

        var createdatabaseFileFullPath = Path.Combine(environmentPath, sqlFileName);

        if (!File.Exists(createdatabaseFileFullPath))
        {
          Console.ForegroundColor = ConsoleColor.Red;
          Console.WriteLine($"The file {createdatabaseFileFullPath} does not exist!");
          Console.ResetColor();

          continue;
        }


        List<Tuple<string, string>> oldNewNames = new();

        bool patched = false;

        // Индикатор того, что файлик создания баз данных уже пропатчен давно. 
        // Текущая сессия тоже не обладает информацией, как он был пропатчен.
        bool alreadyPatched = false;

        // Пробуем получить из сессиии. Возможно другой environment уже преобразовал
        // и мы можем получить готовые имена баз данных.
        if (patchSession.FileToOldNewNames.ContainsKey(createdatabaseFileFullPath))
        {
          oldNewNames = patchSession.FileToOldNewNames[createdatabaseFileFullPath];
        }
        else
        {
          // Пробуем пропатчить файлик и получить имена баз данных.
          patched = DBReplacer.GenerateNamesAndReplaceInSqlFile(createdatabaseFileFullPath, out oldNewNames, out alreadyPatched, Id);
        }

        if (alreadyPatched)
        {
          PatchError = "The environment seems to be already patched.";
          return false;
        }

        if (oldNewNames.Count > 0)
        {
          if (patched)
          {
            patchSession.FileToOldNewNames[createdatabaseFileFullPath] = oldNewNames;

            Console.ForegroundColor = ConsoleColor.Green;
            Console.WriteLine($"The file {createdatabaseFileFullPath} has been patched!");
            Console.ResetColor();
          }

          oldNewNames.ForEach(cf =>
          {
            OldNewDatabaseNames.Add(cf);
          });

          string currentServerName = RegexHelper.ExtractServerName(createDbCommandLine)!;
          if (currentServerName != null)
          {
            ((JValue)createDbCommandLineNode).Value = createDbCommandLine.Replace(currentServerName, Constants.AffordableConnectionName);
          }

          DBReplacer.TryToReplacePassword(createdatabaseFileFullPath);
        }
      }

      PatchCleanSection(sqlFileNames);
      return true;
    }

    // Запускается перед основным патчем. Нужно пройтись по всем энвайронментам и подготовить их к патчам.
    // данный метод проходит и ищет дублирующиеся файлы создагия баз данных. Их нужно будет клонировать. 
    public void Prepare()
    {

      // СОЗДАНИЕ БАЗЫ ДАННЫХ.
      var createDbCommandLineNodes = jsonObject.SelectTokens("build[*].run.code.code", errorWhenNoMatch: false)!.
        Where(t => ((string)t!).Contains(".sql", StringComparison.OrdinalIgnoreCase))!.ToArray();

      if (createDbCommandLineNodes == null || createDbCommandLineNodes.Length == 0)
      {
        return;
      }

      List<string> sqlFileNames = new();

      foreach (var createDbCommandLineNode in createDbCommandLineNodes)
      {
        var createDbCommandLine = (string)createDbCommandLineNode!;

        var environmentPath = Path.GetDirectoryName(this.environmentFullPath)!;
        var sqlFileName = RegexHelper.ExtractSqlFileNameFromCommandLine(createDbCommandLine)!;

        if (string.IsNullOrEmpty(sqlFileName))
        {
          Console.ForegroundColor = ConsoleColor.Yellow;
          Console.WriteLine($"Attention! Could not parce a file file name from command {createDbCommandLine}!");
          Console.ResetColor();
          continue;
        }

        var createdatabaseFileFullPath = Path.Combine(environmentPath, sqlFileName);

        if (!File.Exists(createdatabaseFileFullPath))
        {
          Console.ForegroundColor = ConsoleColor.Red;
          Console.WriteLine($"The file {createdatabaseFileFullPath} does not exist!");
          Console.ResetColor();

          continue;
        }

        if (!processedCreateFilenames.Add(createdatabaseFileFullPath))
        {
          string newCreateFileName = IndexedFilename(environmentPath, Path.GetFileNameWithoutExtension(sqlFileName), Path.GetExtension(sqlFileName));
          var mewCreatedatabaseFileFullPath = Path.Combine(environmentPath, newCreateFileName);
          File.Copy(createdatabaseFileFullPath, mewCreatedatabaseFileFullPath);

          ((JValue)createDbCommandLineNode).Value = createDbCommandLine.Replace(sqlFileName, newCreateFileName);
        }
      }
    }

    private string IndexedFilename(string path, string fileName, string extension)
    {
      int ix = 0;
      string filename = null;
      string fullpathFileName = null;
      do
      {
        ix++;
        filename = String.Format(@"{0}{1}{2}", fileName, ix, extension);
        fullpathFileName = Path.Combine(path, filename);
      } while (File.Exists(fullpathFileName));

      return filename;
    }

    private void PatchCleanSection(List<string> sqlFileNames)
    {
      // check whether the clean_up property already exists
      var cleanUpNode = this.jsonObject.SelectTokens("clean_up", errorWhenNoMatch: false)!.FirstOrDefault();
      if (cleanUpNode != null)
      {
        // Меняем имя соединения.
        var runCommandLineNode = this.jsonObject!.SelectTokens("clean_up[*].actions[*].run.code.code", errorWhenNoMatch: false)!.FirstOrDefault();
        if (runCommandLineNode != null)
        {
          var createDbCommandLine = (string)runCommandLineNode!;
          string currentServerName = RegexHelper.ExtractServerName(createDbCommandLine)!;
          if (currentServerName != null)
          {
            ((JValue)runCommandLineNode).Value = createDbCommandLine.Replace(currentServerName, Constants.AffordableConnectionName);
          }
        }

        // Меняем имя timeout.
        var timeOutNode = this.jsonObject!.SelectTokens("clean_up[*].actions[*].timeout", errorWhenNoMatch: false)!.FirstOrDefault();
        if (timeOutNode != null)
        {
          ((JValue)timeOutNode).Value = 120000;
        }

        // do nothing if it does. 
        return;
      }

      List<string> cleanUpFileNames = new();

      string cleanUpSection =
$@"[{{
""when"":""Always"",
""actions"": [";



      for (int i = 0; i < sqlFileNames.Count; i++)
      {
        string createFileName = sqlFileNames[i];
        string cleanUpFileName = createFileName.Insert(createFileName.IndexOf(".sql"), "_CleanUp");
        cleanUpFileNames.Add(cleanUpFileName);

        if (i > 0)
        {
          cleanUpSection += ",";
        }
        cleanUpSection +=
          $@"
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
          ";

      }

      cleanUpSection +=
        $@"
]  
    
}}]
";

      this.jsonObject.Add("clean_up", JProperty.Parse(cleanUpSection));

      Console.ForegroundColor = ConsoleColor.Green;
      Console.WriteLine($"The clean_up section has been added to an environment!");
      Console.ResetColor();

      CreateCleanUpFile(cleanUpFileNames);
    }

    private void CreateCleanUpFile(List<string> cleanUpFileNames)
    {
      var environmentPath = Path.GetDirectoryName(this.environmentFullPath)!;
      foreach (var cleanUpFileName in cleanUpFileNames)
      {
        var cleanUpFileFullPath = Path.Combine(environmentPath, cleanUpFileName);

        if (!File.Exists(cleanUpFileFullPath))
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
}
