#region Header
//   Vorspire    _,-'/-'/  StinkBomb.cs
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

using Server;
using Server.Items;
using Server.Targeting;

using VitaNex.FX;
#endregion

namespace VitaNex.Items
{
	[Flipable(10248, 10249)]
	public class ThrowableStinkBomb : BaseThrowableAtMobile<Mobile>
	{
		public static Dictionary<Mobile, DateTime> Stinky { get; private set; }

		static ThrowableStinkBomb()
		{
			Stinky = new Dictionary<Mobile, DateTime>();
		}

		[CommandProperty(AccessLevel.GameMaster)]
		public int ExplosionRange { get; set; }

		[CommandProperty(AccessLevel.GameMaster)]
		public TimeSpan StinkyDuration { get; set; }

		[Constructable]
		public ThrowableStinkBomb()
			: this(1)
		{ }

		[Constructable]
		public ThrowableStinkBomb(int amount)
			: base(Utility.RandomList(10248, 10249), amount)
		{
			Name = "Stink Bomb";
			Usage = "Throw To Unleash A Terrible Smell!";
			Hue = 1270;
			Weight = 1.0;
			Stackable = true;

			StinkyDuration = TimeSpan.FromSeconds(30.0);

			TargetFlags = TargetFlags.None;
			Delivery = ThrowableAtMobileDelivery.None;

			ThrowSound = 1491;
			ImpactSound = 1064;

			EffectID = ItemID;
			EffectHue = Hue;

			ThrowRecovery = TimeSpan.FromSeconds(60.0);

			ExplosionRange = 5;

			RequiredSkill = SkillName.Alchemy;
			RequiredSkillValue = 25.0;
		}

		public ThrowableStinkBomb(Serial serial)
			: base(serial)
		{ }

		public static void Initialize()
		{
			PollTimer.CreateInstance(
				TimeSpan.FromSeconds(5),
				() => Stinky.Keys.ForEach(
					m =>
					{
						if (CheckStinky(m))
						{
							DoStinkEffect(m);
						}
					}),
				() => Stinky.Count > 0);
		}

		public static bool CheckStinky(Mobile m)
		{
			if (Stinky.ContainsKey(m))
			{
				if (Stinky[m] >= DateTime.UtcNow)
				{
					return true;
				}

				Stinky.Remove(m);
			}

			return false;
		}

		public static void MakeStinky(Mobile m, TimeSpan duration)
		{
			if (!Stinky.ContainsKey(m))
			{
				Stinky.Add(m, DateTime.UtcNow + duration);
			}
			else
			{
				Stinky[m] = DateTime.UtcNow + duration;
			}
		}

		public static void DoStinkEffect(Mobile m)
		{
			if (!CheckStinky(m) || m.Hidden || !m.Alive)
			{
				return;
			}

			Effects.PlaySound(m.Location, m.Map, 1064);

			new PoisonExplodeEffect(
				m,
				m.Map,
				1,
				effectHandler: e =>
				{
					if (e.ProcessIndex != 0)
					{
						return;
					}

					var targets = e.Source.GetMobilesInRange(e.Map, 0);

					foreach (var t in targets.Where(t => t != null && !t.Deleted && t != m && !t.Hidden && t.Alive && t.Body.IsHuman))
					{
						Effects.PlaySound(t.Location, t.Map, Utility.RandomList(1065, 1066, 1067));
					}

					targets.Free(true);
				}).Send();
		}

		protected override void OnThrownAt(Mobile from, Mobile target)
		{
			if (from == null || from.Deleted || target == null)
			{
				return;
			}

			for (int layer = 0, range = ExplosionRange; layer < ExplosionRange && range >= 0; layer++, range--)
			{
				new PoisonExplodeEffect(target.Clone3D(0, 0, layer * 10), target.Map, range, 0, null, ExplosionStink).Send();
			}

			base.OnThrownAt(from, target);
		}

		public virtual void ExplosionStink(EffectInfo info)
		{
			Effects.PlaySound(info.Source.Location, info.Map, ImpactSound);

			Timer.DelayCall(
				TimeSpan.FromSeconds(1),
				() =>
				{
					var targets = info.Source.GetMobilesInRange(info.Map, 0);

					targets.RemoveAll(t => t == null || t.Deleted || t.Hidden || (!t.Alive && !AllowDeadTarget));

					foreach (Mobile m in targets)
					{
						Effects.PlaySound(m.Location, m.Map, Utility.RandomList(1065, 1066, 1067));

						if (StinkyDuration > TimeSpan.Zero)
						{
							MakeStinky(m, StinkyDuration);
						}
					}

					targets.Free(true);
				});
		}

		public override void Serialize(GenericWriter writer)
		{
			base.Serialize(writer);

			int version = writer.SetVersion(0);

			switch (version)
			{
				case 0:
					{
						writer.Write(ExplosionRange);
						writer.Write(StinkyDuration);
					}
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
					{
						ExplosionRange = reader.ReadInt();
						StinkyDuration = reader.ReadTimeSpan();
					}
					break;
			}

			if (StinkyDuration <= TimeSpan.Zero)
			{
				StinkyDuration = TimeSpan.FromSeconds(30);
			}
		}
	}
}