#region Header
//   Vorspire    _,-'/-'/  MagicalFood.cs
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

using Server;
using Server.Items;
#endregion

namespace VitaNex.Items
{
	public interface IMagicFoodMod
	{ }

	public interface IMagicFood
	{
		TimeSpan BuffDuration { get; set; }
		int IconID { get; set; }
		int EffectID { get; set; }
		int SoundID { get; set; }

		bool Eat(Mobile from);
		bool ApplyBuff(Mobile from);
		bool RemoveBuff(Mobile from);
	}

	public abstract class MagicFood : Food, IMagicFood
	{
		private Timer DelayTimer { get; set; }

		[CommandProperty(AccessLevel.GameMaster)]
		public virtual TimeSpan BuffDuration { get; set; }

		[CommandProperty(AccessLevel.GameMaster)]
		public virtual int IconID { get; set; }

		[CommandProperty(AccessLevel.GameMaster)]
		public virtual int EffectID { get; set; }

		[CommandProperty(AccessLevel.GameMaster)]
		public virtual int SoundID { get; set; }

		public MagicFood(int itemID)
			: this(itemID, 1)
		{ }

		public MagicFood(int itemID, int amount)
			: base(amount, itemID)
		{
			BuffDuration = TimeSpan.FromSeconds(300.0);
		}

		public MagicFood(Serial serial)
			: base(serial)
		{ }

		public override sealed bool Eat(Mobile from)
		{
			if (from == null || from.Deleted)
			{
				return false;
			}

			if (CheckHunger(from))
			{
				from.PlaySound(Utility.Random(0x3A, 3));

				if (from.Body.IsHuman && !from.Mounted)
				{
					from.Animate(34, 5, 1, true, false, 0);
				}

				if (Poison != null)
				{
					from.ApplyPoison(Poisoner, Poison);
				}

				OnEaten(from);
				Consume();
				return true;
			}

			return false;
		}

		public abstract bool ApplyBuff(Mobile from);
		public abstract bool RemoveBuff(Mobile from);

		public override bool CheckHunger(Mobile from)
		{
			if (FillFactor <= 0)
			{
				return true;
			}

			return FillHunger(from, FillFactor);
		}

		protected virtual void OnEaten(Mobile from)
		{
			if (from == null || from.Deleted)
			{
				return;
			}

			DoApplyBuff(from);
		}

		protected virtual void DoApplyBuff(Mobile from)
		{
			if (from == null || from.Deleted)
			{
				return;
			}

			if (ApplyBuff(from))
			{
				if (SoundID > 0)
				{
					from.PlaySound(SoundID);
				}

				if (EffectID > 0)
				{
					from.FixedParticles(EffectID, 10, 15, 5018, EffectLayer.Waist);
				}

				from.Send(new AddBuffPacket(from, new BuffInfo((BuffIcon)IconID, LabelNumber)));

				if (DelayTimer != null && DelayTimer.Running)
				{
					DelayTimer.Stop();
				}

				DelayTimer = Timer.DelayCall(BuffDuration, DoRemoveBuff, from);
			}
		}

		protected virtual void DoRemoveBuff(Mobile from)
		{
			if (from == null || from.Deleted)
			{
				return;
			}

			if (RemoveBuff(from))
			{
				from.Send(new RemoveBuffPacket(from, (BuffIcon)IconID));

				if (DelayTimer != null && DelayTimer.Running)
				{
					DelayTimer.Stop();
				}

				DelayTimer = null;
			}
		}

		public override void Serialize(GenericWriter writer)
		{
			base.Serialize(writer);

			int version = writer.SetVersion(0);

			switch (version)
			{
				case 0:
					{
						writer.Write(BuffDuration);
						writer.Write(IconID);
						writer.Write(EffectID);
						writer.Write(SoundID);
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
						BuffDuration = reader.ReadTimeSpan();
						IconID = reader.ReadInt();
						EffectID = reader.ReadInt();
						SoundID = reader.ReadInt();
					}
					break;
			}
		}
	}
}