using Newtonsoft.Json.Linq;
using System.Runtime.CompilerServices;
using System.Text.Json.Nodes;
using System.Text.RegularExpressions;
using static System.Net.Mime.MediaTypeNames;

namespace Common.Model
{
  public class BaseTest
  {
    private static HashSet<string> GlobalScriptFolderNames = new();

    public BaseTest(JObject? jsonObject, string testFullPath)
    {
      JsonObject = jsonObject;
      TestFullPath = testFullPath;

      EnvironmentId = Guid.Parse(jsonObject!["environment"]!.Value<string>()!);
      Id = Guid.Parse(jsonObject["id"]!.Value<string>()!);
      Name = jsonObject["name"]!.Value<string>()!;
      Description = jsonObject["description"]!.Value<string>()!;
    }

    public Guid EnvironmentId { get; }

    public JObject? JsonObject { get; set; }

    public Guid Id { get; }

    public string Name { get; }

    public string Description { get; set; }

    public string TestFullPath { get; set; }

    public void PatchEnterprise()
    {
      string[] entkeys = { "files_equal", "directory_equal", "console_output_equal" };

      foreach (var entkey in entkeys)
      {
        var enterprise = JsonObject!.SelectTokens($"assert.{entkey}[*].condition", errorWhenNoMatch: false)!.FirstOrDefault();
        if (enterprise == null)
        {
          var filesEqualNode = JsonObject!.SelectTokens($"assert.{entkey}", errorWhenNoMatch: false)!.FirstOrDefault();
          if (filesEqualNode != null)
          {
            var etalonNode = filesEqualNode.SelectTokens("[*].etalon", errorWhenNoMatch: false)!.FirstOrDefault();
            var actualNode = filesEqualNode.SelectTokens("[*].actual", errorWhenNoMatch: false)!.FirstOrDefault();

            // Секция files_equal может быть не правильно оформлена(а именно не как массив. Поэтому не получим здесь значение.)
            if (etalonNode == null)
            {
              // пробуем исправить ситуацию.
              etalonNode = filesEqualNode.SelectTokens("etalon", errorWhenNoMatch: false)!.FirstOrDefault();
              actualNode = filesEqualNode.SelectTokens("actual", errorWhenNoMatch: false)!.FirstOrDefault();

              if (etalonNode == null)
              {
                Console.ForegroundColor = ConsoleColor.Red;
                Console.WriteLine($"The seaction files_equal was not in correct format.");
                Console.ResetColor();
                return;
              }

              var assertNode = (JObject)JsonObject!.SelectTokens("assert", errorWhenNoMatch: false)!.FirstOrDefault()!;

              string files_equalSection =
  $@"
[
             {{
                ""etalon"" : ""{etalonNode!.Value<string>()}"",
                ""actual"" : ""{actualNode!.Value<string>()}""
             }}     
]  
";
              assertNode!.Property($"{entkey}")!.Remove();
              assertNode!.Add($"{entkey}", JArray.Parse(files_equalSection));
              filesEqualNode = JsonObject!.SelectTokens($"assert.{entkey}", errorWhenNoMatch: false)!.FirstOrDefault();
            }

            string addenterpriseSection;

            if (entkey == "console_output_equal")
            {
              addenterpriseSection =
              $@"{{
""condition"": ""Enterprise"",
""etalon"": {{
  ""standard_output"": ""{etalonNode!.SelectTokens("standard_output", errorWhenNoMatch: false)!.FirstOrDefault().Value<string>().Replace(@"\", @"\\")}""
}}

}}";

            }
            else
            {
              addenterpriseSection =
              $@"{{
""condition"": ""Enterprise"",
""etalon"": ""{etalonNode!.Value<string>().Replace(@"\", @"\\")}"",
""actual"": ""{actualNode!.Value<string>().Replace(@"\", @"\\")}""    
}}";
            }


            ((JArray)filesEqualNode).Add(JObject.Parse(addenterpriseSection));



            Console.ForegroundColor = ConsoleColor.Green;
            Console.WriteLine($"The Enterprise section in the test {Id}-{Name} has been added!");
            Console.ResetColor();
          }
        }
      }
    }

    protected void PatchDatabaseNames(List<Tuple<string, string>> oldNewDbNames)
    {
      var createDbNodes = JsonObject!.SelectTokens("pre_run[*].run.code.code", errorWhenNoMatch: false)!.
        Where(t => ((string)t!).Contains("/database:", StringComparison.OrdinalIgnoreCase))!;


      foreach (JValue token in createDbNodes)
      {
        var createDbCommandLine = (string)token!;

        for (int i = 0; i < oldNewDbNames.Count; i++)
        {
          var oldDatabase = oldNewDbNames[i].Item1;
          var newDatabase = oldNewDbNames[i].Item2;

          if (!createDbCommandLine.Contains(oldDatabase))
          {
            continue;
          }

          createDbCommandLine = createDbCommandLine.Replace(oldDatabase, newDatabase);

          token.Value = createDbCommandLine;
          break;
        }

      }
      if (createDbNodes.Count() > 0)
      {
        Console.ForegroundColor = ConsoleColor.Green;
        Console.WriteLine($"The databases in the test {Id}-{Name} have been patched!");
        Console.ResetColor();
      }
    }

    protected void PatchScriptFolderNames()
    {
      var createFolderNodes = JsonObject!.SelectTokens("pre_run[*].run.code.code", errorWhenNoMatch: false)!.
        Where(t => ((string)t!).Contains("/scriptsfolder", StringComparison.OrdinalIgnoreCase))!;

      var patched = false;

      List<string> newFolderNames = new ();

      foreach (JValue token in createFolderNodes)
      {
        patched = true;
        var createDbCommandLine = (string)token!;

        string currentPathName = RegexHelper.ExtractPathName(createDbCommandLine)!;

        string newPathName = currentPathName;
        if (!GlobalScriptFolderNames.Add(currentPathName))
        {
          newPathName = IndexedFolderName(currentPathName);
          GlobalScriptFolderNames.Add(newPathName);


          token.Value = createDbCommandLine.Replace(currentPathName, newPathName);

          // Меняем имена файлов в json-тест-объекте.
          // Create DB
          var jsonObj = (JValue)JsonObject!.SelectTokens("post_run[*].actions[*].run.code.code", errorWhenNoMatch: false)!
            .FirstOrDefault(t => ((string)t!).Contains($"{currentPathName}", StringComparison.OrdinalIgnoreCase))!;
          if (jsonObj != null)
          {
            var dbComandLine = jsonObj.Value as string;
            if (dbComandLine!.Contains(currentPathName, StringComparison.OrdinalIgnoreCase) && dbComandLine!.Contains("cmd.exe /C RD", StringComparison.OrdinalIgnoreCase))
            {
              jsonObj.Value = $"cmd.exe /C IF EXIST {newPathName} RD /S /Q {newPathName}";//dbComandLine!.Replace(currentPathName, Path.GetFileName(newPathName));

              foreach (JValue? runCommandLine in JsonObject!.SelectTokens("pre_run[*].run.code.code", errorWhenNoMatch: false)!
                 .Where(t => ((string)t!).Contains($"{currentPathName}" , StringComparison.OrdinalIgnoreCase)! && ((string)t!).Contains($"/S /Q 2>", StringComparison.OrdinalIgnoreCase))!)
              {
                runCommandLine!.Value = $"cmd.exe /C \"RD {newPathName} /S /Q 2> nul & MD {newPathName}\"";
              }
            }

            newFolderNames.Add(newPathName);
          }

          foreach (JValue? runCommandLine in JsonObject!.SelectTokens("run.test.code.code", errorWhenNoMatch: false)!)
          {
            var fileNames = RegexHelper.ExtractAnyFileNames((string)runCommandLine!);

            foreach (var fileName in fileNames)
            {
              var testPath = Path.GetDirectoryName(TestFullPath)!;
              var fileFullPath = Path.Combine(testPath, fileName);

              if (File.Exists(fileFullPath))
              {
                string[]? fileLines = File.ReadAllLines(fileFullPath);
                for (int i = 0; i < fileLines.Length; i++)
                {
                  // меняем в теге: <BackupName>AutotestBackup_universal_source</BackupName>
                  string pattern = @"(?<=<XmlTagFolderPath>)(.*)(?=\<\/XmlTagFolderPath>)";
                  var match = Regex.Match(fileLines[i], pattern);
                  if (match.Success)
                  {
                    fileLines[i] = fileLines[i].Replace(currentPathName, newPathName);
                  }

                }
                RegexHelper.WriteAllLines(fileFullPath, fileLines);
              }
            }
          }
        }
      }

      
      if (newFolderNames.Count > 0)
      {
        var preRun = JsonObject!.SelectTokens("pre_run", errorWhenNoMatch: false)!.FirstOrDefault()!;

        if (preRun != null)
        {
          foreach (var folderName in newFolderNames)
          {
            AddPreRunRemoveFolder(preRun as JArray, folderName);
          }
        }
      }
    }

    private void AddPreRunRemoveFolder(JArray prerun, string folderName)
    {

var script =
          $@"
{{
                ""run"" : {{
                            ""code"" : {{
                                         ""type"" : ""cmd"",
                                         ""code"" : ""cmd.exe /C IF EXIST {folderName} RD /S /Q {folderName}""
                            }}  
                }},
                ""exit_codes"":[
                  0
                ],
                ""timeout"" : 120000
             }}
          ";

      prerun.Insert(0, JProperty.Parse(script));


    }


    private string IndexedFolderName(string name)
    {
      int ix = 0;
      string folderName = null;
      do
      {
        ix++;
        folderName = String.Format(@"{0}{1}", name, ix);
      } while (GlobalScriptFolderNames.Contains(folderName));

      return folderName;
    }

    protected void PatchTimeout()
    {
      var timeOutNodes = JsonObject!.SelectTokens("..timeout", errorWhenNoMatch: false)!;
      if (timeOutNodes != null)
      {
        foreach (var timeOutNode in timeOutNodes)
        {
          var timeOut = (JValue)timeOutNode;
          timeOut.Value = 120000;
        }
      }
    }

    protected void PatchServerName(List<Tuple<string, string>> oldNewDbNames)
    {
      var createDbNodes = JsonObject!.SelectTokens("pre_run[*].run.code.code", errorWhenNoMatch: false)!.
        Where(t => ((string)t!).Contains("/connection:", StringComparison.OrdinalIgnoreCase))!;

      foreach (JValue token in createDbNodes)
      {
        var createDbCommandLine = (string)token!;

        for (int i = 0; i < oldNewDbNames.Count; i++)
        {
          var oldDatabase = oldNewDbNames[i].Item1;
          var newDatabase = oldNewDbNames[i].Item2;

          string currentServerName = RegexHelper.ExtractServerName(createDbCommandLine)!;
          if (currentServerName != null)
          {
            createDbCommandLine = createDbCommandLine.Replace(currentServerName, ConnectionHelper.GetConnectionName(currentServerName));
          }

          token.Value = createDbCommandLine;
          break;
        }

      }
      if (createDbNodes.Count() > 0)
      {
        Console.ForegroundColor = ConsoleColor.Green;
        Console.WriteLine($"The servername in the test {Id}-{Name} have been patched!");
        Console.ResetColor();
      }
    }

    protected void PatchBackupName()
    {
      var nodes = JsonObject!.SelectTokens("pre_run[*].run.code.code", errorWhenNoMatch: false)!.
        Where(t => ((string)t!).Contains(".bak", StringComparison.OrdinalIgnoreCase))!;

      var nodes2 = JsonObject!.SelectTokens("post_run[*].actions[*].run.code.code", errorWhenNoMatch: false)!.
        Where(t => ((string)t!).Contains(".bak", StringComparison.OrdinalIgnoreCase))!;

      nodes = nodes.Concat(nodes2);

      foreach (JValue runToken in nodes)
      {
        var сommandLine = (string)runToken!;

        var fileNames = RegexHelper.ExtractAnyFileNames((string)сommandLine!);
        string fileName = fileNames.FirstOrDefault(f => f.EndsWith(".bak"))!;
        if (fileName != null)
        {
          сommandLine = сommandLine.Replace(fileName, fileName.Insert(fileName.IndexOf(".bak"), "_" + Id.ToString().Replace("-", "_")));
          runToken.Value = сommandLine;
        }
      }

      if (nodes.Count() > 0)
      {
        Console.ForegroundColor = ConsoleColor.Green;
        Console.WriteLine($"The *.bak file names in the test {Id}-{Name} have been patched!");
        Console.ResetColor();
      }
    }

    protected void PatchDocTemplates(List<Tuple<string, string>> oldNewDbNames, PatchSession patchSession)
    {

      List<string> foundFileNames = new();
      var testPath = Path.GetDirectoryName(TestFullPath)!;

      foreach (JValue? runCommandLine in JsonObject!.SelectTokens("run.test.code.code", errorWhenNoMatch: false)!)
      {
        var fileNames = RegexHelper.ExtractAnyFileNames((string)runCommandLine!);
        foundFileNames.AddRange(fileNames.Except(foundFileNames));
      }

      foreach (JValue? runCommandLine in JsonObject!.SelectTokens("pre_run[*].run.code.code", errorWhenNoMatch: false)!)
      {
        var fileNames = RegexHelper.ExtractAnyFileNames((string)runCommandLine!);
        foundFileNames.AddRange(fileNames.Except(foundFileNames));
      }

      //var runCommandLine = (JValue?)JsonObject!.SelectTokens("run.test.code.code", errorWhenNoMatch: false)!.FirstOrDefault();
      //if (runCommandLine != null)
      //{
      //  var fileNames = RegexHelper.ExtractAnyFileNames((string)runCommandLine!);
      //  foundFileNames.AddRange(fileNames.Except(foundFileNames));
      //}



      var etalonFileName = (JValue?)JsonObject!.SelectTokens("assert.files_equal[*].etalon", errorWhenNoMatch: false)!.FirstOrDefault();
      if (etalonFileName != null)
      {
        var fileNames = RegexHelper.ExtractAnyFileNames((string)etalonFileName!);
        foundFileNames.AddRange(fileNames.Except(foundFileNames));
      }


      foreach (var file in foundFileNames)
      {
        var extension = Path.GetExtension(file).Replace(".", "");
        var fileFullPath = Path.Combine(testPath, file);

        // Если файла нет, то ничего не делаем. Можем просто некорректно выпарсить из командной строки имя файла.
        if (!File.Exists(fileFullPath))
        {
          continue;
        }

        // Skip files which does not exist, for example actual files 
        if (!File.Exists(fileFullPath) || patchSession.PatchedFiles.Contains(fileFullPath))
        {
          continue;
        }

        switch (extension)
        {
          case "scomp":
            patchSession.PatchedFiles.Add(fileFullPath);
            PatchCompFile(fileFullPath, oldNewDbNames);
            PatchScompBackupFile(fileFullPath);
            break;
          case "dcomp":
            patchSession.PatchedFiles.Add(fileFullPath);
            PatchCompFile(fileFullPath, oldNewDbNames);
            break;
          case "det":
          case "dit":
          case "backup":
          case "dgen":
            patchSession.PatchedFiles.Add(fileFullPath);
            PatchOtherFile(fileFullPath, oldNewDbNames);
            break;
          case "sql":
            patchSession.PatchedFiles.Add(fileFullPath);
            PatchSqlFile(fileFullPath, oldNewDbNames);
            //PatchSqlFileForBackups(fileFullPath);
            break;
          default:
            break;
        }
      }
    }

    private void PatchCompFile(string fileName, List<Tuple<string, string>> oldNewDbNames)
    {
      string[]? fileLines = File.ReadAllLines(fileName);

      // Find old db names, generate new names and replace where it occurs first time. 
      for (int i = 0; i < fileLines.Length; i++)
      {
        fileLines[i] = DBReplacer.TryToReplaceServerNameInConnectionString(fileLines[i]);
        fileLines[i] = DBReplacer.TryToReplaceServerNameMySQLInConnectionString(fileLines[i]);
        
        fileLines[i] = DBReplacer.TryToReplaceDbNameInSchemaSection(fileLines[i], oldNewDbNames);
        fileLines[i] = DBReplacer.TryToReplaceDbNameInDataConnectionSection(fileLines[i]);
      }
      File.WriteAllLines(fileName, fileLines);
    }

    private void PatchOtherFile(string fileName, List<Tuple<string, string>> oldNewDbNames)
    {
      string[]? fileLines = File.ReadAllLines(fileName);

      // Find old db names, generate new names and replace where it occurs first time. 
      for (int i = 0; i < fileLines.Length; i++)
      {
        fileLines[i] = DBReplacer.TryToReplaceServerNameInConnectionString(fileLines[i]);
        fileLines[i] = DBReplacer.TryToReplaceServerNameMySQLInConnectionString(fileLines[i]);
        fileLines[i] = DBReplacer.TryToReplaceDbNameInSchemaSection(fileLines[i], oldNewDbNames);
      }
      RegexHelper.WriteAllLines(fileName, fileLines);
    }

    private void PatchSqlFile(string fileName, List<Tuple<string, string>> oldNewDbNames)
    {
      DBReplacer.TryToReplaceNamesInSQLFile(fileName, oldNewDbNames);
    }

    private void PatchSqlFileForBackups(string fileName)
    {
      // Здесь нам нужно добавлять айдишник теста к именам *.bak и просто.
      // TO DISK = N'D:\Backup\2016XE\AutotestBackup_universal_source.bak'
      // WITH NOFORMAT, INIT, NAME = N'AutotestBackup_universal_source', SKIP, NOREWIND, NOUNLOAD, STATS = 10, NO_COMPRESSION

      string[]? fileLines = File.ReadAllLines(fileName);

      bool backupWasPatched = false;
      // Find old db names, generate new names and replace where it occurs first time. 
      for (int i = 0; i < fileLines.Length; i++)
      {
        string filePattern = @"(\s*TO\s+DISK\s*)";
        var fileMatch = Regex.Match(fileLines[i], filePattern);
        if (fileMatch.Success)
        {
          var fileNames = RegexHelper.ExtractAnyFileNames(fileLines[i]);
          if (fileNames.Length > 0 && fileNames[0].EndsWith(".bak"))
          {
            fileLines[i] = fileLines[i].Replace(fileNames[0], fileNames[0].Insert(fileNames[0].IndexOf(".bak"), "_" + Id.ToString().Replace("-", "_")));
            backupWasPatched = true;
          }
        }

        if (backupWasPatched)
        {
          string pattern = @"(?<=NAME\s*=\s*N')\w+(?=')";
          var match = Regex.Match(fileLines[i], pattern);
          if (match.Success)
          {
            fileLines[i] = fileLines[i].Replace(match.Value, match.Value + "_" + Id.ToString().Replace("-", "_"));
          }
        }

      }

      RegexHelper.WriteAllLines(fileName, fileLines);
    }

    private void PatchScompBackupFile(string fileName)
    {
      // Здесь нам нужно добавлять адишник теста к именам *.bak.

      string[]? fileLines = File.ReadAllLines(fileName);

      // Find old db names, generate new names and replace where it occurs first time. 
      for (int i = 0; i < fileLines.Length; i++)
      {
        // меняем в теге: <BackupName>AutotestBackup_universal_source</BackupName>
        string pattern = @"(?<=<BackupName>)(.*)(?=\<\/BackupName>)";
        var match = Regex.Match(fileLines[i], pattern);
        if (match.Success)
        {
          fileLines[i] = fileLines[i].Replace(match.Value, match.Value + "_" + Id.ToString().Replace("-", "_"));
        }

        // меняем в теге <File Name="D:\Backup\2016XE\AutotestBackup_universal_source.bak" />
        string filePattern = @"(<File\s+Name)";
        var fileMatch = Regex.Match(fileLines[i], filePattern);
        if (fileMatch.Success)
        {
          var fileNames = RegexHelper.ExtractAnyFileNames(fileLines[i]);
          if (fileNames.Length > 0 && fileNames[0].EndsWith(".bak"))
          {
            fileLines[i] = fileLines[i].Replace(fileNames[0], fileNames[0].Insert(fileNames[0].IndexOf(".bak"), "_" + Id.ToString().Replace("-", "_")));
          }
        }
      }

      RegexHelper.WriteAllLines(fileName, fileLines);
    }
  }
}
