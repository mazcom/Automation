// --------------------------------------------------------------------------
// <copyright file="EncodingDetector.cs" company="Devart">
//
// Copyright (c) Devart. ALL RIGHTS RESERVED
// The entire contents of this file is protected by International Copyright Laws.
// Unauthorized reproduction, and distribution of all or any portion of the code
// contained in this file is strictly prohibited and may result in severe civil
// and criminal penalties and will be prosecuted to the maximum extent possible.
//
// RESTRICTIONS
//
// THIS SOURCE CODE AND ALL RESULTING INTERMEDIATE FILES ARE CONFIDENTIAL AND
// PROPRIETARY TRADE SECRETS OF DEVART.
//
// THE SOURCE CODE CONTAINED WITHIN THIS FILE AND ALL RELATED FILES OR ANY PORTION
// OF ITS CONTENTS SHALL AT NO TIME BE COPIED, TRANSFERRED, SOLD, DISTRIBUTED,
// OR OTHERWISE MADE AVAILABLE TO OTHER INDIVIDUALS WITHOUT EXPRESS WRITTEN CONSENT
// AND PERMISSION FROM DEVART.
// </copyright>
// --------------------------------------------------------------------------

using System;
using System.IO;
using System.Text;

namespace Text.Compare
{
  internal static class EncodingDetector
  {
    private static readonly Encoding DefaultEncoding;

    static EncodingDetector()
    {
      Encoding.RegisterProvider(System.Text.CodePagesEncodingProvider.Instance);
      var ss= Encoding.GetEncoding(0);
      DefaultEncoding = Encoding.UTF8;  //Encoding.GetEncoding(0);
    }

    public static Encoding Detect(string fileName)
    {
      // Проверить размер файлов, если файлы > 50 mb то загрузить без кодировки, 
      // иначе можно получить OutOfMemory
      if (GetFileLength(fileName) < 52428800)
      {
        return GetFileEncoding(fileName);
      }
      else
      {
        return Encoding.Default;
      }
    }

    /// <summary>
    /// Detects the encoding by looking at the first bytes of the stream of a file.
    /// </summary>
    /// <param name="fileName">File name.</param>
    /// <returns>Encoding for the file.</returns>
    /// <remarks>This method can be used to set encoding to the StreamReader or etc.. 
    /// StreamReader specifically has an encoding detection by reading byte order marks (BOM),
    /// but it does not understand Encoding.Default if it can not find a BOM.</remarks>
    public static Encoding Detect(Stream stream)
    {
      // Проверить размер файлов, если файлы > 50 mb то загрузить без кодировки, 
      // иначе можно получить OutOfMemory
      if (stream.Length < 52428800)
      {
        return GetEncoding(stream, out _);
      }
      else
      {
        return Encoding.Default;
      }
    }

    /// <summary>
    /// Retrieves file length.
    /// </summary>
    /// <param name="fileName">Full file name.</param>
    /// <returns>File length</returns>
    private static int GetFileLength(string fileName)
    {
      var fi = new FileInfo(fileName);
      return (int)((fi.Length + 1048575) >> 20); // File length in Mb
    }

    /// <summary>
    /// Gets encoding of the specified file by reading its preamble.
    /// </summary>
    /// <param name="filePath">File path.</param>
    /// <returns>Encoding for the file if it has determined, otherwise Encoding.Default.</returns>
    private static Encoding GetFileEncoding(string filePath)
    {
      // allow to open file that is currently open by another program to edit.

      using (var file = new FileStream(filePath, FileMode.Open, FileAccess.Read, FileShare.ReadWrite))
      {
        return Detect(file);
      }
    }



    public static Encoding GetEncoding(Stream stream, out int preambleLength)
    {
      if (!stream.CanSeek)
      {
        preambleLength = 0;
        return Encoding.Default;
      }
      else
      {
        return DetectTextEncoding(GetEncodingHeader(stream), out preambleLength);
      }
    }

    private static byte[] GetEncodingHeader(Stream stream)
    {
      var position = stream.Position;
      try
      {
        var header = new byte[10000];
        var realRead = stream.Read(header, 0, 10000);
        Array.Resize(ref header, realRead);
        return header;
      }
      finally
      {
        stream.Position = position; // Back position.
      }
    }

