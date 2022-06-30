using Newtonsoft.Json.Linq;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace Common.Model
{
  public class BaseTest
  {

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
      var enterprise = JsonObject!.SelectTokens("assert.files_equal[*].condition", errorWhenNoMatch: false)!.FirstOrDefault();
      if (enterprise == null)
      {
        var filesEqualNode = JsonObject!.SelectTokens("assert.files_equal", errorWhenNoMatch: false)!.FirstOrDefault();
        if (filesEqualNode != null)
        {
          var etalonNode = filesEqualNode.SelectTokens("[*].etalon", errorWhenNoMatch: false)!.FirstOrDefault();
          var actualNode = filesEqualNode.SelectTokens("[*].actual", errorWhenNoMatch: false)!.FirstOrDefault();

          // Секция files_equal может быть не правильно оформлена(а именно не как массив. Поэтому не получим здесь знанчение.)
          if (etalonNode == null)
          {
            return;
          }

          string addenterpriseSection =
$@"{{
""condition"": ""Enterprise"",
""etalon"": ""{etalonNode!.Value<string>()}"",
""actual"": ""{actualNode!.Value<string>()}""    
}}";
          ((JArray)filesEqualNode).Add(JObject.Parse(addenterpriseSection));
        }

        Console.ForegroundColor = ConsoleColor.Green;
        Console.WriteLine($"The Enterprise section in the test {Id}-{Name} has been added!");
        Console.ResetColor();
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
            createDbCommandLine = createDbCommandLine.Replace(currentServerName, Constants.AffordableConnectionName);
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

    protected void PatchDocTemplates(List<Tuple<string, string>> oldNewDbNames, PatchSession patchSession)
    {
      
      List<string> foundFileNames = new();
      var testPath = Path.GetDirectoryName(TestFullPath)!;

      var runCommandLine = (JValue?)JsonObject!.SelectTokens("run.test.code.code", errorWhenNoMatch: false)!.FirstOrDefault();
      if (runCommandLine != null)
      {
        var fileNames = RegexHelper.ExtractAnyFileNames((string)runCommandLine!);
        foundFileNames.AddRange(fileNames.Except(foundFileNames));
      }

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

        // Skip files which does not exist, for example actual files 
        if (!File.Exists(fileFullPath) || patchSession.PatchedFiles.Contains(fileFullPath))
        {
          continue;
        }

        switch (extension)
        {
          case "scomp":
          case "dcomp":
            patchSession.PatchedFiles.Add(fileFullPath);
            PatchCompFile(fileFullPath, oldNewDbNames);
            break;
          case "sql":
            patchSession.PatchedFiles.Add(fileFullPath);
            PatchSqlFile(fileFullPath, oldNewDbNames);
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
        fileLines[i] = DBReplacer.TryToReplaceDbNameInSchemaSection(fileLines[i], oldNewDbNames);
        fileLines[i] = DBReplacer.TryToReplaceDbNameInDataConnectionSection(fileLines[i]);
      }
      File.WriteAllLines(fileName, fileLines);
    }

    private void PatchSqlFile(string fileName, List<Tuple<string, string>> oldNewDbNames)
    {
      DBReplacer.TryToReplaceNamesInSQLFile(fileName, oldNewDbNames);
    }
  }
}
