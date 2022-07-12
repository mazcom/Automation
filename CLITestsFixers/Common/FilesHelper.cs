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
    private static readonly Regex rxIncorrectExecuteCommand = new(@"/execute\s+[A-Za-z0-9_\s-\(\)]+\.sql", RegexOptions.IgnoreCase);


    private static readonly Regex rxSqlFileName = new(@"[A-Za-z0-9_\s-\(\)]+\.sql", RegexOptions.IgnoreCase);
    private static readonly Regex rxAnyFileName = new(@"[A-Za-z0-9_\s-\(\)]+\.[A-Za-z0-9]+", RegexOptions.IgnoreCase);
    private static readonly Regex rxServerName = new(@"(?<=\/connection:)%[A-Za-z0-9_\:\(\)\*-]+%", RegexOptions.IgnoreCase);

    public static string? ExtractSqlFileName(string fromCommandLine)
    {
      MatchCollection matches = rxSqlFileName.Matches(fromCommandLine);
      return matches.Count == 0 ? null : matches.First().Value;
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

    /// <summary>
    /// </summary>
    /// <param name="fromCommandLine"></param>
    /// <param name="error"></param>
    /// <returns>true - ok(no error), false - there is en arror </returns>
    public static bool CheckIncorrectCmdLine(string fromCommandLine, out string? error)
    {
      error = null;
      MatchCollection matches = rxIncorrectExecuteCommand.Matches(fromCommandLine);

      if (matches.Count == 0)
      {
        return true;
      }
      else
      {
        error = $"The command \"{matches.First().Value}\" is incorrect.";
        return false;
      }
    }
  }
}
