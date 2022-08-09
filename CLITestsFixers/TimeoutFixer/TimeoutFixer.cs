using Newtonsoft.Json.Linq;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Text.RegularExpressions;
using System.Threading.Tasks;

namespace EnterpriseFixer
{
  public class TimeoutFixer
  {
    public static bool TryFix(JObject jsonObject, int value)
    {
      var timeOutNodes = jsonObject!.SelectTokens("..timeout", errorWhenNoMatch: false)!;
      bool wasFixed = false;

      foreach (var timeOutNode in timeOutNodes)
      {
        wasFixed = true;
        var timeOut = (JValue)timeOutNode;
        timeOut.Value = value;
      }

      return wasFixed;
    }
  }
}