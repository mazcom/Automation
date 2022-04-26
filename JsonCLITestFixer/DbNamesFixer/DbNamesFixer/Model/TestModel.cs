using Newtonsoft.Json.Linq;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace DbNamesFixer.Model
{
  public class TestModel
  {
    public Guid Id { get; set; }

    // Test JsonObject
    public JObject? JsonObject { get; set; }

    public List<FileModel> CreateDbFiles { get; set; } = new List<FileModel>();

    public List<FileModel> CleanDbFiles { get; set; } = new List<FileModel>();
  }
}
