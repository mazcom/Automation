// See https://aka.ms/new-console-template for more information

using LostTestsBetweenBranchesFinder;
using Newtonsoft.Json.Linq;

Console.ResetColor();
Console.WriteLine(@"Please, enter a path to the tests(FeatureBranch)  like D:\Projects\commandlinetests1\Tests\SqlServer\Studio\");
//string pathToTestsFeatureBranch = @"D:\Projects\commandlinetestsMR2\Tests\SqlServer\Studio\";
string pathToTestsFeatureBranch = Console.ReadLine()!;

Console.WriteLine(@"Please, enter a path to the tests(StableBranch)  like D:\Projects\commandlinetests2\Tests\SqlServer\Studio\");
//string pathToTestsStableBranch = @"D:\Projects\commandlinetestsMR3\Tests\SqlServer\Studio\";
string pathToTestsStableBranch = Console.ReadLine()!;

if (!Directory.Exists(pathToTestsFeatureBranch))
{
  Console.ForegroundColor = ConsoleColor.Red;
  Console.WriteLine($"Directory ${pathToTestsFeatureBranch} does not exist!");
  Console.ResetColor();
  return;
}

if (!Directory.Exists(pathToTestsStableBranch))
{
  Console.ForegroundColor = ConsoleColor.Red;
  Console.WriteLine($"Directory ${pathToTestsStableBranch} does not exist!");
  Console.ResetColor();
  return;
}

Console.WriteLine($"Start Processing...");

var testsFeatureBranchFullInfo = InfoRetriever.Retrieve(pathToTestsFeatureBranch);
var testsStableBranchFullInfo = InfoRetriever.Retrieve(pathToTestsStableBranch);

var testsFeatureBranch = new HashSet<Guid>(testsFeatureBranchFullInfo.Keys);
var testsStableBranch = new HashSet<Guid>(testsStableBranchFullInfo.Keys);

var testsExistingInStableOnly = testsStableBranch.Except(testsFeatureBranch).ToList();

if (testsExistingInStableOnly.Count == 0)
{
  Console.ForegroundColor = ConsoleColor.Green;
  Console.WriteLine($"Все тесты из бренча Stable присутствуют в бренче Feature!");
  Console.ResetColor();
}
else
{
  Console.ForegroundColor = ConsoleColor.Yellow;
  Console.WriteLine($@"Следующие тесты из бренча Stable отсутствуют в бренче Feature:");
  Console.WriteLine($@"Общее количество таких тестов: ""{testsExistingInStableOnly.Count}""");
  foreach (var testId in testsExistingInStableOnly)
  {
    var path = testsStableBranchFullInfo[testId];
    Console.WriteLine($"Тест: {testId}. Path: {Path.GetDirectoryName(path)} ");
  }
  Console.ResetColor();
}

Console.WriteLine();
Console.ForegroundColor = ConsoleColor.Green;
Console.WriteLine($"Completed!");
Console.ResetColor();
