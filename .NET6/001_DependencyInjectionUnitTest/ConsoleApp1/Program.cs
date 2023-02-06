// See https://aka.ms/new-console-template for more information
using System.Text;


Encoding.RegisterProvider(System.Text.CodePagesEncodingProvider.Instance);

var DefaultEncoding = Encoding.GetEncoding("windows-1251");

using StreamReader streamReader = new StreamReader("c:\\utf8.sql", DefaultEncoding, true, 1024 * 1024);




var s = streamReader.ReadToEnd();

Console.ReadKey();




