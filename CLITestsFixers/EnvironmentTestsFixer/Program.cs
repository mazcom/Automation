using Common;
using EnvironmentTestsFixer;

Console.ResetColor();
Console.WriteLine(@"Please, enter a path to the tests like D:\Projects\commandlinetestsMR2\Tests\SqlServer\Studio\SchemaComparer\Functions\");

string pathToTests = @"D:\Projects\commandlinetestsMR2\Tests\SqlServer\Studio\SchemaComparer\Options\Comparison\IgnoreWithEncryption";
//string pathToTests = @"D:\Projects\commandlinetestsMR2\Tests\SqlServer\Studio\SchemaComparer\Functions\Authorization\";
//string pathToTests = Console.ReadLine()!;


if (!Directory.Exists(pathToTests))
{
  Console.WriteLine($"Directory ${pathToTests} does not exist!");
  return;
}

if (!PathTraversal.TraverseTreeUp(pathToTests, Constants.EnvironmentsPath, out var foundEnvironmentsFullPath))
{
  throw new DirectoryNotFoundException($"The {Constants.EnvironmentsPath} path is not found!");
}

// Create structure of the objects: environments, tests and bind them.
Console.WriteLine("Collecting the tests...");
TestsHolder testsHolder = new(pathToTests);
Console.WriteLine("Collecting the environments...");
EnvironmentsHolder environmentsHolder = new(testsHolder.AllTests, environmentsPath: foundEnvironmentsFullPath);

Console.WriteLine($"Found total tests count {testsHolder.AllTests.Count}");
Console.WriteLine($"Found total environments count {environmentsHolder.Environments.Count}");

// Patch environments and tests.
foreach (var environment in environmentsHolder.Environments)
{
  Console.WriteLine($"Start patching the environment {environment.Id}-{environment.Name}..."); 
  if (environment.Patch())
  {
    Console.WriteLine($"The environment {environment.Id}-{environment.Name} has been successfully patched!");
    environment.Tests.ForEach(t =>
    {
      Console.WriteLine($"Start patching the test {environment.Id}-{environment.Name}...");
      t.Patch();
    });
  }
  else
  {
    Console.ForegroundColor = ConsoleColor.Yellow;
    Console.WriteLine($"The environment {environment.Id}-{environment.Name} was not patched. The reason: {environment.PatchError}");
    Console.ResetColor();
  }
  
}

environmentsHolder.SaveChanges();
testsHolder.SaveChanges();

Console.WriteLine($"The patching has been completed!");

Console.ReadKey();
