using Common.Model;
using Newtonsoft.Json.Linq;

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

    public void Patch(PatchSession patchSession)
    {
      // Path a test info.
      PatchDatabaseNames(environment.OldNewDatabaseNames);
      PatchScriptFolderNames();
      PatchBackupName();
      PatchServerName(environment.OldNewDatabaseNames);
      PatchEnterprise();
      PatchTimeout();

      // Patch the attached separate files(scomp, dcomp, dgen, etalon.sql and etc.) to a test.
      PatchDocTemplates(environment.OldNewDatabaseNames, patchSession);
    }
  }
}
