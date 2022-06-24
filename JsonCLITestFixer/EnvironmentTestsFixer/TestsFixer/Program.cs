// See https://aka.ms/new-console-template for more information
//Console.WriteLine("Hello, World!");

using TestsFixer;

Console.ResetColor();
Console.WriteLine(@"Please, enter a path to the tests like D:\Projects\commandlinetestsMR2\Tests\SqlServer\Studio\SchemaComparer\Functions\");

string pathToTests = @"D:\Projects\commandlinetestsMR2\Tests\SqlServer\Studio\SchemaComparer\Functions\";
//string pathToTests = Console.ReadLine()!;


if (!Directory.Exists(pathToTests))
{
  Console.WriteLine($"Directory ${pathToTests} does not exist!");
  return;
}

const string environmentsPath = "Environments";
if (!PathTraversal.TraverseTreeUp(pathToTests, environmentsPath, out var foundEnvironmentsFullPath))
{
  throw new DirectoryNotFoundException($"The {environmentsPath} path is not found!");
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
  Console.WriteLine($"Start patching the environment {environment.Id}-{environment.Name}"); 
  if (environment.Patch())
  {
    environment.Tests.ForEach(t => t.Patch());
    Console.ForegroundColor = ConsoleColor.Green;
    Console.WriteLine($"The environment {environment.Id}-{environment.Name} has been patched successfully!");
    Console.ResetColor();
  }
  else
  {
    Console.ForegroundColor = ConsoleColor.Yellow;
    Console.WriteLine($"The environment {environment.Id}-{environment.Name} was not patched. The reason: {environment.PatchError}");
    Console.ResetColor();
    // the following tests were not patched because environment was not patached.
    //environment.Tests.
  }
  
}

environmentsHolder.SaveChanges();
testsHolder.SaveChanges();

Console.ReadKey();

/*
  Построение объектов. 
  Environments, tests и связь между ними. 


  Прослеживается несколько этапов:
  Сбор информации(имена баз данных, есть ли clean_up?)
  
  

 
  Указываем папку c тестами для преобразований.
  Сканируем все Environments из этих тестов.
  Находим эти Environments.
  
  Строим связи Environment[1]->tests[N]
  Для каждого Environment получаем /inputfile:\"UDT-Extended Properties-Databases.sql\""
  парсим на предмет имён баз данных(да их там может быть несколько)
  
  
  Update:
    Environment  
      -Добавить Clean_Up секцию.
      -Добавить Clean_Up файлик.
      -Поменять имя сервера в "code": "%dbforgesql% /execute /connection:%sqlserver2019% /inputfile:\"UDT-Extended Properties-Databases.sql\""
      -Замена тайм-аутов  
    
    Test 
      -Менять connection в тестах "code": "%dbforgesql% /scriptsfolder /connection:%sqlserver2019% /database:SC1_aaed6874_8f24_4866_ae26_586a341b2dc1 /path:source_scripts1" 
      -Менять имя базы в тестах "code": "%dbforgesql% /scriptsfolder /connection:%dbfsqlsrv:SQLLAST(su)*-NoEncrypt% /database:SC1 /path:source_scripts" 
      -Поменять имена баз данных в файлха шаблонов *.scomp(в connectionstring так и в структуре файла там где target)
   

  Подумать процессорный движок так,
  что на определённом этапе вызываются процессоры, которые процессят json(добавляют, изменяют инфу) 
 
  Объект содержит массив JArray объектов. 
  также содержит массив сконвертированных из них POCO объектов, которые реально содержат ссылки на JObjectы
  На уровне фасада выставляем только POCO объекты. Доступ черехз интерфейсы. 
    

 
*/