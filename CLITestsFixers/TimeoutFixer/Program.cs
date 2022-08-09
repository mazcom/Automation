// See https://aka.ms/new-console-template for more information

using EnterpriseFixer;
using Newtonsoft.Json.Linq;

Console.ResetColor();
Console.WriteLine(@"Enterprise Fixer ver 1.0");
Console.WriteLine(@"Please, enter a path to the tests like D:\Projects\commandlinetests\Tests\SqlServer\Studio\");
//string pathToTests = @"D:\Projects\commandlinetestsMR2\Tests\SqlServer\Studio\";
string pathToTests = Console.ReadLine()!;

Console.WriteLine(@"Please, enter a timeout value in milliseconds, for example: 120000");
int timeout = Convert.ToInt32(Console.ReadLine()!);
//int timeout = 120000;


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

foreach (var testFile in testFiles)
{
  bool wasFixed = false;
  using (StreamReader sr = new(testFile))
  {
    string json = sr.ReadToEnd();
    JArray jsonObjects = JArray.Parse(json);

    foreach (JObject jsonObject in jsonObjects)
    {
      if (TimeoutFixer.TryFix(jsonObject, timeout))
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
