﻿using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Text.RegularExpressions;
using System.Threading.Tasks;

namespace Common
{
  public static class ConnectionHelper
  {
    public static string GetConnectionName(string connection)
    {
      /*
       если в наименовании соединения указан порт 3306, то меняем на %mysqllast%
       если в наименовании соединения не указан порт (например %dbfmylast(root)*-utf8%), то меняем на %mysqllast%
       если в наименовании соединения указан порт 3320, то меняем на %mariadblast%
      */

      if (connection.Contains("3306"))
      {
        return Constants.AffordableConnectionName;
      }
      else if (connection.Contains("3320"))
      {
        return "%mariadblast%";
      }
      else
      {
        return Constants.AffordableConnectionName;
      }
    }
  }
}