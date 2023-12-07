// See https://aka.ms/new-console-template for more information

using Spectre.Console;

Table drives = new();
drives.AddColumn("[blue]NAME[/]");
drives.AddColumn("[blue]TYPE[/]");
drives.AddColumn("[blue]FORMAT[/]");
drives.AddColumn(new TableColumn(
"[blue]SIZE (BYTES)[/]").RightAligned());
drives.AddColumn(new TableColumn(
"[blue]FREE SPACE[/]").RightAligned());
foreach (DriveInfo drive in DriveInfo.GetDrives())
{
  if (drive.IsReady)
  {
    drives.AddRow(drive.Name, drive.DriveType.ToString(),
    drive.DriveFormat, drive.TotalSize.ToString("N0"),
    drive.AvailableFreeSpace.ToString("N0"));
  }
  else
  {
    drives.AddRow(drive.Name, drive.DriveType.ToString(),
    string.Empty, string.Empty, string.Empty);

  }
}
AnsiConsole.Write(drives);


//var files = Directory.GetFiles(@"D:\Projects\AutotestsOld\Oracle\AutoTests", "*.autotest", SearchOption.AllDirectories);

//Dictionary<string, int> types = new Dictionary<string, int>();

//foreach (var file in files)
//{
//  try
//  {
//    var doc = XDocument.Load(file);
//    IEnumerable<XElement> autotests = doc.XPathSelectElements("//Autotest").ToList();

//    foreach (var element in autotests)
//    {
//      var type = element.Attribute("type").Value;

//      if (types.ContainsKey(type))
//      {
//        types[type]++;
//      }
//      else
//      {
//        types.Add(type, 0);
//      }
//    }
//  }
//  catch 
//  { 
//  }
//}

//foreach (var kvp in types)
//{
//  Console.WriteLine("Type = {0}, Count = {1}", kvp.Key, kvp.Value);
//}

Console.WriteLine();
Console.ForegroundColor = ConsoleColor.Green;
Console.WriteLine($"Completed!");
Console.ResetColor();