using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Text.RegularExpressions;
using System.Threading.Tasks;

namespace Common
{
  public static class DBReplacer
  {
    // Паттерны определяют инструкции в которых мы создаём базы данных.
    private static string[] CreateDbNamesPatterns = {
      @"(?<=SET\s+(@db_name|@name_db|@DatabaseName)\s*=\s*N'+)\w+"
      ,@"(?<=EXEC\s+\[?master\]?.dbo.sp_create_db\s+N'+)\w+"
      ,@"(?<=DECLARE\s+(@db_name|@name_db)\s+NVARCHAR\(max\)\s*=\s*N'+)\w+"
      ,@"(?<=\s*WHERE\s*\[name\] =\s*N'+)\w+"
      ,@"(?<=DROP\s+DATABASE\s+IF\s+EXISTS\s+)\w+"};

    // Патерны определяют использование имён баз данных в скрипте.
    private static string[] UseDbNamesPatterns = {
      @"(?<=IF\s+DB_NAME\(\)\s+<>\s+N')\w+"
      , @"(?<=^(USE|ALTER DATABASE)\s+)\[?\w+\]?"
      , @"(?<=SET\s+IDENTITY_INSERT\s*)\[?\w+\]?"
      , @"(?<=\s*INSERT\s+INTO\s*)\[?\w+\]?"
      , @"(?<=\s*INSERT\s+)\[?\w+\]?"
      , @"(?<=CREATE\s+DATABASE\s+)\[?\w+\]?"};

    public static bool GenerateNamesAndReplaceInSqlFile(string fullFileName, out List<Tuple<string, string>> oldNewNames, out bool alreadyPatched, Guid preferedGuid = default)
    {
      alreadyPatched = false;
      oldNewNames = new List<Tuple<string, string>>();
      string[]? fileLines;
      
      // Предполагаем, что для всего файла нужно сгенерировать один GUID.
      Guid guid = preferedGuid == Guid.Empty ? Guid.NewGuid() : preferedGuid;

      // Читаем контент из файла и запоминаем.
      fileLines = File.ReadAllLines(fullFileName);

      // Find old db names, generate new names and replace where it occurs first time. 
      for (int i = 0; i < fileLines.Length; i++)
      {
        var line = fileLines[i];

        // Проверяем, что уже фалик был пропатчен.
        if (oldNewNames.Count == 0)
        {
          var alreadyPatchedMatch = Regex.Match(line, @"[0-9a-fA-F]{8}_[0-9a-fA-F]{4}_[0-9a-fA-F]{4}_[0-9a-fA-F]{4}_[0-9a-fA-F]{12}");
          if (alreadyPatchedMatch.Success)
          {
            alreadyPatched = true;
            return false;
          }
        }

        //string[] patterns = { @"(?<=SET\s+@db_name\s+=\s+N'+)\w+", @"(?<=EXEC\s+\[?master\]?.dbo.sp_create_db\s+N'+)\w+" };
        foreach (var pattern in CreateDbNamesPatterns)
        {
          var match = Regex.Match(line, pattern);

          if (match.Success)
          {
            string oldDbName = match.Value;
            string newDbName = $"{match}_{guid.ToString().Replace("-", "_")}";
            oldNewNames.Add(new Tuple<string, string>(oldDbName, newDbName));

            string newLine = Regex.Replace(line, pattern, newDbName, RegexOptions.IgnoreCase);
            fileLines[i] = newLine;
          }
        }
      }

      // Replace names in other places.
      if (oldNewNames.Count > 0)
      {
        for (int i = 0; i < fileLines.Length; i++)
        {
          var line = fileLines[i];
          foreach (var pattern in UseDbNamesPatterns)
          {
            var match = Regex.Match(line, pattern, RegexOptions.IgnoreCase);

            if (match.Success && !line.Contains("master"))
            {
              string m = match.Value.Replace("[", string.Empty)!.Replace("]", string.Empty);
              var oldDbNameTuple = oldNewNames.Where(e => e.Item1.Equals(m, StringComparison.OrdinalIgnoreCase)).FirstOrDefault();
              if (oldDbNameTuple != null) 
              {
                string newDbName = oldDbNameTuple.Item2;
                fileLines[i] = fileLines[i].Replace(m, newDbName);//Regex.Replace(line, pattern, newDbName, RegexOptions.IgnoreCase);
                break;
              } 
            }
          }
        }
      }

      if (oldNewNames.Count > 0)
      {
        File.WriteAllLines(fullFileName, fileLines);
        return true;
      }
      else
      {
        return false;
      }
    }

