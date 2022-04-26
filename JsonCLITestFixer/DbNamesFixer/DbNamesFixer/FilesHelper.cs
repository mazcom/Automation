using System.Text.RegularExpressions;

namespace DbNamesFixer
{
  internal class FilesHelper
  {
    private static readonly Regex rx = new(@"[A-Za-z0-9_]+\.sql", RegexOptions.IgnoreCase);

    public static string? ExtractFileName(string fromText)
    {
      MatchCollection matches = rx.Matches(fromText);
      return matches.Count == 0 ? null : matches.First().Value;
    }

    public static string AddIndex(string path, int index)
    {
      string dir = Path.GetDirectoryName(path)!;
      string fileName = Path.GetFileNameWithoutExtension(path);
      string fileExt = Path.GetExtension(path);

      return Path.Combine(dir, fileName + index + fileExt);
    }
  }
}
