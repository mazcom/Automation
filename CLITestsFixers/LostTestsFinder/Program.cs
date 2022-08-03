// See https://aka.ms/new-console-template for more information

using Newtonsoft.Json.Linq;

Console.ResetColor();
Console.WriteLine(@"Please, enter a path to the tests like D:\Projects\commandlinetests\Tests\SqlServer\Studio\");
//string pathToTests = @"D:\Projects\commandlinetestsMR2\Tests\SqlServer\Studio\";
string pathToTests = Console.ReadLine()!;

Console.WriteLine(@"Please, enter a path to the test plans like D:\Projects\commandlinetests\TestPlans\SqlServer\");
//string pathToTestPlans = @"D:\Projects\commandlinetestsMR2\TestPlans\SqlServer\";
string pathToTestPlans = Console.ReadLine()!;

if (!Directory.Exists(pathToTests))
{
  Console.ForegroundColor = ConsoleColor.Red;
  Console.WriteLine($"Directory ${pathToTests} does not exist!");
  Console.ResetColor();
  return;
}

if (!Directory.Exists(pathToTestPlans))
{
  Console.ForegroundColor = ConsoleColor.Red;
  Console.WriteLine($"Directory ${pathToTestPlans} does not exist!");
  Console.ResetColor();
  return;
}

string[] testFiles = Directory.GetFiles(pathToTests,
            "_definition.tests",
            SearchOption.AllDirectories);


string[] testPlanFiles = Directory.GetFiles(pathToTestPlans,
            "*.testplan",
            SearchOption.AllDirectories);

HashSet<Guid> allTests = new();
HashSet<Guid> testsInTestPlans = new();

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
      allTests.Add((Guid)jsonObject.SelectToken("id")!);
    }
  }
}

// Retrieve tests from the test plans.
foreach (var testPlanFile in testPlanFiles)
{
  using (StreamReader sr = new(testPlanFile))
  {
    string json = sr.ReadToEnd();
    JObject jsonObjects = JObject.Parse(json);
    var testsIds = jsonObjects!.SelectTokens("suite[*].id", errorWhenNoMatch: false)!;
    foreach (var testId in testsIds)
    {
      testsInTestPlans.Add(Guid.Parse((testId as JValue).Value.ToString()));
    }
  }
}

Console.WriteLine();

// Находим тесты, которые забыли добавить в какой-либо тест план.
var testsNotAddedToAnyTestPlan = allTests.Except(testsInTestPlans).ToList();
if (testsNotAddedToAnyTestPlan.Count == 0)
{
  Console.ForegroundColor = ConsoleColor.Green;
  Console.WriteLine($"Все тесты из папки {pathToTests} присутствуют в тест-планах!!!");
  Console.ResetColor();
}
else
{
  Console.ForegroundColor = ConsoleColor.Yellow;
  Console.WriteLine($@"Следующие тесты из папки ""{pathToTests}"" отсутствуют в каком-либо тест-плане:");
  Console.WriteLine($@"Общее количество таких тестов: ""{testsNotAddedToAnyTestPlan.Count}""");
  foreach (var testId in testsNotAddedToAnyTestPlan)
  {
    Console.WriteLine($"Тест: {testId} ");
  }
  Console.ResetColor();
}

Console.WriteLine();

// Находим тесты, которые существуют в тест-планах но существуют в папке с тестами.
var testsExistsOnlyInTestPlans = testsInTestPlans.Except(allTests).ToList();
if (testsExistsOnlyInTestPlans.Count == 0)
{
  Console.ForegroundColor = ConsoleColor.Green;
  Console.WriteLine($"Все тесты из тест-планов существуют в тестах!!!");
  Console.ResetColor();
}
else
{
  Console.ForegroundColor = ConsoleColor.Yellow;
  Console.WriteLine($@"Следующие тесты из тест-планов отсутствуют в тестах в папке ""{pathToTests}"" ");
  Console.WriteLine($@"Общее количество таких тестов: {testsExistsOnlyInTestPlans.Count}");
  foreach (var testId in testsExistsOnlyInTestPlans)
  {
    Console.WriteLine($"Тест: {testId} ");
  }
  Console.ResetColor();
}

Console.WriteLine();
Console.ForegroundColor = ConsoleColor.Green;
Console.WriteLine($"Total tests in the _definition.tests files count = {allTests.Count}");
Console.WriteLine($"Total tests in the *.testplan files count = {testsInTestPlans.Count}");
Console.WriteLine($"Completed!");
Console.ResetColor();
