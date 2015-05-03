#region Header
//   Vorspire    _,-'/-'/  SuperGump_Radios.cs
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
		public Dictionary<GumpRadio, Action<GumpRadio, bool>> Radios { get; private set; }

		public Action<GumpRadio, bool> RadioHandler { get; set; }

		public new void AddRadio(int x, int y, int offID, int onID, bool state, int radioID)
		{
			AddRadio(x, y, offID, onID, radioID, state, null);
		}

		public void AddRadio(int x, int y, int offID, int onID, bool state)
		{
			AddRadio(x, y, offID, onID, NewSwitchID(), state, null);
		}

		public void AddRadio(int x, int y, int offID, int onID, bool state, Action<GumpRadio, bool> handler)
		{
			AddRadio(x, y, offID, onID, NewSwitchID(), state, handler);
		}

		public void AddRadio(int x, int y, int offID, int onID, int radioID, bool state)
		{
			AddRadio(x, y, offID, onID, radioID, state, null);
		}

		public void AddRadio(int x, int y, int offID, int onID, int radioID, bool state, Action<GumpRadio, bool> handler)
		{
			AddRadio(new GumpRadio(x, y, offID, onID, state, radioID), handler);
		}

		protected void AddRadio(GumpRadio entry, Action<GumpRadio, bool> handler)
		{
			if (entry == null)
			{
				return;
			}

			if (!Radios.ContainsKey(entry))
			{
				Radios.Add(entry, handler);
			}
			else
			{
				Radios[entry] = handler;
			}

			Add(entry);
		}

		public virtual void HandleRadio(GumpRadio entry, bool state)
		{
			if (RadioHandler != null)
			{
				RadioHandler(entry, state);
			}
			else if (Radios[entry] != null)
			{
				Radios[entry](entry, state);
			}
		}

		public virtual bool CanDisplay(GumpRadio entry)
		{
			return entry != null;
		}

		public GumpRadio GetRadioEntry(int radioID)
		{
			return Radios.Keys.FirstOrDefault(radio => radio.SwitchID == radioID);
		}
	}
}