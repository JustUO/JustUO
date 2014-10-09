#region Header
//   Vorspire    _,-'/-'/  StrobeLantern.cs
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
	public class StrobeLantern : Lantern
	{
		private Timer _Timer;
		private int _HueCycleIndex;
		private TimeSpan _HueCycleDelay;

		[CommandProperty(AccessLevel.GameMaster)]
		public override int Hue
		{
			get
			{
				if (Burning && HueCycle != null && HueCycle.Length > 0)
				{
					if (_HueCycleIndex >= HueCycle.Length)
					{
						_HueCycleIndex = 0;
					}
					else if (_HueCycleIndex < 0)
					{
						_HueCycleIndex = HueCycle.Length - 1;
					}

					return HueCycle[_HueCycleIndex];
				}

				return base.Hue;
			}
			set { base.Hue = value; }
		}

		[CommandProperty(AccessLevel.GameMaster)]
		public virtual TimeSpan HueCycleDelay
		{
			get { return _HueCycleDelay; }
			set
			{
				_HueCycleDelay = TimeSpan.FromSeconds(Math.Max(0.100, value.TotalSeconds));

				if (_Timer != null)
				{
					_Timer.Interval = _HueCycleDelay;
				}
			}
		}

		[CommandProperty(AccessLevel.GameMaster)]
		public virtual bool HueCycleReverse { get; set; }

		public virtual short[] HueCycle { get; set; }
		public virtual LightType[] Lights { get; set; }

		[Constructable]
		public StrobeLantern()
		{
			Name = "Strobe Lantern";
			Weight = 4;
			Light = LightType.Circle300;
			Hue = 0;

			HueCycleDelay = TimeSpan.FromSeconds(0.5);
			HueCycle = new short[] {1, 11, 22, 33, 44, 55, 66, 77, 88, 99};
			Lights = new[] {LightType.Circle150, LightType.Circle225, LightType.Circle300};
		}

		public StrobeLantern(Serial serial)
			: base(serial)
		{ }

#if NEWPARENT
		public override void OnAdded(IEntity parent)
#else
		public override void OnAdded(object parent)
#endif
		{
			base.OnAdded(parent);

			if (!Burning || !(Parent is Mobile))
			{
				EndHueCycle();
				return;
			}

			EndHueCycle();
			BeginHueCycle();
		}

#if NEWPARENT
		public override void OnRemoved(IEntity parent)
#else
		public override void OnRemoved(object parent)
#endif
		{
			base.OnRemoved(parent);

			if (!Burning || !(parent is Mobile))
			{
				EndHueCycle();
				return;
			}

			EndHueCycle();
			BeginHueCycle();
		}

		public override void Ignite()
		{
			base.Ignite();

			if (Burning)
			{
				BeginHueCycle();
			}
		}

		public override void Burn()
		{
			base.Burn();

			if (!Burning)
			{
				EndHueCycle();
			}
		}

		private void BeginHueCycle()
		{
			if (_Timer == null)
			{
				_Timer = Timer.DelayCall(HueCycleDelay, HueCycleDelay, CycleHue);
			}
			else
			{
				_Timer.Start();
			}

			OnHueCycleBegin();
		}

		private void EndHueCycle()
		{
			if (_Timer != null)
			{
				_Timer.Stop();
				_Timer = null;
			}

			Light = LightType.DarkCircle300;

			OnHueCycleEnd();
		}

		private void CycleHue()
		{
			if (Map == null || Map == Map.Internal || (!(Parent is Mobile) && Parent != null))
			{
				EndHueCycle();
				return;
			}

			if (HueCycleReverse)
			{
				--_HueCycleIndex;
			}
			else
			{
				++_HueCycleIndex;
			}

			if (Lights != null && Lights.Length > 0)
			{
				Light = Lights.GetRandom();
			}

			ReleaseWorldPackets();
			Delta(ItemDelta.Update);

			OnHueCycled();
		}

		protected virtual void OnHueCycled()
		{ }

		protected virtual void OnHueCycleBegin()
		{ }

		protected virtual void OnHueCycleEnd()
		{ }

		public override void Serialize(GenericWriter writer)
		{
			base.Serialize(writer);

			writer.Write(0);

			writer.Write(_HueCycleDelay);
			writer.Write(_HueCycleIndex);
			writer.Write(HueCycleReverse);
			writer.WriteArray(HueCycle, writer.Write);
		}

		public override void Deserialize(GenericReader reader)
		{
			base.Deserialize(reader);

			reader.ReadInt();

			_HueCycleDelay = reader.ReadTimeSpan();
			_HueCycleIndex = reader.ReadInt();
			HueCycleReverse = reader.ReadBool();
			HueCycle = reader.ReadArray(reader.ReadShort);
		}
	}
}