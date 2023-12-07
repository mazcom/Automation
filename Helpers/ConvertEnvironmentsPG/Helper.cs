using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace ConvertEnvironmentsPG
{
  internal static class Helper
  {
    public static string ToCamelCase(this string str)
    {
      var words = str.Replace(@"\","_").Replace(@"/", "_").
        Split(new[] { "_", " " }, StringSplitOptions.RemoveEmptyEntries);
      //var leadWord = words[0].ToLower();
      var tailWords = words
          .Select(word => char.ToUpper(word[0]) + word.Substring(1))
          .ToArray();
      return $"{string.Join("-", tailWords)}";
    }
  }
}
