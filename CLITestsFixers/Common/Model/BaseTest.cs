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

    private void PatchOtherFile(string fileName, List<Tuple<string, string>> oldNewDbNames)
    {
      string[]? fileLines = File.ReadAllLines(fileName);

      // Find old db names, generate new names and replace where it occurs first time. 
      for (int i = 0; i < fileLines.Length; i++)
      {
        fileLines[i] = DBReplacer.TryToReplaceServerNameInConnectionString(fileLines[i]);
        fileLines[i] = DBReplacer.TryToReplaceDbNameInSchemaSection(fileLines[i], oldNewDbNames);
      }
      File.WriteAllLines(fileName, fileLines);
    }

    private void PatchSqlFile(string fileName, List<Tuple<string, string>> oldNewDbNames)
    {
      DBReplacer.TryToReplaceNamesInSQLFile(fileName, oldNewDbNames);
    }
  }
}
