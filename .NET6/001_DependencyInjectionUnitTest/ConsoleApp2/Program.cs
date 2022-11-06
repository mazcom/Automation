using System;
using System.Collections.Generic;
using System.Diagnostics;
using System.IO;
using System.Linq;
using System.Text;
using System.Threading;
using System.Threading.Tasks;

namespace ConsoleApp2
{
  internal class Program
  {
    static void Main(string[] args)
    {
      // See https://aka.ms/new-console-template for more information


      Console.WriteLine("Hello, World!");


      var process = new Process();
      //process.StartInfo.RedirectStandardInput = true;
      process.StartInfo.CreateNoWindow = true;
      process.StartInfo.UseShellExecute = false;
      process.StartInfo.FileName = @"C:\Program Files\Devart\dbForge Studio for SQL Server\dbforgesql.com";
      process.StartInfo.Arguments = @" /? >Help_file.txt";
      //process.StartInfo.Arguments = @" /?";
      process.StartInfo.WorkingDirectory = @"C:\tests\Tests\SqlServer\Studio\Help\";
      // Tests\SqlServer\Studio\SchemaComparer\Options\Comparison\IgnoreCache
      process.Start();

      Console.WriteLine("aaaaaa");

      //while (!process.StandardOutput.EndOfStream)
      //{
      //  var line = process.StandardOutput.ReadLine();
      //  Console.WriteLine(line);
      //}


      if (!process.WaitForExit(10000))
      {
        Console.WriteLine("xxxxx");
        process.Close();
      }
      else
      {
        Console.WriteLine("yyyyyy");
      }

      Console.ReadKey();
    }
  }
}
