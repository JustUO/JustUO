using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;

namespace Server.Gumps
{
  public partial class FontHandling
  {
    public const int FONT_LINE_HEIGHT = 18;

    static char[] splitChars = new char[] { ' ', '-', '\t' };
    static string[] lineSeparators = new string[] { Environment.NewLine };

    public enum FontSize
    {
      Small = 3,
      Medium = 4, 
      Large = 5
    }


    public static string[] WordWrap(string str, int width)
    {
      string[] words = Explode(str, splitChars);

      int curLineLength = 0;
      StringBuilder strBuilder = new StringBuilder();
      for (int i = 0; i < words.Length; i += 1)
      {
        string word = words[i];
        // If adding the new word to the current line would be too long,
        // then put it on a new line (and split it up if it's too long).
        if (curLineLength + word.Length > width)
        {
          // Only move down to a new line if we have text on the current line.
          // Avoids situation where wrapped whitespace causes emptylines in text.
          if (curLineLength > 0)
          {
            strBuilder.Append(Environment.NewLine);
            curLineLength = 0;
          }

          // If the current word is too long to fit on a line even on it's own then
          // split the word up.
          while (word.Length > width)
          {
            strBuilder.Append(word.Substring(0, width - 1) + "-");
            word = word.Substring(width - 1);

            strBuilder.Append(Environment.NewLine);
          }

          // Remove leading whitespace from the word so the new line starts flush to the left.
          word = word.TrimStart();
        }
        strBuilder.Append(word);
        curLineLength += word.Length;
      }

      return strBuilder.ToString().Split(lineSeparators, StringSplitOptions.RemoveEmptyEntries);
    }

    private static string[] Explode(string str, char[] splitChars)
    {
      List<string> parts = new List<string>();
      int startIndex = 0;
      while (true)
      {
        int index = str.IndexOfAny(splitChars, startIndex);

        if (index == -1)
        {
          parts.Add(str.Substring(startIndex));
          return parts.ToArray();
        }

        string word = str.Substring(startIndex, index - startIndex);
        char nextChar = str.Substring(index, 1)[0];
        // Dashes and the likes should stick to the word occuring before it. Whitespace doesn't have to.
        if (char.IsWhiteSpace(nextChar))
        {
          parts.Add(word);
          parts.Add(nextChar.ToString());
        }
        else
        {
          parts.Add(word + nextChar);
        }

        startIndex = index + 1;
      }
    }

    public static int CalculateTextLengthInPixels(string text, FontSize size, bool italicized, bool bold )
    {
      int displayTextLengthInPixels = 0;

      int[] characterSet = null;

      if (size == FontSize.Small)
      {
        characterSet = FONT_SIZE_3_UNICODE_CHARACTER_WIDTH_TABLE;
      }
      else if (size == FontSize.Large)
      {
        characterSet = FONT_SIZE_5_UNICODE_CHARACTER_WIDTH_TABLE;
      }
      else
      {
        characterSet = FONT_SIZE_4_UNICODE_CHARACTER_WIDTH_TABLE;
      } 

      try
      {
        for (int i = 0; i < text.Length; ++i)
        {

          if ((int)text[i] < 10000)
          {
            displayTextLengthInPixels += characterSet[(int)text[i]];
          }
        }

        displayTextLengthInPixels += text.Length;

        if (italicized | bold)
        {
          displayTextLengthInPixels += text.Length;
        }
      }
      catch (Exception e)
      {
        Console.WriteLine(e.ToString());
      }

      if (displayTextLengthInPixels > 0 && displayTextLengthInPixels < 3)
      {
        displayTextLengthInPixels = 3;
      }
      return displayTextLengthInPixels;
    }
  }
}
