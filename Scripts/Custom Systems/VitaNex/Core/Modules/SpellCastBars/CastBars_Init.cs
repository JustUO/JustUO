#region Header
//   Vorspire    _,-'/-'/  CastBars_Init.cs
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
using Server.Mobiles;

using VitaNex.IO;
#endregion

namespace VitaNex.Modules.CastBars
{
	[CoreModule("Spell Cast Bars", "1.0.0.0")]
	public static partial class SpellCastBars
	{
		static SpellCastBars()
		{
			States.OnSerialize = Serialize;
			States.OnDeserialize = Deserialize;
		}

		private static void CMConfig()
		{
			_InternalTimer = PollTimer.CreateInstance(
				TimeSpan.FromMilliseconds(10.0), PollCastBarQueue, () => CMOptions.ModuleEnabled);
		}

		private static void CMInvoke()
		{
			EventSink.CastSpellRequest += OnSpellRequest;
		}

		private static void CMEnabled()
		{
			EventSink.CastSpellRequest += OnSpellRequest;
			_InternalTimer.Start();
		}

		private static void CMDisabled()
		{
			EventSink.CastSpellRequest -= OnSpellRequest;
			_InternalTimer.Stop();
		}

		private static void CMSave()
		{
			DataStoreResult result = States.Export();
			CMOptions.ToConsole("{0} profiles saved, {1}", States.Count.ToString("#,0"), result);
		}

		private static void CMLoad()
		{
			DataStoreResult result = States.Import();
			CMOptions.ToConsole("{0} profiles loaded, {1}.", States.Count.ToString("#,0"), result);
		}

		private static bool Serialize(GenericWriter writer)
		{
			int version = writer.SetVersion(0);

			switch (version)
			{
				case 0:
					{
						writer.WriteBlockDictionary(
							_States,
							(w, k, v) =>
							{
								w.Write(k);
								w.Write(v.Item1);
								w.Write(v.Item2.X);
								w.Write(v.Item2.Y);
							});
					}
					break;
			}

			return true;
		}

		private static bool Deserialize(GenericReader reader)
		{
			int version = reader.GetVersion();

			switch (version)
			{
				case 0:
					{
						reader.ReadBlockDictionary(
							r =>
							{
								PlayerMobile k = r.ReadMobile<PlayerMobile>();
								bool v1 = r.ReadBool();
								Point v2 = new Point(r.ReadInt(), r.ReadInt());

								return new KeyValuePair<PlayerMobile, Tuple<bool, Point>>(k, new Tuple<bool, Point>(v1, v2));
							},
							_States);
					}
					break;
			}

			return true;
		}
	}
}