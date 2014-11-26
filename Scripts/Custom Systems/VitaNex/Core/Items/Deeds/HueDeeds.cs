#region Header
//   Vorspire    _,-'/-'/  HueDeeds.cs
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

using Server;
using Server.Items;
using Server.Mobiles;
using Server.Targeting;

using VitaNex.SuperGumps.UI;
using VitaNex.Targets;
#endregion

namespace VitaNex.Items
{
	public static class HueDeeds
	{
		public static event Action<IHueDeed> OnCreated;

		public static void InvokeCreated(IHueDeed d)
		{
			if (OnCreated != null)
			{
				OnCreated(d);
			}
		}
	}

	public interface IHueDeed
	{
		List<int> Hues { get; }

		void AddHue(int hue);
		void AddHues(params int[] hues);
		void AddHueRange(int hue, int count);
	}

	public abstract class BaseHueDeed<TEntity> : Item, IHueDeed
		where TEntity : IEntity
	{
		private HueSelector _Gump;
		private GenericSelectTarget<TEntity> _Target;

		private readonly List<int> _Hues = new List<int>();

		public virtual List<int> Hues { get { return _Hues; } }

		public BaseHueDeed()
			: base(0x14F0)
		{
			Name = "Color Deed";
			Weight = 1.0;
			LootType = LootType.Blessed;

			Timer.DelayCall(HueDeeds.InvokeCreated, this);
		}

		public BaseHueDeed(Serial serial)
			: base(serial)
		{ }

		public void AddHue(int hue)
		{
			Hues.AddOrReplace(hue);
		}

		public void AddHues(params int[] hues)
		{
			if (hues == null || hues.Length == 0)
			{
				return;
			}

			foreach (var h in hues)
			{
				AddHue(h);
			}
		}

		public void AddHueRange(int hue, int count)
		{
			count = Math.Max(0, count);

			while (--count >= 0)
			{
				AddHue(hue++);
			}
		}

		public override void OnDoubleClick(Mobile m)
		{
			if (this.CheckDoubleClick(m, true, false, 2, true))
			{
				BeginTarget(m);
			}
		}

		protected virtual void BeginTarget(Mobile m)
		{
			if (_Target != null)
			{
				if (_Target.User != null && _Target.User.Target == _Target)
				{
					_Target.Cancel(_Target.User, TargetCancelType.Overriden);
				}

				_Target = null;
			}

			m.SendMessage("Target an object to recolor...");

			_Target = new GenericSelectTarget<TEntity>(
				(u, t) =>
				{
					_Target = null;
					OpenGump(u, t);
				},
				u =>
				{
					OnTargetFail(u);
					_Target = null;
				});

			m.Target = _Target;
		}

		protected virtual void OnTargetFail(Mobile m)
		{
			if (_Target.User == m && _Target.Result == TargetResult.Invalid)
			{
				m.SendMessage("That is not a valid target.");
			}
		}

		protected virtual void OpenGump(Mobile m, TEntity t)
		{
			if (_Gump != null)
			{
				_Gump.Close();
				_Gump = null;
			}

			m.SendMessage("Select a color from the chart...");

			_Gump = new HueSelector(m as PlayerMobile)
			{
				PreviewIcon = GetPreviewIcon(t),
				Hues = Hues.ToArray(),
				AcceptCallback = hue =>
				{
					_Gump = null;
					ApplyHue(m, t, hue);
				},
				CancelCallback = hue => _Gump = null
			};

			_Gump.Send();
		}

		public virtual int GetPreviewIcon(TEntity t)
		{
			return HueSelector.DefaultIcon;
		}

		protected virtual void ApplyHue(Mobile m, TEntity t, int hue)
		{
			if (m == null || t == null)
			{
				return;
			}

			Item item = t as Item;

			if (item != null)
			{
				item.Hue = hue;
				m.SendMessage("The item has been recolored.");
				Delete();
				return;
			}

			Mobile mob = t as Mobile;

			if (mob != null)
			{
				mob.Hue = hue;
				m.SendMessage("{0} skin has been recolored.", mob == m ? "Your" : "Their");
				Delete();
				return;
			}

			m.SendMessage("Could not hue that object.");
		}

		public override void OnDelete()
		{
			base.OnDelete();

			_Gump = null;
			_Target = null;

			_Hues.Clear();
			_Hues.TrimExcess();
		}

		public override void Serialize(GenericWriter writer)
		{
			base.Serialize(writer);

			writer.SetVersion(0);

			writer.WriteList(_Hues, writer.Write);
		}

		public override void Deserialize(GenericReader reader)
		{
			base.Deserialize(reader);

			reader.GetVersion();

			reader.ReadList(reader.ReadInt, _Hues);
		}
	}

