#region Header
//   Vorspire    _,-'/-'/  SuperGump_LimitedTextInputs.cs
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
		public Dictionary<GumpTextEntryLimited, Action<GumpTextEntryLimited, string>> LimitedTextInputs { get; private set; }

		public Action<GumpTextEntryLimited, string> LimitedTextInputHandler { get; set; }

		public new void AddTextEntry(int x, int y, int width, int height, int hue, int inputID, string text, int length)
		{
			AddTextEntryLimited(x, y, width, height, hue, inputID, text, length, null);
		}

		public void AddTextEntryLimited(int x, int y, int width, int height, int hue, string text, int length)
		{
			AddTextEntryLimited(x, y, width, height, hue, NewTextEntryID(), text, length, null);
		}

		public void AddTextEntryLimited(
			int x, int y, int width, int height, int hue, string text, int length, Action<GumpTextEntryLimited, string> handler)
		{
			AddTextEntryLimited(x, y, width, height, hue, NewTextEntryID(), text, length, handler);
		}

		public void AddTextEntryLimited(int x, int y, int width, int height, int hue, int inputID, string text, int length)
		{
			AddTextEntryLimited(x, y, width, height, hue, inputID, text, length, null);
		}

		public void AddTextEntryLimited(
			int x,
			int y,
			int width,
			int height,
			int hue,
			int inputID,
			string text,
			int length,
			Action<GumpTextEntryLimited, string> handler)
		{
			AddTextEntryLimited(new GumpTextEntryLimited(x, y, width, height, hue, inputID, text, length), handler);
		}

		protected void AddTextEntryLimited(GumpTextEntryLimited input, Action<GumpTextEntryLimited, string> handler)
		{
			if (input == null)
			{
				return;
			}

			if (!LimitedTextInputs.ContainsKey(input))
			{
				LimitedTextInputs.Add(input, handler);
			}
			else
			{
				LimitedTextInputs[input] = handler;
			}

			Add(input);
		}

		public virtual void HandleLimitedTextInput(GumpTextEntryLimited input, string text)
		{
			if (LimitedTextInputHandler != null)
			{
				LimitedTextInputHandler(input, text);
			}
			else if (LimitedTextInputs[input] != null)
			{
				LimitedTextInputs[input](input, text);
			}
		}

		public virtual bool CanDisplay(GumpTextEntryLimited input)
		{
			return (input != null);
		}

		public GumpTextEntryLimited GetTextEntryLimited(int inputID)
		{
			return LimitedTextInputs.Keys.FirstOrDefault(input => input.EntryID == inputID);
		}
	}
}