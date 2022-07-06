using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace Common.Model
{
  public class PatchSession
  {
    // The patched files(scomp,dcomp) attached to the tests.
    public HashSet<string> PatchedFiles { get; set; } = new();

    // The patched create DB sql files
    public Dictionary<string, List<Tuple<string, string>>> FileToOldNewNames = new();
  }
}
