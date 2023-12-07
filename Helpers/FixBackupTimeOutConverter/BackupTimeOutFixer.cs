using Microsoft.VisualBasic;
using Newtonsoft.Json.Linq;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Text.Json.Nodes;
using System.Text.RegularExpressions;
using System.Threading.Channels;
using System.Threading.Tasks;
using System.Xml.Linq;

namespace EnterpriseFixer
{
  public class BackupTimeOutFixer
  {
    //private const string absoluteBackupFileName = @"[A-Za-z]:\\[A-Za-z0-9_\\s-\\(\\)]+\\.bak\\s?";
    private const string absoluteBackupFileName = @"[A-Za-z]:[A-Za-z0-9_\\s-]+.bak";

    public static bool TryFix(JObject jsonObject)
    {

      // Находим в post run бекапы
      var postRunNodes = jsonObject!.SelectTokens("post_run[*].actions[*].run.code.code", errorWhenNoMatch: false)!.
        Where(t => ((string)t!).Contains(".bak", StringComparison.OrdinalIgnoreCase))!.ToArray();

      // Если нет бекапов то ничего не делаем.
      if (postRunNodes.Length == 0)
      {
        return false;
      }

      List<string> backupFileNames = new();

      // выпаршиваем имена полные пути бекапов
      foreach (JValue runToken in postRunNodes)
      {
        var сommandLine = (string)runToken!;

        var match = Regex.Match(сommandLine, absoluteBackupFileName);
        if (match.Success)
        {
          backupFileNames.Add(match.Value);
        }
      }

      // находим в pre run фиксы с таймтингами сделанные ранее.
      var preRunNodes = jsonObject!.SelectTokens("pre_run[*].run.code.code", errorWhenNoMatch: false)!.
        Where(t => ((string)t!).Contains("$max = 30", StringComparison.OrdinalIgnoreCase))!.ToArray();

      JArray array = null;

      // удаляем старые фиксы с таймингами
      if (preRunNodes.Length > 0)
      {
        foreach (var node in preRunNodes)
        {
          var runNode = node.Parent.Parent.Parent.Parent.Parent.Parent;
          array = runNode.Parent as JArray;
          array.Remove(runNode);
        }
      }

      if (array == null)
      {
        array = (JArray)jsonObject["pre_run"]!;
      }

      // добавляем новые("правильные")
      foreach (var backupFileName in backupFileNames)
      {
        string cleanUpSection =
          $@"
{{
                ""run"" : {{
                            ""code"" : {{
                                         ""type"" : ""ps"",
                                         ""code"" : ""$max = 30 \r\n while (!(Test-Path '{backupFileName.Replace("\\","\\\\")}') -and $max -gt 0 ) \r\n {{ Start-Sleep 1 \r\n $max -= 1}}""
                            }}  
                }},
                ""exit_codes"":[
                  0
                ]
             }}
          ";

        array.Add(JObject.Parse(cleanUpSection));
      }

      return true;
    }
  }
}