using Newtonsoft.Json.Linq;

namespace TestsFixer.Model
{
  internal class TestsEnvironment
  {
    private readonly JObject jsonObject;

    public TestsEnvironment(JObject jsonObject)
    {
      this.jsonObject = jsonObject;
      Id = Guid.Parse(jsonObject["id"]!.Value<string>()!);
      Name = jsonObject["name"]!.Value<string>()!;
    }

    public Guid Id { get; }
    public string Name { get; }
  }
}
