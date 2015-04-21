#region Header
//   Vorspire    _,-'/-'/  SuperGump_Buttons.cs
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
		public GumpButton LastButtonClicked { get; private set; }

		public Dictionary<GumpButton, Action<GumpButton>> Buttons { get; private set; }

		public virtual Action<GumpButton> ButtonHandler { get; set; }

		public new void AddButton(int x, int y, int normalID, int pressedID, int buttonID, GumpButtonType type, int param)
		{
			if (type == GumpButtonType.Page)
			{
				AddButton(new GumpButton(x, y, normalID, pressedID, 0, GumpButtonType.Page, param), null);
			}
			else
			{
				AddButton(x, y, normalID, pressedID, buttonID, null);
			}
		}

		public void AddButton(int x, int y, int normalID, int pressedID)
		{
			AddButton(x, y, normalID, pressedID, NewButtonID(), null);
		}

		public void AddButton(int x, int y, int normalID, int pressedID, Action<GumpButton> handler)
		{
			AddButton(x, y, normalID, pressedID, NewButtonID(), handler);
		}

		public void AddButton(int x, int y, int normalID, int pressedID, int buttonID)
		{
			AddButton(x, y, normalID, pressedID, buttonID, null);
		}

		public void AddButton(int x, int y, int normalID, int pressedID, int buttonID, Action<GumpButton> handler)
		{
			AddButton(new GumpButton(x, y, normalID, pressedID, buttonID, GumpButtonType.Reply, 0), handler);
		}

		protected void AddButton(GumpButton entry, Action<GumpButton> handler)
		{
			if (entry == null || !CanDisplay(entry))
			{
				return;
			}

			if (!Buttons.ContainsKey(entry))
			{
				Buttons.Add(entry, handler);
			}
			else
			{
				Buttons[entry] = handler;
			}

			Add(entry);
		}

		public virtual void HandleButtonClick(GumpButton button)
		{
			DateTime now = DateTime.UtcNow;

			DoubleClicked = LastButtonClicked != null && now < LastButtonClick + DClickInterval &&
							(LastButtonClicked == button || LastButtonClicked.ButtonID == button.ButtonID ||
							 (LastButtonClicked.Parent == button.Parent && LastButtonClicked.X == button.X && LastButtonClicked.Y == button.Y &&
							  LastButtonClicked.Type == button.Type && LastButtonClicked.Param == button.Param));

			LastButtonClicked = button;
			LastButtonClick = now;

			OnClick();

			OnClick(button);

			if (DoubleClicked)
			{
				OnDoubleClick(button);
			}

			if (ButtonHandler != null)
			{
				ButtonHandler(button);
			}
			else if (Buttons[button] != null)
			{
				Buttons[button](button);
			}
		}

		protected virtual void OnClick(GumpButton entry)
		{ }

		protected virtual void OnDoubleClick(GumpButton entry)
		{ }

		protected virtual bool CanDisplay(GumpButton entry)
		{
			return entry != null;
		}

		public GumpButton GetButtonEntry(int buttonID)
		{
			return Buttons.Keys.FirstOrDefault(button => button.ButtonID == buttonID);
		}
	}
}