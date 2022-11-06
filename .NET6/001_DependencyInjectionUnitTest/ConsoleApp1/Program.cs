// See https://aka.ms/new-console-template for more information
using System.Diagnostics;

Console.WriteLine("Hello, World!");


using var process = new Process();

process.StartInfo.FileName = @"C:\Program Files\Devart\dbForge Studio for SQL Server\dbforgesql.com";
process.StartInfo.Arguments = @" /? >Help_file.txt";
process.StartInfo.WorkingDirectory = @"c:\Tests\SqlServer\Studio\Help\";
// Tests\SqlServer\Studio\SchemaComparer\Options\Comparison\IgnoreCache
process.Start();


Console.ReadKey();

