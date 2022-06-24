using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace TestsFixer
{
  internal class PathTraversal
  {

    public static bool TraverseTreeUp(string startPath, string toFindPathName, out string foundFullPath)
    {
      foundFullPath = string.Empty;
      string currentPath = startPath;
      do
      {
        string[] dirs = Directory.GetDirectories(currentPath, $"{toFindPathName}", SearchOption.TopDirectoryOnly);

        // 
        if (dirs.Length == 1)
        {
          // a dir with the environments is found
          foundFullPath = Path.Combine(currentPath, toFindPathName);
          return true;
        }

        // navigate a folder up
        currentPath = Path.GetFullPath(Path.Combine(currentPath, @"..\"));

        DirectoryInfo d = new DirectoryInfo(currentPath);
        if (d.Parent == null)
        {
          return false;
        }

      }
      while (true);
    }
  }
}
