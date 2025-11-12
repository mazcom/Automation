// See https://aka.ms/new-console-template for more information

using System.Text.Json.Nodes;

record Item(string Id, string? Name, string? Directory, string Path);

class Program
{
  static void Main(string[] args)
  {
    //if (args.Length == 0 || !File.Exists(args[0]))
    //{
    //  Console.Error.WriteLine("Usage: FindDuplicateIds <path-to-structure.json>");
    //  Environment.Exit(2);
    //}

    var jsonText = File.ReadAllText($@"D:\Projects\commandlinetestsMaster\TestPlans\MySQL\StudioBase.testplan");
    var root = JsonNode.Parse(jsonText);
    var items = new List<Item>();
    Walk(root, "$", items);

    var dupGroups = items
        .GroupBy(i => i.Id, StringComparer.OrdinalIgnoreCase)
        .Where(g => g.Count() > 1)
        .ToList();

    if (dupGroups.Count == 0)
    {
      Console.WriteLine("No duplicate IDs found.");
      Environment.Exit(0);
    }

    Console.WriteLine("Duplicate IDs found:\n");
    foreach (var g in dupGroups)
    {
      Console.WriteLine($"ID {g.Key} repeats {g.Count()}x");
      foreach (var it in g.OrderBy(i => i.Directory).ThenBy(i => i.Name))
      {
        Console.WriteLine($"  - Name: {it.Name ?? "<null>"} | Dir: {it.Directory ?? "<null>"} | Path: {it.Path}");
      }
      Console.WriteLine();
    }

    Environment.Exit(1);
  }

  static void Walk(JsonNode? node, string path, List<Item> acc)
  {
    if (node is null) return;

    if (node is JsonObject obj)
    {
      // Если есть "id" — это «айтем»
      if (obj.TryGetPropertyValue("id", out var idNode) && idNode is JsonValue)
      {
        var id = idNode!.GetValue<string>();
        obj.TryGetPropertyValue("name", out var nameNode);
        obj.TryGetPropertyValue("directory", out var dirNode);

        acc.Add(new Item(
            id,
            nameNode is JsonValue ? nameNode!.GetValue<string?>() : null,
            dirNode is JsonValue ? dirNode!.GetValue<string?>() : null,
            path
        ));
      }

      // Рекурсивно обходим возможные коллекции детей
      foreach (var childProp in new[] { "suite", "children", "items" })
      {
        if (obj.TryGetPropertyValue(childProp, out var arrNode) && arrNode is JsonArray arr)
        {
          for (int i = 0; i < arr.Count; i++)
          {
            Walk(arr[i], $"{path}/{childProp}[{i}]", acc);
          }
        }
      }
    }
    else if (node is JsonArray arr)
    {
      for (int i = 0; i < arr.Count; i++)
        Walk(arr[i], $"{path}[{i}]", acc);
    }
  }
}


