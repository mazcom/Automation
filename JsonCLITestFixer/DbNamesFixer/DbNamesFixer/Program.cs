// See https://aka.ms/new-console-template for more information

using DbNamesFixer;
using DbNamesFixer.Model;
using Newtonsoft.Json.Linq;

Console.WriteLine(@"Please, provide a path to tests like D:\Projects\commandlinetests2\Tests\SqlServer\Studio\Documenter\");

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

      var pre_runs = jsonObject.SelectToken("pre_run")!?.ToArray();

      if (pre_runs == null)
      {
        continue;
      }

      TestModel test = new() { Id = (Guid)jsonObject.SelectToken("id")! };

      // СОЗДАНИЕ БАЗЫ ДАННЫХ.
      var createDbCommandLine = (string)(jsonObject.SelectTokens("pre_run[*].run.code.code", errorWhenNoMatch: false)!)
       .FirstOrDefault(t => ((string)t!).Contains(".sql", StringComparison.OrdinalIgnoreCase))!;
      if (createDbCommandLine != null)
      {
        // Получаем имя базы данных из теста like Databases.sql
        var createDbFileName = FilesHelper.ExtractFileName(createDbCommandLine)!;
        // Полный путь к файлу создания базы данных.
        string initDbFullFileName = Path.Combine(currentDir, createDbFileName);
        test.CreateDbFiles.Add(new FileModel() { FullPath = initDbFullFileName, FileName = createDbFileName });
      }

      // УДАЛЕНИЕ БАЗЫ ДАННЫХ.
      var cleanDbCommandLine = (string)jsonObject.SelectTokens("post_run[*].actions[*].run.code.code", errorWhenNoMatch: false)!
      .FirstOrDefault(t => ((string)t!).Contains(".sql", StringComparison.OrdinalIgnoreCase))!;
      if (cleanDbCommandLine != null)
      {
        // Получаем имя базы данных из теста like CleanUp.sql
        var cleanUpDbFileName = FilesHelper.ExtractFileName(cleanDbCommandLine)!;
        // Полный путь к файлу создания базы данных.
        string cleanDbFullFileName = Path.Combine(currentDir, cleanUpDbFileName);
        test.CleanDbFiles.Add(new FileModel() { FullPath = cleanDbFullFileName, FileName = cleanUpDbFileName });
      }

      test.JsonObject = (JObject)jsonObject;
      tests.Add(test);
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
    string currentCreateDbShortFileName = test.CreateDbFiles[0].FileName!;
    string currentCleanDbShortFileName = test.CleanDbFiles[0].FileName!;

    string currentCreateDbFileName = test.CreateDbFiles[0].FullPath!;
    string currentCleanDbFileName = test.CleanDbFiles[0].FullPath!;

    if (!origFileIndex.ContainsKey(currentCreateDbFileName))
    {
      origFileIndex[currentCreateDbFileName] = 0;
      origFileIndex[currentCleanDbFileName] = 0;

      // Получаем оригинальный контент из файла создания базы данных.
      using StreamReader srCreateDb = new StreamReader(currentCreateDbFileName);
      origFileContent[currentCreateDbFileName] = srCreateDb.ReadToEnd();

      // Получаем оригинальный контент из файла создания базы данных.
      using StreamReader srCleanDb = new StreamReader(currentCleanDbFileName);
      origFileContent[currentCleanDbFileName] = srCleanDb.ReadToEnd();
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

    // Clean Db File
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
      using StreamWriter swCleanDb = new StreamWriter(newCleanDbFileName);
      swCleanDb.Write(origFileContent[currentCleanDbFileName]);
    }

    // Меняем имена в файлах создания баз данных.
    if (DBNamesReplacer.GenerateAndReplace(newCreateDbFileName, out List<Tuple<string, string>> oldNewNames))
    {
      // Меняем имена в файлах очистки баз данных.
      foreach (var oldNewValue in oldNewNames)
      {
        DBNamesReplacer.Replace(newCleanDbFileName, oldNewValue.Item1, oldNewValue.Item2);
      }
    }

    // Меняем имена файлов в json-тест-объекте.
    // Create DB
    var jsonObj = (JValue)test.JsonObject!.SelectTokens("pre_run[*].run.code.code", errorWhenNoMatch: false)!
      .FirstOrDefault(t => ((string)t!).Contains(".sql", StringComparison.OrdinalIgnoreCase))!;
    if (jsonObj != null)
    {
      var dbComandLine = jsonObj.Value as string;
      jsonObj.Value = dbComandLine!.Replace(currentCreateDbShortFileName, Path.GetFileName(newCreateDbFileName));
    }

    // Clean DB
    jsonObj = (JValue)test.JsonObject!.SelectTokens("post_run[*].actions[*].run.code.code", errorWhenNoMatch: false)!
      .FirstOrDefault(t => ((string)t!).Contains(".sql", StringComparison.OrdinalIgnoreCase))!;
    if (jsonObj != null)
    {
      var dbComandLine = jsonObj.Value as string;
      jsonObj.Value = dbComandLine!.Replace(currentCleanDbShortFileName, Path.GetFileName(newCleanDbFileName));
    }
  }

  // Записываем тесты.
  File.WriteAllText(currentTestFileName, jsonObjects.ToString());
}


Console.WriteLine("Completed!");
