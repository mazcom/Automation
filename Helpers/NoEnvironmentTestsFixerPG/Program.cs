// See https://aka.ms/new-console-template for more information

using Common;
using Common.Model;
using Newtonsoft.Json.Linq;
using NoEnvironmentTestsFixer;
using NoEnvironmentTestsFixer.Model;
using System.Text.Json.Nodes;
using static System.Net.Mime.MediaTypeNames;

Console.ResetColor();
Console.WriteLine(@"Please, enter a path to the tests like D:\Projects\commandlinetestsMR2\Tests\SqlServer\Studio\Documenter\");

//string pathToTests = @"D:\Projects\commandlinetestsMR2\Tests\SqlServer\Studio\Documenter\";
//string pathToTests = @"D:\Projects\commandlinetestsMaster2\Tests\PostgreSQL\Studio\Data Compare\AutoMapping\ComparisonKey\";
//string pathToTests = @"D:\Projects\commandlinetestsMaster2\Tests\PostgreSQL\Studio\Data Compare\AutoMapping\";
string pathToTests = @"D:\Projects\commandlinetestsMaster2\Tests\PostgreSQL\Studio\Data Compare\";

//string pathToTests = Console.ReadLine()!;

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

    foreach (JObject jsonObject in jsonObjects)
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

      TestModel test = new(jsonObject, testFile);
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

    // to do упростить наименование.
    // ориентироваться на свойство test.testfullpath 

    var testName = test.Name.ToCamelCase();
    //var cleanUpFileName = testName + "CleanUp.sql";
    //var createFileName = testName + "Create.sql";
    //var sourceFileName = testName + "Source.sql";
    //var targetFileName = testName + "Target.sql";

    var cleanUpFileName = "CleanUp.sql";
    var createFileName = "Create.sql";
    var sourceFileName = "Source.sql";
    var targetFileName = "Target.sql";


    var dbName1 = "dc1_" + test.Id.ToString().Replace("-", "_");
    var dbName2 = "dc2_" + test.Id.ToString().Replace("-", "_");


    string oldCreateDBObjectsSqlFileName1 = null;
    string oldCreateDBObjectsSqlFileName2 = null;

    var nodes = test.JsonObject!.SelectTokens("pre_run[*].run.code.code", errorWhenNoMatch: false)!.
        Where(t =>
                //((string)t!).Contains("SourceObjects", StringComparison.OrdinalIgnoreCase)
                //|| ((string)t!).Contains("TargetObjects", StringComparison.OrdinalIgnoreCase)
                ((string)t!).Contains("Source.sql", StringComparison.OrdinalIgnoreCase)
                || ((string)t!).Contains("Target.sql", StringComparison.OrdinalIgnoreCase))!.ToList();

    if (nodes.Count == 2)
    {
      oldCreateDBObjectsSqlFileName1 = RegexHelper.ExtractSqlFileNameFromCommandLine((string)nodes[0]!);
      oldCreateDBObjectsSqlFileName2 = RegexHelper.ExtractSqlFileNameFromCommandLine((string)nodes[1]!);

      ArgumentNullException.ThrowIfNull(oldCreateDBObjectsSqlFileName1);
      ArgumentNullException.ThrowIfNull(oldCreateDBObjectsSqlFileName2);
    }
    else
    {

    }


    string preRunJsonString =
$@"[

             {{
                ""run"" : {{
                            ""code"" : {{
                                         ""type"" : ""cmd"",
                                         ""code"" : ""%PostgreSQL_Studio% /execute /connection:{Constants.AffordablePGConnectionName} /inputfile:\""{cleanUpFileName}\""""
                            }}  
                }},
                ""exit_codes"":[
                  0
                ],
                ""timeout"" : 120000
             }},

             {{
                ""run"" : {{
                            ""code"" : {{
                                         ""type"" : ""cmd"",
                                         ""code"" : ""%PostgreSQL_Studio% /execute /connection:{Constants.AffordablePGConnectionName} /inputfile:\""{createFileName}\""""
                            }}  
                }},
                ""exit_codes"":[
                  0
                ],
                ""timeout"" : 120000
             }},

             {{
                ""run"" : {{
                            ""code"" : {{
                                         ""type"" : ""cmd"",
                                         ""code"" : ""%PostgreSQL_Studio% /execute /connection:{Constants.AffordablePGConnectionName} /inputfile:\""{sourceFileName}\"" /database:{dbName1}""
                            }}  
                }},
                ""exit_codes"":[
                  0
                ],
                ""timeout"" : 120000
             }},

             {{
                ""run"" : {{
                            ""code"" : {{
                                         ""type"" : ""cmd"",
                                         ""code"" : ""%PostgreSQL_Studio% /execute /connection:{Constants.AffordablePGConnectionName} /inputfile:\""{targetFileName}\"" /database:{dbName2}""
                            }}  
                }},
                ""exit_codes"":[
                  0
                ],
                ""timeout"" : 120000
             }}

  ]";

    test.JsonObject!["pre_run"] = JValue.Parse(preRunJsonString);


    string cleanUpSection =
