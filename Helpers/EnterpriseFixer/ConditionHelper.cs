using Newtonsoft.Json.Linq;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Text.RegularExpressions;
using System.Threading.Tasks;

namespace EnterpriseFixer
{
  public class ConditionHelper
  {
    public static bool TryFix(JObject jsonObject, string condition)
    {
      string[] entkeys = { "exit_code_equal", "console_output_equal", "files_equal", "directory_equal", "directory_equals_only_file_name", "directory_equals_by_filename_with_content" };

      var testId = (Guid)jsonObject.SelectToken("id")!;

      foreach (var entkey in entkeys)
      {
        var keyNode = jsonObject!.SelectTokens($"assert.{entkey}", errorWhenNoMatch: false)!.FirstOrDefault();

        if (keyNode != null)
        {
          if (entkey == "exit_code_equal")
          {
            var enterprise = jsonObject!.SelectTokens($"assert.{entkey}[*].condition", errorWhenNoMatch: false)!.FirstOrDefault();
            if (enterprise == null)
            {
              string addenterpriseSection;
              var exitCodeEqualNode = jsonObject!.SelectTokens($"assert.{entkey}", errorWhenNoMatch: false)!.FirstOrDefault();
              var expectedNode = exitCodeEqualNode!.SelectTokens("[*].expected", errorWhenNoMatch: false)!.FirstOrDefault();

              addenterpriseSection =
              $@"{{
""condition"": ""{condition}"",
""expected"": {expectedNode!.Value<string>()}
}}";

              ((JArray)exitCodeEqualNode).Add(JObject.Parse(addenterpriseSection));
              return true;
            }
          }
          else if (entkey == "console_output_equal")
          {
            var enterprise = jsonObject!.SelectTokens($"assert.{entkey}[*].condition", errorWhenNoMatch: false)!.FirstOrDefault();
            if (enterprise == null)
            {
              string addenterpriseSection;
              var console_output_equalNode = jsonObject!.SelectTokens($"assert.{entkey}", errorWhenNoMatch: false)!.FirstOrDefault();
              var etalonNode = console_output_equalNode.SelectTokens("[*].etalon", errorWhenNoMatch: false)!.FirstOrDefault();

              // неправильная структура. Не массив а просто вложено.
              if (etalonNode == null)
              {
                // пробуем исправить ситуацию.
                etalonNode = console_output_equalNode.SelectTokens("etalon", errorWhenNoMatch: false)!.FirstOrDefault();
                var error_outputNode1 = console_output_equalNode.SelectTokens("error_output", errorWhenNoMatch: false)!.FirstOrDefault();

                if (etalonNode == null)
                {
                  Console.ForegroundColor = ConsoleColor.Red;
                  Console.WriteLine($"The section {entkey} in test {testId} was not in correct format.");
                  Console.ResetColor();
                  return false;
                }

                var assertNode = (JObject)jsonObject!.SelectTokens("assert", errorWhenNoMatch: false)!.FirstOrDefault()!;

                string console_output_equalSection =
    $@"
[
             {{
                ""etalon"" : ""{etalonNode!.Value<string>()}""";

                if (error_outputNode1 != null)
                {
                  console_output_equalSection += @$",""error_output"" : ""{error_outputNode1!.Value<string>()} """;
                  console_output_equalSection += "}}]";
                }

                assertNode!.Property($"{entkey}")!.Remove();
                assertNode!.Add($"{entkey}", JArray.Parse(console_output_equalSection));
                console_output_equalNode = jsonObject!.SelectTokens($"assert.{entkey}", errorWhenNoMatch: false)!.FirstOrDefault();
              }

              var standard_outputNode = etalonNode.SelectTokens("standard_output", errorWhenNoMatch: false)!.FirstOrDefault();
              var error_outputNode = etalonNode.SelectTokens("error_output", errorWhenNoMatch: false)!.FirstOrDefault();

              addenterpriseSection =
              $@"{{
""condition"": ""{condition}"",
""etalon"": {{
  ""standard_output"": ""{standard_outputNode!.Value<string>().Replace(@"\", @"\\")}""";


              if (error_outputNode != null)
              {
                addenterpriseSection +=
                  $@",""error_output"": ""{error_outputNode!.Value<string>().Replace(@"\", @"\\")}""";
              }


              addenterpriseSection += "}}";

              ((JArray)console_output_equalNode)!.Add(JObject.Parse(addenterpriseSection));
              return true;
            }
          }
          else if (entkey == "files_equal" || entkey == "directory_equal" || entkey == "directory_equals_only_file_name" || entkey == "directory_equals_by_filename_with_content")
          {
            var enterprise = jsonObject!.SelectTokens($"assert.{entkey}[*].condition", errorWhenNoMatch: false)!.FirstOrDefault();
            if (enterprise == null)
            {

              var filesEqualNode = jsonObject!.SelectTokens($"assert.{entkey}", errorWhenNoMatch: false)!.FirstOrDefault();
              if (filesEqualNode != null)
              {
                var etalonNode = filesEqualNode.SelectTokens("[*].etalon", errorWhenNoMatch: false)!.FirstOrDefault();
                var actualNode = filesEqualNode.SelectTokens("[*].actual", errorWhenNoMatch: false)!.FirstOrDefault();

                // неправильная структура. Не массив а просто вложено.
                if (etalonNode == null)
                {
                  // пробуем исправить ситуацию.
                  etalonNode = filesEqualNode.SelectTokens("etalon", errorWhenNoMatch: false)!.FirstOrDefault();
                  actualNode = filesEqualNode.SelectTokens("actual", errorWhenNoMatch: false)!.FirstOrDefault();

                  if (etalonNode == null)
                  {
                    Console.ForegroundColor = ConsoleColor.Red;
                    Console.WriteLine($"The section {entkey} in test {testId} was not in correct format.");
                    Console.ResetColor();
                    return false;
                  }

                  var assertNode = (JObject)jsonObject!.SelectTokens("assert", errorWhenNoMatch: false)!.FirstOrDefault()!;

                  string filesEqualNodeSection =
      $@"
[
             {{
                ""etalon"" : ""{etalonNode!.Value<string>()}""";

                  if (actualNode != null)
                  {
                    filesEqualNodeSection += @$",""actual"" : ""{actualNode!.Value<string>()} """;
                    filesEqualNodeSection += "}]";
                  }

                  assertNode!.Property($"{entkey}")!.Remove();
                  assertNode!.Add($"{entkey}", JArray.Parse(filesEqualNodeSection));
                  filesEqualNode = jsonObject!.SelectTokens($"assert.{entkey}", errorWhenNoMatch: false)!.FirstOrDefault();
                }

                string addenterpriseSection =
              $@"{{
""condition"": ""{condition}"",
""etalon"": ""{etalonNode!.Value<string>().Replace(@"\", @"\\")}"",
""actual"": ""{actualNode!.Value<string>().Replace(@"\", @"\\")}""    
}}";

                ((JArray)filesEqualNode).Add(JObject.Parse(addenterpriseSection));
              }

              return true;
            }
          }
        }
      }

      return false;
    }
  }
}
