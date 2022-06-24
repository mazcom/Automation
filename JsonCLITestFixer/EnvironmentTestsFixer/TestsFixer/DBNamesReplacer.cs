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
    public static bool GenerateNamesAndReplaceInFile(string fullFileName, out List<Tuple<string, string>> oldNewNames, out bool alreadyPatched)
    {
      alreadyPatched = false;
      oldNewNames = new List<Tuple<string, string>>();
      string[]? fileLines;

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

        string[] patterns = { @"(?<=SET\s+@db_name\s+=\s+N'+)\w+", @"(?<=EXEC\s+\[?master\]?.dbo.sp_create_db\s+N'+)\w+" };
        foreach (var pattern in patterns)
        {
          var match = Regex.Match(line, pattern);

          if (match.Success)
          {
            Guid guid = Guid.NewGuid();
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
          string[] patterns = { @"(?<=IF\s+DB_NAME\(\)\s+<>\s+N')\w+", @"(?<=^USE\s+)\w+" };
          foreach (var pattern in patterns)
          {
            var match = Regex.Match(line, pattern, RegexOptions.IgnoreCase);

            if (match.Success && !line.Contains("master"))
            {
              string newDbName = oldNewNames.Where(e => e.Item1.Equals(match.Value, StringComparison.OrdinalIgnoreCase)).First().Item2;
              fileLines[i] = Regex.Replace(line, pattern, newDbName, RegexOptions.IgnoreCase);
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