    // Function to detect the encoding for UTF-7, UTF-8/16/32 (bom, no bom, little
    // & big endian), and local default codepage, and potentially other codepages.
    // 'taster' = number of bytes to check of the file (to save processing). Higher
    // value is slower, but more reliable (especially UTF-8 with special characters
    // later on may appear to be ASCII initially). If taster = 0, then taster
    // becomes the length of the file (for maximum reliability). 'text' is simply
    // the string with the discovered encoding applied to the file.
    private static Encoding DetectTextEncoding(byte[] b, out int preambleLength, int taster = 10000)
    {
      //////////////// First check the low hanging fruit by checking if a
      //////////////// BOM/signature exists (sourced from http://www.unicode.org/faq/utf_bom.html#bom4)
      preambleLength = 4;
      if (b.Length >= 4 && b[0] == 0x00 && b[1] == 0x00 && b[2] == 0xFE && b[3] == 0xFF)
      {
        return Encoding.GetEncoding("utf-32BE");  // UTF-32, big-endian 
      }

      if (b.Length >= 4 && b[0] == 0xFF && b[1] == 0xFE && b[2] == 0x00 && b[3] == 0x00)
      {
        return Encoding.UTF32;  // UTF-32, little-endian
      }

      preambleLength = 2;
      if (b.Length >= 2 && b[0] == 0xFE && b[1] == 0xFF)
      {
        return Encoding.BigEndianUnicode; // UTF-16, big-endian
      }

      if (b.Length >= 2 && b[0] == 0xFF && b[1] == 0xFE)
      {
        return Encoding.Unicode;          // UTF-16, little-endian
      }

      preambleLength = 3;
      if (b.Length >= 3 && b[0] == 0xEF && b[1] == 0xBB && b[2] == 0xBF)
      {
        return Encoding.UTF8;  // UTF-8
      }

      if (b.Length >= 3 && b[0] == 0x2b && b[1] == 0x2f && b[2] == 0x76)
      {
        return Encoding.UTF7;  // UTF-7
      }

      preambleLength = 0;
      //////////// If the code reaches here, no BOM/signature was found, so now
      //////////// we need to 'taste' the file to see if can manually discover
      //////////// the encoding. A high taster value is desired for UTF-8
      if (taster == 0 || taster > b.Length)
      {
        taster = b.Length;    // Taster size can't be bigger than the filesize obviously.
      }


      // Some text files are encoded in UTF8, but have no BOM/signature. Hence
      // the below manually checks for a UTF8 pattern. This code is based off
      // the top answer at: http://stackoverflow.com/questions/6555015/check-for-invalid-utf8
      // For our purposes, an unnecessarily strict (and terser/slower)
      // implementation is shown at: http://stackoverflow.com/questions/1031645/how-to-detect-utf-8-in-plain-c
      // For the below, false positives should be exceedingly rare (and would
      // be either slightly malformed UTF-8 (which would suit our purposes
      // anyway) or 8-bit extended ASCII/UTF-16/32 at a vanishingly long shot).
      var i = 0;
      var utf8 = false;
      while (i < taster - 4)
      {
        if (b[i] <= 0x7F)
        { i += 1; continue; }     // If all characters are below 0x80, then it is valid UTF8, but UTF8 is not 'required' (and therefore the text is more desirable to be treated as the default codepage of the computer). Hence, there's no "utf8 = true;" code unlike the next three checks.
        if (b[i] >= 0xC2 && b[i] <= 0xDF && b[i + 1] >= 0x80 && b[i + 1] < 0xC0)
        { i += 2; utf8 = true; continue; }
        if (b[i] >= 0xE0 && b[i] <= 0xF0 && b[i + 1] >= 0x80 && b[i + 1] < 0xC0 && b[i + 2] >= 0x80 && b[i + 2] < 0xC0)
        { i += 3; utf8 = true; continue; }
        if (b[i] >= 0xF0 && b[i] <= 0xF4 && b[i + 1] >= 0x80 && b[i + 1] < 0xC0 && b[i + 2] >= 0x80 && b[i + 2] < 0xC0 && b[i + 3] >= 0x80 && b[i + 3] < 0xC0)
        { i += 4; utf8 = true; continue; }
        utf8 = false;
        break;
      }
      if (utf8 == true)
      {
        return Encoding.UTF8;
      }

      // The next check is a heuristic attempt to detect UTF-16 without a BOM.
      // We simply look for zeroes in odd or even byte places, and if a certain
      // threshold is reached, the code is 'probably' UF-16.          
      var threshold = 0.1; // proportion of chars step 2 which must be zeroed to be diagnosed as utf-16. 0.1 = 10%
      var count = 0;
      for (var n = 0; n < taster; n += 2)
      {
        if (b[n] == 0)
        {
          count++;
        }
      }

      if (((double)count) / taster > threshold)
      {
        return Encoding.BigEndianUnicode;
      }

      count = 0;
      for (var n = 1; n < taster; n += 2)
      {
        if (b[n] == 0)
        {
          count++;
        }
      }

      if (((double)count) / taster > threshold)
      {
        return Encoding.Unicode; // (little-endian)
      }

      // If all else fails, the encoding is probably (though certainly not
      // definitely) the user's local codepage! One might present to the user a
      // list of alternative encodings as shown here: http://stackoverflow.com/questions/8509339/what-is-the-most-common-encoding-of-each-language
      // A full list can be found using Encoding.GetEncodings();
      return DefaultEncoding;
    }
  }
}