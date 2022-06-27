﻿using Common;
using Common.Model;
using Newtonsoft.Json.Linq;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace EnvironmentTestsFixer.Model
{
  internal class Test : BaseTest
  {
    private TestsEnvironment environment;

    public Test(JObject jsonObject, string testFullPath) : base(jsonObject, testFullPath)
    {
    }

    //public Guid EnvironmentId { get; }
    //public Guid Id { get; }
    //public string Name { get; }
    //public string Description { get; set; }

    public void SetEnvironment(TestsEnvironment environment)
    {
      this.environment = environment;
    }

    public void Patch()
    {
      PatchDatabaseNames();
      PatchEnterprise();
      PatchDocTemplates(environment.OldNewDatabaseNames);
    }

    public void PatchDatabaseNames()
    {
      var createDbNodes = JsonObject!.SelectTokens("pre_run[*].run.code.code", errorWhenNoMatch: false)!.
        Where(t => ((string)t!).Contains("/database:", StringComparison.OrdinalIgnoreCase))!;


      foreach (JValue token in createDbNodes)
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
        Console.WriteLine($"The databases in the test {Id}-{Name} have been patched!");
        Console.ResetColor();
      }
    }
    /*private void PatchEnterprise()
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

        Console.ForegroundColor = ConsoleColor.Green;
        Console.WriteLine($"The Enterprise section in the test {Id}-{Name} has been added!");
        Console.ResetColor();
      }
    }

    private void PatchFiles(List<Tuple<string, string>> oldNewDbNames)
    {
      List<string> foundFileNames = new();
      var testPath = Path.GetDirectoryName(this.testFullPath)!;

      var runCommandLine = (JValue?)this.jsonObject.SelectTokens("run.test.code.code", errorWhenNoMatch: false)!.FirstOrDefault();
      if (runCommandLine != null)
      {
        var fileNames = RegexHelper.ExtractAnyFileNames((string)runCommandLine!);
        foundFileNames.AddRange(fileNames.Except(foundFileNames));
      }

      var etalonFileName = (JValue?)this.jsonObject.SelectTokens("assert.files_equal[*].etalon", errorWhenNoMatch: false)!.FirstOrDefault();
      if (etalonFileName != null)
      {
        var fileNames = RegexHelper.ExtractAnyFileNames((string)etalonFileName!);
        foundFileNames.AddRange(fileNames.Except(foundFileNames));
      }


      foreach (var file in foundFileNames)
      {
        var extension = Path.GetExtension(file).Replace(".", "");
        var fileFullPath = Path.Combine(testPath, file);

        // Scip files which does not exist, for example actual files 
        if (!File.Exists(fileFullPath))
        {
          continue;
        }

        switch (extension)
        {
          case "scomp":
          case "dcomp":
            PatchCompFile(fileFullPath, oldNewDbNames);
            break;
          case "sql":
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
        //var line = fileLines[i];
        fileLines[i] = DBReplacer.TryToReplaceServerNameInConnectionString(fileLines[i]);
        fileLines[i] = DBReplacer.TryToReplaceDbNameInSchemaSection(fileLines[i], environment.OldNewDatabaseNames);
      }
      File.WriteAllLines(fileName, fileLines);
    }

    private void PatchSqlFile(string fileName, List<Tuple<string, string>> oldNewDbNames)
    {
      DBReplacer.TryToReplaceNamesInSQLFile(fileName, oldNewDbNames);
    }*/
  }
}
