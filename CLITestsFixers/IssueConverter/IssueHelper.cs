using Newtonsoft.Json.Linq;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Text.RegularExpressions;
using System.Threading.Tasks;

namespace EnterpriseFixer
{
  public class IssueHelper
  {
    private const string bugPattern = @"DBFORGE-[0-9]+";

    public static bool CheckContainsBug(string value, out string bug)
    {
      bug = null;
      var match = Regex.Match(value, bugPattern);
      if (match.Success)
      {
        // Игнорим
        if (match.Value.Equals("DBFORGE-10799", StringComparison.OrdinalIgnoreCase))
        {
          return false;
        }

        bug = match.Value;
        return true;
      }
      return false;
    }
  }
}