$@"[{{
""when"":""Always"",
""actions"": [
             {{
                ""run"" : {{
                            ""code"" : {{
                                         ""type"" : ""cmd"",
                                         ""code"" : ""%PostgreSQL_Studio% /execute /connection:{Constants.AffordablePGConnectionName} /inputfile:\""{cleanUpFileName}\""""
                            }}  
                }},
                ""exit_codes"":[
                  0
                ],
                ""timeout"" : 120000
             }}     
  ]  
    
}}]";
    test.JsonObject!["post_run"] = JValue.Parse(cleanUpSection);


    //List<string> newCreateDbFiles = new();
    //List<string> newCleanDbFiles = new();

    //foreach (var createDbFileObj in test.CreateDbFiles)
    //{
    //  string currentCreateDbShortFileName = createDbFileObj.FileName!;
    //  string currentCreateDbFileName = createDbFileObj.FullPath!;

    //  if (!origFileIndex.ContainsKey(currentCreateDbFileName))
    //  {
    //    origFileIndex[currentCreateDbFileName] = 0;

    //    // Получаем оригинальный контент из файла создания базы данных.
    //    using StreamReader srCreateDb = new StreamReader(currentCreateDbFileName);
    //    origFileContent[currentCreateDbFileName] = srCreateDb.ReadToEnd();
    //  }

    //  // Create Db File
    //  string newCreateDbFileName;
    //  int dbCreateIndex = origFileIndex[currentCreateDbFileName];
    //  if (dbCreateIndex == 0)
    //  {
    //    newCreateDbFileName = currentCreateDbFileName;
    //    origFileIndex[currentCreateDbFileName] = 1;
    //  }
    //  else
    //  {
    //    newCreateDbFileName = FilesHelper.AddIndex(currentCreateDbFileName, dbCreateIndex);
    //    origFileIndex[currentCreateDbFileName] = dbCreateIndex + 1;
    //    // Создаеём новый новый файл и заполняем его иригинальным коннентом. 
    //    using StreamWriter swCreateDb = new StreamWriter(newCreateDbFileName);
    //    swCreateDb.Write(origFileContent[currentCreateDbFileName]);
    //  }

    //  // Меняем имена файлов в json-тест-объекте.
    //  // Create DB
    //  var jsonObj = (JValue)test.JsonObject!.SelectTokens("pre_run[*].run.code.code", errorWhenNoMatch: false)!
    //    .FirstOrDefault(t => ((string)t!).Contains($"{currentCreateDbShortFileName}", StringComparison.OrdinalIgnoreCase))!;
    //  if (jsonObj != null)
    //  {
    //    var dbComandLine = jsonObj.Value as string;
    //    jsonObj.Value = dbComandLine!.Replace(currentCreateDbShortFileName, Path.GetFileName(newCreateDbFileName));
    //  }

    //  newCreateDbFiles.Add(newCreateDbFileName);
    //}

    //foreach (var cleanDbFileObj in test.CleanDbFiles)
    //{
    //  string currentCleanDbShortFileName = cleanDbFileObj.FileName!;
    //  string currentCleanDbFileName = cleanDbFileObj.FullPath!;

    //  if (!origFileIndex.ContainsKey(currentCleanDbFileName))
    //  {
    //    origFileIndex[currentCleanDbFileName] = 0;

    //    // Получаем оригинальный контент из файла создания базы данных.
    //    using StreamReader srCreateDb = new StreamReader(currentCleanDbFileName);
    //    origFileContent[currentCleanDbFileName] = srCreateDb.ReadToEnd();
    //  }

    //  // clean Db File
    //  string newCleanDbFileName;
    //  int dbCleanIndex = origFileIndex[currentCleanDbFileName];
    //  if (dbCleanIndex == 0)
    //  {
    //    newCleanDbFileName = currentCleanDbFileName;
    //    origFileIndex[currentCleanDbFileName] = 1;
    //  }
    //  else
    //  {
    //    newCleanDbFileName = FilesHelper.AddIndex(currentCleanDbFileName, dbCleanIndex);
    //    origFileIndex[currentCleanDbFileName] = dbCleanIndex + 1;
    //    // Создаеём новый новый файл и заполняем его иригинальным коннентом. 
    //    using StreamWriter swCreateDb = new StreamWriter(newCleanDbFileName);
    //    swCreateDb.Write(origFileContent[currentCleanDbFileName]);
    //  }

    //  // Меняем имена файлов в json-тест-объекте.
    //  // Create DB
    //  var jsonObj = (JValue)test.JsonObject!.SelectTokens("post_run[*].actions[*].run.code.code", errorWhenNoMatch: false)!
    //    .FirstOrDefault(t => ((string)t!).Contains($"{currentCleanDbShortFileName}", StringComparison.OrdinalIgnoreCase))!;
    //  if (jsonObj != null)
    //  {
    //    var dbComandLine = jsonObj.Value as string;
    //    jsonObj.Value = dbComandLine!.Replace(currentCleanDbShortFileName, Path.GetFileName(newCleanDbFileName));
    //  }

    //  newCleanDbFiles.Add(newCleanDbFileName);
    //}

    //PatchSession patchSession = new();
    //// меняем имена баз данных в файлах.
    //foreach (var newCreateDbFile in newCreateDbFiles)
    //{
    //  // Меняем имена баз данных в файлах создания баз данных.  
    //  if (DBReplacer.GenerateNamesAndReplaceInSqlFile(newCreateDbFile, out List<Tuple<string, string>> oldNewNames, out bool alreadyPatched, test.Id))
    //  {

    //    if (newCleanDbFiles.Count > 0)
    //    {
    //      newCleanDbFiles.ForEach(cf =>
    //      {
    //        // Меняем имена в файлах очистки баз данных.
    //        foreach (var oldNewValue in oldNewNames)
    //        {
    //          DBReplacer.Replace(cf, oldNewValue.Item1, oldNewValue.Item2);
    //        }
    //      });

    //    }
    //    else
    //    {
    //      // Создаеём секцию очистки в тесте и sql файл.
    //      test.AddCleanSection(Path.GetFileName(newCreateDbFile), oldNewNames);
    //    }

    //    // Path templates, enterprise and etc.
    //    test.Patch(oldNewNames, patchSession);
    //  }
    //}
    //test.Patch();

    string testFolder = Path.GetDirectoryName(test.TestFullPath)!;

    //
    // Create _cleanUp.sql file
    //
    string cleanUpFileNameText =
$@"SELECT
	pg_terminate_backend(pid),
	pg_cancel_backend(pid)
FROM
	pg_stat_activity
WHERE
	pid<>pg_backend_pid() AND
	datname IN ('{dbName1}', '{dbName2}');

DROP DATABASE IF EXISTS {dbName1};
DROP DATABASE IF EXISTS {dbName2};
";

    string cleanUpFileNameFillPath = Path.Combine(testFolder, cleanUpFileName);
    File.WriteAllText(cleanUpFileNameFillPath, cleanUpFileNameText);
        
    string createFileNameText =
$@"CREATE DATABASE {dbName1};
CREATE DATABASE {dbName2};
";

    string createFileNameFillPath = Path.Combine(testFolder, createFileName);
    File.WriteAllText(createFileNameFillPath, createFileNameText);
    

    string newCreateObjectsFile1 = Path.Combine(testFolder, sourceFileName);
    string newCreateObjectsFile2 = Path.Combine(testFolder, targetFileName);

    // Copy old Create DB objects fileName1
    if (!string.IsNullOrEmpty(oldCreateDBObjectsSqlFileName1) && !string.IsNullOrEmpty(oldCreateDBObjectsSqlFileName2))
    {
      //string oldDir = Path.GetDirectoryName(environmentFile);

      //string oldFile1 = Path.Combine(testFolder, oldCreateDBObjectsSqlFileName1);
      //if (File.Exists(oldFile1))
      //{
      //  File.Copy(oldFile1, newCreateObjectsFile1, overwrite: true);
      //}

      //string oldFile2 = Path.Combine(testFolder, oldCreateDBObjectsSqlFileName2);
      //if (File.Exists(oldFile2))
      //{
      //  File.Copy(oldFile2, newCreateObjectsFile2, overwrite: true);
      //}
    }
    else
    {
      File.WriteAllText(newCreateObjectsFile1, "!!!Could not find a source file name. Please do it yourself! ");
      File.WriteAllText(newCreateObjectsFile2, "!!!Could not find a target file name. Please do it yourself! ");
    }

    foreach (var file in test.CreateDbFiles)
    {
      if (string.Equals(file.FileName, sourceFileName, StringComparison.OrdinalIgnoreCase) || string.Equals(file.FileName, targetFileName, StringComparison.OrdinalIgnoreCase))
      {
        continue;
      }
      if (File.Exists(file.FullPath))
      {
        File.Delete(file.FullPath);
      }

    }

    PatchSession patchSession = new();

    var oldNewNames = new List<Tuple<string, string>>();
    oldNewNames.Add(new Tuple<string, string>("dc1", dbName1));
    oldNewNames.Add(new Tuple<string, string>("dc2", dbName2));
    test.Patch(oldNewNames, patchSession);


    test.Patch();
  }
  // Переписываем файл _definition.tests с тестами с уже исправленными именами файлов.
  File.WriteAllText(currentTestFileName, jsonObjects.ToString());

  // todo: поменять в файлах.
}

Console.WriteLine();
Console.ForegroundColor = ConsoleColor.Green;
Console.WriteLine($"Completed!");
Console.WriteLine($"Total _definition.tests files count = {files.Length}");
Console.ResetColor();

