// See https://aka.ms/new-console-template for more information

using EnterpriseFixer;
using Newtonsoft.Json.Linq;
using System.Xml.Linq;
using System;
using System.Text.RegularExpressions;
using System.Threading.Channels;
using Text.Compare;
using System.Text;

Console.ResetColor();
Console.WriteLine(@"Issue converter ver 1.0");
Console.WriteLine(@"Please, enter a path to the tests like D:\Projects\commandlinetests\Tests\SqlServer\Studio\");
string pathToTests = @"D:\Projects\commandlinetests65\Tests\SqlServer\Studio\";
//string pathToTests = Console.ReadLine()!;

if (!Directory.Exists(pathToTests))
{
  Console.ForegroundColor = ConsoleColor.Red;
  Console.WriteLine($"Directory ${pathToTests} does not exist!");
  Console.ResetColor();
  return;
}

string[] etalonFiles = Directory.GetFiles(pathToTests,"*etalon.*", SearchOption.AllDirectories)
  .Where(s => s.EndsWith(".txt") || s.EndsWith(".log") || s.EndsWith(".sql"))
  .ToArray();

Console.WriteLine("Processing...");

foreach (string etalon in etalonFiles)
{
  var encoding = EncodingDetector.Detect(etalon);
  
  //if (!Encoding.UTF8.Equals(encoding)) 
  //{ 
  //}

  File.AppendAllText(etalon, Environment.NewLine, encoding);
}


Console.WriteLine();
Console.ForegroundColor = ConsoleColor.Green;
Console.WriteLine($"Tests processed: {etalonFiles.Length}");
Console.WriteLine($"Completed!");
Console.ResetColor();
