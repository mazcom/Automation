using Newtonsoft.Json.Linq;

namespace TestsFixer.Model
{
  internal class TestsEnvironment
  {
    private readonly JObject jsonObject;
    private readonly string environmentFullPath;

    public TestsEnvironment(JObject jsonObject, string environmentFullPath)
    {
      this.jsonObject = jsonObject;
      this.environmentFullPath = environmentFullPath;
      Id = Guid.Parse(jsonObject["id"]!.Value<string>()!);
      Name = jsonObject["name"]!.Value<string>()!;
    }

    public Guid Id { get; }
    public string Name { get; }

    public List<Test> Tests { get; } = new();

    public List<string> NewDatabaseNames { get; } = new();
    public List<string> OldDatabaseNames { get; } = new();

    public void FetchInformation()
    {
      FetchDatabaseInfo();
    }

    private void FetchDatabaseInfo()
    {
      // СОЗДАНИЕ БАЗЫ ДАННЫХ.
      foreach (var token in jsonObject.SelectTokens("build[*].run.code.code", errorWhenNoMatch: false)!.
        Where(t => ((string)t!).Contains(".sql", StringComparison.OrdinalIgnoreCase))!)
      {
        var createDbCommandLine = (string)token!;

        var environmentPath = Path.GetDirectoryName(this.environmentFullPath)!;
        var sqlFileName =  FilesHelper.ExtractSqlFileName(createDbCommandLine)!;
        var createdatabaseFileFullPath = Path.Combine(environmentPath, sqlFileName);

        // Меняем имена баз данных в файлах создания баз данных.  
        if (DBNamesReplacer.GenerateNamesAndReplaceInFile(createdatabaseFileFullPath, out List<Tuple<string, string>> oldNewNames))
        {
          oldNewNames.ForEach(cf =>
          {
            OldDatabaseNames.Add(cf.Item1);
            NewDatabaseNames.Add(cf.Item2);
          });
        }

        // Получаем имя базы данных из теста like Databases.sql
        //var createDbFileName = FilesHelper.ExtractFileName(createDbCommandLine)!;
        // Полный путь к файлу создания базы данных.
        //string initDbFullFileName = Path.Combine(currentDir, createDbFileName);
        //test.CreateDbFiles.Add(new FileModel() { FullPath = initDbFullFileName, FileName = createDbFileName });
      }

    }

  }
}
