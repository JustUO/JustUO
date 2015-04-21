#region Header
//   Vorspire    _,-'/-'/  SuperGump_TileButtons.cs
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
		public GumpImageTileButton LastTileButtonClicked { get; private set; }

		public Dictionary<GumpImageTileButton, Action<GumpImageTileButton>> TileButtons { get; private set; }

		public virtual Action<GumpImageTileButton> TileButtonHandler { get; set; }

		public new void AddImageTiledButton(
			int x,
			int y,
			int normalID,
			int pressedID,
			int buttonID,
			GumpButtonType type,
			int param,
			int itemID,
			int hue,
			int width,
			int height)
		{
			if (type == GumpButtonType.Page)
			{
				AddImageTiledButton(
					new GumpImageTileButton(x, y, normalID, pressedID, 0, GumpButtonType.Page, param, itemID, hue, width, height), null);
			}
			else
			{
				AddImageTiledButton(x, y, normalID, pressedID, buttonID, itemID, hue, width, height, null);
			}
		}

		public void AddImageTiledButton(int x, int y, int normalID, int pressedID, int itemID, int hue, int width, int height)
		{
			AddImageTiledButton(x, y, normalID, pressedID, NewButtonID(), itemID, hue, width, height, null);
		}

		public void AddImageTiledButton(
			int x,
			int y,
			int normalID,
			int pressedID,
			int itemID,
			int hue,
			int width,
			int height,
			Action<GumpImageTileButton> handler)
		{
			AddImageTiledButton(x, y, normalID, pressedID, NewButtonID(), itemID, hue, width, height, handler);
		}

		public void AddImageTiledButton(
			int x, int y, int normalID, int pressedID, int buttonID, int itemID, int hue, int width, int height)
		{
			AddImageTiledButton(x, y, normalID, pressedID, buttonID, itemID, hue, width, height, null);
		}

		public void AddImageTiledButton(
			int x,
			int y,
			int normalID,
			int pressedID,
			int buttonID,
			int itemID,
			int hue,
			int width,
			int height,
			Action<GumpImageTileButton> handler)
		{
			AddImageTiledButton(
				new GumpImageTileButton(x, y, normalID, pressedID, buttonID, GumpButtonType.Reply, 0, itemID, hue, width, height),
				handler);
		}

		protected void AddImageTiledButton(GumpImageTileButton entry, Action<GumpImageTileButton> handler)
		{
			if (entry == null || !CanDisplay(entry))
			{
				return;
			}

			if (!TileButtons.ContainsKey(entry))
			{
				TileButtons.Add(entry, handler);
			}
			else
			{
				TileButtons[entry] = handler;
			}

			Add(entry);
		}

		public virtual void HandleTileButtonClick(GumpImageTileButton button)
		{
			DateTime now = DateTime.UtcNow;

			DoubleClicked = LastTileButtonClicked != null && now < LastButtonClick + DClickInterval &&
							(LastTileButtonClicked == button || LastTileButtonClicked.ButtonID == button.ButtonID ||
							 (LastTileButtonClicked.Parent == button.Parent && LastTileButtonClicked.X == button.X &&
							  LastTileButtonClicked.Y == button.Y && LastTileButtonClicked.Type == button.Type &&
							  LastTileButtonClicked.Param == button.Param));

			LastTileButtonClicked = button;
			LastButtonClick = now;

			OnClick(button);

			if (DoubleClicked)
			{
				OnDoubleClick(button);
			}

			if (TileButtonHandler != null)
			{
				TileButtonHandler(button);
			}
			else if (TileButtons[button] != null)
			{
				TileButtons[button](button);
			}
		}

		protected virtual void OnClick(GumpImageTileButton entry)
		{ }

		protected virtual void OnDoubleClick(GumpImageTileButton entry)
		{ }

		protected virtual bool CanDisplay(GumpImageTileButton entry)
		{
			return entry != null;
		}

		public GumpImageTileButton GetTileButtonEntry(int buttonID)
		{
			return TileButtons.Keys.FirstOrDefault(button => button.ButtonID == buttonID);
		}
	}
}