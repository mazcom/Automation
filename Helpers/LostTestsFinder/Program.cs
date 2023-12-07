// See https://aka.ms/new-console-template for more information

using Newtonsoft.Json.Linq;

Console.ResetColor();
Console.WriteLine(@"Please, enter a path to the tests like D:\Projects\commandlinetestsMaster\Tests\PostgreSQL\Studio\");
//string pathToTests = @"D:\Projects\commandlinetestsMaster\Tests\MySql\Studio\";
string pathToTests = @"D:\Projects\commandlinetestsMaster\Tests\PostgreSQL\Studio\";
//string pathToTests = Console.ReadLine()!;


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
HashSet<Guid> testsInTestPlans = new();

Dictionary<Guid, string> pathsToTheLostTests = new();

string s = string.Empty; 

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
      var id = (Guid)jsonObject.SelectToken("id")!;
      if (!allTests.Add(id))
      {
        s += Environment.NewLine + id.ToString();
      }


      if (!pathsToTheLostTests.ContainsKey(id))
      {
        pathsToTheLostTests.Add(id, testFile);
      }
    }
  }
}

// Retrieve tests from the test plans.


Console.WriteLine();
Console.ForegroundColor = ConsoleColor.Green;
Console.WriteLine($"Total tests in the _definition.tests files count = {allTests.Count}");
Console.WriteLine($"Total tests in the *.testplan files count = {testsInTestPlans.Count}");
Console.WriteLine($"Completed!");
Console.ResetColor();
Console.ReadKey();
