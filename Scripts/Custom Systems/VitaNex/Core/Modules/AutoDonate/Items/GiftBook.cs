#region Header
//   Vorspire    _,-'/-'/  GiftBook.cs
//   .      __,-; ,'( '/
//    \.    `-.__`-._`:_,-._       _ , . ``
//     `:-._,------' ` _,`--` -: `_ , ` ,' :
//        `---..__,,--'  (C) 2014  ` -'. -'
//        #  Vita-Nex [http://core.vita-nex.com]  #
//  {o)xxx|===============-   #   -===============|xxx(o}
//        #        The MIT License (MIT)          #
#endregion

#region References
using System.Text;

using Server;
using Server.Items;
#endregion

namespace VitaNex.Modules.AutoDonate
{
	public class DonationGiftBook : RedBook
	{
		[Constructable]
		public DonationGiftBook(Mobile author, string text)
			: base("A Gift", author.RawName, GetPageCount(text), false)
		{
			Hue = 0x89B;
			SetText(text);
		}

		public DonationGiftBook(Serial serial)
			: base(serial)
		{ }

		private static int GetPageCount(string text)
		{
			int wordCount, wordsPerLine, linesPerPage, index, pageCount;
			ParseFactors(text.Split(' '), out wordCount, out wordsPerLine, out linesPerPage, out index, out pageCount);

			return pageCount;
		}

		private void SetText(string text)
		{
			var words = text.Split(' ');

			int wordCount, wordsPerLine, linesPerPage, index, pageCount;
			ParseFactors(words, out wordCount, out wordsPerLine, out linesPerPage, out index, out pageCount);

			for (int currentPage = 0; currentPage < pageCount; currentPage++)
			{
				for (int currentLine = 0; currentLine < linesPerPage; currentLine++)
				{
					Pages[currentPage] = new BookPageInfo(new string[linesPerPage]);
					StringBuilder line = new StringBuilder();

					for (int currentWord = 0; currentWord < wordsPerLine; currentWord++)
					{
						if (index >= wordCount)
						{
							continue;
						}

						line.AppendFormat(" {0}", words[index]);
						index++;
					}

					Pages[currentPage].Lines[currentLine] = line.ToString();
				}
			}
		}

		private static void ParseFactors(
			string[] words, out int wordCount, out int wordsPerLine, out int linesPerPage, out int index, out int pageCount)
		{
			wordCount = words.Length;
			wordsPerLine = 5;
			linesPerPage = 8;
			index = 0;
			pageCount = wordCount / (wordsPerLine * linesPerPage);
		}

		public override void Serialize(GenericWriter writer)
		{
			base.Serialize(writer);

			int version = writer.SetVersion(0);

			switch (version)
			{
				case 0:
					break;
			}
		}

		public override void Deserialize(GenericReader reader)
		{
			base.Deserialize(reader);

			int version = reader.GetVersion();

			switch (version)
			{
				case 0:
					break;
			}
		}
	}
}