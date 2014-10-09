#region Header
//   Vorspire    _,-'/-'/  BaseThrowableOPL.cs
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
using System.Drawing;

using Server;

using VitaNex.Network;
#endregion

namespace VitaNex.Items
{
	public static class BaseThrowableOPL
	{
		public static void Initialize()
		{
			ExtendedOPL.OnItemOPLRequest += OnItemOPLRequest;
		}

		private static void OnItemOPLRequest(Item item, Mobile viewer, ExtendedOPL list)
		{
			if (item == null || list == null)
			{
				return;
			}

			if (viewer == null && item.Parent is Mobile)
			{
				viewer = (Mobile)item.Parent;
			}

			if (viewer == null)
			{
				return;
			}

			var throwable = item as IBaseThrowable;

			if (throwable == null)
			{
				return;
			}

			GetProperties(throwable, viewer, list);
		}

		public static void GetProperties(IBaseThrowable throwable, Mobile viewer, ExtendedOPL list)
		{
			if (throwable == null || list == null)
			{
				return;
			}

			var lines = new List<string>();

			if (throwable.Consumable)
			{
				lines.Add("Consumable");
			}

			if (throwable.ClearHands)
			{
				lines.Add("Clears Hands");
			}

			if (!throwable.AllowCombat)
			{
				lines.Add("Non-Combat");
			}

			if (lines.Count > 0)
			{
				string s = String.Format("<basefont color=#{0:X6}>{1}", Color.Orange.ToArgb(), String.Join(", ", lines));
				lines.Clear();
				lines.Add(s);
			}

			if (throwable.RequiredSkillValue > 0)
			{
				lines.Add(
					String.Format(
						"<basefont color=#ffffff>Required Skill: {0} - {1:F2}%", throwable.RequiredSkill, throwable.RequiredSkillValue));
			}

			DateTime now = DateTime.UtcNow, readyWhen = (throwable.ThrownLast + throwable.ThrowRecovery);
			TimeSpan diff = TimeSpan.Zero;

			if (readyWhen > now)
			{
				diff = readyWhen - now;
			}

			if (diff > TimeSpan.Zero)
			{
				string time = String.Format("{0:D2}:{1:D2}:{2:D2}", diff.Hours, diff.Minutes, diff.Seconds);
				lines.Add(String.Format("<basefont color=#{0:X6}>Use: {1}", Color.LimeGreen.ToArgb(), time));
			}
			else if (!String.IsNullOrWhiteSpace(throwable.Usage))
			{
				lines.Add(String.Format("<basefont color=#{0:X6}>Use: {1}", Color.Cyan.ToArgb(), throwable.Usage));
			}

			if (!String.IsNullOrWhiteSpace(throwable.Token))
			{
				lines.Add(String.Format("<basefont color=#{0:X6}>\"{1}\"", Color.Gold.ToArgb(), throwable.Token));
			}

			if (lines.Count > 0)
			{
				list.Add(String.Format("{0}<basefont color=#ffffff>", String.Join("\n", lines)));
			}

			lines.Clear();
		}
	}
}