#region Header
//   Vorspire    _,-'/-'/  SuperGump_TextInputs.cs
//   .      __,-; ,'( '/
//    \.    `-.__`-._`:_,-._       _ , . ``
//     `:-._,------' ` _,`--` -: `_ , ` ,' :
//        `---..__,,--'  (C) 2014  ` -'. -'
//        #  Vita-Nex [http://core.vita-nex.com]  #
//  {o)xxx|===============-   #   -===============|xxx(o}
//        #        The MIT License (MIT)          #
#endregion

#region References
using System;
using System.Collections.Generic;
using System.Linq;

using Server.Gumps;
#endregion

#pragma warning disable 109

namespace VitaNex.SuperGumps
{
	public abstract partial class SuperGump
	{
		public Dictionary<GumpTextEntry, Action<GumpTextEntry, string>> TextInputs { get; private set; }

		public Action<GumpTextEntry, string> TextInputHandler { get; set; }

		public new void AddTextEntry(int x, int y, int width, int height, int hue, int inputID, string text)
		{
			AddTextEntry(x, y, width, height, hue, inputID, text, null);
		}

		public void AddTextEntry(int x, int y, int width, int height, int hue, string text)
		{
			AddTextEntry(x, y, width, height, hue, NewTextEntryID(), text, null);
		}

		public void AddTextEntry(
			int x, int y, int width, int height, int hue, string text, Action<GumpTextEntry, string> handler)
		{
			AddTextEntry(x, y, width, height, hue, NewTextEntryID(), text, handler);
		}

		public void AddTextEntry(
			int x, int y, int width, int height, int hue, int entryID, string text, Action<GumpTextEntry, string> handler)
		{
			AddTextEntry(new GumpTextEntry(x, y, width, height, hue, entryID, text), handler);
		}

		protected void AddTextEntry(GumpTextEntry input, Action<GumpTextEntry, string> handler)
		{
			if (input == null)
			{
				return;
			}

			if (!TextInputs.ContainsKey(input))
			{
				TextInputs.Add(input, handler);
			}
			else
			{
				TextInputs[input] = handler;
			}

			Add(input);
		}

		public virtual void HandleTextInput(GumpTextEntry input, string text)
		{
			if (TextInputHandler != null)
			{
				TextInputHandler(input, text);
			}
			else if (TextInputs[input] != null)
			{
				TextInputs[input](input, text);
			}
		}

		public virtual bool CanDisplay(GumpTextEntry input)
		{
			return (input != null);
		}

		public new GumpTextEntry GetTextEntry(int inputID)
		{
			return TextInputs.Keys.FirstOrDefault(input => input.EntryID == inputID);
		}
	}
}