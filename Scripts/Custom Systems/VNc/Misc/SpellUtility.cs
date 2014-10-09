#region Header
//   Vorspire    _,-'/-'/  SpellUtility.cs
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

using Server.Spells;
#endregion

namespace VitaNex
{
	public static class SpellUtility
	{
		public static string[] CircleNames = new[]
		{
			"First", "Second", "Third", "Fourth", "Fifth", "Sixth", "Seventh", "Eighth", "Necromancy", "Chivalry", "Bushido",
			"Ninjitsu", "Spellweaving"
		};

		public static bool Initialized { get; private set; }

		public static Dictionary<Type, SpellInfo> SpellsInfo { get; private set; }
		public static Dictionary<string, Dictionary<Type, SpellInfo>> TreeStructure { get; private set; }

		public static void Initialize()
		{
			if (Initialized)
			{
				return;
			}

			VitaNexCore.OnInitialized += () =>
			{
				SpellsInfo = new Dictionary<Type, SpellInfo>();

				SpellRegistry.Types.Where(t => t != null).ForEach(
					t =>
					{
						Spell spell = SpellRegistry.NewSpell(SpellRegistry.GetRegistryNumber(t), null, null);

						if (spell != null)
						{
							SpellsInfo.Add(t, spell.Info);
						}
					});

				InvalidateTreeStructure();
			};

			Initialized = true;
		}

		public static void InvalidateTreeStructure()
		{
			if (TreeStructure == null)
			{
				TreeStructure = new Dictionary<string, Dictionary<Type, SpellInfo>>(CircleNames.Length);
			}
			else
			{
				TreeStructure.Clear();
			}

			CircleNames.ForEach(
				circle =>
				{
					TreeStructure.Add(circle, new Dictionary<Type, SpellInfo>());

					SpellsInfo.Where(
						kvp => kvp.Key != null && kvp.Value != null && kvp.Key.FullName.StartsWith("Server.Spells." + circle))
							  .ForEach(kvp => TreeStructure[circle].Add(kvp.Key, kvp.Value));
				});
		}
	}
}