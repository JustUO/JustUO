#region Header
//   Vorspire    _,-'/-'/  DeceitBrazier.cs
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
using System.Linq;

using Server;
using Server.Items;

using VitaNex.FX;
#endregion

namespace VitaNex.Modules
{
	public sealed class DeceitBrazier : BaseLight
	{
		public static TimeSpan DefaultCoolDown = TimeSpan.FromMinutes(15);

		public DeceitBrazier()
			: base(0xE31)
		{
			Burning = false;

			Light = LightType.Circle300;
			Duration = CoolDown = DefaultCoolDown;

			Movable = false;
			Weight = 20.0;
		}

		public DeceitBrazier(Serial serial)
			: base(serial)
		{
			Duration = CoolDown = DefaultCoolDown;
		}

		public override int LitItemID { get { return 0xE31; } }

		public override int UnlitItemID { get { return 0xE31; } }

		public override int BurntOutItemID { get { return 0xE31; } }

		[CommandProperty(DeceitBraziers.Access)]
		public TimeSpan CoolDown { get; set; }

		public override void GetProperties(ObjectPropertyList list)
		{
			base.GetProperties(list);

			list.Add(
				"<basefont color=#{0:X6}>{1}\n<basefont color=#{2:X6}>{3}<basefont color=#ffffff>",
				Color.SkyBlue.ToArgb(),
				"Use: Summons a random hostile creature",
				Color.SkyBlue.ToArgb(),
				"Regenerating: " + Duration.ToSimpleString("h:m:s"));
		}

		public override void Ignite()
		{
			var points = new List<Point3D>();

			Location.ScanRange(
				Map,
				5,
				r =>
				{
					if (!Map.CanSpawnMobile(r.Current))
					{
						r.Exclude();
					}

					if (!r.Excluded)
					{
						points.Add(r.Current);
					}

					return false;
				});

			if (points.Count == 0)
			{
				return;
			}

			Type t = this.GetRandomSpawn();

			if (t == null)
			{
				return;
			}

			Mobile m = VitaNexCore.TryCatchGet(() => t.CreateInstance<Mobile>(), DeceitBraziers.CMOptions.ToConsole);

			if (m == null)
			{
				return;
			}

			VitaNexCore.TryCatch(
				() =>
				{
					Duration = CoolDown;
					Protected = true;
					base.Ignite();

					FireExplodeEffect fx = new FireExplodeEffect(this, Map, 5, 2)
					{
						Reversed = true,
						EffectHandler =
							e =>
							e.Source.GetMobilesInRange(e.Map, 0)
							 .Where(v => v != null && v.CanBeDamaged())
							 .ForEach(v => AOS.Damage(v, Utility.RandomMinMax(10, 20), 10, 80, 0, 0, 10))
					};

					fx.Callback = () =>
					{
						if (fx.CurrentProcess < fx.Repeat)
						{
							return;
						}

						new SmokeExplodeEffect(Location, Map, 1).Send();
						m.MoveToWorld(points.GetRandom(), Map);
					};

					fx.Send();
				},
				ex => m.Delete());
		}

		public override void Douse()
		{
			BurntOut = false;
			Protected = false;

			base.Douse();

			Duration = CoolDown;
		}

		public override void Serialize(GenericWriter writer)
		{
			base.Serialize(writer);

			int version = writer.SetVersion(0);

			switch (version)
			{
				case 0:
					writer.Write(CoolDown);
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
					Duration = CoolDown = reader.ReadTimeSpan();
					break;
			}

			Timer.DelayCall(TimeSpan.FromSeconds(60), InternalSync);
		}

		private void InternalSync()
		{
			if (!DeceitBraziers.Registry.ContainsKey(this))
			{
				Delete();
			}
		}
	}
}