    public static void TryToReplacePassword(string fullFileName)
    {
      string[]? fileLines = File.ReadAllLines(fullFileName);
      var pattern = @"(?<=with\s+password\s+=\s+\')\w+";
      bool replaced = false;
      for (int i = 0; i < fileLines.Length; i++)
      {
        var line = fileLines[i];
        var match = Regex.Match(line, pattern);
        if (match.Success)
        {
          fileLines[i] = Regex.Replace(line, pattern, "123pwd!", RegexOptions.IgnoreCase);
          replaced = true;
        }
      }

      if (replaced)
      {
        File.WriteAllLines(fullFileName, fileLines);
      }
    }

    public static void TryToReplaceNamesInSQLFile(string fullFileName, List<Tuple<string, string>> oldNewNames)
    {
      string[]? fileLines = File.ReadAllLines(fullFileName);
      bool replaced = false;
      // Find old db names, generate new names and replace where it occurs first time. 
      for (int i = 0; i < fileLines.Length; i++)
      {
        var line = fileLines[i];

        string[] patterns = CreateDbNamesPatterns.Concat(UseDbNamesPatterns).ToArray();
        foreach (var pattern in patterns)
        {
          var match = Regex.Match(line, pattern);

          if (match.Success)
          {
            foreach (var oldNewName in oldNewNames)
            {
              if (line.Contains(oldNewName.Item1))
              {
                fileLines[i] = line.Replace(oldNewName.Item1, oldNewName.Item2);
                replaced = true;
                break;
              }
            }
          }
        }
      }

      if (replaced)
      {
        File.WriteAllLines(fullFileName, fileLines);
      }
    }

    public static string TryToReplaceServerNameInConnectionString(string line)
    {
      string pattern = @"(?<=Data Source=)[A-Za-z0-9_\:\(\)\*-s{0,}\\]+\s{0,};";
      var match = Regex.Match(line, pattern);
      if (match.Success)
      {
        line = line.Replace(match.Value, Constants.AffordableConnectionName.Replace("%", string.Empty) + ";");
      }

      return line;
    }

    /// <summary>
    /// Replace SC1 in <Schema>SC1</Schema>
    /// or
    /// Replace SC1 in <Database>SC1</Database>
    /// or
    /// Replace autotest_dataexport in <DatabaseObject>autotest_dataexport.dbo.FormatsTest</DatabaseObject>
    /// or
    /// <PropertyValue Name="TargetTable" xml:space="preserve">autotest_dataimport.dbo.saleperson</PropertyValue>
    /// </summary>
    public static string TryToReplaceDbNameInSchemaSection(string line, List<Tuple<string, string>> oldNewDatabaseNames)
    {
      string pattern = @"(<TargetTableName)|(<PropertyValue Name=""TargetTable"")|((?<=\<Schema\>)|(?<=\<DatabaseName\>)|(?<=\<Database\>)|(?<=\<DatabaseObject\>)\w+)";
      var match = Regex.Match(line, pattern);
      if (match.Success)
      {
        foreach (var pair in oldNewDatabaseNames)
        {
          if (line.Contains(pair.Item1))
          {
            line = line.Replace(pair.Item1, pair.Item2);
            break;
          }
        }
      }

      return line;
    }

    /// <summary>
    /// Replace DBFSQLSRV\SQL2014 <DataConnection>DBFSQLSRV\SQL2014</DataConnection>
    /// </summary>
    public static string TryToReplaceDbNameInDataConnectionSection(string line)
    {
      string pattern = @"(?<=\<DataConnection\>)\w+[\\]?\w{0,}";
      var match = Regex.Match(line, pattern);
      if (match.Success)
      {
        line = line.Replace(match.Value, Constants.AffordableConnectionName.Replace("%", string.Empty));
      }

      return line;
    }

    public static bool Replace(string fullFileName, string from, string to)
    {
      string[]? fileLines;
      bool replaced = false;

      // Читаем контент из файла и запоминаем.
      fileLines = File.ReadAllLines(fullFileName);

      for (int i = 0; i < fileLines.Length; i++)
      {
        var line = fileLines[i];

        foreach (var pattern in CreateDbNamesPatterns)
        {
          //string pattern = @"(?<=SET\s+@db_name\s+=\s+N'+)\w+";
          var match = Regex.Match(line, pattern);

          if (match.Success && string.Equals(from, match.Value, StringComparison.OrdinalIgnoreCase))
          {
            replaced = true;
            string newLine = Regex.Replace(line, pattern, to, RegexOptions.IgnoreCase);
            fileLines[i] = newLine;
          }
        }
      }

      if (replaced)
      {
        File.WriteAllLines(fullFileName, fileLines);
        return true;
      }
      else
      {
        return false;
      }
    }

  }
}
