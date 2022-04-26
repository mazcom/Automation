// See https://aka.ms/new-console-template for more information

using DbNamesFixer;
using DbNamesFixer.Model;
using Newtonsoft.Json.Linq;

Console.ResetColor();
Console.WriteLine(@"Please, enter a path to tests like D:\Projects\commandlinetests2\Tests\SqlServer\Studio\Documenter\");

// pathToTests = @"D:\Projects\commandlinetests2\Tests\SqlServer\Studio\Documenter\";

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

    foreach (var jsonObject in jsonObjects)
    {
      var environment = (Guid)jsonObject.SelectToken("environment")!;

      // Нас интересуют тесты только с нулевым environment!
      if (environment != Guid.Empty)
      {
        continue;
      }

      // Если нет раздела, где мы используем файлы с созданием баз данных, то ничего не делаем.
      var pre_runs = jsonObject.SelectToken("pre_run")!?.ToArray();
      if (pre_runs == null)
      {
        continue;
      }

      TestModel test = new() { Id = (Guid)jsonObject.SelectToken("id")! };

      // СОЗДАНИЕ БАЗЫ ДАННЫХ.
      foreach (var token in jsonObject.SelectTokens("pre_run[*].run.code.code", errorWhenNoMatch: false)!.
        Where(t => ((string)t!).Contains(".sql", StringComparison.OrdinalIgnoreCase))!)
      {
        var createDbCommandLine = (string)token!;
        // Получаем имя базы данных из теста like Databases.sql
        var createDbFileName = FilesHelper.ExtractFileName(createDbCommandLine)!;
        // Полный путь к файлу создания базы данных.
        string initDbFullFileName = Path.Combine(currentDir, createDbFileName);
        test.CreateDbFiles.Add(new FileModel() { FullPath = initDbFullFileName, FileName = createDbFileName });
      }

      // УДАЛЕНИЕ БАЗЫ ДАННЫХ.
      foreach (var token in jsonObject.SelectTokens("post_run[*].actions[*].run.code.code", errorWhenNoMatch: false)!.
        Where(t => ((string)t!).Contains(".sql", StringComparison.OrdinalIgnoreCase))!)
      {
        var cleanDbCommandLine = (string)token!;
        // Получаем имя базы данных из теста like CleanUp.sql
        var cleanUpDbFileName = FilesHelper.ExtractFileName(cleanDbCommandLine)!;
        // Полный путь к файлу создания базы данных.
        string cleanDbFullFileName = Path.Combine(currentDir, cleanUpDbFileName);
        test.CleanDbFiles.Add(new FileModel() { FullPath = cleanDbFullFileName, FileName = cleanUpDbFileName });
      }

      test.JsonObject = (JObject)jsonObject;
      tests.Add(test);

      if (test.CleanDbFiles.Count == 0)
      {
        Console.WriteLine();
        Console.ForegroundColor = ConsoleColor.Yellow;
        Console.WriteLine($"$ The test with id={test.Id} in the file ${testFile} does not have a clean.sql file!");
        Console.ResetColor();
      }
    }
  }

  //
  // Adjust database names.
  //

  // сопоставляем имя файла и индекс.
  Dictionary<string, int> origFileIndex = new();

  // Имя оригинального файла и его оригинальный контент
  Dictionary<string, string> origFileContent = new();

  foreach (var test in tests)
  {

    List<string> newCreateDbFiles = new();
    List<string> newCleanDbFiles = new();

    foreach (var createDbFileObj in test.CreateDbFiles)
    {
      string currentCreateDbShortFileName = createDbFileObj.FileName!;
      string currentCreateDbFileName = createDbFileObj.FullPath!;

      if (!origFileIndex.ContainsKey(currentCreateDbFileName))
      {
        origFileIndex[currentCreateDbFileName] = 0;

        // Получаем оригинальный контент из файла создания базы данных.
        using StreamReader srCreateDb = new StreamReader(currentCreateDbFileName);
        origFileContent[currentCreateDbFileName] = srCreateDb.ReadToEnd();
      }

      // Create Db File
      string newCreateDbFileName;
      int dbCreateIndex = origFileIndex[currentCreateDbFileName];
      if (dbCreateIndex == 0)
      {
        newCreateDbFileName = currentCreateDbFileName;
        origFileIndex[currentCreateDbFileName] = 1;
      }
      else
      {
        newCreateDbFileName = FilesHelper.AddIndex(currentCreateDbFileName, dbCreateIndex);
        origFileIndex[currentCreateDbFileName] = dbCreateIndex + 1;
        // Создаеём новый новый файл и заполняем его иригинальным коннентом. 
        using StreamWriter swCreateDb = new StreamWriter(newCreateDbFileName);
        swCreateDb.Write(origFileContent[currentCreateDbFileName]);
      }

      // Меняем имена файлов в json-тест-объекте.
      // Create DB
      var jsonObj = (JValue)test.JsonObject!.SelectTokens("pre_run[*].run.code.code", errorWhenNoMatch: false)!
        .FirstOrDefault(t => ((string)t!).Contains($"{currentCreateDbShortFileName}", StringComparison.OrdinalIgnoreCase))!;
      if (jsonObj != null)
      {
        var dbComandLine = jsonObj.Value as string;
        jsonObj.Value = dbComandLine!.Replace(currentCreateDbShortFileName, Path.GetFileName(newCreateDbFileName));
      }

      newCreateDbFiles.Add(newCreateDbFileName);
    }

    foreach (var cleanDbFileObj in test.CleanDbFiles)
    {
      string currentCleanDbShortFileName = cleanDbFileObj.FileName!;
      string currentCleanDbFileName = cleanDbFileObj.FullPath!;

      if (!origFileIndex.ContainsKey(currentCleanDbFileName))
      {
        origFileIndex[currentCleanDbFileName] = 0;

        // Получаем оригинальный контент из файла создания базы данных.
        using StreamReader srCreateDb = new StreamReader(currentCleanDbFileName);
        origFileContent[currentCleanDbFileName] = srCreateDb.ReadToEnd();
      }

      // clean Db File
      string newCleanDbFileName;
      int dbCleanIndex = origFileIndex[currentCleanDbFileName];
      if (dbCleanIndex == 0)
      {
        newCleanDbFileName = currentCleanDbFileName;
        origFileIndex[currentCleanDbFileName] = 1;
      }
      else
      {
        newCleanDbFileName = FilesHelper.AddIndex(currentCleanDbFileName, dbCleanIndex);
        origFileIndex[currentCleanDbFileName] = dbCleanIndex + 1;
        // Создаеём новый новый файл и заполняем его иригинальным коннентом. 
        using StreamWriter swCreateDb = new StreamWriter(newCleanDbFileName);
        swCreateDb.Write(origFileContent[currentCleanDbFileName]);
      }

      // Меняем имена файлов в json-тест-объекте.
      // Create DB
      var jsonObj = (JValue)test.JsonObject!.SelectTokens("post_run[*].actions[*].run.code.code", errorWhenNoMatch: false)!
        .FirstOrDefault(t => ((string)t!).Contains($"{currentCleanDbShortFileName}", StringComparison.OrdinalIgnoreCase))!;
      if (jsonObj != null)
      {
        var dbComandLine = jsonObj.Value as string;
        jsonObj.Value = dbComandLine!.Replace(currentCleanDbShortFileName, Path.GetFileName(newCleanDbFileName));
      }

      newCleanDbFiles.Add(newCleanDbFileName);
    }

    // меняем имена баз данных в файлах.
    foreach (var newCreateDbFile in newCreateDbFiles)
    {
      // Меняем имена баз данных в файлах создания баз данных.  
      if (DBNamesReplacer.GenerateAndReplace(newCreateDbFile, out List<Tuple<string, string>> oldNewNames))
      {

        newCleanDbFiles.ForEach(cf =>
        {
          // Меняем имена в файлах очистки баз данных.
          foreach (var oldNewValue in oldNewNames)
          {
            DBNamesReplacer.Replace(cf, oldNewValue.Item1, oldNewValue.Item2);
          }
        });
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
