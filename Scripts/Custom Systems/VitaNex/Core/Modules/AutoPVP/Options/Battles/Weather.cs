#region Header
//   Vorspire    _,-'/-'/  Weather.cs
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

using VitaNex.Network;
#endregion

namespace VitaNex.Modules.AutoPvP
{
	public enum PvPBattleWeatherDirection
	{
		None,
		North,
		East,
		South,
		West,
		Random,
	}

	public class PvPBattleWeather : PropertyObject
	{
		public static int DefaultEffectID = 14036;
		public static int DefaultImpactEffectID = 14000;
		public static int DefaultImpactSound = 549;
		private int _Density;
		private int _EffectHue;
		private int _EffectID;
		private int _EffectSpeed;
		private int _ImpactEffectDuration;
		private int _ImpactEffectHue;
		private int _ImpactEffectID;
		private int _ImpactEffectSound;
		private int _ImpactEffectSpeed;

		public PvPBattleWeather()
		{
			Enabled = true;
			Force = false;
			Impacts = true;
			Density = 6;
			Direction = PvPBattleWeatherDirection.South;

			EffectID = DefaultEffectID;
			EffectHue = 0;
			EffectSpeed = 10;
			EffectRender = EffectRender.Normal;

			ImpactEffectID = DefaultImpactEffectID;
			ImpactEffectHue = 0;
			ImpactEffectSpeed = 10;
			ImpactEffectDuration = 30;
			ImpactEffectRender = EffectRender.Normal;
			ImpactEffectSound = DefaultImpactSound;
		}

		public PvPBattleWeather(GenericReader reader)
			: base(reader)
		{ }

		[CommandProperty(AutoPvP.Access)]
		public virtual bool Enabled { get; set; }

		[CommandProperty(AutoPvP.Access)]
		public virtual bool Force { get; set; }

		[CommandProperty(AutoPvP.Access)]
		public virtual bool Impacts { get; set; }

		[CommandProperty(AutoPvP.Access)]
		public virtual int Density { get { return _Density; } set { _Density = Math.Max(1, Math.Min(100, value)); } }

		[CommandProperty(AutoPvP.Access)]
		public virtual PvPBattleWeatherDirection Direction { get; set; }

		[CommandProperty(AutoPvP.Access)]
		public virtual int EffectID { get { return _EffectID; } set { _EffectID = Math.Max(0, value); } }

		[CommandProperty(AutoPvP.Access)]
		public virtual int EffectHue { get { return _EffectHue; } set { _EffectHue = Math.Max(0, Math.Min(3000, value)); } }

		[CommandProperty(AutoPvP.Access)]
		public virtual int EffectSpeed { get { return _EffectSpeed; } set { _EffectSpeed = Math.Max(1, Math.Min(10, value)); } }

		[CommandProperty(AutoPvP.Access)]
		public virtual EffectRender EffectRender { get; set; }

		[CommandProperty(AutoPvP.Access)]
		public virtual int ImpactEffectID { get { return _ImpactEffectID; } set { _ImpactEffectID = Math.Max(0, value); } }

		[CommandProperty(AutoPvP.Access)]
		public virtual int ImpactEffectHue { get { return _ImpactEffectHue; } set { _ImpactEffectHue = Math.Max(0, Math.Min(3000, value)); } }

		[CommandProperty(AutoPvP.Access)]
		public virtual int ImpactEffectSpeed { get { return _ImpactEffectSpeed; } set { _ImpactEffectSpeed = Math.Max(1, Math.Min(10, value)); } }

		[CommandProperty(AutoPvP.Access)]
		public virtual int ImpactEffectDuration { get { return _ImpactEffectDuration; } set { _ImpactEffectDuration = Math.Max(0, value); } }

		[CommandProperty(AutoPvP.Access)]
		public virtual EffectRender ImpactEffectRender { get; set; }

		[CommandProperty(AutoPvP.Access)]
		public virtual int ImpactEffectSound { get { return _ImpactEffectSound; } set { _ImpactEffectSound = Math.Max(0, value); } }

		public override void Clear()
		{
			Enabled = false;
			Force = false;
			Impacts = false;
			Density = 0;
			Direction = PvPBattleWeatherDirection.None;

			EffectID = 0;
			EffectHue = 0;
			EffectSpeed = 1;
			EffectRender = EffectRender.Normal;

			ImpactEffectID = 0;
			ImpactEffectHue = 0;
			ImpactEffectSpeed = 1;
			ImpactEffectDuration = 0;
			ImpactEffectRender = EffectRender.Normal;
			ImpactEffectSound = 0;
		}

		public override void Reset()
		{
			Enabled = true;
			Force = false;
			Impacts = true;
			Density = 6;
			Direction = PvPBattleWeatherDirection.South;

			EffectID = DefaultEffectID;
			EffectHue = 0;
			EffectSpeed = 10;
			EffectRender = EffectRender.Normal;

			ImpactEffectID = DefaultImpactEffectID;
			ImpactEffectHue = 0;
			ImpactEffectSpeed = 10;
			ImpactEffectDuration = 30;
			ImpactEffectRender = EffectRender.Normal;
			ImpactEffectSound = DefaultImpactSound;
		}

		public override string ToString()
		{
			return "Battle Weather";
		}

		public override void Serialize(GenericWriter writer)
		{
			base.Serialize(writer);

			int version = writer.SetVersion(0);

			switch (version)
			{
				case 0:
					{
						writer.Write(Enabled);
						writer.Write(Force);
						writer.Write(Impacts);
						writer.Write(Density);
						writer.WriteFlag(Direction);

						writer.Write(EffectID);
						writer.Write(EffectHue);
						writer.Write(EffectSpeed);
						writer.WriteFlag(EffectRender);

						writer.Write(ImpactEffectID);
						writer.Write(ImpactEffectHue);
						writer.Write(ImpactEffectSpeed);
						writer.Write(ImpactEffectDuration);
						writer.WriteFlag(ImpactEffectRender);
						writer.Write(ImpactEffectSound);
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
						Enabled = reader.ReadBool();
						Force = reader.ReadBool();
						Impacts = reader.ReadBool();
						Density = reader.ReadInt();
						Direction = reader.ReadFlag<PvPBattleWeatherDirection>();

						EffectID = reader.ReadInt();
						EffectHue = reader.ReadInt();
						EffectSpeed = reader.ReadInt();
						EffectRender = reader.ReadFlag<EffectRender>();

						ImpactEffectID = reader.ReadInt();
						ImpactEffectHue = reader.ReadInt();
						ImpactEffectSpeed = reader.ReadInt();
						ImpactEffectDuration = reader.ReadInt();
						ImpactEffectRender = reader.ReadFlag<EffectRender>();
						ImpactEffectSound = reader.ReadInt();
					}
					break;
			}
		}
	}
}