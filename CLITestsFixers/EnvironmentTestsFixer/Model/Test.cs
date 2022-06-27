using Common;
using Common.Model;
using Newtonsoft.Json.Linq;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace EnvironmentTestsFixer.Model
{
  internal class Test : BaseTest
  {
    private TestsEnvironment environment;

    public Test(JObject jsonObject, string testFullPath) : base(jsonObject, testFullPath)
    {
    }

    public void SetEnvironment(TestsEnvironment environment)
    {
      this.environment = environment;
    }

    public void Patch()
    {
      PatchDatabaseNames(environment.OldNewDatabaseNames);
      PatchServerName(environment.OldNewDatabaseNames);
      PatchEnterprise();
      PatchDocTemplates(environment.OldNewDatabaseNames);
    }
  }
}
