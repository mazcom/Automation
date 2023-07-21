// See https://aka.ms/new-console-template for more information

using Common;
using Common.Model;
using Newtonsoft.Json.Linq;
using NoEnvironmentTestsFixer;
using NoEnvironmentTestsFixer.Model;
using System.Text.Json.Nodes;
using static System.Net.Mime.MediaTypeNames;

Console.ResetColor();
Console.WriteLine(@"Please, enter a path to the tests like D:\Projects\commandlinetestsMaster\Tests\MySql\Studio\DataCompare\");

//string pathToTests = @"D:\Projects\commandlinetestsMR2\Tests\SqlServer\Studio\Documenter\";
//string pathToTests = @"D:\Projects\commandlinetestsMaster2\Tests\PostgreSQL\Studio\Data Compare\AutoMapping\ComparisonKey\";
//string pathToTests = @"D:\Projects\commandlinetestsMaster2\Tests\PostgreSQL\Studio\Data Compare\AutoMapping\";
//string pathToTests = @"D:\Projects\commandlinetestsMaster\Tests\MySql\Studio\DataCompare\";
string pathToTests = Console.ReadLine()!;

if (!Directory.Exists(pathToTests))
{
  Console.WriteLine($"Directory ${pathToTests} does not exist!");
  return;
}


string[] files = Directory.GetFiles(pathToTests,
            "_definition.tests",
            SearchOption.AllDirectories);

foreach (var testFile in files)
{
  string currentTestFileName = testFile;
  string currentDir = Path.GetDirectoryName(currentTestFileName)!;

  List<TestModel> tests = new();
  JArray jsonObjects;

  using (StreamReader sr = new(currentTestFileName))
  {
    string json = sr.ReadToEnd();
    jsonObjects = JArray.Parse(json);

    foreach (JObject jsonObject in jsonObjects)
    {
      TestModel test = new(jsonObject, testFile);
      tests.Add(test);
    }
  }

  foreach (var test in tests)
  {

    var nodes = test.JsonObject!.SelectTokens("assert.console_output_equal[*].etalon", errorWhenNoMatch: false)!.ToList();

    if (nodes.Count > 0)
    {
      foreach (var node in nodes)
      {
        var standardOutputNode = (string)node.SelectToken("standard_output")!;

        var fileName = Path.GetFileNameWithoutExtension(standardOutputNode);
        var errorFileName = fileName + "_Error_Output.txt";


        var fullPathErrorFileName = Path.Combine(Path.GetDirectoryName(testFile)!, errorFileName);

        if (File.Exists(fullPathErrorFileName))
        {
          var newErrorFileName = errorFileName.Replace("_Error_Output.txt", "_Error.txt");
          File.Copy(fullPathErrorFileName, Path.Combine(Path.GetDirectoryName(testFile)!, newErrorFileName), overwrite: true);

          node["error_output"] = newErrorFileName;
        }

      }
    }
  }


  // Переписываем файл _definition.tests с тестами с уже исправленными именами файлов.
  File.WriteAllText(currentTestFileName, jsonObjects.ToString());
}

Console.WriteLine();
Console.ForegroundColor = ConsoleColor.Green;
Console.WriteLine($"Completed!");
Console.WriteLine($"Total _definition.tests files count = {files.Length}");
Console.ResetColor();

