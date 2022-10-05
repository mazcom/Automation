// See https://aka.ms/new-console-template for more information

using Newtonsoft.Json.Linq;

Console.ResetColor();
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

HashSet<string> allIssues = new();

// Retrieve all tests id.
foreach (var testFile in testFiles)
{
  //JArray jsonObjects;
  using (StreamReader sr = new(testFile))
  {
    string json = sr.ReadToEnd();
    JArray jsonObjects = JArray.Parse(json);
    foreach (JObject jsonObject in jsonObjects)
    {
      var issue = (string)jsonObject.SelectToken("issue")!;
      if (!string.IsNullOrEmpty(issue))
      {
        if (!allIssues.Contains(issue))
        {
          allIssues.Add(issue);
        }
      }
    }
  }
}

Console.WriteLine("Issues:");
var sorted = allIssues.ToList<string>();
sorted.Sort();
foreach (var issue in sorted)
{
  Console.WriteLine(issue);
}

Console.WriteLine();
Console.ForegroundColor = ConsoleColor.Green;
Console.WriteLine($"Total issues count = {allIssues.Count}");
Console.WriteLine($"Completed!");
Console.ResetColor();
