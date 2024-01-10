using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Text.RegularExpressions;
using System.Threading.Tasks;

namespace Common
{
  public class RegexHelper
  {
    // После /execute не может идти сразу имя sql-файла.
    //private static readonly Regex rxIncorrectExecuteCommand = new(@"/execute\s+[A-Za-z0-9_\s-\(\)]+\.sql", RegexOptions.IgnoreCase);


    // Обробатываем случаи:
    // /inputfile:\"Database2014.sql\""
    // /inputfile:Database2014.sql
    // /execute Databases.sql
    // /execute "..\DatabasesOptions2014x.sql"
    private static readonly Regex rxSqlFileNameInCommandLine = new(@"((..\\)[A-Za-z0-9_\s-\(\)]+\.sql)|(\s[A-Za-z0-9_\s-\(\)]+\.sql\s?)|((?<=\/inputfile:\\?""?)[A-Za-z0-9._\s-\(\)]+\.sql)", RegexOptions.IgnoreCase);


    private static readonly Regex rxAnyFileName = new(@"[A-Za-z0-9_\s-\(\)\.]+\.[A-Za-z0-9]+", RegexOptions.IgnoreCase);
    private static readonly Regex rxServerName = new(@"(?<=\/connection:\s*)%[A-Za-z0-9_\:\(\)\*-]+%", RegexOptions.IgnoreCase);
    private static readonly Regex rxPathName = new(@"(?<=\/path:\s*)[A-Za-z0-9_]+", RegexOptions.IgnoreCase);

    public static string? ExtractSqlFileNameFromCommandLine(string fromCommandLine)
    {
      MatchCollection matches = rxSqlFileNameInCommandLine.Matches(fromCommandLine);
      return matches.Count == 0 ? null : matches.First().Value.Trim();
    }

    public static string[] ExtractAnyFileNames(string fromCommandLine)
    {
      MatchCollection matches = rxAnyFileName.Matches(fromCommandLine);
      return matches.Cast<Match>().Select(m => m.Value).ToArray();
    }

    public static string? ExtractServerName(string fromCommandLine)
    {
      MatchCollection matches = rxServerName.Matches(fromCommandLine);
      return matches.Count == 0 ? null : matches.First().Value;
    }

    public static string? ExtractPathName(string fromCommandLine)
    {
      MatchCollection matches = rxPathName.Matches(fromCommandLine);
      return matches.Count == 0 ? null : matches.First().Value;
    }

    public static void WriteAllLines(string fileName, string[]? fileLines)
    {
      if (fileName == null)
        throw new ArgumentNullException("fileName");
      if (fileLines == null)
        throw new ArgumentNullException("fileLines");

      var sb = new StringBuilder();

      for (int i = 0; i < fileLines.Length; i++)
      {
        if (i != 0)
        {
          sb.Append(Environment.NewLine);
        }
        sb.Append(fileLines[i]);
      }

      using StreamWriter sw = new StreamWriter(fileName, append: false);
      sw.Write(sb.ToString());
    }
    /// <summary>
    /// </summary>
    /// <param name="fromCommandLine"></param>
    /// <param name="error"></param>
    /// <returns>true - ok(no error), false - there is en arror </returns>
    //public static bool CheckIncorrectCmdLine(string fromCommandLine, out string? error)
    //{
    //  error = null;
    //  MatchCollection matches = rxIncorrectExecuteCommand.Matches(fromCommandLine);

    //  if (matches.Count == 0)
    //  {
    //    return true;
    //  }
    //  else
    //  {
    //    error = $"The command \"{matches.First().Value}\" is incorrect.";
    //    return false;
    //  }
    //}
  }
}
