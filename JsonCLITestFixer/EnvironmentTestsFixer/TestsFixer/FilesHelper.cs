using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Text.RegularExpressions;
using System.Threading.Tasks;

namespace TestsFixer
{
  internal class FilesHelper
  {
    private static readonly Regex rx = new(@"[A-Za-z0-9_-]+\.sql", RegexOptions.IgnoreCase);

    public static string? ExtractSqlFileName(string fromCommandLine)
    {
      MatchCollection matches = rx.Matches(fromCommandLine);
      return matches.Count == 0 ? null : matches.First().Value;
    }
  }
}
