using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Text.RegularExpressions;
using System.Threading.Tasks;

namespace TestsFixer
{
  internal class RegexHelper
  {
    private static readonly Regex rxSqlFileName = new(@"[A-Za-z0-9_\s-]+\.sql", RegexOptions.IgnoreCase);
    private static readonly Regex rxServerName = new(@"(?<=\/connection:)%[A-Za-z0-9_\:\(\)\*-]+%", RegexOptions.IgnoreCase);
    //private static readonly Regex rx = new(@"[A-Za-z0-9_\s-]+\.sql", RegexOptions.IgnoreCase);

    public static string? ExtractSqlFileName(string fromCommandLine)
    {
      MatchCollection matches = rxSqlFileName.Matches(fromCommandLine);
      return matches.Count == 0 ? null : matches.First().Value;
    }

    public static string? ExtractServerName(string fromCommandLine)
    {
      MatchCollection matches = rxServerName.Matches(fromCommandLine);
      return matches.Count == 0 ? null : matches.First().Value;
    }
  }
}
