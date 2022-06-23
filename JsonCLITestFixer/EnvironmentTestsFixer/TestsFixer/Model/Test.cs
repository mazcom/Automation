using Newtonsoft.Json.Linq;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace TestsFixer.Model
{
  internal class Test
  {
    private readonly JObject jsonObject;

    public Test(JObject jsonObject)
    {
      this.jsonObject = jsonObject;
      Environment = Guid.Parse(jsonObject["environment"]!.Value<string>()!);
      Name = jsonObject["name"]!.Value<string>()!;
      Description = jsonObject["description"]!.Value<string>()!;
    }

    public Guid Environment { get; }
    public string Name { get; }
    public string Description { get; set; }
  }
}
