// See https://aka.ms/new-console-template for more information

using EnterpriseFixer;
using Newtonsoft.Json.Linq;
using System.Xml.Linq;
using System;
using System.Text.RegularExpressions;
using System.Threading.Channels;

Console.ResetColor();
Console.WriteLine(@"Issue converter ver 1.0");
Console.WriteLine(@"Please, enter a path to the tests like D:\Projects\commandlinetests\Tests\SqlServer\Studio\");
//string pathToTests = @"D:\Projects\commandlinetestsMR2\Tests\SqlServer\Studio\";
string pathToTests = Console.ReadLine()!;

if (!Directory.Exists(pathToTests))
{
  Console.ForegroundColor = ConsoleColor.Red;
  Console.WriteLine($"Directory ${pathToTests} does not exist!");
  Console.ResetColor();
  return;
}

string[] testFiles = Directory.GetFiles(pathToTests,
            "_definition.tests",
            SearchOption.AllDirectories);

Console.WriteLine("Processing...");

int testProcessed = 0;

foreach (var testFile in testFiles)
{
  bool wasConverted = false;
  using (StreamReader sr = new(testFile))
  {
    string json = sr.ReadToEnd();
    JArray jsonObjects = JArray.Parse(json);

    foreach (JObject jsonObject in jsonObjects)
    {
      //EnvironmentId = Guid.Parse(jsonObject!["environment"]!.Value<string>()!);
      var id = Guid.Parse(jsonObject["id"]!.Value<string>()!);
      var name = jsonObject["name"]!.Value<string>()!;
      var description = jsonObject["description"]!.Value<string>()!;

      if (BackupTimeOutFixer.TryFix(jsonObject))
      {
        testProcessed++;
        wasConverted = true;
      }
    }

    sr.Close();
    if (wasConverted)
    {
      File.WriteAllText(testFile, jsonObjects.ToString());
    }
  }
}

Console.WriteLine();
Console.ForegroundColor = ConsoleColor.Green;
Console.WriteLine($"Tests processed: {testProcessed}");
Console.WriteLine($"Completed!");
Console.ResetColor();