	public abstract class ItemHueDeed<TItem> : BaseHueDeed<TItem>
		where TItem : Item
	{
		public ItemHueDeed()
		{ }

		public ItemHueDeed(Serial serial)
			: base(serial)
		{ }

		protected override void OpenGump(Mobile m, TItem t)
		{
			if (m == null || t == null)
			{
				return;
			}

			if (m.AccessLevel < AccessLevel.GameMaster)
			{
				if (!t.Movable && !t.IsAccessibleTo(m))
				{
					m.SendMessage("That item is not accessible.");
					return;
				}

				if (!t.IsChildOf(m.Backpack))
				{
					m.SendMessage("That item must be in your pack to recolor it.");
					return;
				}
			}

			base.OpenGump(m, t);
		}

		public override int GetPreviewIcon(TItem t)
		{
			return t == null ? HueSelector.DefaultIcon : t.ItemID;
		}

		public override void Serialize(GenericWriter writer)
		{
			base.Serialize(writer);

			writer.SetVersion(0);
		}

		public override void Deserialize(GenericReader reader)
		{
			base.Deserialize(reader);

			reader.GetVersion();
		}
	}

	public abstract class MobileHueDeed<TMobile> : BaseHueDeed<TMobile>
		where TMobile : Mobile
	{
		public MobileHueDeed()
		{ }

		public MobileHueDeed(Serial serial)
			: base(serial)
		{ }

		protected override void OpenGump(Mobile m, TMobile t)
		{
			if (m == null || t == null)
			{
				return;
			}

			if (m.AccessLevel < t.AccessLevel)
			{
				m.SendMessage("They are not accessible.");
				return;
			}

			base.OpenGump(m, t);
		}

		public override int GetPreviewIcon(TMobile t)
		{
			return t == null ? HueSelector.DefaultIcon : ShrinkTable.Lookup(t);
		}

		public override void Serialize(GenericWriter writer)
		{
			base.Serialize(writer);

			writer.SetVersion(0);
		}

		public override void Deserialize(GenericReader reader)
		{
			base.Deserialize(reader);

			reader.GetVersion();
		}
	}

	public class SkinHueDeed : MobileHueDeed<PlayerMobile>
	{
		[Constructable]
		public SkinHueDeed()
		{
			Name = "Skin Recolor Deed";
			AddHueRange(1002, 57);
		}

		public SkinHueDeed(Serial serial)
			: base(serial)
		{ }

		protected override void BeginTarget(Mobile m)
		{
			OpenGump(m, m as PlayerMobile);
		}

		protected override void OpenGump(Mobile m, PlayerMobile t)
		{
			if (m == null || t == null)
			{
				return;
			}

			if (m.AccessLevel < AccessLevel.GameMaster)
			{
				if (m != t)
				{
					m.SendMessage("You can only recolor your own skin.");
					return;
				}
			}

			base.OpenGump(m, t);
		}

		public override void Serialize(GenericWriter writer)
		{
			base.Serialize(writer);

			writer.SetVersion(0);
		}

		public override void Deserialize(GenericReader reader)
		{
			base.Deserialize(reader);

			reader.GetVersion();
		}
	}

	public class PetHueDeed : MobileHueDeed<BaseCreature>
	{
		[Constructable]
		public PetHueDeed()
		{
			Name = "Pet Recolor Deed";
			AddHueRange(2301, 18);
		}

		public PetHueDeed(Serial serial)
			: base(serial)
		{ }

		protected override void OpenGump(Mobile m, BaseCreature t)
		{
			if (m == null || t == null)
			{
				return;
			}

			if (m.AccessLevel < AccessLevel.GameMaster)
			{
				if (!t.Controlled || t.GetMaster() != m)
				{
					m.SendMessage("You can only recolor the skin of pets that you own.");
					return;
				}
			}

			base.OpenGump(m, t);
		}

		public override void Serialize(GenericWriter writer)
		{
			base.Serialize(writer);

			writer.SetVersion(0);
		}

		public override void Deserialize(GenericReader reader)
		{
			base.Deserialize(reader);

			reader.GetVersion();
		}
	}

	public class WeaponHueDeed : ItemHueDeed<BaseWeapon>
	{
		[Constructable]
		public WeaponHueDeed()
		{
			Name = "Weapon Recolor Deed";
			AddHueRange(2401, 30);
		}

		public WeaponHueDeed(Serial serial)
			: base(serial)
		{ }

		public override void Serialize(GenericWriter writer)
		{
			base.Serialize(writer);

			writer.SetVersion(0);
		}

		public override void Deserialize(GenericReader reader)
		{
			base.Deserialize(reader);

			reader.GetVersion();
		}
	}

	public class ArmorHueDeed : ItemHueDeed<BaseArmor>
	{
		[Constructable]
		public ArmorHueDeed()
		{
			Name = "Armor Recolor Deed";
			AddHueRange(2401, 30);
		}

		public ArmorHueDeed(Serial serial)
			: base(serial)
		{ }

		public override void Serialize(GenericWriter writer)
		{
			base.Serialize(writer);

			writer.SetVersion(0);
		}

		public override void Deserialize(GenericReader reader)
		{
			base.Deserialize(reader);

			reader.GetVersion();
		}
	}

	public class ClothingHueDeed : ItemHueDeed<BaseClothing>
	{
		[Constructable]
		public ClothingHueDeed()
		{
			Name = "Clothing Recolor Deed";
			AddHueRange(2, 1000);
		}

		public ClothingHueDeed(Serial serial)
			: base(serial)
		{ }

		public override void Serialize(GenericWriter writer)
		{
			base.Serialize(writer);

			writer.SetVersion(0);
		}

		public override void Deserialize(GenericReader reader)
		{
			base.Deserialize(reader);

			reader.GetVersion();
		}
	}
}