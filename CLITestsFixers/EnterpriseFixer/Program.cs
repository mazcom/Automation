// See https://aka.ms/new-console-template for more information

using EnterpriseFixer;
using Newtonsoft.Json.Linq;

Console.ResetColor();
Console.WriteLine(@"Enterprise Fixer ver 1.0");
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

HashSet<Guid> allTests = new();


Dictionary<Guid, string> pathsToTheLostTests = new();

string targetCondition = "Enterprise";

// Retrieve all tests id.
foreach (var testFile in testFiles)
{
  bool wasFixed = false;
  using (StreamReader sr = new(testFile))
  {
    string json = sr.ReadToEnd();
    JArray jsonObjects = JArray.Parse(json);

    foreach (JObject jsonObject in jsonObjects)
    {
      if (ConditionHelper.TryFix(jsonObject, targetCondition))
      {
        wasFixed = true;
      }
    }
    sr.Close();
    if (wasFixed)
    {
      File.WriteAllText(testFile, jsonObjects.ToString());
    }
  }
}



Console.WriteLine();
Console.ForegroundColor = ConsoleColor.Green;
Console.WriteLine($"Completed!");
Console.ResetColor();
