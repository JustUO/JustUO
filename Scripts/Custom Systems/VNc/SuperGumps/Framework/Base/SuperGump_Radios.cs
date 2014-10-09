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

namespace VitaNex.SuperGumps
{
	public abstract partial class SuperGump
	{
		public Dictionary<GumpRadio, Action<GumpRadio, bool>> Radios { get; private set; }

		public Action<GumpRadio, bool> RadioHandler { get; set; }

		public void AddRadio(int x, int y, int offID, int onID, bool state, int radioID)
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

		protected void AddRadio(GumpRadio radio, Action<GumpRadio, bool> handler)
		{
			if (radio == null)
			{
				return;
			}

			if (!Radios.ContainsKey(radio))
			{
				Radios.Add(radio, handler);
			}
			else
			{
				Radios[radio] = handler;
			}

			Add(radio);
		}

		public virtual void HandleRadio(GumpRadio radio, bool state)
		{
			if (RadioHandler != null)
			{
				RadioHandler(radio, state);
			}
			else if (Radios[radio] != null)
			{
				Radios[radio](radio, state);
			}
		}

		public virtual bool CanDisplay(GumpRadio radio)
		{
			return (radio != null);
		}

		public GumpRadio GetRadioEntry(int radioID)
		{
			return Radios.Keys.FirstOrDefault(radio => radio.SwitchID == radioID);
		}
	}
}