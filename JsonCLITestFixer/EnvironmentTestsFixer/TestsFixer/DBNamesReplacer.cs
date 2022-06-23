using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Text.RegularExpressions;
using System.Threading.Tasks;

namespace TestsFixer
{
  internal static class DBNamesReplacer
  {
    public static bool GenerateNamesAndReplaceInFile(string fullFileName, out List<Tuple<string, string>> oldNewNames)
    {
      oldNewNames = new List<Tuple<string, string>>();
      string[]? fileLines;

      // Читаем контент из файла и запоминаем.
      fileLines = File.ReadAllLines(fullFileName);

      string? oldDbName = null;
      string? newDbName = null;
      for (int i = 0; i < fileLines.Length; i++)
      {
        var line = fileLines[i];

        string[] patterns = { @"(?<=SET\s+@db_name\s+=\s+N'+)\w+", @"(?<=EXEC\s+\[master\].dbo.sp_create_db\s+N'+)\w+" };

        foreach (var pattern in patterns)
        {
          var match = Regex.Match(line, pattern);

          if (match.Success)
          {
            Guid guid = Guid.NewGuid();
            oldDbName = match.Value;
            newDbName = $"{match}_{guid.ToString().Replace("-", "_")}";
            oldNewNames.Add(new Tuple<string, string>(oldDbName, newDbName));

            string newLine = Regex.Replace(line, pattern, newDbName, RegexOptions.IgnoreCase);
            fileLines[i] = newLine;
          }
          else if (oldDbName != null)
          {
            string newLine = line.Replace(oldDbName, newDbName);
            fileLines[i] = newLine;
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

    public static bool Replace(string fullFileName, string from, string to)
    {
      string[]? fileLines;
      bool replaced = false;

      // Читаем контент из файла и запоминаем.
      fileLines = File.ReadAllLines(fullFileName);

      for (int i = 0; i < fileLines.Length; i++)
      {
        var line = fileLines[i];

        string pattern = @"(?<=SET\s+@db_name\s+=\s+N'+)\w+";
        var match = Regex.Match(line, pattern);

        if (match.Success && string.Equals(from, match.Value, StringComparison.OrdinalIgnoreCase))
        {
          replaced = true;
          string newLine = Regex.Replace(line, pattern, to, RegexOptions.IgnoreCase);
          fileLines[i] = newLine;
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
