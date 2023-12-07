//// See https://aka.ms/new-console-template for more information

//using Common;
//using Common.Model;
//using Newtonsoft.Json.Linq;
//using NoEnvironmentTestsFixer;
//using NoEnvironmentTestsFixer.Model;
//using System.Linq;

//Console.ResetColor();
//Console.WriteLine(@"Please, enter a path to the tests like D:\Projects\commandlinetestsMR2\Tests\SqlServer\Studio\Documenter\");

////string pathToTests = @"D:\Projects\commandlinetestsMR2\Tests\SqlServer\Studio\Documenter\";
////string pathToTests = @"D:\Projects\commandlinetestsMR2\Tests\SqlServer\Studio\Data Comparer\Command_Line\MappingOptions\CLR_as_BINARY\";

//string file = @"D:\Projects\commandlinetestsMR2\Environments\SQLServer\Studio\Data Compare\SQL Server Data Comparer.environments";
//string path = Path.GetDirectoryName(file)!;
//string json;
//using (StreamReader sr = new(file))
//{
//  json = sr.ReadToEnd();
//}



////string pathToTests = Console.ReadLine()!;

////if (!Directory.Exists(pathToTests))
////{
////  Console.WriteLine($"Directory ${pathToTests} does not exist!");
////  return;
////}

//Dictionary<string, int> counts = new();
//HashSet<string> files = new();

//JArray jsonObjects = JArray.Parse(json);

//foreach (JObject jsonObject in jsonObjects)
//{
//  var environment = (Guid)jsonObject.SelectToken("id")!;

//  var createDbCommandLineNodes = jsonObject.SelectTokens("build[*].run.code.code", errorWhenNoMatch: false)!.
//        Where(t => ((string)t!).Contains(".sql", StringComparison.OrdinalIgnoreCase))!.ToArray();

//  foreach (var createDbCommandLineNode in createDbCommandLineNodes)
//  {
//    var createDbCommandLine = (string)createDbCommandLineNode!;
//    var sqlFileName = RegexHelper.ExtractSqlFileName(createDbCommandLine)!;

//    if (counts.ContainsKey(sqlFileName))
//    {
//      counts[sqlFileName] += 1;
//    }
//    else
//    {
//      counts[sqlFileName] = 1;
//    }
//  }
//}

//int cnt = 0;
//foreach (var keyValue in counts)
//{
//  if (keyValue.Value > 1)
//  {
//    Console.WriteLine($"File {keyValue.Key} count = {keyValue.Value}");
//    cnt++;
//  }
//}


//// сопоставляем имя файла и индекс.
//Dictionary<string, int> origFileIndex = new();

//foreach (JObject jsonObject in jsonObjects)
//{
//  var environment = (Guid)jsonObject.SelectToken("id")!;

//  var createDbCommandLineNodes = jsonObject.SelectTokens("build[*].run.code.code", errorWhenNoMatch: false)!.
//        Where(t => ((string)t!).Contains(".sql", StringComparison.OrdinalIgnoreCase))!.ToArray();

//  foreach (var createDbCommandLineNode in createDbCommandLineNodes)
//  {
//    var createDbCommandLine = (string)createDbCommandLineNode!;
//    var sqlFileName = RegexHelper.ExtractSqlFileName(createDbCommandLine)!;

//    var count = counts[sqlFileName];

//    if (count > 1)
//    {
//      string sourceFile = Path.Combine(path, sqlFileName.Trim());

//      if (!File.Exists(sourceFile))
//      {
//        continue;
//      }

//      if (!origFileIndex.ContainsKey(sqlFileName))
//      {
//        origFileIndex[sqlFileName] = 0;
//      }

//      int dbCreateIndex = origFileIndex[sqlFileName];

//      if (dbCreateIndex == 0)
//      {
//        origFileIndex[sqlFileName] += 1;
//        continue;
//      }


//      string newCreateDbFileName = FilesHelper.AddIndex(sqlFileName, dbCreateIndex);
//      origFileIndex[sqlFileName] += 1;

//      //string newCreateDbFileNamefullPath = Path.Combine(path, newCreateDbFileName);

//      using StreamReader srCreateDb = new StreamReader(Path.Combine(path, sqlFileName.Trim()));
//      string content = srCreateDb.ReadToEnd();

//      // Создаеём новый новый файл и заполняем его иригинальным коннентом. 
//      using StreamWriter swCreateDb = new StreamWriter(Path.Combine(path, newCreateDbFileName));
//      swCreateDb.Write(content);

//      // Меняем имена файлов в json-тест-объекте.
//      // Create DB
//      var jsonObj = (JValue)createDbCommandLineNode!;
//      if (jsonObj != null)
//      {
//        var dbComandLine = jsonObj.Value as string;
//        jsonObj.Value = dbComandLine!.Replace(sqlFileName, Path.GetFileName(newCreateDbFileName));
//      }
//    }
//  }
//}

//File.WriteAllText(file, jsonObjects.ToString());

//Console.ReadLine();




//Console.WriteLine();
//Console.ForegroundColor = ConsoleColor.Green;
//Console.WriteLine($"Completed!");
//Console.ResetColor();
